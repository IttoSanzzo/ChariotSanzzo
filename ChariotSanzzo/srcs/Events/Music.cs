using System.Diagnostics;
using System.Drawing;
using ChariotSanzzo.Commands.Slash;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace ChariotSanzzo.Events {
	public static class Music {
		public static async Task PlayNext(LavalinkGuildConnection conn, TrackFinishEventArgs ctx) {
			if (ctx.Reason != TrackEndReason.Finished)
				return ;
			var	queue = MusicCommands.QColle.GetQueue((long)ctx.Player.Guild.Id, conn, null);
			var	toPlayNow = queue.UseNextTrack();
			if (toPlayNow == null) {
				if (queue._chat != null) {
					var	embed = new DiscordEmbedBuilder() {
						Color = DiscordColor.Purple,
						Description = "There are no tracks left to play next."
					};
					await queue._chat.SendMessageAsync(embed: embed);
				}
				return ;
			}
			await queue._conn.PlayAsync(toPlayNow);
			return ;
		}
		/*
		public static async Task NowPlaying(LavalinkGuildConnection conn, TrackStartEventArgs ctx) {
			var	queue = MusicCommands.QColle.GetQueue((long)ctx.Player.Guild.Id, conn, null);
			if (queue._loop == 1)
				return ;
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Purple,
				Description = $"_**Now Playing:**_ {ctx.Track.Title}\n" +
										$"_**Length:**_ {ctx.Track.Length}\n" +
										$"_**Author:**_ {ctx.Track.Author}\n" +
										$"_**URL:**_ {ctx.Track.Uri}"
			};
			if (ctx.Track.Uri.ToString().Contains("youtube.com") == true)
				embed.WithImageUrl($"https://img.youtube.com/vi/{ctx.Track.Uri.ToString().Substring(32)}/maxresdefault.jpg");
			if (queue._chat != null)
				await queue._chat.SendMessageAsync(embed: embed);
		}
		*/
	}
}