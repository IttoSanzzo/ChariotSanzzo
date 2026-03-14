using ChariotSanzzo.Components.Gjallar.Types;
using ChariotSanzzo.Services.Wrappers;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Lavalink4NET.Rest.Entities.Tracks;

namespace ChariotSanzzo.Components.Gjallar {
	public class GjallarContext {
		public class GTXInfo {
			public bool Priority { get; set; } = false;
			public string Query { get; set; } = "NULL";
			public TrackSearchMode Plataform { get; set; } = TrackSearchMode.None;
			public GjallarPauseState PauseType { get; set; } = GjallarPauseState.Switch;
			public GjallarLoopState LoopType { get; set; } = GjallarLoopState.LoopQueue;
			public int SkipCount { get; set; } = 1;
			public int MiscValue { get; set; } = 0;
			public double MiscDoubleValue { get; set; } = 0;
			public long Position { get; set; } = -1;
			public bool VipCall { get; set; } = false;
			public bool WithResponse { get; set; } = true;
			public bool IsFromEvent { get; set; } = false;
		}

		public DiscordColor Color { get; set; } = DiscordColor.Black;
		public string Command { get; set; } = "Missing";
		public string? Message { get; set; } = null;
		public string TrackLink { get; set; } = "Missing";
		public DiscordGuild Guild { get; set; } = null!;
		public DiscordChannel? ChatChannel { get; set; } = null;
		public DiscordChannel? VoiceChannel { get; set; } = null;
		public DiscordMember Member { get; set; } = null!;
		public string Username => Member.Username;
		public string UserIcon => Member.AvatarUrl;
		public ulong UserId => Member.Id;
		private ulong ChatChannelId { get; set; }
		public CommandContext? Ictx { get; set; } = null;
		public GTXInfo Data { get; set; } = new GTXInfo();
		public GjallarCallResult Result { get; set; } = new("");

		public GjallarContext() { }
		public async Task GjallarContextUnsecureAsync(GjallarGenericCommand genericCommand) {
			this.Command = genericCommand.Command;
			if (!string.IsNullOrEmpty(genericCommand.GuildId) && ulong.TryParse(genericCommand.GuildId, out var guildId))
				this.Guild = (await GetGuildAsync(guildId))!;
			if (this.Guild is null)
				throw new Exception("GUILD_NOT_FOUND_EXCEPTION");
			if (!ulong.TryParse(genericCommand.UserId, out var userIdUlong))
				throw new Exception("USERID_INVALID_EXCEPTION");
			this.Member = await Guild.GetMemberAsync(userIdUlong) ?? throw new Exception("MEMBER_NOT_FOUND_EXCEPTION");
			this.VoiceChannel = await GetVoiceChannelAsync();
			if (!string.IsNullOrEmpty(genericCommand.ChannelId)) {
				this.ChatChannelId = ulong.Parse(genericCommand.ChannelId);
				this.ChatChannel = await GetChannelAsync(this.ChatChannelId);
			}
			if (!string.IsNullOrEmpty(genericCommand.TrackUrl))
				this.TrackLink = genericCommand.TrackUrl;
			if (genericCommand.TrackPosition != null)
				this.Data.MiscValue = (int)genericCommand.TrackPosition;
			if (genericCommand.SeekSeconds != null)
				this.Data.Position = (long)genericCommand.SeekSeconds;
			this.Data.Query = this.TrackLink;
			this.Data.Priority = genericCommand.Priority;
			this.Result = new(this.Command);
		}
		public GjallarContext(string command, DiscordChannel chatChannel, DiscordMember member, CommandContext? ictx = null, DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.ChatChannel = chatChannel;
			this.ChatChannelId = this.ChatChannel.Id;
			this.Guild = this.ChatChannel.Guild;
			this.Ictx = ictx;
			this.Member = member;
			this.VoiceChannel = GetVoiceChannelAsync().Result;
			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}
		public GjallarContext(string command, DiscordGuild guild, DiscordMember member, CommandContext? ictx = null, DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.Guild = guild;
			this.Ictx = ictx;
			this.Member = member;
			this.VoiceChannel = GetVoiceChannelAsync().Result;
			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}
		public GjallarContext(CommandContext ctx, string command = "Default", DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.ChatChannel = ctx.Channel;
			this.ChatChannelId = this.ChatChannel.Id;
			this.Guild = this.ChatChannel.Guild;
			this.Ictx = ctx;
			this.Member = ctx.Member!;
			this.VoiceChannel = GetVoiceChannelAsync().Result;

			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}
		public GjallarContext(ComponentInteractionCreatedEventArgs ctx, string command = "Default", DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.ChatChannel = ctx.Channel;
			this.ChatChannelId = this.ChatChannel.Id;
			this.Guild = this.ChatChannel.Guild;
			this.Ictx = null;
			this.Member = GetMemberAsync(ctx.User.Id).Result;
			this.VoiceChannel = GetVoiceChannelAsync().Result;
			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}

		public async Task<string> TryCallingAsync() {
			return await GjallarCaller.TryCallingAsync(this);
		}

		private async Task<DiscordMember> GetMemberAsync(ulong userId) => await Guild.GetMemberAsync(userId);
		private async Task<DiscordChannel?> GetVoiceChannelAsync() => Member.VoiceState != null ? await Member.VoiceState.GetChannelAsync() : null;
		private static async Task<DiscordGuild?> GetGuildAsync(ulong guildId) {
			try {
				return await Discord.Client.GetGuildAsync(guildId);
			} catch {
				return null;
			}
		}
		private static async Task<DiscordChannel?> GetChannelAsync(ulong channelId) {
			try {
				return await Discord.Client.GetChannelAsync(channelId);
			} catch {
				return null;
			}
		}

		public async Task GTXEmbedTimerAsync(int seconds, DiscordEmbed embed) {
			var message = await this.GTXEmbedSendAsync(embed);
			if (message == null)
				return;
			Thread thread = new(() => WaitForCleaning(seconds, message));
			thread.Start();
		}
		public async Task GTXEmbedTimerAsync(int seconds, DiscordMessageBuilder messageBuilder) {
			var message = await this.GTXEmbedSendAsync(messageBuilder);
			if (message == null)
				return;
			Thread thread = new(() => WaitForCleaning(seconds, message));
			thread.Start();
		}
		public async Task<DiscordMessage?> GTXEmbedSendAsync(DiscordEmbed embed) {
			if (this.Ictx != null) {
				if (this.Data.WithResponse == false) {
					await this.Ictx.DeleteResponseAsync();
					return (null);
				}
				return await this.Ictx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
			} else if (ChatChannel! != null!) {
				if (this.Data.WithResponse == false)
					return null;
				return await this.ChatChannel.SendMessageAsync(embed);
			}
			return null;
		}
		public async Task<DiscordMessage?> GTXEmbedSendAsync(DiscordMessageBuilder messageBuilder) {
			if (this.Ictx != null) {
				if (this.Data.WithResponse == false) {
					await this.Ictx.DeleteResponseAsync();
					return null;
				}
				return (await this.Ictx.EditResponseAsync(new DiscordWebhookBuilder(messageBuilder)));
			} else if (ChatChannel! != null!) {
				if (this.Data.WithResponse == false)
					return null;
				return await this.ChatChannel.SendMessageAsync(messageBuilder);
			}
			return null;
		}
		private static async void WaitForCleaning(int seconds, DiscordMessage message) {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
		}
	}
}
