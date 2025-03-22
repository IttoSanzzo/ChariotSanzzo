using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using ChariotSanzzo.Database;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChariotSanzzo.Commands.Prefix {
	public class TestCommands : BaseCommandModule {
		[Command("test")]
		[Aliases("hello", "HelloWorld")]
		[Description("Tests if Chariot is online and running correctly.")]
		public async Task Test(CommandContext ctx) {
			await ctx.Message.RespondAsync("<:Chariot:1263663980497604659>Hello World!");
		}
		
		[Command("tea")]
		[Description("Pause for tea.")]
		public async Task Tea(CommandContext ctx) {
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder {
    			Title = "Tea? 🍵",
    			Color = DiscordColor.Gold,
    			Description = $"How about a tea now{((ctx.Member != null) ? ", " + ctx.Member.DisplayName + "?" : "?")}",
				ImageUrl = "https://i.pinimg.com/originals/33/2b/67/332b67a92964df5e0676e7ccde1f4750.jpg"
			};
			await ctx.Channel.SendMessageAsync(embed: embed);
		}

		[Command("embed")]
		[Description("Just for testing ends.")]
		public async Task Embed(CommandContext ctx) {
			var mss = new DiscordMessageBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Black)
					.WithTitle("Embed Test")
					.WithImageUrl("https://i.pinimg.com/originals/33/2b/67/332b67a92964df5e0676e7ccde1f4750.jpg")
					.WithDescription($"Done by {((ctx.Member != null) ? ctx.Member.DisplayName : "you")}."));
			await ctx.Channel.SendMessageAsync(mss);
		}

		[Command("inter")]
		[Description("Just for testing ends.")]
		public async Task Inter(CommandContext ctx) {
			await ctx.Channel.SendMessageAsync("Oh...");
			var interactivity = Program.Client.GetInteractivity();

			var mssToRet = await interactivity.WaitForMessageAsync(mss => mss.Content != "");
			if (mssToRet.Result.Content == "Hello")
				await ctx.Channel.SendMessageAsync("YES.");
			else
				await ctx.Channel.SendMessageAsync("NO.");
		}
	
		[Command("cool")]
		[Description("Just for testing ends.")]
		[Cooldown(2, 10, CooldownBucketType.Global)]
		public async Task Coll(CommandContext ctx) {
			await ctx.Channel.SendMessageAsync("Colldown test!");
		}
	
		[Command("stopthechariot")]
		[Aliases("stop")]
		public async Task Stop(CommandContext ctx) {
			if (ctx.User.Username == "ittosanzzo" || ctx.User.Username == "nasasanzzo")
				await ctx.Message.RespondAsync("Stopping the Chariot!");
			// 0. Setting Up
				string		hostName		= Dns.GetHostName();
				IPHostEntry	localhost		= await Dns.GetHostEntryAsync(hostName);
				IPAddress	localIpAddress	= localhost.AddressList[0];
				IPEndPoint	ipEndPoint		= new(localIpAddress, 11365);


			// 1. Building Socket
				using Socket client = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				await client.ConnectAsync(ipEndPoint);

			// 2. Running
				while (true) {
					await client.SendAsync(Encoding.UTF8.GetBytes("Stop ChariotSanzzo<|EOM|>"), SocketFlags.None);
					client.Shutdown(SocketShutdown.Both);
					break;
				}
		}
		[Command("restartthechariot")]
		[Aliases("restart")]
		public async Task Restart(CommandContext ctx, [RemainingText]string botName = "") {
			if ((ctx.User.Username == "ittosanzzo" || ctx.User.Username == "nasasanzzo") && (botName == "chariot" || botName == "gjallarhorn" || botName == "")) {
				if (botName == "")
					botName = "All The Bots";
				await ctx.Message.RespondAsync($"Restarting the {botName}!");
			}
			else
				return ;
			// 0. Setting Up
				string		hostName		= Dns.GetHostName();
				IPHostEntry	localhost		= await Dns.GetHostEntryAsync(hostName);
				IPAddress	localIpAddress	= localhost.AddressList[0];
				IPEndPoint	ipEndPoint		= new(localIpAddress, 11365);


			// 1. Building Socket
				using Socket client = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				await client.ConnectAsync(ipEndPoint);

			// 2. Running
				while (true) {
					await client.SendAsync(Encoding.UTF8.GetBytes($"Restart {botName}<|EOM|>"), SocketFlags.None);
					client.Shutdown(SocketShutdown.Both);
					break;
				}
		}

		[Command("testfan")]
		public async Task TestFan(CommandContext ctx) {
			DBEngine engine = new DBEngine();
			DBFanfare dicef = await engine.GetDiceFanfareAsync(20);
			await ctx.Channel.SendMessageAsync("Testing");
			Program.WriteLine($"[\n{dicef.Message}\n{dicef.GifLink}\n]");
		}
	}
}
