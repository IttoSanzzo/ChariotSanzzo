using System.Drawing;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("SFX", "SFX Commands")]
	public class SFXCommands : ApplicationCommandModule {
	// 0. Member Variables
		private static ulong			_NasaId			{get; set;} = 982651003134419057;
		private static ulong			_GjallarhornId	{get; set;} = 1273070668451418122;
		private static DiscordChannel?	_LogChannel		{get; set;} = ((Program.Client != null) ? Program.Client.GetChannelAsync(1273347333429399633).Result : null);
		
	// 1. Core
		[SlashCommand("play", "Play the given track link as SFX.")]
		public async Task Play(InteractionContext ctx, [Option("link", "The link for the SFX")] string sfxLink) {
			await ctx.DeferAsync();
			await ctx.DeleteResponseAsync();
			if (await SFXCommands.CheckGajPresence(ctx.Channel) == false)
				return ;

		// 0. Check and Run
			if (sfxLink.Contains("https://") == false) {
				await SFXCommands.SendGjallarhornMessageAsync(ctx.Channel.Id, DiscordColor.Red, "Invalid SFX Link!");
				return ;
			}
			await SFXCommands.SendGjallarhornCommAsync("Play", ctx.Channel.Id, ctx.Member.VoiceState.Channel.Id, sfxLink);
		}
		[SlashCommand("stop", "Play the given track link as SFX.")]
		public async Task Stop(InteractionContext ctx) {
			await ctx.DeferAsync();
			await ctx.DeleteResponseAsync();
			if (await SFXCommands.CheckGajPresence(ctx.Channel) == false)
				return ;
			await SFXCommands.SendGjallarhornCommAsync("Exit", ctx.Channel.Id, ctx.Member.VoiceState.Channel.Id, "");
		}

	// 2. Gjallarhorn Miscs
		private static async Task	SendGjallarhornCommAsync(string cmd, ulong channelId, ulong voiceChannelId, string args) {
			await SFXCommands.PostGjallarhornCall($"{cmd}\n" + $"{channelId}\n" + $"{voiceChannelId}\n" + $"{args}");
		}
		private static async Task	SendGjallarhornMessageAsync(ulong channelId, DiscordColor color, string cmd) {
			await SFXCommands.PostGjallarhornCall("Message\n" + $"{channelId}\n" +  $"{color}\n"+ cmd);
		}
		private static async Task<DiscordMessage?>	PostGjallarhornCall(string args) {
			if (Program.Client == null || SFXCommands._LogChannel == null) {
				Console.WriteLine("Error: PostGjallarhornCall Fail.");
				return (null);
			}
			return (await SFXCommands._LogChannel.SendMessageAsync("GjallarhornCall\n" + args));
		}
		private static bool	CheckGjallarhornInChannel(DiscordMember[] members) {
			for (int i = 0; i < members.Length; i++)
				if (members[i].Id == SFXCommands._GjallarhornId)
					return (true);
			return (false);
		}
		private static async Task<bool> CheckGajPresence(DiscordChannel channel) {
			if (channel.Guild.Members.ContainsKey(SFXCommands._GjallarhornId) == false || SFXCommands.CheckGjallarhornInChannel(channel.Users.ToArray()) == false) {
				Console.WriteLine("GjarNULL");
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
		private static async Task	DelMssTimerAsync(int seconds, DiscordMessage message) /* Deletes the given discord message past the given seconds */ {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
			return ;
		}
	}
}