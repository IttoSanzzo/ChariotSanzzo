using ChariotSanzzo.Components.Gjallar.Types;
using ChariotSanzzo.Infrastructure.Config;
using ChariotSanzzo.Utils;
using DSharpPlus.Entities;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Rest.Entities.Tracks;

namespace ChariotSanzzo.Components.Gjallar {
	public sealed record GjallarPlayerOptions : LavalinkPlayerOptions { }
	public sealed class GjallarPlayer : LavalinkPlayer {
		public IPlayerProperties<GjallarPlayer, GjallarPlayerOptions> Properties { get; private set; } = null!;
		public DiscordMember Owner { get; set; } = null!;
		public DiscordChannel? Chat { get; private set; } = null;
		public DiscordGuild Guild { get; private set; }
		public bool PauseState { get; private set; } = false;
		public GjallarLoopState LoopState { get; private set; } = 0;
		public List<GjallarTrack> Tracks { get; set; } = [];
		private bool CleanConfig { get; set; } = true;
		private bool AdvanConfig { get; set; } = true;
		public bool IsFinished { get; set; } = true;
		public DiscordMessage? PauseMss { get; private set; } = null;
		public DiscordMessage? ActivePlayerMss { get; private set; } = null;
		public GjallarContext? LastGjallarContext { get; set; } = null;
		public int TotalTracks => Tracks.Count;
		public int CurrentPosition { get; private set; } = 0;
		public bool IsCurrentPositionInBounds => CurrentPosition >= 1 && CurrentPosition <= TotalTracks;
		public GjallarTrack? CurrentGjallarTrack => IsCurrentPositionInBounds ? Tracks[CurrentPosition - 1] : null;
		public TimeSpan? PlaybackPosition => Position?.Position;
		private SemaphoreSlim ActivePlayerSemaphore { get; } = new(1, 1);
		private static Random Random { get; set; } = new Random();

		~GjallarPlayer() {
			if (this.ActivePlayerMss! != null!)
				this.ActivePlayerMss.DeleteAsync();
			Program.WriteLine($"GjallarQueuePlayer Destructed! Guild: {this.GuildId}");
		}
		public GjallarPlayer(IPlayerProperties<GjallarPlayer, GjallarPlayerOptions> properties, DiscordGuild guild, DiscordMember owner, DiscordChannel? chat) : base(properties) {
			this.Guild = guild;
			this.Properties = properties;
			this.Owner = owner;
			this.Chat = chat;
			if (this.CurrentTrack != null) StopAsync();
			Program.WriteLine($"GjallarPlayer Constructed! Guild: {this.GuildId} {this.Guild.Name}");
		}

		public void AddTrackToQueue(GjallarTrack track) {
			if (TrackExist(track) != true)
				Tracks.Add(track);
		}
		public void RemoveTrackFromQueue(int position) {
			if (this.Tracks.Count == 0 || position < 1 || position > this.TotalTracks) return;
			Program.WriteLine($"RemoveTrackEntered");
			Tracks.RemoveAt(position - 1);
		}
		public bool MoveTrackToEndOfQueue(GjallarTrack track) {
			var position = GetTrackPosition(track);
			if (position == 0 || position == CurrentPosition) return false;
			if (position == TotalTracks) return true;
			Tracks.RemoveAt(position - 1);
			Tracks.Add(track);
			if (position < CurrentPosition)
				CurrentPosition -= 1;
			return true;
		}
		public void SetLoop(GjallarLoopState newState) {
			this.LoopState = newState;
		}
		public bool GoNextPosition(bool fromEvent = false) {
			if (this.TotalTracks == 0) {
				this.CurrentPosition = 0;
				return false;
			} else if (
					this.LoopState == GjallarLoopState.None
					|| (this.LoopState == GjallarLoopState.LoopTrack && fromEvent == false)
				) {
				if (this.CurrentPosition > this.TotalTracks)
					this.CurrentPosition = this.TotalTracks;
				if (this.CurrentPosition == this.TotalTracks)
					return false;
				this.CurrentPosition += 1;
			} else if (this.LoopState == GjallarLoopState.LoopTrack) {
				if (this.CurrentPosition < 1)
					this.CurrentPosition = 1;
				else if (this.CurrentPosition > this.TotalTracks)
					this.CurrentPosition = this.TotalTracks;
			} else if (this.LoopState == GjallarLoopState.LoopQueue) {
				this.CurrentPosition += 1;
				if (this.CurrentPosition > this.TotalTracks)
					this.CurrentPosition = 1;
			}
			return true;
		}
		public bool GoPreviousPosition(bool fromEvent = false) {
			if (this.TotalTracks == 0) {
				this.CurrentPosition = 0;
				return false;
			} else if (
					this.LoopState == GjallarLoopState.None
					|| (this.LoopState == GjallarLoopState.LoopTrack && fromEvent == false)
				) {
				if (this.CurrentPosition < 1)
					this.CurrentPosition = 1;
				if (this.CurrentPosition == 1)
					return false;
				this.CurrentPosition -= 1;
			} else if (this.LoopState == GjallarLoopState.LoopTrack) {
				if (this.CurrentPosition < 1)
					this.CurrentPosition = 1;
				else if (this.CurrentPosition > this.TotalTracks)
					this.CurrentPosition = this.TotalTracks;
			} else if (this.LoopState == GjallarLoopState.LoopQueue) {
				this.CurrentPosition -= 1;
				if (this.CurrentPosition < 1)
					this.CurrentPosition = this.TotalTracks;
			}
			return true;
		}
		public void SetPauseMessage(DiscordMessage? pauseMss) {
			this.PauseMss = pauseMss;
		}
		public void SetLastPlayerMessage(DiscordMessage? lastPlayerMss) {
			this.ActivePlayerMss = lastPlayerMss;
		}
		public bool SetPauseState(bool state) {
			this.PauseState = state;
			return state;
		}
		public async Task<DiscordChannel> SetChatChannel(DiscordChannel chatChannel) {
			this.Chat = chatChannel;
			await this.NowPlayingAsync();
			return this.Chat;
		}

