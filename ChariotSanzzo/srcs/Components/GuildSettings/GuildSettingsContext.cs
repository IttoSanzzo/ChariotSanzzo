using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace ChariotSanzzo.Components.GuildSettings {
	public class GuildSettingsContext {
		public class SettingsData {
			public bool VipServer	{get; set;} = false;
			public bool	AutoDice	{get; set;} = true;
			public bool	DiceFanfare	{get; set;} = true;
			public bool	ReplayDice	{get; set;} = true;
		}
	// M. Member Variables
		public SettingsData		Data	{get; set;} = new();
		public ulong			GuildId	{get; set;} = 0;

	// C. Contructors
		public GuildSettingsContext(ulong guildId) {
			this.GuildId = guildId;
		}
		public GuildSettingsContext(string jsonSource) {
			try {
				ulong? guildId = (ulong?)JObject.Parse(jsonSource)["GuildId"];
				if (guildId != null)
					this.GuildId = (ulong)guildId;
				this.Data.VipServer		= GuildSettingsContext.GetValueFromJson(jsonSource, "VipServer", this.Data.VipServer);
				this.Data.AutoDice		= GuildSettingsContext.GetValueFromJson(jsonSource, "AutoDice", this.Data.AutoDice);
				this.Data.ReplayDice	= GuildSettingsContext.GetValueFromJson(jsonSource, "ReplayDice", this.Data.ReplayDice);
				this.Data.DiceFanfare	= GuildSettingsContext.GetValueFromJson(jsonSource, "DiceFanfare", this.Data.DiceFanfare);
			} catch (Exception ex) {
				Program.WriteException(ex);
			}
		}

	// 0. Core
		public async Task<bool>	SetAsync() {
			return (await GuildSettingsCore.SetGuildSettingsAsync(this));
		}
		public string			GetJson() {
		// S. Start
			string settingsJson = "{";
			settingsJson += ($"\"GuildId\": {this.GuildId}, ");
			settingsJson += GenJsonBool("VipServer", this.Data.VipServer);
			settingsJson += GenJsonBool("AutoDice", this.Data.AutoDice);
			settingsJson += GenJsonBool("ReplayDice", this.Data.ReplayDice);
			settingsJson += GenJsonBool("DiceFanfare", this.Data.DiceFanfare);
		// E. End
			settingsJson = settingsJson.Remove(settingsJson.Length - 2) + "}";
			return (settingsJson);
		}
		public string			GetEmbedDescription() {
			string	description = "```ansi\n";
			description += GenDescriptionRow("🌟", "VipServer....:", this.Data.VipServer);
			description += GenDescriptionRow("🎲", "AutoDice.....:", this.Data.AutoDice);
			description += GenDescriptionRow("🔄", "ReplayDice...:", this.Data.ReplayDice);
			description += GenDescriptionRow("🥳", "DiceFanfare..:", this.Data.DiceFanfare);
			return (description + "```");
		}

	// U. Utils
		private static string	GenDescriptionRow(string emote, string name, bool value) {
			if (value == true)
				return ($"{emote}[2;34m{name}[2;0m -> [2;32mTrue[2;0m\n");
			return ($"{emote}[2;34m{name}[2;0m -> [2;31mFalse[2;0m\n");
		}
		private static string	GenJsonBool(string name, bool value) {
			if (value == true)
				return ($"\"{name}\": true, ");
			return ($"\"{name}\": false, ");
		}
		private static bool		GetValueFromJson(string jsonSource, string name, bool defaultValue) {
			bool? value = (bool?)JObject.Parse(jsonSource)[name];
			if (value != null)
				return ((bool)value);
			return (defaultValue);
		}
	}
}