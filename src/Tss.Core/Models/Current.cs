using System;
using System.Text.RegularExpressions;
using LanguageExt;
using SpotifyAPI.Web;

namespace Tss.Core.Models
{
	public record Current(Track Track, Playlist Playlist)
	{
		private static TryAsync<Current> New(ISpotifyClient client, bool loadTracks)
		{
			return from current in GetCurrent()
				from playlistId in ExtractPlaylistId(current.Context.Uri)
				from playlist in loadTracks ? Playlist.New(client, playlistId) : Playlist.Empty(client, playlistId)
				let track = Track.New(current.Item)
				select new Current(track, playlist);

			TryAsync<CurrentlyPlaying> GetCurrent()
			{
				return async () =>
				{
					CurrentlyPlaying? current =
						await client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
					if (current == null) throw new Exception("Not currently playing anything");
					return current;
				};
			}

			TryAsync<string> ExtractPlaylistId(string uri) =>
				async () => Regex.Match(uri, "playlist:(?<id>.*)").Groups["id"].Value;
		}

		public static TryAsync<Current> New(ISpotifyClient client) => New(client, true);
		public static TryAsync<Current> Empty(ISpotifyClient client) => New(client, false);
	}
}