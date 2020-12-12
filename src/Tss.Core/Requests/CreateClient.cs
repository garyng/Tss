using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SpotifyAPI.Web;

namespace Tss.Core.Requests
{
	public record CreateClient(PKCETokenResponse Token) : IRequest<ISpotifyClient>;

	public class CreateClientRequestHandler : IRequestHandler<CreateClient, ISpotifyClient>
	{
		private readonly ILogger<CreateClientRequestHandler> _logger;
		private TssConfig _config;

		public CreateClientRequestHandler(ILogger<CreateClientRequestHandler> logger, IOptions<TssConfig> config)
		{
			_logger = logger;
			_config = config.Value;
		}

		public async Task<ISpotifyClient> Handle(CreateClient request, CancellationToken cancellationToken)
		{
			var authenticator = new PKCEAuthenticator(_config.ClientId, request.Token);
			authenticator.TokenRefreshed += async (_, t) => await SaveToken(t);

			var config = SpotifyClientConfig.CreateDefault()
				.WithAuthenticator(authenticator);

			await SaveToken(request.Token);
			return new SpotifyClient(config);
		}

		private async Task SaveToken(PKCETokenResponse token)
		{
			EnsureDirectoryExist(_config.CredentialsPath);
			var json = JsonConvert.SerializeObject(token);
			await File.WriteAllTextAsync(_config.CredentialsPath, json);
			_logger.LogInformation("Token saved to '{filename}'", _config.CredentialsPath);
		}

		private void EnsureDirectoryExist(string path)
		{
			var dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
		}
	}
}