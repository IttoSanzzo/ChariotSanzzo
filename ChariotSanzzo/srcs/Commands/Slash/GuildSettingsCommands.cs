using ChariotSanzzo.Components.GuildSettings;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
// -1. Enum
	public enum ChangeOptions {
		[ChoiceName("VipServer")]
		VipServer,
		[ChoiceName("AutoDice")]
		AutoDice,
		[ChoiceName("ReplayDice")]
		ReplayDice,
		[ChoiceName("DiceFanfare")]
		DiceFanfare
	}

// 0. Core
	[SlashCommandGroup("Settings", "Commands for changing and viewing server settings.")]
	public class GuildSettingsCommands : ApplicationCommandModule {
	// 0. Commands
		[SlashCommand("ShowAll", "Shows all the current server settings.")]
		public static async Task	ShowSettings(InteractionContext ctx) {
			await ctx.DeferAsync();
		// 0. Starting
			var sCtx = await GuildSettingsCore.GetGuildSettingsAsync(ctx.Guild.Id);
			var embed = new DiscordEmbedBuilder();
			embed.WithColor(DiscordColor.Purple);
			embed.WithTitle($"{ctx.Guild.Name} Settings");
			embed.WithThumbnail(ctx.Guild.IconUrl);
			embed.WithDescription(sCtx.GetEmbedDescription());
			await ctx.RespondWithEmbedAsync(60, embed);
		}
		[SlashCommand("Change", "Changes a setting in the server.")]
		public static async Task	Change(InteractionContext ctx, [Option("Setting", "Option")]ChangeOptions option, [Option("Value", "If it should be enabled(True) or disabled(False).")]bool value) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();
			if (ctx.Member.Permissions.HasPermission(Permissions.Administrator) == false && (ctx.User.Id != 301498447088058368 && ctx.User.Id != 982651003134419057)) {
				embed.WithColor(DiscordColor.Black);
				embed.WithDescription("User lacks permissions to use this command. Please, ask an Administrator to do it.");
				await ctx.RespondWithEmbedAsync(30, embed);
				return ;
			}

		// 0. Starting
			GuildSettingsContext sCtx = await GuildSettingsCore.GetGuildSettingsAsync(ctx.Guild.Id);
			string description = "```ansi\n[2;34m";
			switch (option) {
				case (ChangeOptions.VipServer):
					if (ctx.User.Id != 301498447088058368 && ctx.User.Id != 982651003134419057) {
						embed.WithColor(DiscordColor.Black);
						embed.WithDescription("User lacks the permission to do that.");
						await ctx.RespondWithEmbedAsync(30, embed);
						return ;
					}
					sCtx.Data.VipServer = value;
					description += "🌟VipServer";
				break;
				case (ChangeOptions.AutoDice):
					sCtx.Data.AutoDice = value;
					description += "🎲AutoDice";
				break;
				case (ChangeOptions.ReplayDice):
					sCtx.Data.ReplayDice = value;
					description += "🔄ReplayDice";
				break;
				case (ChangeOptions.DiceFanfare):
					sCtx.Data.DiceFanfare = value;
					description += "🥳DiceFanfare";
				break;
			}
			await sCtx.SetAsync();
			if (value == true) {
				embed.WithColor(DiscordColor.Green);
				description += " [2;32mwas set on!";
			}
			else {
				embed.WithColor(DiscordColor.Red);
				description += " [2;31mwas set off!";
			}
			embed.WithTitle($"{ctx.Guild.Name} Settings");
			embed.WithThumbnail(ctx.Guild.IconUrl);
			embed.WithDescription(description + "\n```");
			await ctx.RespondWithEmbedAsync(30, embed);
		}
	}
	static class DiscordSendExtensions {
		public static async Task	RespondWithEmbedAsync(this InteractionContext ctx, int seconds, DiscordEmbed embed) {
			var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
			if (message == null)
				return ;
			Thread thread = new Thread(() => WaitForCleaning(seconds, message));
			thread.Start();
		}
		public static async Task	SendAsync(this DiscordEmbedBuilder embed, DiscordChannel channel) {
			await embed.Build().SendAsync(channel);
		}
		public static async Task	SendAsync(this DiscordEmbed embed, DiscordChannel channel) {
			var message = await channel.SendMessageAsync(embed);
			if (message == null)
				return ;
			Thread thread = new Thread(() => WaitForCleaning(30, message));
			thread.Start();
		}
		private static async void	WaitForCleaning(int seconds, DiscordMessage message) {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
		}
	}
}