using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SpotifyVolumeExtensionApi
{
	public static class Program
	{
		public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

		public static IWebHostBuilder CreateHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
				.ConfigureLogging(logger =>
				{
					logger
						.SetMinimumLevel(LogLevel.Debug)
						.AddFilter("Microsoft", LogLevel.Information)
						.AddFilter("System", LogLevel.Information);
				})
				.UseStartup<Startup>();
	}
}
