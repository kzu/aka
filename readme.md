# Azure serverless URL shortening service

An Azure function implementing a URL shortening service backed by Azure blob storage 
in the fewest lines of code possible (~70 LoC).

[Read the announcement](https://www.cazzulino.com/minimalist-shortlinks.html)

## Installing

1. Fork the repo
2. Create an Azure Functions app for it
3. Add an `X-Authorization` setting containing a shared key used for creating the short links
4. Add automated deployment from the repo

The short URLs will be persisted in the same Azure storage account used by the app, in a table 
named `Aka`.

## Usage

The function provides a REST API for URL shortening, as well as the resolving itself.
All mutating operations (POST, PUT, DELETE) require the client providing an `X-Authorization` 
value that needs to match the setting configured server-side (that is, a shared key/secret).

Usage is trivial: just `POST` or `PUT` to the desired short URL you want, passing the target 
URL in the body, like so: 

```
# create or update shortlink
curl -d [TARGET_URL] -H "X-Authorization: [SHARED_SECRET]" https://[FUNCTION_APP_URL]/[SHORT_ALIAS]
```

The `X-Authorization` must match what you specified in step 3 during install/configuration.

That's it.

Example redirection [stats](https://github.com/kzu/aka/blob/main/stats/run.csx) badge:

![redirects badge](https://img.shields.io/endpoint.svg?url=https://aka.kzu.dev/stats/redirect&label=%E2%A5%A4%20redirects&color=brightgreen&logo=Azure-Functions&logoColor=brightgreen)
