using System.Collections;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("Channel", "Channel Commands")]
	public class ChannelCommands : ApplicationCommandModule {

	// 0. Core
		[SlashCommand("Clean", "Cleans the chat by the given ammount of messages.")]
		public static async Task Clean(InteractionContext ctx, [Option("Count", "How many messages should be clean")] long count) {
			await ctx.DeferAsync();

		// 0. Check
			var retMss = new DiscordWebhookBuilder();
			if (ctx.Member.Permissions.HasPermission(Permissions.ManageMessages) == false) {
				retMss.Content = $"You do not have the permission to do this.";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(10000);
				await ctx.DeleteResponseAsync();
				return ;
			}
			else if (count < 1 || count > 100000) {
				retMss.Content = $"Invalid clean count value!";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(10000);
				await ctx.DeleteResponseAsync();
				return ;
			}
			var chariotMember = await ctx.Guild.GetMemberAsync(1070103829934260344);
			if (chariotMember.Permissions.HasPermission(Permissions.ManageMessages) == false) {
				retMss.Content = $"Chariot does not have the permission to \"Manage Messages\".";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(10000);
				await ctx.DeleteResponseAsync();
				return ;
			}
			retMss.Content = $"Excluding {count} messages!";
			var anchor = await ctx.EditResponseAsync(retMss);

		// 1. Search
			var mssList = await ctx.Channel.GetMessagesBeforeAsync(anchor.Id, (int)count);
		// 2. Core
			try {
				await ctx.Channel.DeleteMessagesAsync(mssList);
			} catch(Exception ex) {
				Program.WriteException(ex);
			}
		// 3. Closing
			await Task.Delay(1500);
			await ctx.DeleteResponseAsync();
		}
		[SlashCommand("CopyChannelMessages", "Copies messages from a channel to another one.")]
		public static async Task CopyChannelMessages(InteractionContext ctx, [Option("Target", "The Id for the channel to paste all messages.")] string targetId, [Option("Count", "How many messages should be copied? (Default for Max 100K)")] long count = 100000) {
			await ctx.DeferAsync();
			DiscordWebhookBuilder retMss = new();
			DiscordChannel targetChannel;
			ulong channelId = 0;

		// 0. Checks
			
			if (ctx.Member.Permissions.HasPermission(Permissions.Administrator) == false) {
				retMss.Content = $"You do not have the permission to do this.";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(10000);
				await ctx.DeleteResponseAsync();
				return ;
			}
			else if (count < 1 || count > 100000) {
				retMss.Content = $"Invalid clean count value!";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(10000);
				await ctx.DeleteResponseAsync();
				return ;
			}
			try {
				channelId = ulong.Parse(targetId);
				targetChannel = await ctx.Client.GetChannelAsync(channelId);
			} catch (Exception ex) {
				Program.WriteException(ex);
				retMss.Content = "Invalid Target Channel Id!";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(3000);
				await ctx.DeleteResponseAsync();
				return ;
			}

		// 1. Core
			retMss.Content = $"Are you sure you want to copy up to `{count}` messages from `{ctx.Channel.Name}` to `{targetChannel.Name}`? (Yes/No)";
			var ctxResponse = await ctx.EditResponseAsync(retMss);
			var response = await ctx.Channel.GetNextMessageAsync(ctx.User);
			if (response.Result.Content != "Yes") {
				await response.Result.DeleteAsync();
				retMss.Content = "Canceling...";
				await ctx.EditResponseAsync(retMss);
				await Task.Delay(3000);
				await ctx.DeleteResponseAsync();
				return ;
			}
			await response.Result.DeleteAsync();
			retMss.Content = $"Then be it. Copying... `(x/x)`";
			ctxResponse = await ctx.EditResponseAsync(retMss);

		// 2. Search
			var mssList = await ctx.Channel.GetMessagesBeforeAsync(ctxResponse.Id, (int)count);
			retMss.Content = $"Then be it. Copying... `(0/{mssList.Count}`)";
			await ctx.EditResponseAsync(retMss);

			DiscordMember		lastMember = (DiscordMember)mssList[mssList.Count - 1].Author;
			DiscordEmbed		headerEmbed = ChannelCommands.GetNewHeader(mssList[mssList.Count - 1]);
			string				sendingMessage = "";
			int					timeCount = 0;

			await targetChannel.SendMessageAsync(headerEmbed);
			for (int i = mssList.Count - 1; i >= 0; i--) {
				timeCount++;
				try {
					if (mssList[i].Content == "" && mssList[i].Embeds.Count == 0 && mssList[i].Attachments.Count == 0)
						continue ;
					else if (lastMember != mssList[i].Author) {
						await targetChannel.SendMessageAsync(sendingMessage);
						sendingMessage = "";
						await Task.Delay(700);
						lastMember = (DiscordMember)mssList[i].Author;
						await targetChannel.SendMessageAsync(ChannelCommands.GetNewHeader(mssList[i]));
						await Task.Delay(700);
					}
					if (mssList[i].Content != "") {
						if (sendingMessage.Length + mssList[i].Content.Length > 1950) {
							await targetChannel.SendMessageAsync(sendingMessage);
							sendingMessage = "";
							await Task.Delay(700);
						}
						else
							sendingMessage += "\n";
						sendingMessage += mssList[i].Content;
					}
					if (mssList[i].Embeds.Count != 0 || mssList[i].Attachments.Count != 0) {
						if (mssList[i].Attachments[0].Url.Substring(18) == "https://tenor.com/")
							continue ;
						if (sendingMessage != "") {
							await targetChannel.SendMessageAsync(sendingMessage);
							sendingMessage = "";
							await Task.Delay(700);
						}
						foreach(DiscordAttachment element in mssList[i].Attachments) {
							DiscordMessageBuilder tempMssBuilder = new();
							tempMssBuilder.Content = element.Url;
							await targetChannel.SendMessageAsync(tempMssBuilder);
							await Task.Delay(700);
						}
						continue ;
					}
				} catch(Exception ex) {
					Program.WriteException(ex);
				}
				await ChannelCommands.UpdateCopyCounterAsync(ctx, retMss, timeCount, mssList.Count);
			}
			await targetChannel.SendMessageAsync(sendingMessage);

		// 3. Closing
			retMss.Content = $"Copied Succesfully!";
			await ctx.EditResponseAsync(retMss);
		}
		private static async Task	UpdateCopyCounterAsync(InteractionContext ctx, DiscordWebhookBuilder retMss, int index, int count) {
			if (index % 10 == 0) {
				double remainingSeconds = (count - index) * 1.2;
				var timesp = TimeSpan.FromSeconds(remainingSeconds);
				string content = $"Then be it. Copying... `({index}/{count})` (About ";
				if (timesp.Hours > 0)
					content += $"{timesp.Hours} Hours, ";
				if (timesp.Minutes > 1)
					content += $"{timesp.Minutes} Minutes and ";
				content += $"{timesp.Seconds} Seconds)";
				retMss.Content = content;
				await ctx.EditResponseAsync(retMss);
			}
		}
		private static DiscordEmbed	GetNewHeader(DiscordMessage message) {
			DiscordEmbedBuilder embed = new();
			embed.WithColor(((DiscordMember)message.Author).Color);
			embed.WithFooter($"{message.Author.Username} - {message.Timestamp}", message.Author.AvatarUrl);
			return (embed);
		}
	}
}