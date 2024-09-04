using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("Testing", "Test Commands")]
	public class TestCommands : ApplicationCommandModule {
		[SlashCommand("test1", "Tests if Chariot is online and running correctly.")]
		public async Task Test(InteractionContext ctx) {
			await ctx.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Hello World!"));
		}
		[SlashCommand("test2", "Just Because.")]
		public async Task Testing(InteractionContext ctx, [Option("Anything", "Really, just anything.")] string arg) {
			await ctx.DeferAsync();
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hello, {arg}."));
		}
		[SlashCommand("sendChariotGjalLinkTest", "Tests if Chariot is able to connect.")]
		public async Task SendChariotGjalLinkTest(InteractionContext ctx) {
			await ctx.DeferAsync();
			await ctx.Channel.SendMessageAsync("GjallarhornCall\nJustTesting\nUmu Umu\nhehehehe\nhahahaha");
			await ctx.DeleteResponseAsync();
		}
	}
}
