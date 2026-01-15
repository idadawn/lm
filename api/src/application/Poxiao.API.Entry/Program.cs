Serve.Run(RunOptions.Default.AddWebComponent<WebComponent>().WithArgs(args));

public class WebComponent : IWebComponent
{
    public void Load(WebApplicationBuilder builder, ComponentContext componentContext)
    {
        // 注册 AI 服务
        builder.Services.AddScoped<Poxiao.AI.Interfaces.IAiService, Poxiao.AI.Service.AiService>();
        builder.Services.AddScoped<Poxiao.AI.Interfaces.IAppearanceFeatureAnalysisService, Poxiao.AI.Service.AppearanceFeatureAnalysisService>();

        // 日志过滤
        builder.Logging.AddFilter(
            (provider, category, logLevel) =>
            {
                return !new[] { "Microsoft.Hosting", "Microsoft.AspNetCore" }.Any(u =>
                        category.StartsWith(u)
                    )
                    && logLevel >= LogLevel.Information;
            }
        );

        builder.WebHost.ConfigureKestrel(options =>
        {
            // 长度最好不要设置 null
            options.Limits.MaxRequestBodySize = 52428800;
        });

        builder.Logging.AddConsoleFormatter(options =>
        {
            options.DateFormat = "yyyy-MM-dd HH:mm:ss(zzz) dddd";
        });
    }

    public void Load(IWebHostBuilder builder, ComponentContext componentContext)
    {
        builder.ConfigureLogging(logging =>
        {
            // 日志过滤
            logging.AddFilter(
                (provider, category, logLevel) =>
                {
                    return !new[] { "Microsoft.Hosting", "Microsoft.AspNetCore" }.Any(u =>
                            category.StartsWith(u)
                        )
                        && logLevel >= LogLevel.Information;
                }
            );

            logging.AddConsoleFormatter(options =>
            {
                options.DateFormat = "yyyy-MM-dd HH:mm:ss(zzz) dddd";
            });
        });

        builder.ConfigureKestrel(options =>
        {
            // 长度最好不要设置 null
            options.Limits.MaxRequestBodySize = 52428800;
        });
    }
}
