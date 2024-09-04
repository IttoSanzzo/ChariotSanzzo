using ChariotSanzzo.Config;
using Npgsql;

namespace ChariotSanzzo.Database {
	public class DBEngine {
	// M. Member Variables
		public static bool	Debug	{get; set;} = true;

	// 0. Main Functions
		public async Task<bool>				StoreUserAsync(DBUser user) {
			var	DBEngine = new DBEngine();
			var	retrieve = await DBEngine.GetUserAsync(user.UserName, user.ServerId);
			if (retrieve.Item1 == true)
				return (true);
			var	userNo = await GetAllRowsCountAsync("data.userinfo") + 1;
			if (userNo == -1)
				throw new Exception();
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string	query = "INSERT INTO data.userinfo (userno, username, servername, serverid) " + 
									$"VALUES ('{userNo}', '{user.UserName}', '{user.ServerName}', '{user.ServerId}')";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						await cmd.ExecuteNonQueryAsync();
					}
				}
				return (true);
			} catch (Exception ex) {
				Program.WriteLine(ex.ToString());
				return (false);
			}
		}
		
	// G. Get Functions
		public async Task<(bool, DBUser?)>	GetUserAsync(string userName, ulong serverId) {
			DBUser	user;
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync(); 
					string query = "SELECT u.userno, u.username, u.servername, u.serverid " + 
					"FROM data.userinfo u " +
					$"WHERE username = '{userName}' AND serverid = {serverId}";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var	reader = await cmd.ExecuteReaderAsync();
						await reader.ReadAsync();
						user = new DBUser {
							UserName = reader.GetString(1),
							ServerName = reader.GetString(2),
							ServerId = (ulong)reader.GetInt64(3)
						};
					}
				}
				return (true, user);
			}
			catch (Exception ex) {
				Program.WriteLine($"[->GetUserAsyncError\n{ex.ToString()}\n]");
				return (false, null);
			}
		}
		public async Task<DBFanfare>		GetDiceFanfareAsync(int dice) {
			DBFanfare	dicef = new DBFanfare();
			if (dice != 1 && dice != 20)
				return (dicef);
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query = "SELECT d.message, d.glink " +
									"FROM botmiscs.dfanfare d " +
									$"WHERE type = {dice} " +
									"ORDER BY random() LIMIT 1";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var reader = await cmd.ExecuteReaderAsync();
						await reader.ReadAsync();
						dicef.Message = reader.GetString(0);
						dicef.GifLink = reader.GetString(1);
					}
				}
				return (dicef);
			}
			catch (Exception ex) {
				Program.WriteLine($"[->GetDiceFanfareError\n{ex.ToString()}\n]");
				return (dicef);
			}
		}
	// U. Utils
		public static async Task<long>		GetAllRowsCountAsync(string table, string? condition = null) {
			Object?	userCount;
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query = $"SELECT COUNT (*) FROM {table}";
					if (condition != null)
						query += $"\nWHERE {condition}";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						userCount = await cmd.ExecuteScalarAsync();
					}
				}
				return (Convert.ToInt64(userCount));
			} catch (Exception ex) {
				Program.WriteLine(ex.ToString());
				return (-1);
			}
		}
	}
}
