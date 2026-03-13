using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;

namespace ChariotSanzzo.Services.Wrappers {
	public static class Lavalink {
		public static IAudioService LavalinkNode { get; private set; } = null!;
		public static IPlayerManager PlayerManager => LavalinkNode.Players;
		public static ITrackManager Tracks => LavalinkNode.Tracks;

		public static void Initialize(IAudioService lavalinkNode) {
			LavalinkNode = lavalinkNode;
		}
	}
}
