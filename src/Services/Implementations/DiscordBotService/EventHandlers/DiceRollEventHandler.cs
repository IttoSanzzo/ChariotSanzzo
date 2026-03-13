using ChariotSanzzo.Components.DiceRoller;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Services {
	public class DiceRollEventHandler : IEventHandler<MessageCreatedEventArgs> {
		public async Task HandleEventAsync(DiscordClient client, MessageCreatedEventArgs ctx) {
			if (ctx.Author.IsBot)
				return;

			DiceExpression diceExpression = new(ctx.Message.Content);
			if (diceExpression.IsValid == false)
				return;
			var (_, embed) = diceExpression.RollForDiscord();
			await ctx.Message.RespondAsync(embed);
		}
	}
}
