using System;
using System.Threading.Tasks;
using Tss.Core;

namespace Tss.Cli
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var service = new TssService(@"credentials.json");
			await service.Init();
			await service.MoveToGoodPlaylist();
			Console.ReadKey();
		}
	}
}
