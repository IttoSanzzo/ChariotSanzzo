using ChariotSanzzo.Events;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ChariotSanzzo.Components.MusicQueue {
	public class TrackQueue {
	// 0. Member Variables
		private static Random			_random			{get; set;} = new Random();
		public LavalinkGuildConnection	_conn			{get; private set;}
		public DiscordChannel?			_chat			{get; private set;} = null;
		public long						_serverId		{get; private set;}
		public bool						_pauseState		{get; private set;} = false;
		public int						_length			{get; private set;} = 0;
		public int						_loop			{get; private set;} = 0;
		public int						_currentIndex	{get; private set;} = -1;
		public LavalinkTrack[]			_tracks			{get; private set;} = new LavalinkTrack[0];
		private bool					_cleanConfig	{get; set;} = true;
		private bool					_advanConfig	{get; set;} = true;
		public DiscordMessage?			_pauseMss		{get; private set;} = null;
		public DiscordMessage?			_lastPlayerMss	{get; private set;} = null;

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
		public bool	GoNextIndex() {
			if (this._currentIndex >= this._length - 1) {
				if (this._loop == 2)
					this._currentIndex = -1;
				else if (this._loop == 0) {
					Console.WriteLine("LaterUse None");
					return (false);
				}
			}
			if (this._loop != 1)
				this._currentIndex += 1;
			if (this._currentIndex < 0)
				this._currentIndex = 0;
			return (true);
		}
		public void SetPauseMessage(DiscordMessage? pauseMss) {
			this._pauseMss = pauseMss;
		}
		public void SetLastPlayerMessage(DiscordMessage? lastPlayerMss) {
			this._lastPlayerMss = lastPlayerMss;
		}
		public bool SetPauseState(bool state) {
			this._pauseState = state;
			return (state);
		}
	// 3. Gets
		public async Task<LavalinkTrack?>	UseNextTrackAsync() {
			Console.WriteLine($"BeforeUse lenght {this._length}");
			if (this.GoNextIndex() == false)
				return (null);
			Console.WriteLine($"LaterUse Index {this._currentIndex}");
			await this.NowPlayingAsync(this, this._tracks[this._currentIndex]);
			this._pauseState = false;
			return (this._tracks[this._currentIndex]);
		}
		public async Task<LavalinkTrack?>	UsePreviousTrackAsync() {
			this._currentIndex -= 1;
			if (this._currentIndex < 0)
				this._currentIndex = this._length - 1;
			await this.NowPlayingAsync(this, this._tracks[this._currentIndex]);
			this._pauseState = false;
			return (this._tracks[this._currentIndex]);
		}
		public async Task<LavalinkTrack?>	UseCurrentTrackAsync() {
			await this.NowPlayingAsync(this, this._tracks[this._currentIndex]);
			this._pauseState = false;
			return (this._tracks[this._currentIndex]);
		}
		public async Task<LavalinkTrack?>	UseIndexTrackAsync(int index) {
			if (index < 0 || index > this._tracks.Length - 1)
				return (null);
			this._currentIndex = index;
			await this.NowPlayingAsync(this, this._tracks[this._currentIndex]);
			this._pauseState = false;
			return (this._tracks[this._currentIndex]);
		}
		public DiscordEmbed		GetQueueEmbed() {
			var	embed = new DiscordEmbedBuilder();
			if (this._tracks.Length == 0) {
				embed.WithColor(DiscordColor.Gray);
				embed.WithDescription("The queue is empty...");
				return (embed.Build());
			}
			embed.WithTitle("Queue");
			embed.WithColor(DiscordColor.Black);
			string	description = new string("");
			for (int i = 0; i < this._tracks.Length; i++) {
				if (i == this._currentIndex)
					description += $"```ansi\n[2;34m{i + 1} -> {this._tracks[i].Title}[0m\n```";
				else
					description += $"` {i + 1} ` -> {this._tracks[i].Title}\n";
			}
			embed.WithDescription(description);
			return (embed.Build());
		}

	// 4. Utils
		public bool TrackExist(LavalinkTrack track) {
			for (int i = 0; i < this._length; i++)
				if (this._tracks[i].Uri == track.Uri)
					return (true);
			Console.WriteLine($"{this._length} lenght: Track does not already exist!");
			return (false);
		}
		public async Task NowPlayingAsync(TrackQueue queue, LavalinkTrack track) {
			if (queue._loop == 1)
				return ;
			if (this._cleanConfig == true && this._lastPlayerMss != null)
				await this._lastPlayerMss.DeleteAsync();
			if (queue._chat != null){
				var message = GenNowPlayingAsync(queue, this._tracks[this._currentIndex]);
				if (message != null)
					this._lastPlayerMss = await queue._chat.SendMessageAsync(message);
			}
		}
		public DiscordMessageBuilder?	GenNowPlayingAsync(TrackQueue queue, LavalinkTrack track) {
		// 0. Embed Construction
			var	embed = new DiscordEmbedBuilder();
			string description = "";
			if (track.Uri.ToString().Contains("youtube.com") == true) {
				embed.WithColor(DiscordColor.Red);
				description += "<:YoutubeIcon:1269684532777320448> ";
				embed.WithImageUrl($"https://img.youtube.com/vi/{track.Uri.ToString().Substring(32)}/maxresdefault.jpg");
			}
			else
				embed.WithColor(DiscordColor.Purple);
			description += $"_**Now Playing:**_ [{track.Title}]({track.Uri})\n" +
										$"_**Author:**_ {track.Author}\n" +
										$"_**Length:**_ {track.Length}\t_**Index:**_ ` {this._currentIndex + 1} `\n";
			embed.WithDescription(description);
		
		// 2. Message Construction
			var message = new DiscordMessageBuilder();
			message.AddEmbed(embed: embed);
			if (this._advanConfig == true) {
				message.AddComponents(
					new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, "MusicPreviousTrackButton", null, false, new DiscordComponentEmoji(1269698996830605342)),
					((this._pauseState == false)
					? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "MusicPlayPauseButton", null, false, new DiscordComponentEmoji(1269697085834395738)))
					: (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "MusicPlayPauseButton", null, false, new DiscordComponentEmoji(1269697085834395738)))),
					new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, "MusicNextTrackButton", null, false, new DiscordComponentEmoji(1269698987259330702)));
			}
			return (message);
		}
		public int SwitchPause() {
			this._pauseState = !this._pauseState;
			if (this._pauseState == true)
				return (1);
			return (0);
		}

		// 5. Shuffle
		public bool	ShuffleTracks() {
			if (this._tracks.Length == 0)
				return (false);
			var	temp = this._tracks[this._currentIndex].Uri;
			TrackQueue.Shuffle(this._tracks);
			for (int i = 0; i < this._tracks.Length; i++)
				if (this._tracks[i].Uri == temp)
					this._currentIndex = i;
			return (true);
		}
		private static void	Shuffle(LavalinkTrack[] tracks) {
			int n = tracks.Length;
			while (n > 1) {
				int k = TrackQueue._random.Next(n--);
				LavalinkTrack temp = tracks[n];
				tracks[n] = tracks[k];
				tracks[k] = temp;
			}
		}
	}
}
