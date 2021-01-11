# Azure serverless URL shortening service

An Azure function implementing a URL shortening service backed by Azure blob storage 
in the fewest lines of code possible.

## Installing

1. Fork the repo
2. Create an Azure Functions app for it
3. Add an `X-Authorization` setting containing a shared key used for creating the short links
4. Add automated deployment from the repo


## Usage

The function provides a REST API for URL shortening, as well as the resolving itself.
All mutating operations (POST, PUT, DELETE) require the client providing an `X-Authorization` 
value that needs to match the setting configured server-side (that is, a shared key/secret).



