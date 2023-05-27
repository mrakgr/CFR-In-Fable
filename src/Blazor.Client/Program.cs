using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazor.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();

// Good, it runs.
// As a reminder, dotnet watch run doesn't accept launch profiles due to being buggy so it will just run the first one.
// And we need to use it because unlike VS, Rider can't run Blazor in watch mode or with HMR.

// "hotReloadEnabled": true,
// "hotReloadProfile": "aspnetcore",

// We had to add these two fields to enable hot module reloading.

// Ok.

// Let's slowly start building out the UI. We've cut the branch, and now we can get to work on building it.