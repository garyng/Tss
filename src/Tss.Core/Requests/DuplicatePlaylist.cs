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
	public record DuplicatePlaylist(SpotifyClient Client, Playlist Playlist) : IRequest<Void>;

	public class DuplicatePlaylistRequestHandler : IRequestHandler<DuplicatePlaylist, Void>
	{
		public async Task<Void> Handle(DuplicatePlaylist request, CancellationToken cancellationToken)
		{
			var (client, playlist) = request;
			var userId = (await client.UserProfile.Current()).Id;

			var page = playlist.Tracks;

			var name = $"{playlist.Name} ({DateTime.Now:yyyyMMdd-HHmmss})";

			var backup = await client.Playlists.Create(userId, new PlaylistCreateRequest(name));

			var batches = page.Batch(99);

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