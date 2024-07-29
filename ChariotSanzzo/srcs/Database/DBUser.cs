using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Database {
	public class DBUser {
		public string	_userName {get; set;} = "";
		public string	_serverName {get; set;} = "";
		public ulong	_serverId {get; set;} = 0;
		public	DBUser() {}
		public	DBUser(InteractionContext ctx) {
			_userName = ctx.User.Username;
			_serverName = ctx.Guild.Name;
			_serverId = ctx.Guild.Id;
		}
	}
}
