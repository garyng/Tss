using SpotifyAPI.Web;

namespace Tss.Core.Models
{
	public record Track(string Id, string Uri, string Name)
	{
		public static Track New(IPlayableItem item) => item switch
		{
			FullTrack track => GetTrack(track),
			FullEpisode episode => new(episode.Uri, episode.Uri, episode.Name),
		};

		private static Track GetTrack(FullTrack track)
		{
			if (track.ExternalIds.TryGetValue("isrc", out var id))
			{
				return new(id.ToLower(), track.Uri, track.Name);
			}
			return new(track.Id, track.Uri, track.Name);
		}

		public override string ToString()
		{
			return $"\"{Name}\" ({Uri})";
		}

		public virtual bool Equals(Track? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Id == other.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}