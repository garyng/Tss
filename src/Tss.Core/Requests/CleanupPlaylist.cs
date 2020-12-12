using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GaryNg.Utils.Void;
using MediatR;
using Microsoft.Extensions.Logging;
using MoreLinq;
using SpotifyAPI.Web;
using Tss.Core.Models;

namespace Tss.Core.Requests
{
	public record CleanupPlaylist
		(ISpotifyClient Client, Playlist Current, Playlist Good, Playlist NotGood) : IRequest<Void>;

	public class CleanupPlaylistRequestHandler : IRequestHandler<CleanupPlaylist, Void>
	{
		private readonly ILogger<CleanupPlaylistRequestHandler> _logger;

		public CleanupPlaylistRequestHandler(ILogger<CleanupPlaylistRequestHandler> logger)
		{
			_logger = logger;
		}

		public async Task<Void> Handle(CleanupPlaylist request, CancellationToken cancellationToken)
		{
			var (client, current, good, notGood) = request;

			var cleanTracks = current.Tracks.Except(good.Tracks)
				.Except(notGood.Tracks)
				.ToList();

			if (cleanTracks.SequenceEqual(current.Tracks))
			{
				_logger.LogInformation("Nothing to remove from {playlist})", current);
				return Void.Default;
			}

			// clear playlist
			await client.Playlists.ReplaceItems(current.Id, new PlaylistReplaceItemsRequest(new List<string>()));

			await cleanTracks
				.Batch(99)
				.Select(ts => new PlaylistAddItemsRequest(ts.Select(t => t.Uri).ToList()))
				.ToAsyncEnumerable()
				.SelectAwait(async request => client.Playlists.AddItems(current.Id, request))
				.ToListAsync();

			var removed = current.Tracks.Count() - cleanTracks.Count();
			_logger.LogInformation("Removed {count} tracks from {playlist})", removed, current);

			return Void.Default;
		}
	}
}