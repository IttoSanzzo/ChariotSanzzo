using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Infrastructure.Extensions {
	public static class DiscordExtensions {
		public static async Task RespondWithEmbedAsync(this CommandContext ctx, int seconds, DiscordEmbed embed) {
			var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
			if (message == null)
				return;
			Thread thread = new(() => WaitForCleaning(seconds, message));
			thread.Start();
		}
		public static async Task SendAsync(this DiscordEmbedBuilder embed, DiscordChannel channel) {
			await embed.Build().SendAsync(channel);
		}
		public static async Task SendAsync(this DiscordEmbed embed, DiscordChannel channel) {
			var message = await channel.SendMessageAsync(embed);
			if (message == null)
				return;
			Thread thread = new(() => WaitForCleaning(30, message));
			thread.Start();
		}
		private static async void WaitForCleaning(int seconds, DiscordMessage message) {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
		}
	}
}
