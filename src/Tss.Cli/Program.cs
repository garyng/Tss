using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tss.Core;

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
				.ConfigureServices((context, services) =>
				{
					var tssConfig = context.Configuration.GetSection(nameof(TssConfig));
					services.Configure<TssConfig>(tssConfig);

					var mappingsConfig = context.Configuration.GetSection("TssMappings");
					services.Configure<TssPlaylistMapping>(mappingsConfig);

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
		//private readonly IOptions<TssConfig> _config;
		//private readonly IOptionsMonitor<TssPlaylistMapping> _mappings;

		//public Startup(IOptions<TssConfig> config, IOptionsMonitor<TssPlaylistMapping> mappings)
		//{
		//	_config = config;
		//	_mappings = mappings;
		//	//_service = service;
		//}

		public Startup(StandaloneTssService service)
		{
			_service = service;
		}

		public async Task Run()
		{
			await _service.Login();
			// await _service.MoveCurrentToGood();
			// await _service.MoveCurrentToNotGood();
		}
	}
}