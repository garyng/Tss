using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using MoreLinq;

namespace Tss.Core
{
	public class Track : IEquatable<Track>
	{
		public bool Equals(Track? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Uri == other.Uri;
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Track) obj);
		}

		public override int GetHashCode()
		{
			return Uri.GetHashCode();
		}

		public static bool operator ==(Track? left, Track? right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Track? left, Track? right)
		{
			return !Equals(left, right);
		}

		[NotNull]
		public string Uri { get; set; }

		[NotNull]
		public string Name { get; set; }

		public Track([NotNull] IPlayableItem item)
		{
			(Uri, Name) = item switch
			{
				FullTrack track => (track.Uri, track.Name),
				FullEpisode episode => (episode.Uri, episode.Name),
			};
		}

		public void Deconstruct(out string uri, out string name)
		{
			uri = Uri;
			name = Name;
		}
	}


	public class TssService
	{
		protected string _clientId;
		protected string _credentialsPath;
		protected string _callbackUrl;
		protected int _callbackPort;

		protected TssLoginFlow _loginFlow;
		protected SpotifyClient? _client;
		private IOptionsMonitor<TssMappings> _mappings;
		private readonly ILogger<TssService> _logger;

		public TssService(IOptions<TssConfig> config, IOptionsMonitor<TssMappings> mappings, ILogger<TssService> logger)
		{
			_mappings = mappings;
			_logger = logger;
			var c = config.Value;
			_clientId = c.ClientId;
			_credentialsPath = c.CredentialsPath;
			_callbackUrl = $"http://localhost:{c.CallbackPort}/callback";
			_callbackPort = c.CallbackPort;
		}

		public async Task<TryLoginResult> TryLogin()
		{
			if (File.Exists(_credentialsPath))
			{
				var token = await LoadToken();
				_logger.LogInformation("Loaded token from '{filename}'", _credentialsPath);
				await CreateClient(token);
				return new TryLoginResult(true, null);
			}

			_loginFlow = new TssLoginFlow(_clientId, _callbackUrl);
			var url = await _loginFlow.Start();
			_logger.LogInformation("Started authentication flow");
			return new TryLoginResult(false, url);
		}

		public async Task CompleteLogin(string code)
		{
			var token = await _loginFlow!.Complete(code);
			_logger.LogInformation("Completed authentication flow");
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
			_logger.LogInformation("Token saved to '{filename}'", _credentialsPath);
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
			_logger.LogInformation("Moved current to good");
		}

		public async Task MoveCurrentToNotGood()
		{
			await MoveCurrentTo(m => m.NotGood);
			_logger.LogInformation("Moved current to not good");
		}

		public async Task MoveCurrentTo(Func<TssMappings.Mapping, string> getPlaylistId, bool skip = true)
		{
			if (_client == null) return;

			// todo: return track name, playlist name (source and target)?

			var current = await Current();

			var targetPlaylistId = GetTargetPlaylistId(current.playlistId, getPlaylistId);

			if (!current.track.HasValue) return;

			var (name, trackUri) = current.track.Value;

			await TryRemoveFromPlaylist(current.playlistId, trackUri);
			await AddToPlaylist(targetPlaylistId, trackUri);

			if (skip)
			{
				await _client.Player.SkipNext();
			}
		}


		private string GetTargetPlaylistId(string? currentPlaylistId, Func<TssMappings.Mapping, string> select)
		{
			var mappings = _mappings.CurrentValue;

			if (currentPlaylistId == null) return select(mappings.Default);

			var found = mappings.Mappings.TryGetValue(currentPlaylistId, out var target);
			if (!found) target = mappings.Default;

			return select(target!);
		}

		private async Task TryRemoveFromPlaylist(string? playlistId, string trackUri)
		{
			if (_client == null) return;
			if (string.IsNullOrEmpty(playlistId)) return;

			try
			{
				_logger.LogInformation("Try remove {trackUri} from {playlistId}", trackUri, playlistId);
				await _client.Playlists.RemoveItems(playlistId, new PlaylistRemoveItemsRequest
				{
					Tracks = new[] {new SpotifyAPI.Web.PlaylistRemoveItemsRequest.Item() {Uri = trackUri}}
				});
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error removing {trackUri} from {playlistId}", trackUri, playlistId);
			}
		}

		private async Task AddToPlaylist(string playlistId, string trackUri)
		{
			if (_client == null) return;
			_logger.LogInformation("Add {trackUri} to {playlistId}", trackUri, playlistId);
			await _client.Playlists.AddItems(playlistId, new PlaylistAddItemsRequest(new[] {trackUri}));
		}


		private async Task<((string name, string trackUri)? track, string? playlistId)> Current()
		{
			if (_client == null) return (null, null);

			var current = await _client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
			var currentPlaylistId = Regex.Match(current?.Context.Uri ?? "", "playlist:(?<id>.*)").Groups["id"].Value;

			((string name, string uri)? track, string playlistId) result = current?.Item switch
			{
				FullTrack track => ((track.Name, track.Uri), currentPlaylistId),
				FullEpisode episode => ((episode.Name, episode.Uri), currentPlaylistId),
				_ => (null, null)
			};
			_logger.LogInformation("[Current] track: {trackName} ({trackUri}) playlist: {playlistId}",
				result.track?.name, result.track?.uri, result.playlistId);
			return result;
		}

		private async Task<((string name, string trackUri)? track, string? playlistId)> Previous()
		{
			var previousTrack = (await _client.Player.GetRecentlyPlayed(new PlayerRecentlyPlayedRequest
			{
				Limit = 1
			}))?.Items?.FirstOrDefault();

			throw new NotImplementedException("Recently played is not recent enough.");
		}

		private async Task<IEnumerable<IEnumerable<Track>>> GetTrackBatches(string playlistId, int size = 99)
		{
			var page = await _client.Playlists.GetItems(playlistId);
			return await GetTrackBatches(page);
		}

		private async Task<IEnumerable<IEnumerable<Track>>> GetTrackBatches(Paging<PlaylistTrack<IPlayableItem>> page,
			int size = 99)
		{
			return (await GetTracks(page))
				.Batch(size);
		}

		private async Task<IEnumerable<Track>> GetTracks(string playlistId)
		{
			var page = await _client.Playlists.GetItems(playlistId);
			return await GetTracks(page);
		}

		private async Task<IEnumerable<Track>> GetTracks(Paging<PlaylistTrack<IPlayableItem>> page)
		{
			return await _client.Paginate(page)
				.Select(item => new Track(item.Track))
				.ToListAsync();
		}

		public async Task CleanupPlaylist(string playlistId)
		{
			// backup playlist
			// remove good and not good tracks from current playlist

			if (_client == null) return;


			// var current = await Current();
			var currentPlaylistId = playlistId;
			var currentTracks = await GetTracks(currentPlaylistId);

			var good = GetTargetPlaylistId(currentPlaylistId, m => m.Good);
			var goodTracks = await GetTracks(good);

			var notGood = GetTargetPlaylistId(currentPlaylistId, m => m.NotGood);
			var notGoodTracks = await GetTracks(notGood);

			await DuplicatePlaylist(currentPlaylistId);

			var cleanTracks = currentTracks.Except(goodTracks)
				.Except(notGoodTracks);

			if (cleanTracks.SequenceEqual(currentTracks)) return;

			await _client.Playlists.ReplaceItems(currentPlaylistId, new PlaylistReplaceItemsRequest(new List<string>()));

			await cleanTracks
				.Batch(99)
				.Select(ts => new PlaylistAddItemsRequest(ts.Select(t => t.Uri).ToList()))
				.ToAsyncEnumerable()
				.SelectAwait(async request => _client.Playlists.AddItems(currentPlaylistId, request))
				.ToListAsync();
		}

		public async Task DuplicatePlaylist(string playlistId)
		{
			if (_client == null) return;


			var playlist = await _client.Playlists.Get(playlistId);

			var page = playlist.Tracks;

			var name = $"{playlist.Name} ({DateTime.Now:yyyyMMdd-HHmmss})";

			var backup = await _client.Playlists.Create(playlist.Owner.Id, new PlaylistCreateRequest(name));

			var batches = await GetTrackBatches(page);

			await batches
				.Select(uris => new PlaylistAddItemsRequest(uris
					.Select(t => t.Uri)
					.ToList()))
				.ToAsyncEnumerable()
				.SelectAwait(async request => await _client.Playlists.AddItems(backup.Id, request))
				.ToListAsync();
		}
	}
}