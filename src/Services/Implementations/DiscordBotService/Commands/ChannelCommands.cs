using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands {
	[Command("Channel"), Description("Channel Commands")]
	public class ChannelCommands {

		// M. Member Variables
		private const int midMessageTime = 800;

		// 0. Core
		[Command("Clean"), Description("Cleans the chat by the given ammount of messages.")]
		[RequireGuild]
		public static async Task Clean(CommandContext ctx, [Description("How many messages should be clean")] long count) {
			await ctx.DeferResponseAsync();

			// 0. Check
			var retMss = new DiscordWebhookBuilder();
			if (ctx.Member!.Permissions.HasPermission(DiscordPermission.ManageMessages) == false) {
				retMss.Content = $"You do not have the permission to do this.";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(10000);
				await ctx.DeleteResponseAsync();
				return;
			} else if (count < 1 || count > 100000) {
				retMss.Content = $"Invalid clean count value!";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(10000);
				await ctx.DeleteResponseAsync();
				return;
			}
			var chariotMember = await ctx.Guild!.GetMemberAsync(1070103829934260344);
			if (chariotMember.Permissions.HasPermission(DiscordPermission.ManageMessages) == false) {
				retMss.Content = $"Chariot does not have the permission to \"Manage Messages\".";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(10000);
				await ctx.DeleteResponseAsync();
				return;
			}
			retMss.Content = $"Excluding {count} messages!";
			var anchor = await ctx.EditResponseAsync(retMss);

			// 1. Search
			var mssList = ctx.Channel.GetMessagesBeforeAsync(anchor.Id, (int)count);
			// 2. Core
			try {
				await ctx.Channel.DeleteMessagesAsync(mssList);
			} catch (Exception ex) {
				Program.WriteException(ex);
			}
			// 3. Closing
			await Task.Delay(1500);
			await ctx.DeleteResponseAsync();
		}
	}
}