		public GjallarTrack? GetTrackByPosition(int position) {
			if (position < 1 || position > this.TotalTracks)
				return null;
			return this.Tracks[position - 1];
		}
		public int GetTrackPosition(GjallarTrack track) {
			return 1 + Tracks.FindIndex(qTrack => qTrack.Uri!.AbsoluteUri == track.Uri!.AbsoluteUri);
		}
		public async Task<GjallarTrack?> UseTrackAsync(GjallarTrack track) {
			int index = GetTrackPosition(track);
			if (index == 0)
				return null;
			this.PauseState = false;
			return await UseTrackByPositionAsync(index);
		}
		public async Task<GjallarTrack?> UseNextTrackAsync(bool fromEvent = false) {
			if (GoNextPosition(fromEvent) == false) return null;
			await this.NowPlayingAsync();
			this.PauseState = false;
			return CurrentGjallarTrack;
		}
		public async Task<GjallarTrack?> UsePreviousTrackAsync(bool fromEvent = false) {
			if (this.GoPreviousPosition(fromEvent) == false) return null;
			await this.NowPlayingAsync();
			this.PauseState = false;
			return CurrentGjallarTrack;
		}
		public async Task<GjallarTrack?> UseCurrentTrackAsync() {
			if (CurrentGjallarTrack == null) return null;
			await this.NowPlayingAsync();
			this.PauseState = false;
			return CurrentGjallarTrack;
		}
		public async Task<GjallarTrack?> UseTrackByPositionAsync(int position) {
			if (position < 1 || position > this.TotalTracks) return null;
			this.CurrentPosition = position;
			await this.NowPlayingAsync();
			this.PauseState = false;
			return CurrentGjallarTrack;
		}
		public DiscordEmbed[] GetQueueEmbed() {
			if (this.TotalTracks == 0) {
				var errembed = new DiscordEmbedBuilder();
				errembed.WithColor(DiscordColor.Gray);
				errembed.WithDescription("The queue is empty...");
				return [errembed.Build()];
			}
			var retEmbedArr = Array.Empty<DiscordEmbed>();
			int i = 0;
			while (i < this.TotalTracks) {
				var tempArr = new DiscordEmbed[retEmbedArr.Length + 1];
				for (int j = 0; j < retEmbedArr.Length; j++)
					tempArr[j] = retEmbedArr[j];
				retEmbedArr = tempArr;
				var embed = new DiscordEmbedBuilder();
				embed.WithColor(DiscordColor.Black);
				if (retEmbedArr.Length == 1)
					embed.WithTitle("Queue");
				string description = "";
				while (i < this.TotalTracks && description.Length < 3600) {
					Program.WriteLine($"Entry index [{i}]");
					if (i == this.CurrentPosition - 1)
						description += $"```ansi\n[2;34m{i + 1} -> {this.Tracks[i].Title}[0m\n```";
					else
						description += $"{this.Tracks[i].Favicon} ` {i + 1} ` -> {this.Tracks[i].Title}\n";
					i++;
				}
				embed.WithDescription(description);
				retEmbedArr[^1] = embed.Build();
			}
			return retEmbedArr;
		}
		public void ClearTracks() {
			this.Tracks.Clear();
			this.CurrentPosition = 0;
			this.LoopState = GjallarLoopState.None;
			this.PauseState = false;
		}

