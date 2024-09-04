using ChariotSanzzo.Components.DiceRoller;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Events {
	public static class STPDiceRoller {
		public static async Task DiceRoller(DiscordClient sender, MessageCreateEventArgs ctx) {
			if (ctx.Author.IsBot)
				return ;
			DiceSet	dSet = new DiceSet(ctx.Message.Content);
			if (dSet.CheckDice() == true) {
				var diceMss = await ctx.Message.RespondAsync(dSet.GetEmbed());
				if (dSet.TriggerNatDice() != 0)
					await diceMss.RespondAsync(await dSet.GetEventEmbed(dSet.TriggerNatDice(), ctx.Author.Username));
			}
		}
	}
}
