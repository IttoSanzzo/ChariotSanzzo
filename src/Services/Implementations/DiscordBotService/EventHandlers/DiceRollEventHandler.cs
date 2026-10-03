using ChariotSanzzo.Components.DiceRoller;
using ChariotSanzzo.Infrastructure.Config;
using ChariotSanzzo.Utils;
using DSharpPlus;
using DSharpPlus.EventArgs;
using PeerApiAuth;

namespace ChariotSanzzo.Services {
	public class DiceRollEventHandler : IEventHandler<MessageCreatedEventArgs> {
		public async Task HandleEventAsync(DiscordClient client, MessageCreatedEventArgs ctx) {
			if (ctx.Author.IsBot && ctx.Author.Id != DiscordWidgetBotConfig.BotId)
				return;

			DiceExpression diceExpression = new(ctx.Message.Content);
			if (diceExpression.IsValid == false)
				return;
			var results = diceExpression.Roll();
			if (results.WasSuccess)
				_ = SendRollResultsToAlbinaVtt(results, ctx.Channel.Id, ctx.Author.Id);
			var (_, embed) = await results.ToDiscordEmbedAsync(client, ctx.Author.Id);
			await ctx.Message.RespondAsync(embed);
		}

		private async Task SendRollResultsToAlbinaVtt(DiceResults results, ulong channelId, ulong userId) {
			await PeerApiHttpClient.PostAsync(
				LinkData.GetAlbinaApiFullAddress($"/peer-in/vtt-input/dice-roller/{channelId}/{userId}"),
				results
			);
		}
	}
}
