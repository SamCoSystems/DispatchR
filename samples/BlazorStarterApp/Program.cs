using BlazorStarterApp.Counters;
using BlazorStarterApp.Weather;
using DispatchR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

services.AddRazorPages();
services.AddServerSideBlazor();

services.AddDispatchR(config =>
{
	config.RegisterTransient<FetchWeatherData.Handler>();
	config.RegisterScoped<Count.Handler>();
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
