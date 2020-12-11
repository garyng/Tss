using SpotifyAPI.Web;

namespace Tss.Core.Models
{
	public record Track(string Uri, string Name)
	{
		public static Track New(IPlayableItem item) => item switch
		{
			FullTrack track => new (track.Uri, track.Name),
			FullEpisode episode => new (episode.Uri, episode.Name),
		};
	}
}