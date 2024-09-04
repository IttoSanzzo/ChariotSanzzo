using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Components.MusicComponent {
	public class GjallarhornContext {
	// -1. Extras
		public class GTXInfo {
		// M. Member Variables
			public bool		Priority	{get; set;} = false;
			public string	Query		{get; set;} = "NULL";
			public int		Plataform	{get; set;} = 0;
			public int		PauseType	{get; set;} = 2;
			public int		LoopType	{get; set;} = 3;
			public int 		SkipCount	{get; set;} = 1;

		// C. Constructor
			public GTXInfo() {
			}
		}
	
	// M. Member Variables
		public DiscordColor			Color			{get; set;} = DiscordColor.Black;
		public string				Command			{get; set;} = "Missing";
		public string				Username		{get; set;} = "Missing";
		public string				UserIcon		{get; set;} = "Missing";
		public string?				Message			{get; set;} = null;
		public string				TrackLink		{get; set;} = "Missing";
		public DiscordGuild?		Guild			{get; set;} = null;
		public DiscordChannel?		ChatChannel		{get; set;} = null;
		public DiscordChannel?		VoiceChannel	{get; set;} = null;
		public DiscordMember?		Member			{get; set;} = null;
		private ulong				ChatChannelId	{get; set;}
		private ulong?				VoiceChannelId	{get; set;} = null;
		private ulong?				UserId			{get; set;} = null;
		public InteractionContext?	Ictx			{get; set;} = null;
		public GTXInfo				Data			{get; set;} = new GTXInfo();

	// C. Constructor
		public GjallarhornContext(string gString) {
			string[] args = gString.Split('\n');
			bool temp;
			for (int i = 0; i < args.Length; i++)
				temp = this.SetParameter(args[i]).Result;
			if (this.UserId != null)
				temp = this.GetDataFromMember().Result;
			this.Data.Query = this.TrackLink;
		}
		public GjallarhornContext(string command, InteractionContext? ictx, DiscordChannel chatChannel, DiscordMember member, DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.ChatChannel = chatChannel;
			this.ChatChannelId = this.ChatChannel.Id;
			this.Guild = this.ChatChannel.Guild;
			this.Username = member.Username;
			this.UserIcon = member.AvatarUrl;
			this.UserId = member.Id;
			this.Member = member;
			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
		}
		public GjallarhornContext(InteractionContext ctx , string command, DiscordColor? color = null, string? message = null, string link = "") {
			this.Command = command;
			this.ChatChannel = ctx.Channel;
			this.ChatChannelId = this.ChatChannel.Id;
			this.Guild = this.ChatChannel.Guild;
			this.Username = ctx.Member.Username;
			this.UserIcon = ctx.Member.AvatarUrl;
			this.UserId = ctx.Member.Id;
			this.Member = ctx.Member;
			if (color != null)
				this.Color = (DiscordColor)color;
			else
				this.Color = DiscordColor.White;
			this.TrackLink = link;
			this.Message = message;
			this.Data.Query = this.TrackLink;
		}

	// 0. Core
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
					this.ChatChannel = await GjallarhornContext.SetChannelAsync((ulong)this.ChatChannelId);
					if (this.ChatChannel == null)
						return (false);
					this.Guild = this.ChatChannel.Guild;
				break;
				case ("<|VoiceChannelId|>"):
					this.VoiceChannelId = ulong.Parse(value);
					this.VoiceChannel = await GjallarhornContext.SetChannelAsync((ulong)this.VoiceChannelId);
				break;
				case ("<|Link|>"):
					this.TrackLink = value;
				break;
				default:
					return (false);
			}
			return (true);
		}
		private async Task<bool>					GetDataFromMember() {
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
		private static async Task<DiscordChannel?>	SetChannelAsync(ulong channelId) {
			if (Program.Client == null)
				return (null);
			return (await Program.Client.GetChannelAsync(channelId));
		}
	}
}
