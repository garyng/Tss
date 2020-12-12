using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MoreLinq;
using SpotifyAPI.Web;
using Tss.Core.Models;
using Void = GaryNg.Utils.Void.Void;

namespace Tss.Core.Requests
{
	public record DuplicatePlaylist(ISpotifyClient Client, Playlist Playlist) : IRequest<Void>;

	public class DuplicatePlaylistRequestHandler : IRequestHandler<DuplicatePlaylist, Void>
	{
		private readonly ILogger<DuplicatePlaylistRequestHandler> _logger;

		public DuplicatePlaylistRequestHandler(ILogger<DuplicatePlaylistRequestHandler> logger)
		{
			_logger = logger;
		}

		public async Task<Void> Handle(DuplicatePlaylist request, CancellationToken cancellationToken)
		{
			var (client, playlist) = request;
			var userId = (await client.UserProfile.Current()).Id;

			var tracks = playlist.Tracks;

			var name = $"{playlist.Name} ({DateTime.Now:yyyyMMdd-HHmmss})";

			if (tracks.Count == 0)
			{
				_logger.LogInformation("{playlist} is empty, skip duplicating", playlist);
				return Void.Default;
			}

			var backup = await client.Playlists.Create(userId, new PlaylistCreateRequest(name));

			var batches = tracks
				.Batch(99);

			await batches
				.Select(uris => new PlaylistAddItemsRequest(uris
					.Select(t => t.Uri)
					.ToList()))
				.ToAsyncEnumerable()
				.SelectAwait(async r => await client.Playlists.AddItems(backup.Id, r))
				.ToListAsync(cancellationToken);

			return Void.Value;
		}
	}
}