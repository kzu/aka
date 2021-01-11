using System;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

static readonly string authorization = Environment.GetEnvironmentVariable("X-Authorization");

public static IActionResult Run(
    HttpRequest req, 
    string alias, 
    Aka aka, 
    ILogger log, 
    out Aka output)
{
    log.LogInformation($"alias={alias}, PK={aka?.PartitionKey}, RK={aka?.RowKey}, Url={aka?.Url}");

    output = default;

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

    return new RedirectResult(aka.Url);
}

public class Aka
{
    public string PartitionKey { get; set; } = "aka";
    public string RowKey { get; set; }
    public string Url { get; set; }
    public string ETag { get; } = "*";
}