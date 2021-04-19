# AdaCache
Simple adaptive caching for ASP.NET Core.

## Table of contents
* [General Info](#general-info)
* [Architecture](#architecture)
* [Setup](#setup)
* [Example](#example)

## General Info
AdaCache is a simple configuration-less automated caching solution that works with ASP.NET Core web projects. To maintain its configuration-less feature, AdaCache makes few assumptions:
* Using HTTP methods as intended. For e.g., using GET to retrieve resource and using POST to create new resource. If GET is used to create a resource, then the caching algorithm would not work as expected.

* Controllers should follow Single Responsibility Principle, AdaCache is most effective in that case. For e.g., a controller named WeatherController should only serve CRUD operations for Weather resource. AdaCahe would still work if SRP is not followed, but the efficiency will be less.

## Architecture

AdaCache serves as a middleware that intercepts incoming requests and outgoing responses and performs cache lookup and cache store. Here is a simple diagram of the architecture:

### When and what to cache?
AdaCache caches response for every GET request. It internally maps each request to a controller and an action method to identify each request uniquely. Controller-action pair is always unique, so AdaCache generates a hash for each controller-action pair to perform cache storage and lookup.

### How is caching performed?
AdaCache internally implements an AVL tree that stores several responses uniquely identifiable via controller-action hashcode.

### When to delete from cache?
Whenever a PUT/POST/DELETE/PATCH action is performed by a controller, AdaCache assumes that the cached responses for that specific controller are no longer be valid since the resource has now been modified, it will discard the response(s). 

There is also a TTL value associated with each response (default 30 seconds), after which the response will automatically get discarded. After every succesfull cache hit, TTL value will get reset. 
