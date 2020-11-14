using System;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace Tss.Core
{
	public class TssLoginFlow
	{
		private readonly string _clientId;
		private readonly Uri _callbackUrl;
		private string _verifier;

		public TssLoginFlow(string clientId, string callbackUrl)
		{
			_clientId = clientId;
			_callbackUrl = new Uri(callbackUrl);
		}

		/// <summary>
		/// Start a new authentication flow.
		/// </summary>
		/// <returns>The login url</returns>
		public async Task<string> Start()
		{
			var (verifier, challenge) = PKCEUtil.GenerateCodes();
			_verifier = verifier;

			var request = new LoginRequest(_callbackUrl, _clientId, LoginRequest.ResponseType.Code)
			{
				CodeChallenge = challenge,
				CodeChallengeMethod = "S256",
				Scope = new[]
				{
					Scopes.UserReadRecentlyPlayed, Scopes.UserReadCurrentlyPlaying,
					Scopes.UserModifyPlaybackState,
					Scopes.PlaylistModifyPrivate, Scopes.PlaylistModifyPublic
				}
			};

			return request.ToUri().ToString();
		}

		/// <summary>
		/// Finish the authentication flow.
		/// </summary>
		/// <returns>Token</returns>
		public async Task<PKCETokenResponse> Complete(string code)
		{
			return await new OAuthClient().RequestToken(
				new PKCETokenRequest(_clientId, code, _callbackUrl, _verifier)
			);
		}
	}
}