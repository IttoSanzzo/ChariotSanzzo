using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.UserCommands;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands {
	[Command("Link"), Description("Command for connecting with other plataforms.")]
	public class LinkCommands {
		public static string ChariotChariotApiSecret { get; set; } = Environment.GetEnvironmentVariable("CHARIOT_CHARIOTAPI_SECRET") ?? throw new InvalidOperationException("CHARIOT_CHARIOTAPI_SECRET not set");

		[Command("albina"), Description("Connects your Chariot to minecraft.")]
		public async Task Test(UserCommandContext ctx, [Description("Your Albina Account's UUID.")] string AlbinaId) {
			await ctx.DeferResponseAsync(ephemeral: true);
			if (!Guid.TryParse(AlbinaId, out var albinaIdGuid)) {
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("AlbinaId is invalid"));
				return;
			}
			var payload = new {
				albinaId = albinaIdGuid
			};
			using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"https://chariotapi.setsu.party/users/{ctx.User.Id}/link/albina");
			requestMessage.Headers.Add("STP-ChariotChariotApiSecret", ChariotChariotApiSecret);
			requestMessage.Content = JsonContent.Create(payload);

			var response = await Program.HttpClient.SendAsync(requestMessage);
			if (response.IsSuccessStatusCode)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Albina account successfully linked."));
			else
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to link Albina account.\nStatus: {(int)response.StatusCode}"));
		}
		[Command("minecraft"), Description("Connects your Chariot to minecraft.")]
		public async Task Test(UserCommandContext ctx, [Description("Your Minecraft Account's UUID.")] string MinecraftId, [Description("Your Minecraft Client Token.")] string MinecraftClientToken) {
			await ctx.DeferResponseAsync(ephemeral: true);
			if (!Guid.TryParse(MinecraftId, out var minecraftIdGuid) || !Guid.TryParse(MinecraftClientToken, out var minecraftClientTokenGuid)) {
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("AlbinaId is invalid"));
				return;
			}
			var payload = new {
				minecraftId = minecraftIdGuid,
				minecraftClientToken = minecraftClientTokenGuid,
			};
			using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"https://chariotapi.setsu.party/users/{ctx.User.Id}/link/minecraft");
			requestMessage.Headers.Add("STP-ChariotChariotApiSecret", ChariotChariotApiSecret);
			requestMessage.Content = JsonContent.Create(payload);

			var response = await Program.HttpClient.SendAsync(requestMessage);
			if (response.IsSuccessStatusCode)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Minecraft Companion successfully linked."));
			else
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to link Minecraft Companion.\nStatus: {(int)response.StatusCode}"));
		}
	}
}
