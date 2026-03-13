using ChariotSanzzo.Components.DiceRoller;
using DSharpPlus;
using Microsoft.AspNetCore.Mvc;

namespace ChariotSanzzo.Routes {
	file class Route() : WithFilePath(), IRoute {
		public Delegate Handle => Handler;
		public RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
			=> builder.WithName("Minecraft Dice Roller");
		private static async Task<IResult> Handler(DiscordClient client, [FromBody] DiceExpressionDto dto, HttpRequest request) {
			request.Headers.TryGetValue("STP-DiscordUserId", out var discordUserIdRaw);

			var diceExpression = dto.ToDiceExpression();
			if (diceExpression.IsValid == false)
				return Results.BadRequest("Invalid dice expression.");
			var results = diceExpression.Roll();
			if (!string.IsNullOrEmpty(discordUserIdRaw) && ulong.TryParse(discordUserIdRaw, out var discordUserId)) {
				var (success, embed) = await results.ToDiscordEmbedAsync(client, discordUserId);
				if (success) {
					await client!.SendMessageAsync(
						await client!.GetChannelAsync(756752942085963847),
						embed
					);
				}
			}
			return Results.Ok(new { diceResults = results });
		}
	}
}
