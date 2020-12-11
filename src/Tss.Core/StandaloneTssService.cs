using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web.Auth;

namespace Tss.Core
{
	public class StandaloneTssService : TssService
	{
		public StandaloneTssService(IOptions<TssConfig> config, IOptionsMonitor<TssMappings> mappings,
			ILogger<TssService> logger, IMediator mediator) : base(config, mappings, logger, mediator)
		{
		}

		/// <summary>
		/// Encapsulate the login flow with <see cref="EmbedIOAuthServer"/>, useful for console application.
		/// </summary>
		public async Task Login()
		{
			var server = new EmbedIOAuthServer(new Uri(_callbackUrl), _callbackPort);
			await server.Start();

			var auth = new TaskCompletionSource();
			server.AuthorizationCodeReceived += async (_, response) =>
			{
				await CompleteLogin(response.Code);
				auth.SetResult();
			};

			var result = await TryLogin();
			if (!result.Success)
			{
				BrowserUtil.Open(new Uri(result.LoginUrl!));
				await auth.Task;
			}

			await server.Stop();
			server.Dispose();
		}
	}
}