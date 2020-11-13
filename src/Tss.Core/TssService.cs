using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Tss.Core
{
	
	public class TssService
	{
		private readonly string _credentialsPath;
		private PKCEAuthenticator _authenticator;
		private SpotifyClient _client;

		// todo: don't commit!
		// todo: pass in as config
		private const string CLIENT_ID = "xxx";
		private const string CALLBACK_URL = "http://localhost:8123/callback";
		private const int CALLBACK_PORT = 8123;


		public TssService(string credentialsPath)
		{
			_credentialsPath = credentialsPath;
		}

		public async Task Init()
		{
			(_authenticator, _client) = await CreateClient();
		}

		private async Task<(PKCEAuthenticator authenticator, SpotifyClient client)> CreateClient()
		{
			var token = File.Exists(_credentialsPath) switch
			{
				true => await LoadToken(),
				false => await TryLogin()
			};

			var authenticator = new PKCEAuthenticator(CLIENT_ID, token);
			authenticator.TokenRefreshed += async (_, t) => await SaveToken(t);

			var config = SpotifyClientConfig.CreateDefault()
				.WithAuthenticator(authenticator);

			var client = new SpotifyClient(config);
			return (authenticator, client);
		}

		private async Task<PKCETokenResponse> LoadToken()
		{
			var json = await File.ReadAllTextAsync(_credentialsPath);
			return JsonConvert.DeserializeObject<PKCETokenResponse>(json);
		}

		private async Task SaveToken(PKCETokenResponse token)
		{
			var json = JsonConvert.SerializeObject(token);
			await File.WriteAllTextAsync(_credentialsPath, json);
		}

		private async Task<PKCETokenResponse> TryLogin()
		{
			var server = new EmbedIOAuthServer(new Uri(CALLBACK_URL), CALLBACK_PORT);
			var (verifier, challenge) = PKCEUtil.GenerateCodes();

			await server.Start();

			var tokenSource = new TaskCompletionSource<PKCETokenResponse>();
			server.AuthorizationCodeReceived += async (_, response) =>
			{
				await server.Stop();
				var t = await new OAuthClient().RequestToken(
					new PKCETokenRequest(CLIENT_ID, response.Code, server.BaseUri, verifier)
				);
				tokenSource.SetResult(t);
			};

			var request = new LoginRequest(server.BaseUri, CLIENT_ID, LoginRequest.ResponseType.Code)
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

			var uri = request.ToUri();
			BrowserUtil.Open(uri);

			var token = await tokenSource.Task;
			await SaveToken(token);
			server.Dispose();
			return token;
		}

		public async Task MoveToGoodPlaylist()
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