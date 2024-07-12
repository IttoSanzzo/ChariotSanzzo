using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace ChariotSanzzo.commands {
	public class AlbinaNotionLink : BaseCommandModule {
		[Command("AlbinaLink")]
		public async Task AlbinaNotionCore(CommandContext ctx) {
			await ctx.Channel.SendMessageAsync("Heres' the Main Albina Link: https://albinarpg.notion.site/SISTEMA-ALBINA-080f1dcc12624a07ba4f83296d4119f5?pvs=4");
		}
	}
}
