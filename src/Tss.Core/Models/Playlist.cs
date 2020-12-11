using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SpotifyAPI.Web;

namespace Tss.Core.Models
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
					.Select(item => Track.New(item.Track))
					.ToListAsync();
		}
	}
}