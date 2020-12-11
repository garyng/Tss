﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using Tss.Core.Models;
using Void = GaryNg.Utils.Void.Void;

namespace Tss.Core.Requests
{
	public record MoveTrack
		(ISpotifyClient Client, Track Track, Playlist Source, Playlist Target, bool Skip = false) : IRequest<Void>;

	public class MoveTrackRequestHandler : IRequestHandler<MoveTrack, Void>
	{
		private readonly ILogger<MoveTrackRequestHandler> _logger;

		public MoveTrackRequestHandler(ILogger<MoveTrackRequestHandler> logger)
		{
			_logger = logger;
		}

		public async Task<Void> Handle(MoveTrack request, CancellationToken cancellationToken)
		{
			var (client, track, source, target, skip) = request;

			try
			{
				await client.Playlists.RemoveItems(source.Id, new PlaylistRemoveItemsRequest
				{
					Tracks = new[] {new PlaylistRemoveItemsRequest.Item {Uri = track.Uri}}
				});
			}
			catch (Exception e)
			{
				// will throw when attempting to remove track from playlist of other user
				_logger.Error(e, "Error while removing \"{trackName}\" ({trackUri}) from \"{sourceName}\" ({sourceId})",
					track.Name, track.Uri, source.Name, source.Id);
			}

			await client.Playlists.AddItems(target.Id, new PlaylistAddItemsRequest(new[] {track.Uri}));
			if (skip)
			{
				await client.Player.SkipNext();
			}

			return Void.Default;
		}
	}
}