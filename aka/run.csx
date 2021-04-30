using System;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

static readonly string authorization = Environment.GetEnvironmentVariable("X-Authorization");

public static IActionResult Run(
    HttpRequest req, 
    Aka aka, 
    ILogger log, 
    out Aka output, 
    string alias = "400")
{
    log.LogInformation($"alias={alias}, PK={aka?.PartitionKey}, RK={aka?.RowKey}, Url={aka?.Url}");

    output = default;

    if (alias == "400" || string.IsNullOrEmpty(alias))
        return new BadRequestResult();

    // Create
    if (req.Method == "POST" || req.Method == "PUT")
    {
        if (req.Headers.TryGetValue("X-Authorization", out var values) && 
            values.FirstOrDefault() == authorization)
        {
            using (var reader = new StreamReader(req.Body))
            {
                var url = reader.ReadToEnd();
                if (aka != null)
                    aka.Url = url;
                else
                    aka = new Aka { RowKey = alias, Url = url }; 

                output = aka;
                return new RedirectResult(url);
            }    
        }
        else
        {
            return new UnauthorizedResult();
        }
    }

    if (aka == null)
        return new NotFoundResult();

    var uri = new Uri(aka.Url);
    if (string.IsNullOrEmpty(uri.Query))
        return new RedirectResult(aka.Url + req.QueryString.Value);
    else
        return new RedirectResult(aka.Url + req.QueryString.Value.Substring(1));
}

public class Aka
{
    string rowKey = "400";

    public string PartitionKey { get; set; } = "aka";
    public string RowKey 
    { 
        get => rowKey;
        set 
        {
            if (!string.IsNullOrEmpty(value))
                rowKey = value;  
        }
    }

    public string Url { get; set; }
    public string ETag { get; } = "*";
}