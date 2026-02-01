using Microsoft.Extensions.Configuration;
using System;

namespace Poxiao.API.Entry
{
    public static class ConfigurationHelper
    {
        public static IConfigurationBuilder AddEnvironmentVariables(this IConfigurationBuilder builder)
        {
            // Load from .env file if exists
            var envFile = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") switch
            {
                "Production" => ".env.production",
                "Development" => ".env.development",
                "Staging" => ".env.staging",
                _ => ".env"
            };

            if (System.IO.File.Exists(envFile))
            {
                builder.AddDotNetEnv(envFile);
            }

            return builder;
        }

        public static string GetRequiredValue(this IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Configuration value for '{key}' is required but not found.");
            }
            return value;
        }

        public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
        {
            var value = configuration.GetValue<T>(key);
            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
            {
                throw new InvalidOperationException($"Configuration value for '{key}' is required but not found.");
            }
            return value;
        }
    }

    // Extension method to load .env files
    public static class DotEnvExtensions
    {
        public static IConfigurationBuilder AddDotNetEnv(this IConfigurationBuilder builder, string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return builder;

            var envVars = System.IO.File.ReadAllLines(filePath);
            var config = new ConfigurationBuilder();

            foreach (var line in envVars)
            {
                var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    // Skip comments and empty lines
                    if (!key.StartsWith("#") && !string.IsNullOrEmpty(key))
                    {
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
            }

            return builder.AddEnvironmentVariables();
        }
    }
}