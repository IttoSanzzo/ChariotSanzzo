using System.Drawing;
using ChariotSanzzo.Events;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("SFX", "SFX Commands")]
	public class SFXCommands : ApplicationCommandModule {
	// 0. Member Variables
		private static ulong			_NasaId			{get; set;} = 982651003134419057;
		private static ulong			_GjallarhornId	{get; set;} = 1273070668451418122;
		private static DiscordChannel?	_LogChannel		{get; set;} = ((Program.Client != null) ? Program.Client.GetChannelAsync(1273347333429399633).Result : null);
		
	// 1. Core
		[SlashCommand("play", "Plays the given track link as SFX.")]
		public async Task Play(InteractionContext ctx, [Option("link", "The link for the SFX")] string sfxLink) {
			await ctx.DeferAsync();
			await ctx.DeleteResponseAsync();
			if (await SFXCommands.CheckGajPresence(ctx.Channel) == false)
				return ;

		// 0. Check and Run
			string	username = ctx.Member.Nickname;
			if (string.IsNullOrEmpty(username))
				username = ctx.User.Username;
			if (sfxLink.Contains("https://") == false) {
				await SFXCommands.PostGjallarhornCall("Message", username, ctx.Member.AvatarUrl, DiscordColor.Red, $"ChatChannelId:\t{ctx.Channel.Id}\n" + "Message:\tInvalid SFX Link!");
				return ;
			}
			if (sfxLink.Contains("youtube.com/playlist?") == true
				|| sfxLink.Contains("spotify.com/playlist") == true
				|| (sfxLink.Contains("soundcloud.com/") == true && sfxLink.Contains("/sets") == true)) {
				await SFXCommands.PostGjallarhornCall("Message", username, ctx.Member.AvatarUrl, DiscordColor.Red, $"ChatChannelId:\t{ctx.Channel.Id}\n" + "Message:\tPlaylists are forbidden in SFX!");
				return ;
			}
			else if (ctx.Member.VoiceState == null) {
				await SFXCommands.PostGjallarhornCall("Message", username, ctx.Member.AvatarUrl, DiscordColor.Red, $"ChatChannelId:\t{ctx.Channel.Id}\n" + "Message:\tFirst, enter a Voice Channel!");
				return ;
			}
			await SFXCommands.SendGjallarhornCommAsync("Play", username, ctx.Member.AvatarUrl, DiscordColor.Aquamarine, ctx.Channel.Id, ctx.Member.VoiceState.Channel.Id, $"Link:\t{sfxLink}");
		}
		[SlashCommand("stop", "Plays the given track link as SFX.")]
		public async Task Stop(InteractionContext ctx) {
			await ctx.DeferAsync();
			await ctx.DeleteResponseAsync();
			if (await SFXCommands.CheckGajPresence(ctx.Channel) == false)
				return ;
			string	username = ctx.Member.Nickname;
			if (string.IsNullOrEmpty(username))
				username = ctx.User.Username;
			await SFXCommands.SendGjallarhornCommAsync("Exit", username, ctx.Member.AvatarUrl, DiscordColor.Black, ctx.Channel.Id, ctx.Member.VoiceState.Channel.Id, "");
		}
		[SlashCommand("index", "Plays the track in the given index position from the current server Music Queue.")]
		public async Task Play(InteractionContext ctx, [Option("index", "The index for the track")] long index) {
			await ctx.DeferAsync();
			await ctx.DeleteResponseAsync();
			if (await SFXCommands.CheckGajPresence(ctx.Channel) == false)
				return ;

		// 0. Check and Run
		string	username = ctx.Member.Nickname;
			if (string.IsNullOrEmpty(username))
				username = ctx.User.Username;
			if (ctx.Member.VoiceState == null) {
				await SFXCommands.PostGjallarhornCall("Message", username, ctx.Member.AvatarUrl, DiscordColor.Red, $"ChatChannelId:\t{ctx.Channel.Id}\n" + "Message:\tFirst, enter a Voice Channel!");
				return ;
			}
			var queue = MusicCommands.QColle.GetQueueUnsafe(ctx.Guild.Id);
			if (queue == null) {
				await SFXCommands.PostGjallarhornCall("Message", username, ctx.Member.AvatarUrl, DiscordColor.Red, $"ChatChannelId:\t{ctx.Channel.Id}\n" + "Message:\tThere is no queue currently in your server!");
				return ;
			}
			LavalinkTrack? track = queue.GetIndexTrack((int)(index - 1));
			if (track == null) {
				await SFXCommands.PostGjallarhornCall("Message", username, ctx.Member.AvatarUrl, DiscordColor.Red, $"ChatChannelId:\t{ctx.Channel.Id}\n" + "Message:\tQueue does not have that index!");
				return ;
			}
			string sfxLink = track.Uri.AbsoluteUri;
			await SFXCommands.SendGjallarhornCommAsync("Play", username, ctx.Member.AvatarUrl, DiscordColor.Aquamarine, ctx.Channel.Id, ctx.Member.VoiceState.Channel.Id, $"Link:\t{sfxLink}");
		}

	// 2. Gjallarhorn Miscs
	private static async Task<DiscordMessage?>	PostGjallarhornCall(string type, string username, string iconUrl, DiscordColor color, string args) {
			Program.WriteLine("Sending SFX Query");
			if (Program.Client == null || SFXCommands._LogChannel == null) {
				Program.WriteLine("Error: PostGjallarhornCall Fail.");
				return (null);
			}
			var embed = new DiscordEmbedBuilder();
			embed.WithTitle(type);
			embed.WithFooter($"By: {username}", iconUrl);
			embed.WithColor(color.Value);
			embed.WithDescription(args);
			return (await SFXCommands._LogChannel.SendMessageAsync(embed.Build()));
		}
		private static async Task				SendGjallarhornCommAsync(string type, string username, string userIcon, DiscordColor color, ulong channelId, ulong voiceChannelId, string extras) {
			await SFXCommands.PostGjallarhornCall(type, username, userIcon, color,
													$"ChatChannelId:\t{channelId}\n"
													+ $"VoiceChannelId:\t{voiceChannelId}\n"
													+ $"{extras}");
		}
		private static bool						CheckGjallarhornInChannel(DiscordMember[] members) {
			for (int i = 0; i < members.Length; i++)
				if (members[i].Id == SFXCommands._GjallarhornId)
					return (true);
			return (false);
		}
		private static async Task<bool>			CheckGajPresence(DiscordChannel channel) {
			if (channel.Guild.Members.ContainsKey(SFXCommands._GjallarhornId) == false || SFXCommands.CheckGjallarhornInChannel(channel.Users.ToArray()) == false) {
				Program.WriteLine("GjarNULL");
				var embedNoGaj = new DiscordEmbedBuilder();
				embedNoGaj.WithColor(DiscordColor.Red);
				if (channel.Guild.Members.ContainsKey(SFXCommands._GjallarhornId) == false)
					embedNoGaj.WithDescription("Gjallarhorn is not in the server, but you can add it througth this [Invite Link](https://discord.com/oauth2/authorize?client_id=1273070668451418122&permissions=3149056&integration_type=0&scope=bot).");
				else if (SFXCommands.CheckGjallarhornInChannel(channel.Users.ToArray()) == false)
					embedNoGaj.WithDescription("Gjallarhorn does not meet the requirements to be in this channel.");
				await SFXCommands.DelMssTimerAsync(20, await channel.SendMessageAsync(embedNoGaj.Build()));
				return (false);
			}
			return (true);
		}
		private static async Task				DelMssTimerAsync(int seconds, DiscordMessage message) /* Deletes the given discord message past the given seconds */ {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
			return ;
		}
	}
}
