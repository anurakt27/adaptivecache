# AdaCache
Simple adaptive caching for ASP.NET Core.

## Table of contents
* [General Info](#general-info)
* [Architecture](#architecture)
* [Setup](#setup)
* [Example](#example)
* [Contact](#contact)

## General Info
AdaCache is a simple configuration-less automated caching solution that works with ASP.NET Core web projects. To maintain its configuration-less feature, AdaCache makes few assumptions:
* Using HTTP methods as intended. For e.g., using ```GET``` to retrieve resource and using ```POST``` to create new resource. If ```GET``` is used to create a resource, then the caching algorithm would not work as expected.

* Controllers should follow Single Responsibility Principle, AdaCache is most effective in that case. For e.g., a controller named WeatherController should only serve CRUD operations for Weather resource. AdaCahe would still work if SRP is not followed, but the efficiency will be less.

## Architecture

AdaCache serves as a middleware that intercepts incoming requests and outgoing responses and performs cache lookup and cache store. Here is a simple diagram of the architecture:

<img src="images/Presentation1.png" width="400" height="300"/>

### When and what to cache?
AdaCache caches response for every ```GET``` request. It internally maps each request to a controller and an action method to identify each request uniquely. Controller-action pair is always unique, so AdaCache generates a hash for each controller-action pair to perform cache storage and lookup.

### How is caching performed?
AdaCache internally implements an AVL tree that stores several responses uniquely identifiable via controller-action hashcode.

### When to delete from cache?
Whenever a ```PUT```/```POST```/```DELETE```/```PATCH``` action is performed by a controller, AdaCache assumes that the cached responses for that specific controller are no longer be valid since the resource has now been modified, it will discard the response(s). 

There is also a TTL value associated with each response (default 30 seconds), after which the response will automatically get discarded. After every succesfull cache hit, TTL value will get reset. 

## Setup
Add a project reference to AdaCache project, and then add the following to your Startup.cs file

```
 public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
 {
    ...
    app.UseMiddleware(typeof(AdaCacheMiddleware));
    ...
}
```
and done, no other configuration required!

## Example

This is dummy controller used to enact a long running web request using ```Thread.Sleep()```

```
[HttpGet]
public IEnumerable<WeatherForecast> Get()
{
    var rng = new Random();
    System.Threading.Thread.Sleep(2000);
    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        })
        .ToArray();
}
```

##### Response timing of first ```GET``` request

<img src="images/req1PNG.PNG" />

##### Response timing of second ```GET``` request

<img src="images/req2.PNG" />

## Contact
Reach out to me at anuraktofficial@gmail.com if you have any ideas or wish to contribute to this project.
