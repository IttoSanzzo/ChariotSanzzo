using DSharpPlus.Entities;
using Gjallarhorn.HttpServer;

namespace Gjallarhorn.Components {
	public class GjallarhornContext {
	// 0. Member Variables
		public DiscordColor			_color					{get; set;} = DiscordColor.Black;
		public string						_command				{get; set;} = "Missing";
		public string						_username				{get; set;} = "Missing";
		public string						_userIcon				{get; set;} = "Missing";
		public string?					_message				{get; set;} = null;
		public string						_trackLink			{get; set;} = "Missing";
		public DiscordGuild?		_guild					{get; set;} = null;
		public DiscordChannel?	_chatChannel		{get; set;} = null;
		public DiscordChannel?	_voiceChannel		{get; set;} = null;
		private ulong						_chatChannelId	{get; set;}
		private ulong?					_voiceChannelId	{get; set;} = null;
		public ulong?						_userId					{get; set;} = null;

	// 1. Constructor

		public GjallarhornContext(PlayerGenericCommand genericCommand) {
			this._command = genericCommand.Command;
			if (!string.IsNullOrEmpty(genericCommand.ChannelId)) {
				this._chatChannelId = ulong.Parse(genericCommand.ChannelId);
				this._chatChannel = GjallarhornContext.SetChannelAsync((ulong)this._chatChannelId).Result;
				if (_chatChannel != null)
					this._guild = this._chatChannel.Guild;
			}
			if (!string.IsNullOrEmpty(genericCommand.TrackUrl)) {
				this._trackLink = genericCommand.TrackUrl;
			}
			this._userId = ulong.Parse(genericCommand.UserId);
			bool temp;
			if (this._userId != null)
				temp = this.GetDataFromMember().Result;
		}
		public GjallarhornContext(GjallarhornPostBody body) {
			this._color = new DiscordColor(body.Color);
			this._command = body.Command;
			this._message = body.Message;
			if (!string.IsNullOrEmpty(body.TrackUrl))
				this._trackLink = body.TrackUrl;
			if (!string.IsNullOrEmpty(body.ChannelId)) {
				this._chatChannelId = ulong.Parse(body.ChannelId);
				this._chatChannel = GjallarhornContext.SetChannelAsync((ulong)this._chatChannelId).Result;
				if (_chatChannel != null)
					this._guild = this._chatChannel.Guild;
			}
			this._userId = ulong.Parse(body.UserId);
			bool temp;
			if (this._userId != null)
				temp = this.GetDataFromMember().Result;
		}
		public GjallarhornContext(string gString) {
			string[] args = gString.Split('\n');
			bool temp;
			for (int i = 0; i < args.Length; i++)
				temp = this.SetParameter(args[i]).Result;
			if (this._userId != null)
				temp = this.GetDataFromMember().Result;
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
			if (member.VoiceState != null) {
				this._voiceChannel = member.VoiceState.Channel;
				this._voiceChannelId = member.VoiceState.Channel.Id;
			}
			return (true);
		}
		private static async Task<DiscordChannel?>	SetChannelAsync(ulong channelId) {
			if (Program.Client == null)
				return (null);
			try {
				return (await Program.Client.GetChannelAsync(channelId));
			} catch {
				return (null);
			}
		}
	}
}
