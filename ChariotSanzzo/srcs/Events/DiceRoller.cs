using ChariotSanzzo.Components.DiceRoller;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Events {
	public static class STPDiceRoller {
		public static async Task DiceRoller(DiscordClient sender, MessageCreateEventArgs ctx) {
			if (ctx.Author.IsBot)
				return ;
			if (ctx.Message.Content == "dice") {// TODO DiceRoller
				// await ctx.Message.RespondAsync("Oka");
				DiceSet dSet = new DiceSet {
					_dString = "10#10d20a2 (*5+15)",
					_dEquat = "*5+15",
					_dTimes = 10,
					_dCount = 10,
					_dSides = 20,
					_dAdvan = 2
				};
				if (dSet.RunDice() != true) {
					await ctx.Message.RespondAsync("Dice Set was unnable to run.");
					return ;
				}
				await ctx.Message.RespondAsync(dSet.GetFinalEmbed());
				// await ctx.Message.RespondAsync(dSet.GetFinalString());
			}
		}
	}
}
