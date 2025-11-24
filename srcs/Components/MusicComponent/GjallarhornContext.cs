using ChariotSanzzo.Components.HttpServer;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Components.MusicComponent {
	public class GjallarhornCallResult(string command, bool wasSuccess = false, string? errorMessage = null) {
		public string		Command			{get; set;} = command;
		public bool			WasSuccess	{get; set;} = wasSuccess;
		public string?	ErrorMessage	{get; set;} = errorMessage;
	}
	public class GjallarhornContext {
	// -1. Extras
		public class GTXInfo {
		// M. Member Variables
			public bool		Priority			{get; set;} = false;
			public string	Query					{get; set;} = "NULL";
			public int		Plataform			{get; set;} = 0;
			public int		PauseType			{get; set;} = 2;
			public int		LoopType			{get; set;} = 3;
			public int 		SkipCount			{get; set;} = 1;
			public int		MiscValue			{get; set;} = 0;
			public long		Position			{get; set;} = -1;
			public bool		VipCall				{get; set;} = false;
			public bool		WithResponse	{get; set;} = true;
			public bool		IsFromEvent			{get; set;} = false;

		// C. Constructor
			public GTXInfo() {
			}
		}
	
	// M. Member Variables
		public DiscordColor						Color						{get; set;} = DiscordColor.Black;
		public string									Command					{get; set;} = "Missing";
		public string									Username				{get; set;} = "Missing";
		public string									UserIcon				{get; set;} = "Missing";
		public string?								Message					{get; set;} = null;
		public string									TrackLink				{get; set;} = "Missing";
		public DiscordGuild?					Guild						{get; set;} = null;
		public DiscordChannel?				ChatChannel			{get; set;} = null;
		public DiscordChannel?				VoiceChannel		{get; set;} = null;
		public DiscordMember?					Member					{get; set;} = null;
		private ulong									ChatChannelId		{get; set;}
		private ulong?								VoiceChannelId	{get; set;} = null;
		private ulong?								UserId					{get; set;} = null;
		public InteractionContext?		Ictx						{get; set;} = null;
		public GTXInfo								Data						{get; set;} = new GTXInfo();
		public GjallarhornCallResult	Result					{get;	set;} = null!;

	// C. Constructor
		public GjallarhornContext() {
			this.Result = new("");
		}
		public GjallarhornContext(PlayerGenericCommand genericCommand) {
			this.Command = genericCommand.Command;
			if (!string.IsNullOrEmpty(genericCommand.GuildId) && ulong.TryParse(genericCommand.GuildId, out var guildId)) {
				this.Guild = GjallarhornContext.GetGuildAsync(guildId).Result;
			}
			if (!string.IsNullOrEmpty(genericCommand.ChannelId) && ulong.TryParse(genericCommand.GuildId, out var channelId)) {
				this.ChatChannelId = ulong.Parse(genericCommand.ChannelId);
				this.ChatChannel = GjallarhornContext.GetChannelAsync(channelId).Result;
			}
			if (!string.IsNullOrEmpty(genericCommand.TrackUrl)) {
				this.TrackLink = genericCommand.TrackUrl;
			}
			if (genericCommand.Position != null)
				this.Data.Position = (long)genericCommand.Position;
			this.UserId = ulong.Parse(genericCommand.UserId);
			_ = this.GetDataFromMember().Result;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}
		public async Task GjallarhornContextAsync(PlayerGenericCommand genericCommand) {
			this.Command = genericCommand.Command;
			if (!string.IsNullOrEmpty(genericCommand.GuildId) && ulong.TryParse(genericCommand.GuildId, out var guildId)) {
				this.Guild = await GjallarhornContext.GetGuildAsync(guildId);
			}
			if (!string.IsNullOrEmpty(genericCommand.ChannelId)) {
				this.ChatChannelId = ulong.Parse(genericCommand.ChannelId);
				this.ChatChannel = await GjallarhornContext.GetChannelAsync((ulong)this.ChatChannelId);
			}
			if (!string.IsNullOrEmpty(genericCommand.TrackUrl)) {
				this.TrackLink = genericCommand.TrackUrl;
			}
			if (genericCommand.Position != null)
				this.Data.Position = (long)genericCommand.Position;
			this.UserId = ulong.Parse(genericCommand.UserId);
			_ = this.GetDataFromMember().Result;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}
		public GjallarhornContext(string gString) {
			string[] args = gString.Split('\n');
			for (int i = 0; i < args.Length; i++)
				_ = this.SetParameter(args[i]).Result;
			if (this.UserId != null)
				_ = this.GetDataFromMember().Result;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}
		public GjallarhornContext(string command, DiscordChannel chatChannel, InteractionContext? ictx = null, DiscordMember? member = null, DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.ChatChannel = chatChannel;
			this.ChatChannelId = this.ChatChannel.Id;
			this.Guild = this.ChatChannel.Guild;
			this.Ictx = ictx;
			this.Member = member;
			if (this.Member != null) {
				this.Username = this.Member.Username;
				this.UserIcon = this.Member.AvatarUrl;
				this.UserId = this.Member.Id;
			}
			_ = this.GetDataFromMember().Result;
			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}
		public GjallarhornContext(string command, DiscordGuild guild, InteractionContext? ictx = null, DiscordMember? member = null, DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.Guild = guild;
			this.Ictx = ictx;
			this.Member = member;
			if (this.Member != null) {
				this.Username = this.Member.Username;
				this.UserIcon = this.Member.AvatarUrl;
				this.UserId = this.Member.Id;
			}
			var trash = this.GetDataFromMember().Result;
			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}
		public GjallarhornContext(InteractionContext ctx , string command = "Default", DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.ChatChannel = ctx.Channel;
			this.ChatChannelId = this.ChatChannel.Id;
			this.Guild = this.ChatChannel.Guild;
			this.Ictx = ctx;
			this.Member = ctx.Member;
			this.Username = this.Member.Username;
			this.UserIcon = this.Member.AvatarUrl;
			this.UserId = this.Member.Id;
			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
			this.Result = new(this.Command);
		}
		public GjallarhornContext(ComponentInteractionCreateEventArgs ctx , string command = "Default", DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.ChatChannel = ctx.Channel;
			this.ChatChannelId = this.ChatChannel.Id;
			this.Guild = this.ChatChannel.Guild;
			this.Ictx = null;
			this.UserId = ctx.User.Id;
			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
			_ = this.GetDataFromMember().Result;
			this.Result = new(this.Command);
		}

	// 0. Core
		public async Task<string>					TryCallingAsync() {
			return (await ChariotMusicCalls.TryCallAsync(this));
		}

	// E1. Miscs
		private async Task<bool>					SetParameter(string argLine) {
		// Checks and Sets
			if (argLine == null)
				return false;
			string[] parts = argLine.Split("<|Value|>");
			if (parts.Length != 2)
				return (false);
			string type = parts[0];
			string value = parts[1];
		// Core
			switch (type) {
				case ("<|UserId|>"):
					this.UserId = ulong.Parse(value);
				break;
				case ("<|Color|>"):
					this.Color = new DiscordColor(value);
				break;
				case ("<|Command|>"):
					this.Command = value;
				break;
				case ("<|Username|>"):
					this.Username = value;
				break;
				case ("<|Usericon|>"):
					this.UserIcon = value;
				break;
				case ("<|Message|>"):
					this.Message = value;
				break;
				case ("<|ChatChannelId|>"):
					this.ChatChannelId = ulong.Parse(value);
					this.ChatChannel = await GjallarhornContext.GetChannelAsync((ulong)this.ChatChannelId);
					if (this.ChatChannel == null)
						return (false);
					this.Guild = this.ChatChannel.Guild;
				break;
				case ("<|VoiceChannelId|>"):
					this.VoiceChannelId = ulong.Parse(value);
					this.VoiceChannel = await GjallarhornContext.GetChannelAsync((ulong)this.VoiceChannelId);
				break;
				case ("<|Link|>"):
					this.TrackLink = value;
				break;
				default:
					return (false);
			}
			return (true);
		}
		private async Task<bool>										GetDataFromMember() {
			if (Program.Client == null || this.Guild == null || this.UserId == null)
				return (false);
			DiscordMember member = await this.Guild.GetMemberAsync((ulong)this.UserId);
			this.UserIcon = member.AvatarUrl;
			this.Username = member.Username;
			this.Guild = member.Guild;
			this.Member = member;
			if (member.VoiceState != null) {
				this.VoiceChannel = member.VoiceState.Channel;
				this.VoiceChannelId = member.VoiceState.Channel.Id;
			}
			return (true);
		}
		private static async Task<DiscordGuild?>		GetGuildAsync(ulong guildId) {
			if (Program.Client == null)
				return null;
			try {
				return await Program.Client.GetGuildAsync(guildId);
			} catch {
				return null;
			}
		}
		private static async Task<DiscordChannel?>	GetChannelAsync(ulong channelId) {
			if (Program.Client == null)
				return null;
			try {
				return await Program.Client.GetChannelAsync(channelId);
			} catch {
				return null;
			}
		}
		
	// E2. Message Handlers
		public async Task										GTXEmbedTimerAsync(int seconds, DiscordEmbed embed) {
			var message = await this.GTXEmbedSendAsync(embed);
			if (message == null)
				return ;
			Thread thread = new Thread(() => WaitForCleaning(seconds, message));
			thread.Start();
		}
		public async Task										GTXEmbedTimerAsync(int seconds, DiscordMessageBuilder messageBuilder) {
			var message = await this.GTXEmbedSendAsync(messageBuilder);
			if (message == null)
				return ;
			Thread thread = new Thread(() => WaitForCleaning(seconds, message));
			thread.Start();
		}
		public async Task<DiscordMessage?>	GTXEmbedSendAsync(DiscordEmbed embed) {
			if (this.Ictx != null) {
				if (this.Data.WithResponse == false) {
					await this.Ictx.DeleteResponseAsync();
					return (null);
				}
				return (await this.Ictx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed)));
			} else if (this.ChatChannel != null) {
				if (this.Data.WithResponse == false)
					return (null);
				return (await this.ChatChannel.SendMessageAsync(embed));
			}
			return (null);
		}
		public async Task<DiscordMessage?>	GTXEmbedSendAsync(DiscordMessageBuilder messageBuilder) {
			if (this.Ictx != null) {
				if (this.Data.WithResponse == false) {
					await this.Ictx.DeleteResponseAsync();
					return (null);
				}
				return (await this.Ictx.EditResponseAsync(new DiscordWebhookBuilder(messageBuilder)));
			} else if (this.ChatChannel != null) {
				if (this.Data.WithResponse == false)
					return (null);
				return (await this.ChatChannel.SendMessageAsync(messageBuilder));
			}
			return (null);
		}
		private static async void						WaitForCleaning(int seconds, DiscordMessage message) {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
		}
	}
}
