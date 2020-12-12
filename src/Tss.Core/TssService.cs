using System;
using System.Threading.Tasks;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using Tss.Core.Extensions;
using Tss.Core.Models;
using Tss.Core.Requests;
using Void = GaryNg.Utils.Void.Void;

namespace Tss.Core
{
	public class TssService
	{
		protected Option<ISpotifyClient> _client;
		protected Option<TssLoginFlow> _loginFlow;

		private IOptionsMonitor<TssMappings> _mappings;
		private readonly ILogger<TssService> _logger;
		private readonly IMediator _mediator;
		protected TssConfig _config;

		public TssService(IOptions<TssConfig> config, IOptionsMonitor<TssMappings> mappings, ILogger<TssService> logger,
			IMediator mediator)
		{
			_mappings = mappings;
			_logger = logger;
			_mediator = mediator;
			_config = config.Value;
		}

		public async Task<Option<string>> TryLogin()
		{
			return await (await _mediator.Send(new TryLogin()))
				.MatchAsync(
					CreateClient,
					StartFlow);

			async Task<Option<string>> StartFlow(TssLoginFlow flow)
			{
				_loginFlow = flow;
				return await flow.Start();
			}

			async Task<Option<string>> CreateClient(PKCETokenResponse token)
			{
				_client = Prelude.Some(await _mediator.Send(new CreateClient(token)));
				return default;
			}
		}

		public async Task CompleteLogin(string code)
		{
			await _loginFlow.Match(async flow =>
			{
				var token = await flow.Complete(code);
				_logger.LogInformation("Completed authentication flow");
				_client = Prelude.Some(await _mediator.Send(new CreateClient(token)));
			}, async () => _logger.Error("Unable to complete login flow"));
		}

		public async Task CleanupCurrentPlaylist()
		{
			var result = await (
				from client in _client.ToTryAsync()
				from current in Current.New(client)
				let _ = _logger.Information("Cleaning current playlist")
				select CleanupPlaylist(current.Playlist.Id)).Try();

			result.IfFail(e => _logger.Error(e, "Unable to clean current playlist"));
		}

		public async Task CleanupPlaylist(string playlistId)
		{
			var result = await (
				from client in _client.ToTryAsync()
				from current in Playlist.New(client, playlistId)
				from goodId in GetTargetPlaylistId(current.Id, m => m.Good)
				from notGoodId in GetTargetPlaylistId(current.Id, m => m.NotGood)
				from good in Playlist.New(client, goodId)
				from notGood in Playlist.New(client, notGoodId)
				let _ = _logger.Information("Cleaning {current} (good: {good}, not good: {notGood})", current, good,
					notGood)
				from __ in _mediator.TrySend(new DuplicatePlaylist(client, current))
				let ___ = _logger.Information("Duplicated playlist: {playlist}", current)
				from ____ in _mediator.TrySend(new CleanupPlaylist(client, current, good, notGood))
				let _____ = _logger.Information("Cleaned playlist: {playlist}", current)
				select current).Try();

			result.IfFail(e => _logger.Error(e, "Error while cleaning playlist"));
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
			var result = await (
				from client in _client.ToTryAsync()
				from _ in _mediator.TrySend(new MoveTrack(client, track, source, target, skip))
				let __ = _logger.Information(
					"Moved {track} from {source} to {target}", track, source, target)
				select Void.Default).Try();

			result.IfFail(e => _logger.Error(e, "Error while moving track"));
		}

		public async Task MoveCurrentToNotGood()
		{
			_logger.Information("Move current to not good");
			await MoveCurrentTo(m => m.NotGood, true);
		}

		public async Task MoveCurrentToGood()
		{
			_logger.Information("Move current to good");
			await MoveCurrentTo(m => m.Good, false);
		}

		public async Task MoveCurrentTo(Func<TssMappings.Mapping, string> getPlaylistId, bool skip)
		{
			var result = await (
				from client in _client.ToTryAsync()
				from current in Current.Empty(client)
				from targetId in GetTargetPlaylistId(current.Playlist.Id, getPlaylistId)
				from target in Playlist.Empty(client, targetId)
				select MoveTrack(current.Track, current.Playlist, target, skip)).Try();

			result.IfFail(e => _logger.Error(e, "Error while moving current track"));
		}
	}
}