using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tss.Core;
using Serilog;

namespace Tss.Cli
{
	class Program
	{
		static async Task Main(string[] args)
		{
			using var host = Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, config) =>
				{
					var tssConfig = config.Build().GetSection(nameof(TssConfig))
						.Get<TssConfig>();

					if (File.Exists(tssConfig.MappingsPath))
					{
						config.AddYamlFile(tssConfig.MappingsPath, optional: false, reloadOnChange: true);
					}
				})
				.ConfigureLogging((context, builder) =>
				{
					Log.Logger = new LoggerConfiguration()
						.Enrich.FromLogContext()
						.WriteTo.Console(
							outputTemplate:
							"{Level:u3}: [{Timestamp:HH:mm:ss}] {SourceContext} {Scope:lj} {NewLine}     {Message:lj}{NewLine}{Exception}{NewLine}")
						.ReadFrom.Configuration(context.Configuration)
						.CreateLogger();

					builder.ClearProviders()
						.AddSerilog();
				})
				.ConfigureServices((context, services) =>
				{
					var tssConfig = context.Configuration.GetSection(nameof(TssConfig));
					services.Configure<TssConfig>(tssConfig);

					var mappingsConfig = context.Configuration.GetSection(nameof(TssMappings));
					services.Configure<TssMappings>(mappingsConfig);

					services.AddSingleton<StandaloneTssService>();
					services.AddSingleton<Startup>();
				}).Build();
			await host.StartAsync();

			await host.Services.GetRequiredService<Startup>().Run();
			Console.ReadKey();
			await host.StopAsync();
		}
	}

	public class Startup
	{
		private readonly StandaloneTssService _service;

		public Startup(StandaloneTssService service)
		{
			_service = service;
		}

		public async Task Run()
		{
			await _service.Login();
			await _service.MoveCurrentToGood();
			// await _service.MoveCurrentToNotGood();
		}
	}
}