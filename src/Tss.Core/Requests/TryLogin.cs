using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using Tss.Core.Extensions;

namespace Tss.Core.Requests
{
	public record TryLogin : IRequest<Either<TssLoginFlow, PKCETokenResponse>>;

	public class TryLoginRequestHandler : IRequestHandler<TryLogin, Either<TssLoginFlow, PKCETokenResponse>>
	{
		private readonly ILogger<TryLoginRequestHandler> _logger;
		private TssConfig _config;

		public TryLoginRequestHandler(ILogger<TryLoginRequestHandler> logger, IOptions<TssConfig> config)
		{
			_logger = logger;
			_config = config.Value;
		}

		public async Task<Either<TssLoginFlow, PKCETokenResponse>> Handle(TryLogin request,
			CancellationToken cancellationToken)
		{
			return (await LoadToken(_config.CredentialsPath))
				.Case switch
				{
					PKCETokenResponse token => Prelude.Right(token),
					_ => Prelude.Left(new TssLoginFlow(_config.ClientId, _config.CallbackUrl))
				};
		}

		private async Task<Option<PKCETokenResponse>> LoadToken(string credentialsPath)
		{
			if (!File.Exists(credentialsPath)) return Option<PKCETokenResponse>.None;
			var json = await File.ReadAllTextAsync(credentialsPath);
			_logger.Information("Loaded token from \"{filepath}\"", credentialsPath);
			return JsonConvert.DeserializeObject<PKCETokenResponse>(json);
		}
	}
}