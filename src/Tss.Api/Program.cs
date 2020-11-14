using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tss.Core;

namespace Tss.Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, config) =>
				{
					var tssConfig = config.Build().GetSection(nameof(TssConfig))
						.Get<TssConfig>();

					if (File.Exists(tssConfig.MappingsPath))
					{
						config.AddYamlFile(tssConfig.MappingsPath, optional: false, reloadOnChange: true);
					}

				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
