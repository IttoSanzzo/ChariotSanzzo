using DSharpPlus.Entities;

namespace Gjallarhorn.Components {
	public class GjallarhornContext {
	// 0. Member Variables
		public DiscordColor		_color			{get; set;}
		public string			_command		{get; set;}
		public string			_username		{get; set;}
		public string			_userIcon		{get; set;}
		public string?			_message		{get; set;} = null;
		public string			_trackLink		{get; set;} = "";
		public DiscordGuild?	_guild			{get; set;} = null;
		public DiscordChannel?	_chatChannel	{get; set;} = null;
		public DiscordChannel?	_voiceChannel	{get; set;} = null;
		private ulong			_chatChannelId	{get; set;}
		private ulong?			_voiceChannelId	{get; set;} = null;

	// 1. Constructor
		public GjallarhornContext(DiscordEmbed gEmbed) {
		// 1.0 Base
			this._color = gEmbed.Color.Value;
			this._command = gEmbed.Title;
			this._username = gEmbed.Footer.Text;
			this._userIcon = gEmbed.Footer.IconUrl.ToString();
		// 1.1 Ids
			string[] args = gEmbed.Description.Split('\n');
			bool temp;
			for (int i = 0; i < args.Length; i++)
				temp = this.SetParameter(args[i]).Result;
		}

	// 2. Utils
		private async Task<bool>	SetParameter(string argLine) {
		// Checks and Sets
			string[] parts = argLine.Split('\t');
			string	type = parts[0];
			string	value = parts[1];
		// Core
			switch (type) {
				case ("Message:"):
					this._message = value;
				break;
				case ("ChatChannelId:"):
					this._chatChannelId = ulong.Parse(value);
					this._chatChannel = await GjallarhornContext.SetChannelAsync((ulong)this._chatChannelId);
					if (this._chatChannel == null)
						return (false);
					this._guild = this._chatChannel.Guild;
				break;
				case ("VoiceChannelId:"):
					this._voiceChannelId = ulong.Parse(value);
					this._voiceChannel = await GjallarhornContext.SetChannelAsync((ulong)this._voiceChannelId);
				break;
				case ("Link:"):
					this._trackLink = value;
				break;
				default:
					return (false);
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
