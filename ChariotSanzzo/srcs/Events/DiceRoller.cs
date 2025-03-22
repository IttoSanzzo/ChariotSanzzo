using ChariotSanzzo.Components.DiceRoller;
using ChariotSanzzo.Components.GuildSettings;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Events {
	public static class STPDiceRoller {
		public static async Task DiceRoller(DiscordClient sender, MessageCreateEventArgs ctx) {
			if (ctx.Author.IsBot)
				return ;
	// TODO: Consertar o problema com o DB
	//		if (await GuildSettingsCore.GetStateAutoDice(ctx.Guild.Id) == false)
	//			return ;
			DiceSet	dSet = new DiceSet(ctx.Message.Content);
			if (dSet.CheckDice() == true) {
				var diceMss = await ctx.Message.RespondAsync(dSet.GetEmbed());
				/*
				TODO: Consertar DB Fanfare Gifs de dados
				if (dSet.TriggerNatDice() != 0 && await GuildSettingsCore.GetStateDiceFanfare(ctx.Guild.Id) != false)
					await diceMss.RespondAsync(await dSet.GetEventEmbed(dSet.TriggerNatDice(), ctx.Author.Username));
				*/
			}
		}
	}
}
