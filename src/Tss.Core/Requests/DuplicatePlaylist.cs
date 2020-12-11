using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using MoreLinq;
using SpotifyAPI.Web;
using Unit = MediatR.Unit;

namespace Tss.Core.Requests
{
	public record Playlist(string Id, string Name, List<Track> Tracks)
	{
		public static TryAsync<Playlist> New(ISpotifyClient client, string id,
			Option<Paging<PlaylistTrack<IPlayableItem>>> firstPage = default)
		{
			return from playlist in GetPlaylist(id)
				let page = firstPage.IfNone(() => playlist.Tracks)
				from tracks in GetTracks(page)
				select new Playlist(id, playlist.Name, tracks);

			TryAsync<FullPlaylist> GetPlaylist(string id) => async () => await client.Playlists.Get(id);

			TryAsync<List<Track>> GetTracks(Paging<PlaylistTrack<IPlayableItem>> page) => async () =>
				await client.Paginate(page)
					.Select(item => new Track(item.Track))
					.ToListAsync();
		}
	}

	public static class MediatorExtensions
	{
		public static TryAsync<TResponse> TrySend<TResponse>(this IMediator @this, IRequest<TResponse> request,
			CancellationToken cancellationToken = default) => async () => await @this.Send(request, cancellationToken);
	}

	public record DuplicatePlaylist(SpotifyClient Client, Playlist Playlist) : IRequest<Unit>;

	public class DuplicatePlaylistRequestHandler : IRequestHandler<DuplicatePlaylist, Unit>
	{
		private readonly ILogger<DuplicatePlaylistRequestHandler> _logger;

		public DuplicatePlaylistRequestHandler(ILogger<DuplicatePlaylistRequestHandler> logger)
		{
			_logger = logger;
		}

		public async Task<Unit> Handle(DuplicatePlaylist request, CancellationToken cancellationToken)
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

			return Unit.Value;
		}
	}
}