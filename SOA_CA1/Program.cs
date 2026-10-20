using SOA_CA1.Components;
using SOA_CA1.ApiClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient<RawgApiClient>(client =>
    client.BaseAddress = new Uri("https://api.rawg.io/api/"));
builder.Services.AddHttpClient<CheapSharkApiClient>(client =>
{
    client.BaseAddress = new Uri("https://www.cheapshark.com/api/1.0/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "GameHub/1.0 (+https://github.com/cheryl7114/gaming-deals-hub)");
});
builder.Services.AddHttpClient<FrankfurterApiClient>(client =>
    client.BaseAddress = new Uri("https://api.frankfurter.dev/"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();