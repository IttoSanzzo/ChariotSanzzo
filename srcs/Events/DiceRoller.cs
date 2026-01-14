using ChariotSanzzo.Components.DiceRoller;
using ChariotSanzzo.Components.GuildSettings;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Events {
	public static class STPDiceRoller {
		public static async Task DiceRoller(DiscordClient sender, MessageCreateEventArgs ctx) {
			if (ctx.Author.IsBot)
				return;

			DiceExpression diceExpression = new(ctx.Message.Content);
			if (diceExpression.IsValid == false)
				return;
			var (wasSuccess, embed) = diceExpression.RollForDiscord();
			if (wasSuccess)
				await ctx.Message.RespondAsync(embed);
		}
	}
}
