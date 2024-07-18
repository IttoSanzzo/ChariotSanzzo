using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Events {
	public static class STPCommandErrored {
		public static async Task CmdErrTask(CommandsNextExtension sender, CommandErrorEventArgs ctx) {
			string?				timeLeft = null;
			DiscordEmbedBuilder	embed;

			if (ctx.Exception is ChecksFailedException exception)
				foreach (var check in exception.FailedChecks) {
					var coolDown = (CooldownAttribute)check;
					timeLeft = coolDown.GetRemainingCooldown(ctx.Context).ToString(@"mm\:ss");
				}
			if (timeLeft == null)
				embed = new DiscordEmbedBuilder()
								.WithColor(DiscordColor.Red)
								.WithTitle("Error!")
								.WithDescription($"Error using command{((ctx.Command != null)
									? " \"" + ctx.Command.Name + "\""
									: "")}.");
			else
				embed = new DiscordEmbedBuilder()
								.WithColor(DiscordColor.Blue)
								.WithTitle("Please wait for the cooldown to end!")
								.WithDescription($"Command{((ctx.Command != null)
									? " \"" + ctx.Command.Name + "\""
									: "")} in cooldown. Wait {timeLeft} to use again!");
			await ctx.Context.Message.RespondAsync(embed: embed);
		}
	}
}
