using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using System.Diagnostics;

namespace Gjallarhorn.Commands.Prefix {
	public class TestCommands : BaseCommandModule {
		[Command("test")]
		[Aliases("hello", "HelloWorld")]
		[Description("Tests if Chariot is online and running correctly.")]
		public async Task Test(CommandContext ctx) {
			Program.WriteLine("Test Command Run");
			await ctx.Channel.SendMessageAsync("Hello World!");
		}
		[Command("chariotGjalLinkTest")]
		[Description("Tests if Chariot is able to connect.")]
		public async Task chariotGjalLinkTest(CommandContext ctx) {
			await ctx.Channel.SendMessageAsync("chariotGjalLinkTest");
		}
	}
}