		public bool TrackExist(GjallarTrack track) => Tracks.Any(qTrack => qTrack.Uri!.AbsoluteUri == track.Uri!.AbsoluteUri);
		public async Task NowPlayingAsync() {
			await ActivePlayerSemaphore.WaitAsync();
			try {

				if (this.CleanConfig == true && this.ActivePlayerMss! != null!)
					await this.ActivePlayerMss.DeleteAsync();
				if (this.Chat is not null) {
					var message = await GenNowPlayingAsync();
					if (message != null) {
						this.ActivePlayerMss = await this.Chat.SendMessageAsync(message);
					}
				}
			} finally {
				ActivePlayerSemaphore.Release();
			}
		}
		public async Task<DiscordMessageBuilder?> GenNowPlayingAsync() {
			var currentTrack = CurrentGjallarTrack;

			if (currentTrack is null)
				return null;
			var message = new DiscordMessageBuilder();
			message.AddEmbed(await currentTrack.GetEmbedAsync(this.CurrentPosition));
			if (this.AdvanConfig == true) {
				message.AddActionRowComponent([
					LoopState == GjallarLoopState.None
						? new DiscordButtonComponent(DiscordButtonStyle.Secondary, "MusicLoopButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularNotLoopIcon)))
						: LoopState == GjallarLoopState.LoopTrack
							? new DiscordButtonComponent(DiscordButtonStyle.Success, "MusicLoopButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularOneLoopIcon)))
							: new DiscordButtonComponent(DiscordButtonStyle.Danger, "MusicLoopButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularLoopIcon))),
					CheckSOQ()
						? new DiscordButtonComponent(DiscordButtonStyle.Primary, "MusicPreviousTrackButton", "", true, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularPreviousTrackIcon)))
						: new DiscordButtonComponent(DiscordButtonStyle.Primary, "MusicPreviousTrackButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularPreviousTrackIcon))),
					PauseState == false
						? new DiscordButtonComponent(DiscordButtonStyle.Success, "MusicPlayPauseButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularPlayPauseIcon)))
						: new DiscordButtonComponent(DiscordButtonStyle.Secondary, "MusicPlayPauseButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularPlayPauseIcon))),
					CheckEOQ()
						? new DiscordButtonComponent(DiscordButtonStyle.Primary, "MusicNextTrackButton", "", true, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularNextTrackIcon)))
						: new DiscordButtonComponent(DiscordButtonStyle.Primary, "MusicNextTrackButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularNextTrackIcon))),
					LoopState == GjallarLoopState.None
						? new DiscordButtonComponent(DiscordButtonStyle.Secondary, "MusicShuffleButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularShuffleIcon)))
						: LoopState == GjallarLoopState.LoopTrack
							? new DiscordButtonComponent(DiscordButtonStyle.Success, "MusicReplayTrackButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularReplayIcon)))
							: new DiscordButtonComponent(DiscordButtonStyle.Danger, "MusicShuffleButton", "", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularShuffleIcon)))
				]);
			}
			return message;
		}
		public GjallarPauseState SwitchPause() {
			PauseState = !PauseState;
			return PauseState ? GjallarPauseState.Pause : GjallarPauseState.Resume;
		}
		private bool CheckSOQ() {
			if (this.TotalTracks < 2)
				return true;
			if (this.LoopState == GjallarLoopState.LoopQueue)
				return false;
			return this.CurrentPosition <= 1;
		}
		private bool CheckEOQ() {
			if (this.TotalTracks < 2)
				return true;
			if (this.LoopState == GjallarLoopState.LoopQueue)
				return false;
			return this.CurrentPosition >= this.TotalTracks;
		}

		protected override async ValueTask NotifyTrackEndedAsync(ITrackQueueItem track, TrackEndReason endReason, CancellationToken cancellationToken = default) => await GjallarEventHandlers.HandleTrackEndedEvent(this, endReason, cancellationToken);
		protected override async ValueTask NotifyTrackStuckAsync(ITrackQueueItem track, TimeSpan threshold, CancellationToken cancellationToken = default) => await GjallarEventHandlers.HandleTrackStuckEvent(this, threshold, cancellationToken);
		protected override async ValueTask NotifyTrackExceptionAsync(ITrackQueueItem track, TrackException exception, CancellationToken cancellationToken = default) => await GjallarEventHandlers.HandleTrackExceptionEvent(this, exception, cancellationToken);
		// protected override async ValueTask NotifyTrackStartedAsync(ITrackQueueItem track, CancellationToken cancellationToken = default) => await GjallarEventHandlers.HandleTrackStartedEvent(this, track, cancellationToken);
		// protected override async ValueTask NotifyChannelUpdateAsync(ulong? voiceChannelId, CancellationToken cancellationToken = default) { }
		// protected override async ValueTask NotifyFiltersUpdatedAsync(IPlayerFilters filters, CancellationToken cancellationToken = default) { }
		// protected override async ValueTask NotifyWebSocketClosedAsync(WebSocketCloseStatus closeStatus, string reason, bool byRemote = false, CancellationToken cancellationToken = default) { }

		public async Task<bool> ShuffleTracks() {
			if (TotalTracks == 0)
				return false;
			Shuffle(Tracks);
			var toPlayNow = await UseTrackByPositionAsync(1);
			if (toPlayNow != null)
				await PlayAsync(toPlayNow.Track);
			return true;
		}
		public async Task<bool> DeleteActivePlayerMessageAsync() {
			try {
				if (this.ActivePlayerMss is not null)
					await this.ActivePlayerMss.DeleteAsync();
				return true;
			} catch (Exception) {
				return false;
			}
		}
		private static void Shuffle(List<GjallarTrack> tracks) {
			int n = tracks.Count;
			while (n > 1) {
				int k = Random.Next(n--);
				(tracks[k], tracks[n]) = (tracks[n], tracks[k]);
			}
		}
	}
}
