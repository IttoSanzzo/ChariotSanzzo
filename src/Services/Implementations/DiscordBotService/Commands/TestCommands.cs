using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands {
	[Command("testing")]
	[Description("Test Commands")]
	public class TestCommands {
		[Command("test1"), DefaultGroupCommand]
		[Description("Tests if Chariot is online and running correctly.")]
		public static async Task TestAsync(CommandContext ctx) {
			await ctx.RespondAsync("Hello World!");
		}
		[Command("test2")]
		[Description("Just Because.")]
		public static async Task Testing(CommandContext ctx, [Description("Really, just anything.")] string Anything) {
			await ctx.DeferResponseAsync();
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hello, {Anything}."));
		}
		[Command("sendChariotGjalLinkTest")]
		[Description("Tests if Chariot is able to connect.")]
		public static async Task SendChariotGjalLinkTest(CommandContext ctx) {
			await ctx.DeferResponseAsync();
			await ctx.Channel.SendMessageAsync("GjallarhornCall\nJustTesting\nUmu Umu\nhehehehe\nhahahaha");
			await ctx.DeleteResponseAsync();
		}
	}
}
