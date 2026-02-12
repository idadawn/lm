using Microsoft.Extensions.Hosting;
using Poxiao.Lab.CalcWorker.Extensions;
using Poxiao.Lab.CalcWorker.Services;
using Poxiao.Lab.CalcWorker.Worker;
using Poxiao.Lab.Entity.Config;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/calc-worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("========================================");
    Log.Information("  计算 Worker 服务启动中...");
    Log.Information("========================================");

    // 不使用 Poxiao 框架的 Inject()（会引入大量 Web 服务自动发现，导致 Worker 退出）
    // 仅使用 .NET 原生 Host + 手动注册所需服务
    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureAppConfiguration((hostContext, config) =>
        {
            // 加载 Configurations 子目录下的配置文件
            var basePath = AppContext.BaseDirectory;
            var configDir = Path.Combine(basePath, "Configurations");
            if (Directory.Exists(configDir))
            {
                foreach (var jsonFile in Directory.GetFiles(configDir, "*.json"))
                {
                    config.AddJsonFile(jsonFile, optional: true, reloadOnChange: false);
                }
            }
        })
        .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            // 绑定配置
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));
            services.Configure<LabOptions>(configuration.GetSection("Lab"));

            // 注册 SqlSugar（使用 WorkerSqlSugarRepository，不依赖 App.GetConfig）
            services.AddSqlSugarForWorker(configuration);

            // 注册 Lab 计算相关服务
            services.AddLabCalculationServices();

            // 注册 RabbitMQ 进度发布器
            services.AddSingleton<CalcProgressPublisher>();

            // 注册批次进度追踪器（内存单例，线程安全）
            services.AddSingleton<BatchProgressTracker>();

            // 注册核心 Worker（计算队列 + 判定队列各自独立消费）
            services.AddHostedService<CalcTaskConsumer>();
            services.AddHostedService<JudgeTaskConsumer>();
        });

    var host = builder.Build();

    Log.Information("计算 Worker 服务已成功启动，等待任务...");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "计算 Worker 服务启动失败！");
}
finally
{
    Log.Information("计算 Worker 服务已停止");
    await Log.CloseAndFlushAsync();
}
