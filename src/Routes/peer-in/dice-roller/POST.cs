using System.Text.Json;
using ChariotSanzzo.Components.DiceRoller;
using DSharpPlus;
using Microsoft.AspNetCore.Mvc;

namespace ChariotSanzzo.Routes {
	file class Route() : WithFilePath(), IRoute {
		public Delegate Handle => Handler;
		public RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
			=> builder.WithName("PeerIn DiceRoll");
		private static async Task<IResult> Handler(DiscordClient client, [FromBody] DiceExpressionDto dto, HttpRequest request) {
			// request.Headers.TryGetValue("STP-AlbinaUserId", out var albinaUserId);
			request.Headers.TryGetValue("STP-DiscordUserId", out var discordUserIdRaw);

			ulong[] diceChannels = [];
			if (request.Headers.TryGetValue("STP-DiscordChatIds", out var discordChatIdsRaw)) {
				try {
					diceChannels = JsonSerializer.Deserialize<ulong[]>(discordChatIdsRaw.ToString()) ?? [];
				} catch (JsonException) { }
			}

			var diceExpression = dto.ToDiceExpression();
			if (diceExpression.IsValid == false)
				return Results.BadRequest("Invalid dice expression.");
			var results = diceExpression.Roll();
			if (!string.IsNullOrEmpty(discordUserIdRaw) && ulong.TryParse(discordUserIdRaw, out var discordUserId)) {
				var (success, embed) = await results.ToDiscordEmbedAsync(client, discordUserId);
				if (success) {
					foreach (var channelId in diceChannels) {
						_ = client!.SendMessageAsync(
							await client!.GetChannelAsync(channelId),
							embed
						);
					}
				}
			}
			return Results.Ok(new { diceResults = results });
		}
	}
}
