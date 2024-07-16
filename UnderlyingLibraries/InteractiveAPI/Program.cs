using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();


app.UseHttpsRedirection();

var csharpKernel = new CSharpKernel()
    .UseNugetDirective()
    .UseKernelHelpers()
    .UseWho();

app.MapPost("/submit", async (Request request) =>
{
    object? output = null;
    bool hasAlreadySubmit = false;
    
    Formatter.SetPreferredMimeTypeFor(typeof(object), "text/plain");
    Formatter.Register<object>(o => o.ToString());

    while (output is null)
    {
        KernelCommandResult? response = null;
        if (!hasAlreadySubmit)
        {
            response = await csharpKernel.SendAsync(new SubmitCode(request.Submission));
            response!.KernelEvents.Subscribe(
                e =>
                {
                    switch (e)
                    {
                        case CommandFailed failed:
                            output = failed.Message;
                            break;
                        case DisplayEvent display:
                            output = display.FormattedValues.First().Value;
                            break;
                    }
                });
        }
        else hasAlreadySubmit = true;
    }

    return output;
});

app.Run();
