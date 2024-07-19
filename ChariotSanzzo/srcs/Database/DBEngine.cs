using System.Data;
using ChariotSanzzo.Config;
using Npgsql;

namespace ChariotSanzzo.Database {
	public class DBEngine {
		private string	_conn {get; set;} = Program.DbConfigSet._conn;
		public async Task<bool> StoreUserAsync(DBUser user) {
			var	DBEngine = new DBEngine();
			var	retrieve = await DBEngine.GetUserAsync(user._userName, user._serverId);
			if (retrieve.Item1 == true)
				return (true);
			var	userNo = await GetTotalUsersAsync() + 1;
			if (userNo == -1)
				throw new Exception();
			try {
				using (var conn = new NpgsqlConnection(this._conn)) {
					await conn.OpenAsync();
					string	query = "INSERT INTO data.userinfo (userno, username, servername, serverid) " + 
									$"VALUES ('{userNo}', '{user._userName}', '{user._serverName}', '{user._serverId}')";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						await cmd.ExecuteNonQueryAsync();
					}
				}
				return (true);
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				return (false);
			}
		}
		
		public async Task<(bool, DBUser?)> GetUserAsync(string userName, ulong serverId) {
			DBUser	user;
			try {
				using (var conn = new NpgsqlConnection(this._conn)) {
					await conn.OpenAsync();
					string	query = "SELECT u.userno, u.username, u.servername, u.serverid " + 
					"FROM data.userinfo u " +
					$"WHERE username = '{userName}' and serverid = {serverId}";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var	reader = await cmd.ExecuteReaderAsync();
						await reader.ReadAsync();
						user = new DBUser {
							_userName = reader.GetString(1),
							_serverName = reader.GetString(2),
							_serverId = (ulong)reader.GetInt64(3)
						};
					}
				}
				return (true, user);
			}
			catch (Exception ex) {
				Console.WriteLine($"[->GetUserAsyncError\n{ex.ToString()}\n]");
				return (false, null);
			}
		}
		
		public async Task<long> GetTotalUsersAsync() {
			Object?	userCount;
			try {
				using (var conn = new NpgsqlConnection(this._conn)) {
					await conn.OpenAsync();
					string query = "SELECT COUNT (*) FROM data.userinfo";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						userCount = await cmd.ExecuteScalarAsync();
					}
				}
				return (Convert.ToInt64(userCount));
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				return (-1);
			}
		}
	
	}
}
