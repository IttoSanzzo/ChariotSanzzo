using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Database {
	public class DBUser {
	// M. Member Variables
		public string	UserName	{get; set;} = "";
		public string	ServerName	{get; set;} = "";
		public ulong	ServerId	{get; set;} = 0;
	
	// C. Constructors
		public	DBUser() {}
		public	DBUser(InteractionContext ctx) {
			UserName = ctx.User.Username;
			ServerName = ctx.Guild.Name;
			ServerId = ctx.Guild.Id;
		}
	}
}
