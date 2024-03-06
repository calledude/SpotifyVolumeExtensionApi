using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace SpotifyVolumeExtensionApi;

public static class Program
{
	public static void Main(string[] args)
	{
		var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
		var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");

		if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
			return;

		var builder = WebApplication.CreateSlimBuilder(args);
		builder.WebHost.UseKestrelHttpsConfiguration();

		builder.Logging
			.SetMinimumLevel(LogLevel.Debug)
			.AddFilter("Microsoft", LogLevel.Information)
			.AddFilter("System", LogLevel.Information);

		builder.Services.AddHttpClient();

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseHttpsRedirection();

		app.MapSpotifyAuthenticationRoutes(clientId, clientSecret);

		app.Run();
	}
}
