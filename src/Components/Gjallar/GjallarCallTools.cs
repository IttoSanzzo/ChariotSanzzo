using ChariotSanzzo.Infrastructure.Config;
using ChariotSanzzo.Services.Wrappers;
using DSharpPlus.Entities;
using Lavalink4NET.Players;
using Microsoft.Extensions.Options;

namespace ChariotSanzzo.Components.Gjallar {
	public class GjallarCallTools {
		public GjallarPlayer Player { get; set; } = null!;
		public GjallarContext Ctx { get; set; } = null!;
		public ulong GuildId => Player.GuildId;
		public bool IsValid => Player is not null && Ctx is not null;
		private bool IsForDisconnection => Ctx.Command == "Stop" || Ctx.Command == "Disconnect";

		public async Task InitializeAsync(GjallarContext ctx) {
			this.Ctx = ctx;
			if (IsForDisconnection && !Lavalink.PlayerManager.Players.Any(player => player.GuildId == player.GuildId)) {
				this.Player = null!;
				return;
			}
			if (IsForDisconnection) {
				ctx.Command = "Stop";
				ctx.Result.Command = "Stop";
			}
			Player = (await GetGjallarPlayer())!;
		}
		private async Task<GjallarPlayer?> GetGjallarPlayer() {
			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Red };
			if (
				(Ctx.Member.VoiceState is null || Ctx.Member.VoiceState.ChannelId == null)
				&& Ctx.Member.Id != DiscordBotConfig.BotUserId
			) {
				embed.WithDescription("Please, enter a Voice Channel!");
				await Ctx.GTXEmbedTimerAsync(20, embed);
				return null;
			} else if (Ctx.VoiceChannel?.Type != DiscordChannelType.Voice) {
				embed.WithDescription("Please, enter a valid Voice Channel!");
				await Ctx.GTXEmbedTimerAsync(20, embed);
				return null;
			}
			PlayerRetrieveOptions retrieveOptions = new(ChannelBehavior: Ctx.Command switch {
				"Play" => PlayerChannelBehavior.Move,
				"Stop" => PlayerChannelBehavior.None,
				_ => PlayerChannelBehavior.None
			});
			IOptions<GjallarPlayerOptions> options = Options.Create(new GjallarPlayerOptions() {
				DisconnectOnDestroy = true,
				DisconnectOnStop = false,
			});
			PlayerResult<GjallarPlayer> playerResult = await Lavalink.PlayerManager.RetrieveAsync(
				Ctx.Guild.Id,
				Ctx.VoiceChannel.Id,
				PlayerFactory.Create<GjallarPlayer, GjallarPlayerOptions>(properties => {
					return new GjallarPlayer(
						properties,
						Ctx.Guild,
						Ctx.Member,
						Ctx.ChatChannel
					);
				}),
				options,
				retrieveOptions
			);
			if (playerResult.IsSuccess == false) {
				embed.WithDescription("Chariot is not in a channel to perform such action!");
				await Ctx.GTXEmbedTimerAsync(20, embed);
				return null;
			}
			return (Ctx.Command == "Disconnect" || await ExtraChecks(Ctx, playerResult.Player, embed)) ? playerResult.Player : null;
		}
		private static async Task<bool> ExtraChecks(GjallarContext ctx, GjallarPlayer player, DiscordEmbedBuilder embed) {
			if (player.CurrentTrack == null && ctx.Data.VipCall == false && ctx.Command != "Play") {
				switch (ctx.Command) {
					case "Stop":
						embed.WithDescription("There's no music playing to be stopped!");
						break;
					case "Pause":
						embed.WithDescription("There's no music playing to be paused!");
						break;
					case "Loop":
						embed.WithDescription("There's no music playing to be looped!");
						break;
					case "Change":
						embed.WithDescription("There's no music playing to change tracks!");
						break;
					case "Shuffle":
						embed.WithDescription("There's no music playing to shuffle!");
						break;
					case "Reset":
						embed.WithDescription("There's no music playing to clear!");
						break;
					case "Volume":
						embed.WithDescription("There's no music playing to change volume!");
						break;
					case "Index":
						embed.WithDescription("There's no music playing to index!");
						break;
					default:
						embed.WithDescription("Default ExtraChecks error!");
						break;
				}
				await ctx.GTXEmbedTimerAsync(20, embed);
				return false;
			}
			return true;
		}
	}
}
