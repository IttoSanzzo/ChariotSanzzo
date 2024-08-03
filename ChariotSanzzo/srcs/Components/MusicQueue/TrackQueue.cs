using ChariotSanzzo.Events;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ChariotSanzzo.Components.MusicQueue {
	public class TrackQueue {
	// 0. Member Variables
		public LavalinkGuildConnection	_conn			{get; private set;}
		public DiscordChannel?			_chat			{get; private set;} = null;
		public long						_serverId		{get; private set;}
		public int						_length			{get; private set;} = 0;
		public int						_loop			{get; private set;} = 0;
		public int						_currentIndex	{get; private set;} = -1;
		public LavalinkTrack[]			_tracks			{get; private set;} = new LavalinkTrack[0];

	// 1. Constructors
		~TrackQueue() {
			Console.WriteLine("Queue Destructed!");
		}
		public TrackQueue(long serverId, LavalinkGuildConnection conn, DiscordChannel? chat) {
			this._serverId = serverId;
			this._conn = conn;
			this._chat = chat;
			this._conn.PlaybackFinished += Music.PlayNext;
			// this._conn.PlaybackStarted += Music.NowPlaying;
			Console.WriteLine("Queue Constructed!");
		}

	// 2. Sets
		public void	AddTrackToQueue(LavalinkTrack track) {
			Console.WriteLine($"AddTrackEntered {this._length}");
			if (TrackExist(track) == true)
				return ;
			LavalinkTrack[] temp = new LavalinkTrack[this._length + 1];
			int	i = -1;
			while (++i < this._length)
				temp[i] = this._tracks[i];
			temp[i] = track;
			this._tracks = temp;
			this._length += 1;
			Console.WriteLine($"AddTrackExit {this._length}\n");
		}
		public void	SetLoop(int type) {
			if (type >= 0 && type <= 2)
				this._loop = type;
		}
	// 3. Gets
		public LavalinkTrack?	UseNextTrack() {
			Console.WriteLine($"BeforeUse lenght {this._length}");
			if (this._currentIndex >= this._length - 1) {
				if (this._loop == 2)
					this._currentIndex = -1;
				else if (this._loop == 0) {
					Console.WriteLine("LaterUse None");
					return (null);
				}
			}
			if (this._loop != 1)
				this._currentIndex += 1;
			if (this._currentIndex < 0)
				this._currentIndex = 0;
			Console.WriteLine($"LaterUse Index {this._currentIndex}");
			this.NowPlaying(this, this._tracks[this._currentIndex]);
			return (this._tracks[this._currentIndex]);
		}
		public LavalinkTrack?	UsePreviousTrack() {
			this._currentIndex -= 1;
			if (this._currentIndex < 0)
				this._currentIndex = this._length - 1;
			this.NowPlaying(this, this._tracks[this._currentIndex]);
			return (this._tracks[this._currentIndex]);
		}
		public LavalinkTrack?	UseCurrentTrack() {
			this.NowPlaying(this, this._tracks[this._currentIndex]);
			return (this._tracks[this._currentIndex]);
		}

	// 4. Utils
		public bool TrackExist(LavalinkTrack track) {
			for (int i = 0; i < this._length; i++)
				if (this._tracks[i] == track)
					return (true);
			Console.WriteLine($"{this._length} lenght: Track does not already exist!");
			return (false);
		}

		public Task NowPlaying(TrackQueue queue, LavalinkTrack track) {
			if (queue._loop == 1)
				return (Task.CompletedTask);
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Purple,
				Description = $"_**Now Playing:**_ {track.Title}\n" +
										$"_**Length:**_ {track.Length}\n" +
										$"_**Author:**_ {track.Author}\n" +
										$"_**URL:**_ {track.Uri}"
			};
			if (track.Uri.ToString().Contains("youtube.com") == true)
				embed.WithImageUrl($"https://img.youtube.com/vi/{track.Uri.ToString().Substring(32)}/maxresdefault.jpg");
			if (queue._chat != null)
				queue._chat.SendMessageAsync(embed: embed);
			return (Task.CompletedTask);
		}
	}
}