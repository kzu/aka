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

    // Create if authorized, otherwise we just redirect as normal if we have one.
    if ((req.Method == "POST" || req.Method == "PUT") &&
        req.Headers.TryGetValue("X-Authorization", out var values) &&
        values.FirstOrDefault() == authorization)
    {
        log.LogInformation($"Found expected authorization header updating redirect URL.");
        using (var reader = new StreamReader(req.Body))
        {
            var url = reader.ReadToEnd();
            if (!Uri.TryCreate(url, UriKind.Absolute, out var redirectUri))
            {
                log.LogWarning($"Invalid body cannot be parsed  as a URL: {url}");
                return new BadRequestResult();
            }

            if (aka != null)
                aka.Url = url;
            else
                aka = new Aka { RowKey = alias, Url = url };

            output = aka;
            return new RedirectResult(url);
        }
    }

    if (aka == null)
        return new NotFoundResult();

    var uri = new Uri(aka.Url);
    if (aka.Fetch)
    {
        // fetch url and return raw content from upstream
        log.LogInformation($"Fetching URL: {aka.Url}");
        using (var client = new HttpClient())
        {
            var response = client.GetAsync(aka.Url).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                return new ContentResult
                {
                    Content = content,
                    ContentType = response.Content.Headers.ContentType?.MediaType ?? "text/plain",
                    StatusCode = (int)response.StatusCode
                };
            }
            else
            {
                log.LogWarning($"Failed to fetch URL: {aka.Url}, Status Code: {response.StatusCode}");
                return new StatusCodeResult((int)response.StatusCode);
            }
        }
    }

    if (string.IsNullOrEmpty(uri.Query))
        return new RedirectResult(aka.Url + req.QueryString.Value) { PreserveMethod = true };
    else
        return new RedirectResult(aka.Url + req.QueryString.Value.TrimStart('?')) { PreserveMethod = true };
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
    public bool Fetch { get; set; }
    public string ETag { get; } = "*";
}
