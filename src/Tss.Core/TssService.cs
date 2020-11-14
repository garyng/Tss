using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Tss.Core
{
	// todo: use record when resharper supports it
	//public record TryLoginResult(bool Success, string? LoginUrl);

	public class TssMappings
	{
		public class Mapping
		{
			public string Good { get; set; }
			public string NotGood { get; set; }
		}

		public Dictionary<string, Mapping> Mappings { get; set; }
		public Mapping Default { get; set; }
	}


	public class TssConfig
	{
		public string ClientId { get; set; }
		public string CredentialsPath { get; set; }
		public string MappingsPath { get; set; }
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

	public class TssService
	{
		protected const string CALLBACK_URL = "http://localhost:8123/callback";

		protected string _clientId;
		protected string _credentialsPath;

		protected TssLoginFlow _loginFlow;
		protected SpotifyClient _client;
		private IOptionsMonitor<TssMappings> _mappings;

		public TssService(IOptions<TssConfig> config, IOptionsMonitor<TssMappings> mappings)
		{
			_mappings = mappings;
			var c = config.Value;
			_clientId = c.ClientId;
			_credentialsPath = c.CredentialsPath;
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
			EnsureDirectoryExist(_credentialsPath);
			var json = JsonConvert.SerializeObject(token);
			await File.WriteAllTextAsync(_credentialsPath, json);
		}

		private void EnsureDirectoryExist(string path)
		{
			var dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
		}

		public async Task MoveCurrentToGood()
		{
			await MoveCurrentTo(m => m.Good, false);
		}

		public async Task MoveCurrentToNotGood()
		{
			await MoveCurrentTo(m => m.NotGood);
		}

		public async Task MoveCurrentTo(Func<TssMappings.Mapping, string> getPlaylistId, bool skip = true)
		{
			// todo: return track name, playlist name (source and target)?

			var current = await Current();

			var targetPlaylistId = GetTargetPlaylistId(current.playlistId, getPlaylistId);

			if (!current.track.HasValue) return;

			var (name, trackUri) = current.track.Value;

			await TryRemoveFromPlaylist(current.playlistId, trackUri);
			await AddToPlaylist(targetPlaylistId, trackUri);

			await _client.Player.SkipNext();
		}


		private string GetTargetPlaylistId(string? currentPlaylistId, Func<TssMappings.Mapping, string> select)
		{
			var mappings = _mappings.CurrentValue;

			if (currentPlaylistId == null)  return select(mappings.Default);

			var found = mappings.Mappings.TryGetValue(currentPlaylistId, out var target);
			if (!found) target = mappings.Default;

			return select(target!);
		}

		private async Task TryRemoveFromPlaylist(string? playlistId, string trackUri)
		{
			if (string.IsNullOrEmpty(playlistId)) return;

			try
			{
				await _client.Playlists.RemoveItems(playlistId, new PlaylistRemoveItemsRequest
				{
					Tracks = new[] {new SpotifyAPI.Web.PlaylistRemoveItemsRequest.Item() {Uri = trackUri}}
				});
			}
			catch (Exception)
			{
				// todo: log
			}
		}

		private async Task AddToPlaylist(string playlistId, string trackUri)
		{
			await _client.Playlists.AddItems(playlistId, new PlaylistAddItemsRequest(new[] {trackUri}));
		}


		private async Task<((string name, string trackUri)? track, string? playlistId)> Current()
		{
			var current = await _client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
			var currentPlaylistId = current?.Context.Uri.Replace("spotify:playlist:", "");

			return current?.Item switch
			{
				FullTrack track => ((track.Name, track.Uri), currentPlaylistId),
				FullEpisode episode => ((episode.Name, episode.Uri), currentPlaylistId),
				_ => (null, null)
			};
		}

		private async Task<((string name, string trackUri)? track, string? playlistId)> Previous()
		{
			var previousTrack = (await _client.Player.GetRecentlyPlayed(new PlayerRecentlyPlayedRequest
			{
				Limit = 1
			}))?.Items?.FirstOrDefault();

			throw new NotImplementedException("Recently played is not recent enough.");

		}
	}
}