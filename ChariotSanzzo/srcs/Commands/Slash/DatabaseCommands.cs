using ChariotSanzzo.Database;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("DataBase", "DataBase Commands.")]
	public class DatabaseCommands : ApplicationCommandModule {
		[SlashCommand("StoreUser", "Stores a user.")]
		public async Task StoreUser(InteractionContext ctx) {
			await ctx.DeferAsync();
			var	DbEngine = new DBEngine();
			var	userInfo = new DBUser(ctx);
			if (await DbEngine.StoreUserAsync(userInfo))
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Succesfully Stored!"));
			else
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error... something went wrong..."));
		}

		[SlashCommand("GetUser", "Retrieves a user from the database.")]
		public async Task GetUser(InteractionContext ctx, [Option("UserName", "Enter the target username. Default's for your own.")] string userName = "") {
			await ctx.DeferAsync();
			var	DBEngine = new DBEngine();
			var	retrieve = await DBEngine.GetUserAsync(string.IsNullOrEmpty(userName) ? ctx.User.Username : userName, ctx.Guild.Id);
			if (retrieve.Item1 == true && retrieve.Item2 != null) {
				var embed = new DiscordEmbedBuilder() {
					Color = DiscordColor.DarkBlue,
					Title = $"{retrieve.Item2.UserName}'s Profile",
					Description = $"Server: {retrieve.Item2.ServerName}\n" +
									$"ServerId: {retrieve.Item2.ServerId}",
				};
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			}
			else
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Error... there was no user entry found for {(string.IsNullOrEmpty(userName) ? ctx.User.Username : userName)} here."));
		}
	}
}
