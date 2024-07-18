using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using STPlib;

namespace ChariotSanzzo.commands {
	public class TestCommand : BaseCommandModule {
		[Command("test")]
		public async Task HelloWorld(CommandContext ctx) {
			await ctx.Channel.SendMessageAsync("Hello World!");
		}
		[Command("tea")]
		public async Task TeaTest(CommandContext ctx) {
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder {
    			Title = "Tea? 🍵",
    			Color = DiscordColor.Gold,
    			Description = $"How about a tea now{((ctx.Member != null) ? ", " + ctx.Member.DisplayName + "?" : "?")}",
				ImageUrl = "https://i.pinimg.com/originals/33/2b/67/332b67a92964df5e0676e7ccde1f4750.jpg"
			};
			await ctx.Channel.SendMessageAsync(embed: embed);
		}

		[Command("add")]
		public async Task AddTest(CommandContext ctx) {await ctx.Channel.SendMessageAsync($"Sums \"First Argument\" plus \"Second Argument\".");	}
		[Command("add")]
		public async Task AddTest(CommandContext ctx, string? arg1, string? arg2) {
			if (string.IsNullOrWhiteSpace(arg1) || string.IsNullOrWhiteSpace(arg2))
				Console.WriteLine("[Was Empty]");
			await ctx.Channel.SendMessageAsync($"{arg1.StoI() + arg2.StoI()}");
		}

		[Command("embed")]
		public async Task EmbedMessage(CommandContext ctx) {
			var mss = new DiscordMessageBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Black)
					.WithTitle("Embed Test")
					.WithImageUrl("https://i.pinimg.com/originals/33/2b/67/332b67a92964df5e0676e7ccde1f4750.jpg")
					.WithDescription($"Done by {((ctx.Member != null) ? ctx.Member.DisplayName : "you")}."));
			await ctx.Channel.SendMessageAsync(mss);
		}

		[Command("toss")]
		public async Task TossCoin(CommandContext ctx) {
			var	coin = new Random();
			var res = coin.Next(1, 3);
			var	embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.Yellow)
				.WithTitle("Result..:")
				.WithDescription($"The winner was... {(((res == 1) ? "Heads": "Tails"))}!")
				.WithImageUrl($"{((res == 1)
					? "https://m.media-amazon.com/images/I/71goga5omHL._AC_SX679_.jpg" 
					: "https://m.media-amazon.com/images/I/71NHDGRgDrL._AC_SX679_.jpg" )}");
			await ctx.Channel.SendMessageAsync(embed: embed);
		}
	}
}
