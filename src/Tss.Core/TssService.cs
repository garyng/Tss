using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using MoreLinq;
using Tss.Core.Models;
using Tss.Core.Requests;
using Void = GaryNg.Utils.Void.Void;

namespace Tss.Core
{
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
		private readonly IMediator _mediator;

		public TssService(IOptions<TssConfig> config, IOptionsMonitor<TssMappings> mappings, ILogger<TssService> logger,
			IMediator mediator)
		{
			_mappings = mappings;
			_logger = logger;
			_mediator = mediator;
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


		public async Task CleanupCurrentPlaylist()
		{
			if (_client == null) return;
			var result = await (from current in Current.New(_client)
				select CleanupPlaylist(current.Playlist.Id)).Try();

			result.Match(_ => _logger.Information("Cleaned current playlist"),
				e => _logger.Error(e, "Unable to clean current playlist"));
		}

		public async Task CleanupPlaylist(string playlistId)
		{
			if (_client == null) return;

			var result = await (from current in Playlist.New(_client, playlistId)
				from goodId in GetTargetPlaylistId(current.Id, m => m.Good)
				from notGoodId in GetTargetPlaylistId(current.Id, m => m.NotGood)
				from good in Playlist.New(_client, goodId)
				from notGood in Playlist.New(_client, notGoodId)
				let _ = _logger.Information("Cleaning {current} (good: {good}, not good: {notGood})", current, good,
					notGood)
				from __ in _mediator.TrySend(new DuplicatePlaylist(_client, current))
				let ___ = _logger.Information("Duplicated playlist: {playlist}", current)
				from ____ in _mediator.TrySend(new CleanupPlaylist(_client, current, good, notGood))
				select current).Try();

			result.Match(
				current => _logger.Information("Cleaned playlist: {playlist}", current),
				e => _logger.Error(e, "Error while cleaning playlist"));
		}

		private TryAsync<string> GetTargetPlaylistId(string currentPlaylistId,
			Func<TssMappings.Mapping, string> @select) =>
			async () =>
			{
				var mappings = _mappings.CurrentValue;

				var found = mappings.Mappings.TryGetValue(currentPlaylistId, out var target);
				if (!found) target = mappings.Default;

				return @select(target!);
			};


		public async Task MoveTrack(Track track, Playlist source, Playlist target, bool skip)
		{
			if (_client == null) return;

			var result = await (from _ in _mediator.TrySend(new MoveTrack(_client, track, source, target, skip))
				let __ = _logger.Information(
					"Moved {track} from {source} to {target}", track, source, target)
				select Void.Default).Try();

			result.IfFail(e => _logger.Error(e, "Error while moving track"));
		}

		public async Task MoveCurrentToNotGood()
		{
			if (_client == null) return;
			await MoveCurrentTo(m => m.NotGood, true);
			_logger.Information("Moved current to not good");
		}

		public async Task MoveCurrentToGood()
		{
			if (_client == null) return;
			await MoveCurrentTo(m => m.Good, false);
			_logger.Information("Moved current to good");
		}

		public async Task MoveCurrentTo(Func<TssMappings.Mapping, string> getPlaylistId, bool skip)
		{
			if (_client == null) return;
			var result = await (from current in Current.Empty(_client)
				from targetId in GetTargetPlaylistId(current.Playlist.Id, getPlaylistId)
				from target in Playlist.Empty(_client, targetId)
				select MoveTrack(current.Track, current.Playlist, target, skip)).Try();

			result.IfFail(e => _logger.Error(e, "Error while moving current track"));
		}

		public async Task Testing()
		{
			// await CleanupCurrentPlaylist();
			await MoveCurrentToNotGood();
		}
	}
}