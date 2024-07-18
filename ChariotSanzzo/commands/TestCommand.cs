using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace ChariotSanzzo.commands {
	public class TestCommand : BaseCommandModule {
		[Command("test")]
		public async Task HelloWorld(CommandContext ctx) {
			await ctx.Channel.SendMessageAsync("Hello World!");
		}
		[Command("tea")]
		public async Task TeaTest(CommandContext ctx) {

			if (ctx.Member != null) {
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder {
    				Title = "Tea? 🍵",
    				Color = DiscordColor.Gold,
    				Description = $"How about a tea now, {ctx.Member.DisplayName}?",
					ImageUrl = "https://i.pinimg.com/originals/33/2b/67/332b67a92964df5e0676e7ccde1f4750.jpg"
				};
				await ctx.Channel.SendMessageAsync(embed: embed);
			}
			else
				await ctx.Channel.SendMessageAsync("How about a tea now?");
		}


		[Command("add")]
		public async Task AddTest(CommandContext ctx, string args) {
			await ctx.Channel.SendMessageAsync($"{args[0] + args[1]}");
		}

	}
}
