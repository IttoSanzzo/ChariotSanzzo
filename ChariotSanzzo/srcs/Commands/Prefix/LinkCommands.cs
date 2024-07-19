using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Commands.Prefix {
	public class LinkCommands : BaseCommandModule {
		[Command("AlbinaLink")]
		public async Task AlbinaNotionLinkTask(CommandContext ctx) {
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder {
    			Title = "🔗Heres' the Main Albina Link..:",
    			Color = DiscordColor.Gold,
    			Description = "https://albinarpg.notion.site/SISTEMA-ALBINA-080f1dcc12624a07ba4f83296d4119f5?pvs=4",
				ImageUrl= "https://albinarpg.notion.site/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2Ff27a3abd-0d96-4119-a3e6-b30c4419a489%2FRPG_Saga_Albina_20200919_152059.jpg?table=block&id=080f1dcc-1262-4a07-ba4f-83296d4119f5&spaceId=75f35b14-f973-44fe-b8d6-4f1f1c6e7f27&width=250&userId=&cache=v2"
			};
			await ctx.Channel.SendMessageAsync(embed: embed);
		}
	}
}
