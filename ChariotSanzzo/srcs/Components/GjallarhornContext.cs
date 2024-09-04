using System.Data.Common;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Components {
	public class GjallarhornContext {
		public class GTXInfo {
		// 0. Member Variables
			public string	Query		{get; set;} = "NULL";
			public int		Plataform	{get; set;} = 0;
			public int		PauseType	{get; set;} = 2;
			public int		LoopType	{get; set;} = 3;

		// 1. Constructor
		public GTXInfo() {
		}

		}
	// 0. Member Variables
		public DiscordColor			_color			{get; set;} = DiscordColor.Black;
		public string				_command		{get; set;} = "Missing";
		public string				_username		{get; set;} = "Missing";
		public string				_userIcon		{get; set;} = "Missing";
		public string?				_message		{get; set;} = null;
		public string				_trackLink		{get; set;} = "Missing";
		public DiscordGuild?		_guild			{get; set;} = null;
		public DiscordChannel?		_chatChannel	{get; set;} = null;
		public DiscordChannel?		_voiceChannel	{get; set;} = null;
		public DiscordMember?		_member			{get; set;} = null;
		private ulong				_chatChannelId	{get; set;}
		private ulong?				_voiceChannelId	{get; set;} = null;
		private ulong?				_userId			{get; set;} = null;
		public InteractionContext?	_Ictx			{get; set;} = null;
		public GTXInfo				_Data			{get; set;} = new GTXInfo();

	// 1. Constructor
		public GjallarhornContext(string gString) {
			string[] args = gString.Split('\n');
			bool temp;
			for (int i = 0; i < args.Length; i++)
				temp = this.SetParameter(args[i]).Result;
			if (this._userId != null)
				temp = this.GetDataFromMember().Result;
			this._Data.Query = this._trackLink;
		}
		public GjallarhornContext(string command, InteractionContext? ictx, DiscordChannel chatChannel, DiscordMember member, DiscordColor color, string? message = null, string link = "") {
			this._command = command;
			this._chatChannel = chatChannel;
			this._chatChannelId = chatChannel.Id;
			this._guild = this._chatChannel.Guild;
			this._username = member.Username;
			this._userIcon = member.AvatarUrl;
			this._userId = member.Id;
			this._member = member;
			this._color = color;
			this._trackLink = link;
			this._message = message;
			this._Data.Query = this._trackLink;
		}

	// 2. Utils
		private async Task<bool>	SetParameter(string argLine) {
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
					this._userId = ulong.Parse(value);
				break;
				case ("<|Color|>"):
					this._color = new DiscordColor(value);
				break;
				case ("<|Command|>"):
					this._command = value;
				break;
				case ("<|Username|>"):
					this._username = value;
				break;
				case ("<|Usericon|>"):
					this._userIcon = value;
				break;
				case ("<|Message|>"):
					this._message = value;
				break;
				case ("<|ChatChannelId|>"):
					this._chatChannelId = ulong.Parse(value);
					this._chatChannel = await GjallarhornContext.SetChannelAsync((ulong)this._chatChannelId);
					if (this._chatChannel == null)
						return (false);
					this._guild = this._chatChannel.Guild;
				break;
				case ("<|VoiceChannelId|>"):
					this._voiceChannelId = ulong.Parse(value);
					this._voiceChannel = await GjallarhornContext.SetChannelAsync((ulong)this._voiceChannelId);
				break;
				case ("<|Link|>"):
					this._trackLink = value;
				break;
				default:
					return (false);
			}
			return (true);
		}
		private async Task<bool>	GetDataFromMember() {
			if (Program.Client == null || this._guild == null || this._userId == null)
				return (false);
			DiscordMember member = await this._guild.GetMemberAsync((ulong)this._userId);
			this._userIcon = member.AvatarUrl;
			this._username = member.Username;
			this._guild = member.Guild;
			this._member = member;
			if (member.VoiceState != null) {
				this._voiceChannel = member.VoiceState.Channel;
				this._voiceChannelId = member.VoiceState.Channel.Id;
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
