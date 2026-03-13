using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services {
	public static class CommandErrorEventHandler {
		public static async Task HandleEventAsync(CommandsExtension _, CommandErroredEventArgs ctx) {
			string? timeLeft = null;
			DiscordEmbedBuilder embed;
			if (ctx.Context.Command == null) {
				embed = new DiscordEmbedBuilder()
								.WithColor(DiscordColor.Red)
								.WithDescription("Unknown Command.");
				await ctx.Context.RespondAsync(embed: embed);
				return;
			}
			// if (ctx.Exception is ChecksFailedException exception)
			// foreach (var check in exception.) {
			// var coolDown = (CooldownAttribute)check;
			// timeLeft = coolDown.GetRemainingCooldown(ctx.Context).ToString(@"mm\:ss");
			// }
			if (timeLeft == null)
				embed = new DiscordEmbedBuilder()
								.WithColor(DiscordColor.Red)
								.WithTitle("Error!")
								.WithDescription($"Error using command{((ctx.Context.Command != null)
									? " \"" + ctx.Context.Command.Name + "\""
									: "")}.");
			else
				embed = new DiscordEmbedBuilder()
								.WithColor(DiscordColor.Blue)
								.WithTitle("Please wait for the cooldown to end!")
								.WithDescription($"Command{((ctx.Context.Command != null)
									? " \"" + ctx.Context.Command.Name + "\""
									: "")} in cooldown. Wait {timeLeft} to use again!");
			await ctx.Context.RespondAsync(embed: embed);
		}
	}
}
