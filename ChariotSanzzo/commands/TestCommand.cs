using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace ChariotSanzzo.commands {
	public class TestCommand : BaseCommandModule {
		[Command("test")]
		public async Task FirstCommand(CommandContext ctx) {
			await ctx.Channel.SendMessageAsync("Hello World!");
		}
	}
	public class TeaCommand : BaseCommandModule {
		[Command("tea")]
		public async Task FirstCommand(CommandContext ctx) {
			if (ctx.Member != null)
				await ctx.Channel.SendMessageAsync($"```How about a tea now, {ctx.Member.DisplayName}?\nhttps://i.pinimg.com/originals/33/2b/67/332b67a92964df5e0676e7ccde1f4750.jpg```");
			else
				await ctx.Channel.SendMessageAsync("How about a tea now?");
		}
	}
}
