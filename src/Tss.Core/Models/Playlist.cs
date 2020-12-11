using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using SpotifyAPI.Web;

namespace Tss.Core.Models
{
	// todo: I need Union type!
	public record Playlist(string Id, string Name, List<Track> Tracks)
	{
		private static TryAsync<Playlist> New(ISpotifyClient client, string id, bool loadTracks,
			Option<Paging<PlaylistTrack<IPlayableItem>>> firstPage)
		{
			return from playlist in GetPlaylist()
				let page = firstPage.IfNone(() => playlist.Tracks)
				from tracks in GetTracks(page)
				select new Playlist(id, playlist.Name, tracks);

			TryAsync<List<Track>> GetTracks(Paging<PlaylistTrack<IPlayableItem>> page) => async () =>
				loadTracks
					? await client.Paginate(page)
						.Select(item => Track.New(item.Track))
						.ToListAsync()
					: new List<Track>();

			TryAsync<FullPlaylist> GetPlaylist() =>
				async () => await client.Playlists.Get(id);
		}

		public static TryAsync<Playlist> New(ISpotifyClient client, string id,
			Option<Paging<PlaylistTrack<IPlayableItem>>> firstPage = default)
			=> New(client, id, true, firstPage);

		public static TryAsync<Playlist> Empty(ISpotifyClient client, string id)
			=> New(client, id, false, default);
		
		public TryAsync<Playlist> Refresh(ISpotifyClient client) => New(client, Id);
	}
}