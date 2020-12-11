using System;
using System.Text.RegularExpressions;
using LanguageExt;
using SpotifyAPI.Web;

namespace Tss.Core.Models
{
	public record Current2(Track Track, Playlist Playlist)
	{
		public static TryAsync<Current2> New(ISpotifyClient client)
		{
			return from current in GetCurrent()
				from playlistId in ExtractPlaylistId(current.Context.Uri)
				from playlist in Playlist.New(client, playlistId)
				let track = Track.New(current.Item)
				select new Current2(track, playlist);

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
	}
}