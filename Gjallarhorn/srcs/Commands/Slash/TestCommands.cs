using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Gjallarhorn.Commands.Slash {
	[SlashCommandGroup("Testing", "Test Commands")]
	public class TestCommands : ApplicationCommandModule {
		[SlashCommand("test1", "Tests if Gjallarhorn is online and running correctly.")]
		public async Task Test(InteractionContext ctx) {
			await ctx.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Hello World!"));
		}
		[SlashCommand("chariotGjalLinkTest", "Tests if Chariot is able to connect.")]
		public async Task ChariotGjalLinkTest(InteractionContext ctx) {
			await ctx.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("CONNECTED!"));
		}
	}
}
