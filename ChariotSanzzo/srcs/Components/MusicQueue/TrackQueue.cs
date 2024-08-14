using System.Collections;
using ChariotSanzzo.Events;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ChariotSanzzo.Components.MusicQueue {
	public class TrackQueue {
	// 0. Member Variables
		private QueueCollection			_qColle			{get; set;}
		private static Random			_random			{get; set;} = new Random();
		public LavalinkGuildConnection	_conn			{get; private set;}
		public DiscordChannel?			_chat			{get; private set;} = null;
		public long						_serverId		{get; private set;}
		public bool						_pauseState		{get; private set;} = false;
		public int						_length			{get; private set;} = 0;
		public int						_loop			{get; private set;} = 0;
		public int						_currentIndex	{get; private set;} = -1;
		public ChariotTrack[]			_tracks			{get; set;} = new ChariotTrack[0];
		private bool					_cleanConfig	{get; set;} = true;
		private bool					_advanConfig	{get; set;} = true;
		public DiscordMessage?			_pauseMss		{get; private set;} = null;
		public DiscordMessage?			_lastPlayerMss	{get; private set;} = null;

	// 1. Constructors
		~TrackQueue() {
			Console.WriteLine("Queue Destructed!");
		}
		public TrackQueue(long serverId, LavalinkGuildConnection conn, DiscordChannel? chat, QueueCollection qColle) {
			this._qColle = qColle;
			this._serverId = serverId;
			this._conn = conn;
			this._chat = chat;
			if (this._conn.CurrentState.CurrentTrack != null)
				this._conn.StopAsync();
			this._conn.PlaybackFinished += Music.PlayNext;
			Console.WriteLine("Queue Constructed!");
		}

	// 2. Sets
		public void	AddTrackToQueue(ChariotTrack ctrack) {
			Console.WriteLine($"AddTrackEntered {this._length + 1}");
			if (TrackExist(ctrack) == true)
				return ;
			ChariotTrack[] temp = new ChariotTrack[this._length + 1];
			int	i = -1;
			while (++i < this._length)
				temp[i] = this._tracks[i];
			temp[i] = ctrack;
			this._tracks = temp;
			this._length += 1;
		}
		public void	RemoveTrackFromQueue(int index) {
			if ((index < 0 || index > this._tracks.Length - 1) && this._tracks.Length != 0)
				return ;
			Console.WriteLine($"RemoveTrackEntered {this._length}");
			ChariotTrack[] temp = new ChariotTrack[this._length - 1];
			int	i = -1;
			int	j = 0;
			while (++i < this._length) {
				if (i == index) {
					j = 1;
					continue;
				}
				temp[i - j] = this._tracks[i];
			}
			this._tracks = temp;
			this._length -= 1;
		}
		public void	SetLoop(int type) {
			if (type >= 0 && type <= 2)
				this._loop = type;
			else
				this._loop = 0;
		}
		public bool	GoNextIndex() {
			if (this._currentIndex >= this._length - 1) {
				if (this._loop == 2)
					this._currentIndex = -1;
				else if (this._loop == 0)
					return (false);
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
		public QueueCollection				GetQueueCollection() {
			return (this._qColle);
		}
		public async Task<LavalinkTrack?>	UseNextTrackAsync() {
			if (this.GoNextIndex() == false)
				return (null);
			await this.NowPlayingAsync();
			this._pauseState = false;
			return (this._tracks[this._currentIndex]._llTrack);
		}
		public async Task<LavalinkTrack?>	UsePreviousTrackAsync() {
			this._currentIndex -= 1;
			if (this._currentIndex < 0)
				this._currentIndex = this._length - 1;
			await this.NowPlayingAsync();
			this._pauseState = false;
			return (this._tracks[this._currentIndex]._llTrack);
		}
		public async Task<LavalinkTrack?>	UseCurrentTrackAsync() {
			await this.NowPlayingAsync();
			this._pauseState = false;
			return (this._tracks[this._currentIndex]._llTrack);
		}
		public async Task<LavalinkTrack?>	UseIndexTrackAsync(int index) {
			if (index < 0 || index > this._tracks.Length - 1)
				return (null);
			this._currentIndex = index;
			await this.NowPlayingAsync();
			this._pauseState = false;
			return (this._tracks[this._currentIndex]._llTrack);
		}
		public DiscordEmbed[]				GetQueueEmbed() {
			Console.WriteLine("GET EMBED QUEUE ENTER");
		// 0. Base Check
			if (this._tracks.Length == 0) {
				var	errembed = new DiscordEmbedBuilder();
				errembed.WithColor(DiscordColor.Gray);
				errembed.WithDescription("The queue is empty...");
				return (new DiscordEmbed[1] {errembed.Build()});
			}
		// 1. Core
			var retEmbedArr = new DiscordEmbed[0];
			int i = 0;
			while (i < this._tracks.Length) {
				var	tempArr = new DiscordEmbed[retEmbedArr.Length + 1];
				for (int j = 0; j < retEmbedArr.Length; j++)
					tempArr[j] = retEmbedArr[j];
				retEmbedArr = tempArr;
				var	embed = new DiscordEmbedBuilder();
				embed.WithColor(DiscordColor.Black);
				if (retEmbedArr.Length == 1)
					embed.WithTitle("Queue");
				string description = "";
				while (i < this._tracks.Length && description.Length < 3600) {
					Console.WriteLine($"Entry index [{i}]");
					if (i == this._currentIndex)
						description += $"```ansi\n[2;34m{i + 1} -> {this._tracks[i]._title}[0m\n```";
					else
						description += $"{this._tracks[i]._favicon} ` {i + 1} ` -> {this._tracks[i]._title}\n";
					i++;
				}
				embed.WithDescription(description);
				retEmbedArr[^1] = embed.Build();
			}
		// 2. Return
			return (retEmbedArr);
		}

	// 4. Utils
		public bool									TrackExist(ChariotTrack track) {
			for (int i = 0; i < this._length; i++)
				if (this._tracks[i]._uri.AbsoluteUri == track._uri.AbsoluteUri)
					return (true);
			return (false);
		}
		public async Task							NowPlayingAsync() {
			if (this._loop == 1)
				return ;
			if (this._cleanConfig == true && this._lastPlayerMss != null)
				await this._lastPlayerMss.DeleteAsync();
			if (this._chat != null){
				var message = await GenNowPlayingAsync();
				if (message != null)
					this._lastPlayerMss = await this._chat.SendMessageAsync(message);
			}
		}
		public async Task<DiscordMessageBuilder?>	GenNowPlayingAsync() {
		// 0. Message Construction
			var message = new DiscordMessageBuilder();
			message.AddEmbed(await this._tracks[this._currentIndex].GetEmbed(this._currentIndex));
			if (this._advanConfig == true) {
				message.AddComponents(
					((this._loop == 0)
						? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "MusicLoopButton", null, false, new DiscordComponentEmoji(1271598875374784644)))
						: (((this._loop == 1)
							? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "MusicLoopButton", null, false, new DiscordComponentEmoji(1269881536552108135)))
							: (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "MusicLoopButton", null, false, new DiscordComponentEmoji(1271598889501196290)))
						))),
					new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, "MusicPreviousTrackButton", null, false, new DiscordComponentEmoji(1269698996830605342)),
					((this._pauseState == false)
						? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "MusicPlayPauseButton", null, false, new DiscordComponentEmoji(1269697085834395738)))
						: (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "MusicPlayPauseButton", null, false, new DiscordComponentEmoji(1269697085834395738)))),
					new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, "MusicNextTrackButton", null, false, new DiscordComponentEmoji(1269698987259330702)),
					new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "MusicShuffleButton", null, false, new DiscordComponentEmoji(1271602111783895150)));
			}
			return (message);
		}
		public int									SwitchPause() {
			this._pauseState = !this._pauseState;
			if (this._pauseState == true)
				return (1);
			return (0);
		}

		// 5. Shuffle
		public async Task<bool>	ShuffleTracks() {
			if (this._tracks.Length == 0)
				return (false);
			TrackQueue.Shuffle(this._tracks);
			var toPlayNow = await this.UseIndexTrackAsync(0);
			if (toPlayNow != null)
				await this._conn.PlayAsync(toPlayNow);
			return (true);
		}
		private static void		Shuffle(ChariotTrack[] tracks) {
			int n = tracks.Length;
			while (n > 1) {
				int k = TrackQueue._random.Next(n--);
				ChariotTrack temp = tracks[n];
				tracks[n] = tracks[k];
				tracks[k] = temp;
			}
		}
	}
}
