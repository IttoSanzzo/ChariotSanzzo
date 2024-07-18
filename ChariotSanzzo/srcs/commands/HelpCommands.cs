using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Commands {
	public class HelpCommands : BaseCommandModule {
		[Command("help")]
		public async Task HelpCore(CommandContext ctx) {
			Console.WriteLine("Help Launch");
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder {
    			Title = "",
    			Color = DiscordColor.Aquamarine,
    			Description = "test -> Runs a simple \"Check if i'm online\" check.\ntea -> Tea?\nAlbinaLink -> Returns the link to the main Albina page.",
			};
			await ctx.Channel.SendMessageAsync(embed: embed);
			Console.WriteLine("Help Out");
		}
	}
}