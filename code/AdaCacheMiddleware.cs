using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.IO;
using System.Text;

namespace AdaptiveCache
{
    public class AdaCacheMiddleware
    {
        private readonly RequestDelegate next;
        private static Dictionary<string, List<int>> mapper;
        private readonly HttpMethod[] nonCacheableMethods;
        private static TreeBuilder tree;

        public AdaCacheMiddleware(RequestDelegate next)
        {
            this.next = next;
            this.nonCacheableMethods = new HttpMethod[] { HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete };
            mapper = new Dictionary<string, List<int>>();
            tree = new TreeBuilder();

            Task.Run(async () =>
            {
                while (true)
                {
                    tree.DeleteExpiredNodes();
                    await Task.Delay(CONSTANTS.DeletionInterval);
                }
            });
        }

        public async Task Invoke(HttpContext context)
        {
            string key = context.Request.RouteValues["controller"]?.ToString();
            int treeKey = new string[]
            {
                context.Request.RouteValues["controller"]?.ToString() ?? string.Empty,
                context.Request.RouteValues["action"]?.ToString() ?? string.Empty
            }
            .Aggregate(string.Empty, (acc, x) => string.Concat(acc, " ", x))
            .Trim()
            .GetHashCode();
            HttpMethod httpMethod = new HttpMethod(context.Request.Method);

            if (key != null)
            {
                if (mapper.ContainsKey(key))
                {
                    // return cached response
                    if (!nonCacheableMethods.Contains(httpMethod) && mapper[key].Contains((int)treeKey))
                    {
                        HttpResponseMessage responseMessage = tree.GetValue((int)treeKey) as HttpResponseMessage;
                        if(responseMessage != null)
                        {
                            await CopyToHttpResponse(context, responseMessage);
                            return;
                        }
                        await this.next(context);
                    }
                    // cache response
                    else if (!nonCacheableMethods.Contains(httpMethod) && !mapper[key].Contains((int)treeKey))
                    {
                        var stream = context.Response.Body;
                        using (var ms = new MemoryStream())
                        {
                            context.Response.Body = ms;
                            await this.next(context);
                            mapper[key].Add((int)treeKey);
                            HttpResponseMessage response = await CopyFromHttpResponse(context.Response);
                            tree.Add((int)treeKey, response);
                            await ms.CopyToAsync(stream);
                        }
                    }
                    // delete cached entries for the given controller
                    else
                    {
                        tree.DeleteRange(mapper[key]);
                        mapper[key].Clear();
                    }
                }
                // cache response
                else if (!nonCacheableMethods.Contains(httpMethod))
                {
                    var stream = context.Response.Body;
                    using (var ms = new MemoryStream())
                    {
                        context.Response.Body = ms;
                        await this.next(context);
                        mapper.Add(key, new List<int>() { (int)treeKey });
                        HttpResponseMessage response = await CopyFromHttpResponse(context.Response);
                        tree.Add((int)treeKey, response);
                        await ms.CopyToAsync(stream);
                    }
                }
                else await this.next(context);
            }
            else
            {
                await this.next(context);
            }
        }

        private async Task<HttpResponseMessage> CopyFromHttpResponse(HttpResponse response)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage((HttpStatusCode)response.StatusCode);

            if (response.Body != null)
            {
                response.Body.Position = 0;
                byte[] body = Encoding.UTF8.GetBytes((await new StreamReader(response.Body).ReadToEndAsync()));
                responseMessage.Content = new StreamContent(new MemoryStream(body));
                response.Body.Position = 0;
            }

            foreach (var header in response.Headers)
            {
                responseMessage.Content?.Headers.Add(header.Key, header.Value.ToArray());
            }

            return responseMessage;
        }

        private async Task CopyToHttpResponse(HttpContext context, HttpResponseMessage responseMessage)
        {
            context.Response.StatusCode = (int)responseMessage.StatusCode;

            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            await responseMessage.Content.CopyToAsync(context.Response.Body);
        }
    }
}
