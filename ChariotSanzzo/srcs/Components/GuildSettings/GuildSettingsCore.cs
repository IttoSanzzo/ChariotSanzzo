using ChariotSanzzo.Config;
using Npgsql;

namespace ChariotSanzzo.Components.GuildSettings {
	public static class GuildSettingsCore {
	// M. Member Variables
	
	// 0. Core
		public static async Task<GuildSettingsContext>	GetGuildSettingsAsync(ulong guildId) {
			if (await GuildSettingsCore.GSExistsAsync(guildId) == false)
				return (new GuildSettingsContext(guildId));
			try {
				string SettingsJson = "";
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query = "SELECT gs.settingsjson FROM data.guildsettings gs " +
									$"WHERE guildid = {guildId}";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var reader = await cmd.ExecuteReaderAsync();
						await reader.ReadAsync();
						SettingsJson = reader.GetString(0);
					}
				}
				return (new GuildSettingsContext(SettingsJson));
			} catch (Exception ex) {
				Program.WriteException(ex);
				return (new GuildSettingsContext(guildId));
			}
		}
		public static async Task<bool>					SetGuildSettingsAsync(GuildSettingsContext ctx) {
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query;
					if (await GuildSettingsCore.GSExistsAsync(ctx.GuildId) == false)
						query = "INSERT INTO data.guildsettings " +
									"(guildid, settingsjson) " +
									"VALUES " +
									$"({ctx.GuildId}, '{ctx.GetJson()}')";
					else
						query = "UPDATE data.guildsettings " +
									$"SET settingsjson = '{ctx.GetJson()}' " +
									$"WHERE guildid = {ctx.GuildId}";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var rowsAffected = await cmd.ExecuteNonQueryAsync();
					}
				}
				return (true);
			} catch (Exception ex) {
				Program.WriteException(ex);
				return (false);
			}
		}

	// 1. GetStates
		public static async Task<bool>					GetStateVipServer(ulong guildId) {
			var ctx = await GuildSettingsCore.GetGuildSettingsAsync(guildId);
			return (ctx.Data.VipServer);
		}
		public static async Task<bool>					GetStateAutoDice(ulong guildId) {
			var ctx = await GuildSettingsCore.GetGuildSettingsAsync(guildId);
			return (ctx.Data.AutoDice);
		}
		public static async Task<bool>					GetStateReplayDice(ulong guildId) {
			var ctx = await GuildSettingsCore.GetGuildSettingsAsync(guildId);
			return (ctx.Data.ReplayDice);
		}
		public static async Task<bool>					GetStateDiceFanfare(ulong guildId) {
			var ctx = await GuildSettingsCore.GetGuildSettingsAsync(guildId);
			return (ctx.Data.DiceFanfare);
		}
	// U. Utils
		public static async Task<bool>					GSExistsAsync(ulong guildId) {
			bool found = false;
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query = "SELECT * FROM data.guildsettings " +
									$"WHERE guildid = {guildId}";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var reader = await cmd.ExecuteReaderAsync();
						await reader.ReadAsync();
						if ((ulong)reader.GetInt64(1) == guildId)
							found = true;
					}
				}
				return (found);
			} catch (Exception ex) {
				Program.WriteException(ex);
				return (false);
			}
		}
	}
}