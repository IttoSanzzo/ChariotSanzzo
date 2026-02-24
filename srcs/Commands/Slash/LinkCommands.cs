using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("Link", "Command for connecting with other plataforms.")]
	public class LinkCommands : ApplicationCommandModule {
		public static string ChariotChariotApiSecret { get; set; } = Environment.GetEnvironmentVariable("CHARIOT_CHARIOTAPI_SECRET") ?? throw new InvalidOperationException("CHARIOT_CHARIOTAPI_SECRET not set");

		[SlashCommand("albina", "Connects your Chariot to minecraft.")]
		public async Task Test(InteractionContext ctx, [Option("AlbinaId", "Your Albina Account's UUID.")] string albinaIdRaw) {
			await ctx.DeferAsync(ephemeral: true);
			if (!Guid.TryParse(albinaIdRaw, out var albinaIdGuid)) {
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
		[SlashCommand("minecraft", "Connects your Chariot to minecraft.")]
		public async Task Test(InteractionContext ctx, [Option("MinecraftId", "Your Minecraft Account's UUID.")] string minecraftIdRaw, [Option("MinecraftClientToken", "Your Minecraft Client Token.")] string minecraftClientTokenRaw) {
			await ctx.DeferAsync(ephemeral: true);
			if (!Guid.TryParse(minecraftIdRaw, out var minecraftIdGuid) || !Guid.TryParse(minecraftClientTokenRaw, out var minecraftClientTokenGuid)) {
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
