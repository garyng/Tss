using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web.Auth;

namespace Tss.Core
{
	public class StandaloneTssService : TssService
	{
		protected const int CALLBACK_PORT = 8123;

		public StandaloneTssService(IOptions<TssConfig> config, IOptionsMonitor<TssMappings> mappings) :
			base(config, mappings)
		{
		}

		/// <summary>
		/// Encapsulate the login flow with <see cref="EmbedIOAuthServer"/>, useful for console application.
		/// </summary>
		public async Task Login()
		{
			var server = new EmbedIOAuthServer(new Uri(CALLBACK_URL), CALLBACK_PORT);
			await server.Start();

			var auth = new TaskCompletionSource();
			server.AuthorizationCodeReceived += async (_, response) =>
			{
				await CompleteLogin(response.Code);
				auth.SetResult();
			};

			var (success, url) = await TryLogin();
			if (!success)
			{
				BrowserUtil.Open(new Uri(url!));
				await auth.Task;
			}

			await server.Stop();
			server.Dispose();
		}
	}
}