using ChariotSanzzo.Events;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ChariotSanzzo.Components.MusicComponent {
	public class TrackQueue {
	// M. Member Variables
		private QueueCollection					QColle									{get; set;}
		public DiscordMember						Owner										{get; set;}
		private static Random						Random									{get; set;} = new Random();
		public LavalinkGuildConnection	Conn										{get; private set;}
		public DiscordChannel?					Chat										{get; private set;} = null;
		public ulong										ServerId								{get; private set;}
		public bool											PauseState							{get; private set;} = false;
		public int											Length									{get; private set;} = 0;
		public int											Loop										{get; private set;} = 0;
		public int											CurrentIndex						{get; private set;} = -1;
		public ChariotTrack[]						Tracks									{get; set;} = [];
		private bool										CleanConfig							{get; set;} = true;
		private bool										AdvanConfig							{get; set;} = true;
		public bool											IsFinished							{get; set;} = true;
		public DiscordMessage?					PauseMss								{get; private set;} = null;
		public DiscordMessage?					ActivePlayerMss					{get; private set;} = null;
		public GjallarhornContext?			LastGjallarhornContext	{get; set;} = null;

	// C. Constructors
		~TrackQueue() {
			if (this.ActivePlayerMss != null)
				this.ActivePlayerMss.DeleteAsync();
			Program.WriteLine($"Queue Destructed! Guild: {this.ServerId}");
		}
		public TrackQueue(ulong serverId, DiscordMember owner, LavalinkGuildConnection conn, DiscordChannel? chat, QueueCollection qColle) {
			this.Owner = owner;
			this.QColle = qColle;
			this.ServerId = serverId;
			this.Conn = conn;
			this.Chat = chat;
			if (this.Conn.CurrentState.CurrentTrack != null)
				this.Conn.StopAsync();
			this.Conn.PlaybackFinished += CharitoMusicEvents.TrackEndedEvent;
			this.Conn.PlaybackStarted += CharitoMusicEvents.TrackStartedEvent;
			Program.WriteLine($"Queue Constructed! Guild: {this.ServerId}");
		}

	// S. Sets
		public void	AddTrackToQueue(ChariotTrack ctrack) {
			Program.WriteLine($"AddTrackEntered {this.Length + 1}");
			if (TrackExist(ctrack) == true)
				return ;
			ChariotTrack[] temp = new ChariotTrack[this.Length + 1];
			int	i = -1;
			while (++i < this.Length)
				temp[i] = this.Tracks[i];
			temp[i] = ctrack;
			this.Tracks = temp;
			this.Length += 1;
		}
		public void	RemoveTrackFromQueue(int index) {
			if ((index < 0 || index > this.Tracks.Length - 1) && this.Tracks.Length != 0)
				return ;
			Program.WriteLine($"RemoveTrackEntered {this.Length}");
			ChariotTrack[] temp = new ChariotTrack[this.Length - 1];
			int	i = -1;
			int	j = 0;
			while (++i < this.Length) {
				if (i == index) {
					j = 1;
					continue;
				}
				temp[i - j] = this.Tracks[i];
			}
			this.Tracks = temp;
			this.Length -= 1;
		}
		public void	SetLoop(int type) {
			if (type >= 0 && type <= 2)
				this.Loop = type;
			else
				this.Loop = 0;
		}
		public bool	GoNextIndex() {
			if (this.CurrentIndex >= this.Length - 1) {
				if (this.Loop == 2)
					this.CurrentIndex = -1;
				else
					return (false);
			}
			this.CurrentIndex += 1;
			if (this.CurrentIndex < 0)
				this.CurrentIndex = 0;
			return (true);
		}
		public void SetPauseMessage(DiscordMessage? pauseMss) {
			this.PauseMss = pauseMss;
		}
		public void SetLastPlayerMessage(DiscordMessage? lastPlayerMss) {
			this.ActivePlayerMss = lastPlayerMss;
		}
		public bool SetPauseState(bool state) {
			this.PauseState = state;
			return (state);
		}
		public DiscordChannel SetChatChannel(DiscordChannel chatChannel) {
			this.Chat = chatChannel;
			return this.Chat;
		}

	// G. Gets
		public QueueCollection				GetQueueCollection() {
			return (this.QColle);
		}
		public LavalinkTrack?				GetIndexTrack(int index) {
			if (index < 0 || index > this.Tracks.Length - 1)
				return (null);
			return (this.Tracks[index].LlTrack);
		}
		public int							GetTrackIndex(ChariotTrack track) {
			for (int i = 0; i < this.Length; i++)
				if (this.Tracks[i].Uri.AbsoluteUri == track.Uri.AbsoluteUri)
					return (i);
			return (-1);
		}
		public async Task<LavalinkTrack?>	UseTrackAsync(ChariotTrack track) {
			int index = this.GetTrackIndex(track);
			if (index == -1)
				return (null);
			return (await this.UseIndexTrackAsync(index));
		}
		public async Task<LavalinkTrack?>	UseNextTrackAsync() {
			if (this.GoNextIndex() == false)
				return (null);
			await this.NowPlayingAsync();
			this.PauseState = false;
			return (this.Tracks[this.CurrentIndex].LlTrack);
		}
		public async Task<LavalinkTrack?>	UsePreviousTrackAsync() {
			this.CurrentIndex -= 1;
			if (this.CurrentIndex < 0) {
				if (this.Loop == 2)
					this.CurrentIndex = this.Length - 1;
				else {
					this.CurrentIndex = 0;
					return (null);
				}
			}
			await this.NowPlayingAsync();
			this.PauseState = false;
			return (this.Tracks[this.CurrentIndex].LlTrack);
		}
		public async Task<LavalinkTrack?>	UseCurrentTrackAsync() {
			await this.NowPlayingAsync();
			this.PauseState = false;
			return (this.Tracks[this.CurrentIndex].LlTrack);
		}
		public async Task<LavalinkTrack?>	UseIndexTrackAsync(int index) {
			if (index < 0 || index > this.Tracks.Length - 1)
				return (null);
			this.CurrentIndex = index;
			await this.NowPlayingAsync();
			this.PauseState = false;
			return (this.Tracks[this.CurrentIndex].LlTrack);
		}
		public DiscordEmbed[]				GetQueueEmbed() {
			Program.WriteLine("GET EMBED QUEUE ENTER");
		// 0. Base Check
			if (this.Tracks.Length == 0) {
				var	errembed = new DiscordEmbedBuilder();
				errembed.WithColor(DiscordColor.Gray);
				errembed.WithDescription("The queue is empty...");
				return new DiscordEmbed[1] {errembed.Build()};
			}
		// 1. Core
			var retEmbedArr = new DiscordEmbed[0];
			int i = 0;
			while (i < this.Tracks.Length) {
				var	tempArr = new DiscordEmbed[retEmbedArr.Length + 1];
				for (int j = 0; j < retEmbedArr.Length; j++)
					tempArr[j] = retEmbedArr[j];
				retEmbedArr = tempArr;
				var	embed = new DiscordEmbedBuilder();
				embed.WithColor(DiscordColor.Black);
				if (retEmbedArr.Length == 1)
					embed.WithTitle("Queue");
				string description = "";
				while (i < this.Tracks.Length && description.Length < 3600) {
					Program.WriteLine($"Entry index [{i}]");
					if (i == this.CurrentIndex)
						description += $"```ansi\n[2;34m{i + 1} -> {this.Tracks[i].Title}[0m\n```";
					else
						description += $"{this.Tracks[i].Favicon} ` {i + 1} ` -> {this.Tracks[i].Title}\n";
					i++;
				}
				embed.WithDescription(description);
				retEmbedArr[^1] = embed.Build();
			}
		// 2. Return
			return (retEmbedArr);
		}

	// U. Utils
		public bool									TrackExist(ChariotTrack track) {
			for (int i = 0; i < this.Length; i++)
				if (this.Tracks[i].Uri.AbsoluteUri == track.Uri.AbsoluteUri)
					return (true);
			return (false);
		}
		public async Task							NowPlayingAsync() {
			if (this.CleanConfig == true && this.ActivePlayerMss != null)
				await this.ActivePlayerMss.DeleteAsync();
			if (this.Chat != null){
				var message = await GenNowPlayingAsync();
				if (message != null)
					this.ActivePlayerMss = await this.Chat.SendMessageAsync(message);
			}
		}
		public async Task<DiscordMessageBuilder?>	GenNowPlayingAsync() {
		// 0. Message Construction
			var message = new DiscordMessageBuilder();
			message.AddEmbed(await this.Tracks[this.CurrentIndex].GetEmbed(this.CurrentIndex));
			if (this.AdvanConfig == true) {
				message.AddComponents(
					((this.Loop == 0)
						? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary,		"MusicLoopButton",			null, false, 	new DiscordComponentEmoji(1271598875374784644)))
						: (((this.Loop == 1)
							? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success,	"MusicLoopButton",			null, false, 	new DiscordComponentEmoji(1269881536552108135)))
							: (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger,	"MusicLoopButton",			null, false, 	new DiscordComponentEmoji(1271598889501196290)))
					))),
					((this.CheckSOQ())
						? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary,		"MusicPreviousTrackButton",	null, true,		new DiscordComponentEmoji(1269698996830605342)))
						: (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary,		"MusicPreviousTrackButton",	null, false,	new DiscordComponentEmoji(1269698996830605342)))
					),
					((this.PauseState == false)
						? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success,		"MusicPlayPauseButton",		null, false, 	new DiscordComponentEmoji(1269697085834395738)))
						: (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary,		"MusicPlayPauseButton",		null, false, 	new DiscordComponentEmoji(1269697085834395738)))
					),
					((this.CheckEOQ())
						? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary,		"MusicNextTrackButton",		null, true,		new DiscordComponentEmoji(1269698987259330702)))
						: (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary,		"MusicNextTrackButton",		null, false,	new DiscordComponentEmoji(1269698987259330702)))
					),
					((this.Loop == 0)
						? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary,		"MusicShuffleButton",		null, false, 	new DiscordComponentEmoji(1271602111783895150)))
						: ((this.Loop == 1)
							? (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success,		"MusicReplayTrackButton",	null, false, new DiscordComponentEmoji(1281561425864691735)))
							: (new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger,		"MusicShuffleButton",		null, false, new DiscordComponentEmoji(1271602111783895150)))
					))
					);
			}
			return (message);
		}
		public int									SwitchPause() {
			this.PauseState = !this.PauseState;
			if (this.PauseState == true)
				return (1);
			return (0);
		}
		private bool								CheckSOQ() {
			if (this.Length < 2)
				return (true);
			if (this.Loop == 2)
				return (false);
			return (this.CurrentIndex <= 0);
		}
		private bool								CheckEOQ() {
			if (this.Length < 2)
				return (true);
			if (this.Loop == 2)
				return (false);
			return (this.CurrentIndex >= this.Length - 1);
		}

	// E. Miscs
		public async Task<bool>	ShuffleTracks() {
			if (this.Tracks.Length == 0)
				return (false);
			TrackQueue.Shuffle(this.Tracks);
			var toPlayNow = await this.UseIndexTrackAsync(0);
			if (toPlayNow != null)
				await this.Conn.PlayAsync(toPlayNow);
			return (true);
		}
		private static void		Shuffle(ChariotTrack[] tracks) {
			int n = tracks.Length;
			while (n > 1) {
				int k = TrackQueue.Random.Next(n--);
				(tracks[k], tracks[n]) = (tracks[n], tracks[k]);
			}
		}
	}
}
