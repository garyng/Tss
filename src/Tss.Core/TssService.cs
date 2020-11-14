using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyAPI.Web;

namespace Tss.Core
{
	public class TssConfig
	{
		public string ClientId { get; set; }
		public string CredentialsPath { get; set; }
	}

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

	public class TssService
	{
		private const string CALLBACK_URL = "http://localhost:8123/callback";

		private string _clientId;
		private string _credentialsPath;

		private TssLoginFlow _loginFlow;
		private SpotifyClient _client;

		public TssService(TssConfig config)
		{
			_clientId = config.ClientId;
			_credentialsPath = config.CredentialsPath;
		}

		public async Task<(bool success, string? loginUrl)> TryLogin()
		{
			if (File.Exists(_credentialsPath))
			{
				var token = await LoadToken();
				await CreateClient(token);
				return (true, null);
			}

			_loginFlow = new TssLoginFlow(_clientId, CALLBACK_URL);
			var url = await _loginFlow.Start();
			return (false, url);
		}

		public async Task CompleteLogin(string? code)
		{
			var token = await _loginFlow?.Complete(code);
			await CreateClient(token);
		}

		public async Task CreateClient(PKCETokenResponse token)
		{
			var authenticator = new PKCEAuthenticator(_clientId, token);
			authenticator.TokenRefreshed += async (_, t) => await SaveToken(t);

			var config = SpotifyClientConfig.CreateDefault()
				.WithAuthenticator(authenticator);

			await SaveToken(token);
			_client = new SpotifyClient(config);
		}

		private async Task<PKCETokenResponse> LoadToken()
		{
			var json = await File.ReadAllTextAsync(_credentialsPath);
			return JsonConvert.DeserializeObject<PKCETokenResponse>(json);
		}
		
		private async Task SaveToken(PKCETokenResponse token)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_credentialsPath));
			var json = JsonConvert.SerializeObject(token);
			await File.WriteAllTextAsync(_credentialsPath, json);
		}

		// todo: _service.Current.MoveToGood()
		// todo: _service.Previous.MoveToGood()
		public async Task MoveCurrentToGood()
		{
			var goodPlaylistId = "3PuDN2O1rz5wKmAEMnendn";
			var current = await _client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());

			if (current.Item is FullTrack track)
			{
				// todo: context will be null if not inside playlist
				var currentPlaylistId = current?.Context.Uri.Replace("spotify:playlist:", "");

				// todo: can't remove item from tracks you dont' own, eg: other playlist/radio
				await _client.Playlists.RemoveItems(currentPlaylistId, new PlaylistRemoveItemsRequest
				{
					Tracks = new[] { new SpotifyAPI.Web.PlaylistRemoveItemsRequest.Item() { Uri = track.Uri } }
				});

				await _client.Playlists.AddItems(goodPlaylistId, new PlaylistAddItemsRequest(new[] { track.Uri }));

				await _client.Player.SkipNext();
			}
		}
	}
}