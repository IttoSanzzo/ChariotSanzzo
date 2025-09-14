using ChariotSanzzo.Components.MusicComponent;
using ChariotSanzzo.HttpServer;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("SFX", "SFX Commands")]
	public class SFXCommands : ApplicationCommandModule {
	// M. Member Variables
		private static ulong			NasaId			{get; set;} = 982651003134419057;
		private static ulong			GjallarhornId	{get; set;} = 1273070668451418122;
		private static DiscordChannel?	LogChannel		{get; set;} = ((Program.Client != null) ? Program.Client.GetChannelAsync(1273347333429399633).Result : null);
		
	// 0. Core
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
			if (sfxLink.Contains("youtube.com/playlist?") == true
				|| sfxLink.Contains("spotify.com/playlist") == true
				|| (sfxLink.Contains("soundcloud.com/") == true && sfxLink.Contains("/sets") == true)) {
				await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "Message", DiscordColor.Red, "Playlists are forbidden in SFX!"));
				return ;
			}
			else if (ctx.Member.VoiceState == null) {
				await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "Message", DiscordColor.Red, "First, enter a Voice Channel!"));
				return ;
			}
			await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "Play", DiscordColor.Aquamarine, null, sfxLink));
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
			await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "Stop", DiscordColor.Black));
		}
		[SlashCommand("index", "Plays the track in the given index position from the current server Music Queue.")]
		public async Task Index(InteractionContext ctx, [Option("index", "The index for the track")] long index) {
			await ctx.DeferAsync();
			await ctx.DeleteResponseAsync();
			if (await SFXCommands.CheckGajPresence(ctx.Channel) == false)
				return ;

		// 0. Check and Run
			string	username = ctx.Member.Nickname;
			if (string.IsNullOrEmpty(username))
				username = ctx.User.Username;
			if (ctx.Member.VoiceState == null) {
				await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "Message", DiscordColor.Red, "First, enter a Voice Channel!"));
				return ;
			}
			var queue = ChariotMusicCalls.QColle.GetQueueUnsafe(ctx.Guild.Id);
			if (queue == null) {
				await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "Message", DiscordColor.Red, "There is no queue currently in your server!"));
				return ;
			}
			LavalinkTrack? track = queue.GetIndexTrack((int)(index - 1));
			if (track == null) {
				await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "Message", DiscordColor.Red, "Queue does not have that index!"));
				return ;
			}
			string sfxLink = track.Uri.AbsoluteUri;
			await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "Play", DiscordColor.Aquamarine, null, sfxLink));
		}
		[SlashCommand("loop", "Swiches the loop state of the SFX player.")]
		public async Task Loop(InteractionContext ctx) {
			await ctx.DeferAsync();
			await ctx.DeleteResponseAsync();
			if (await SFXCommands.CheckGajPresence(ctx.Channel) == false)
				return ;
			await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "Loop", DiscordColor.Aquamarine, null, null));
		}
		[SlashCommand("ControlPanel", "Returns your SFX ControlPanel link for this Channel.")]
		public async Task ControlPanel(InteractionContext ctx) {
			await ctx.DeferAsync();
			await ctx.DeleteResponseAsync();
			if (await SFXCommands.CheckGajPresence(ctx.Channel) == false)
				return ;
			await ChariotSanzzoHttpServer.GjallarhornGenericCommandPost(SFXCommands.BuildPostBody(ctx, "ControlPanel", DiscordColor.DarkBlue, null, null));
		}

	// 1. Gjallarhorn Miscs
		private static bool									CheckGjallarhornInChannel(DiscordMember[] members) {
			for (int i = 0; i < members.Length; i++)
				if (members[i].Id == 1273070668451418122)
					return (true);
			return (false);
		}
		private static async Task<bool>			CheckGajPresence(DiscordChannel channel) {
			if (channel.Guild.Members.ContainsKey(1273070668451418122) == false || SFXCommands.CheckGjallarhornInChannel(channel.Users.ToArray()) == false) {
				Program.WriteLine("GjarNULL");
				var embedNoGaj = new DiscordEmbedBuilder();
				embedNoGaj.WithColor(DiscordColor.Red);
				if (channel.Guild.Members.ContainsKey(1273070668451418122) == false)
					embedNoGaj.WithDescription("Gjallarhorn is not in the server, but you can add it througth this [Invite Link](https://discord.com/oauth2/authorize?client_id=1273070668451418122&permissions=3149056&integration_type=0&scope=bot).");
				else if (SFXCommands.CheckGjallarhornInChannel(channel.Users.ToArray()) == false)
					embedNoGaj.WithDescription("Gjallarhorn does not meet the requirements to be in this channel.");
				await SFXCommands.DelMssTimerAsync(20, await channel.SendMessageAsync(embedNoGaj.Build()));
				return (false);
			}
			return (true);
		}
		private static async Task						DelMssTimerAsync(int seconds, DiscordMessage message) {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
			return ;
		}
		private static GjallarhornPostBody	BuildPostBody(InteractionContext ctx, string command, DiscordColor color, string? message = null, string? link = null) {
			GjallarhornPostBody body = new() {
				Command = command,
				Color = color.ToString(),
				Message = message,
				TrackUrl = link,
				ChannelId = ctx.Channel.Id.ToString(),
				UserId = ctx.User.Id.ToString(),
			};
			return (body);
		}
	}
}
