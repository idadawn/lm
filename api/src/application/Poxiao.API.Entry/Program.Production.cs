using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace Poxiao.API.Entry
{
    public partial class Program
    {
        public static IHostBuilder CreateProductionHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Clear default configuration sources
                    config.Sources.Clear();

                    // Set base path
                    config.SetBasePath(Directory.GetCurrentDirectory());

                    // Load environment-specific .env file first
                    var env = hostingContext.HostingEnvironment;
                    var envFile = $".env.{env.EnvironmentName.ToLower()}";

                    if (File.Exists(envFile))
                    {
                        config.AddDotNetEnv(envFile);
                    }

                    // Add appsettings files
                    config.AddJsonFile("Configurations/appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"Configurations/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    // Add environment variables last (highest priority)
                    config.AddEnvironmentVariables();

                    // Add command line arguments if any
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    // Configure Kestrel for production
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Limits.MaxRequestBodySize = 104857600; // 100MB
                        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                        options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
                    });
                })
                .UseWindowsService() // Enable Windows Service hosting
                .UseSystemd(); // Enable Systemd hosting for Linux
    }
}