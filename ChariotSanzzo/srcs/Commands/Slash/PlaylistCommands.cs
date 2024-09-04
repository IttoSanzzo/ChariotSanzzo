using ChariotSanzzo.Components.MusicComponent;
using ChariotSanzzo.Config;
using ChariotSanzzo.Database;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using Npgsql;
using STPlib;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("music_playlist", "General Playlist Slash Commands.")]
	public class PlaylistCommands : ApplicationCommandModule {
	// C. Constructor
		static PlaylistCommands() {}

	// 0. Core
		[SlashCommand("show", "Shows your saved playlists.")]
		public static async Task show(InteractionContext ctx, [Option("index", "Index for the playlist to be shown.")] long listindex = 0, [Option("User", "The user from which I should retrieve the list.")] DiscordUser? givenUser = null) {
			await ctx.DeferAsync();
			var		embed = new DiscordEmbedBuilder();
			DiscordUser dUser = ctx.User;
			if (givenUser != null)
				dUser = givenUser;
			long	entriesCount = await DBEngine.GetAllRowsCountAsync("data.cm_playlists", $"userid = {dUser.Id}");
			var		entriesValue = await PlaylistCommands.GetUserPlaylistsAsync((long)dUser.Id);
			if (entriesCount == 0 || entriesValue == null) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("There where no playlists saved to be shown!");
				await DelMssTimerAsync(20, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
				return ;
			}
			else if (listindex < 0 || listindex > entriesCount) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("A playlist does not exist in that index!");
				await DelMssTimerAsync(20, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
				return ;
			}
			if (listindex == 0) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithThumbnail(dUser.AvatarUrl);
				embed.WithTitle($"{dUser.Username}'s Playlists");
				string description = "";
				for (int i = 0; i < entriesValue.Length; i++) {
					Uri? uri = null;
					if (entriesValue[i][1].Contains("https://") == true || entriesValue[i][1].Contains("http://") == true)
						uri = new Uri(entriesValue[i][1]);
					if (uri != null)
						switch (uri.Host) { // Chooses favicon based on the plataform
							case ("youtube.com"):
							case ("www.youtube.com"):
								description += "<:YoutubeIcon:1269684532777320448> ";
							break;
							case ("soundcloud.com"):
								description += "<:SoundCloudIcon:1269685534737825822> ";
							break;
							case ("open.spotify.com"):
								description += "<:SpotifyIcon:1269685522528211004> ";
							break;
						}
					description += $"` {i + 1} ` -> {entriesValue[i][0]}\n";
				}
				embed.WithDescription(description);
				await DelMssTimerAsync(30, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
				return ;
			} else {
			// Error Search
				var llInstace = ctx.Client.GetLavalink();
				if (!llInstace.ConnectedNodes.Any()) {
					embed.WithColor(DiscordColor.Red);
					embed.WithDescription("The connection is not stablished!");
					await DelMssTimerAsync(20, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
					return ;
				}
				var node = llInstace.ConnectedNodes.Values.First();
				LavalinkLoadResult	searchQuery = await node.Rest.GetTracksAsync(entriesValue[listindex - 1][1], LavalinkSearchType.Plain);
				if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
					embed.WithColor(DiscordColor.Red);
					embed.WithDescription("Failed to find proper music using the given query.");
					await DelMssTimerAsync(20, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
					return ;
				}
			
			// Search Playlists
				LavalinkTrack[]	musicTracks = searchQuery.Tracks.ToArray();
				ChariotTrack[] tracks = new ChariotTrack[musicTracks.Length];
				for (int i = 0; i < musicTracks.Length; i++)
					tracks[i] = new ChariotTrack(musicTracks[i], ctx.User);
			// Sending Response
				await DelMssTimerAsync(90, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbeds(GetPlaylistEmbed(tracks, ctx.User))));
			}
		}
		[SlashCommand("export", "Exports the given playlist to the users database.")]
		public static async Task export(InteractionContext ctx, [Option("Name", "A nickname for the playlist.")] string listname, [Option("Link", "The playlist's link.")] string listlink) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();
			if (await PlaylistCommands.CountNameEntriesAsync((long)ctx.User.Id, listname) != 0) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("There is already a playlist saved with that name!");
				await DelMssTimerAsync(20, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
				return ;
			}
			else {
				if (listlink.Contains("youtube.com/playlist?") == false
						&& listlink.Contains("spotify.com/playlist") == false
						&& (listlink.Contains("soundcloud.com/") == false && listlink.Contains("/sets") == false)) {
					embed.WithColor(DiscordColor.Red);
					embed.WithDescription("The given link does not lead to a valid playlist!");
					await DelMssTimerAsync(20, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
					return ;
				}
			}
			embed.WithColor(DiscordColor.Aquamarine);
			embed.WithDescription("Playlist Saved!");
			await PlaylistCommands.ExportPlaylistAsync((long)ctx.User.Id, ctx.User.Username, listname, listlink);
			await DelMssTimerAsync(20, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
		}
		[SlashCommand("delete", "Deletes permanently a playlist from your database.")]
		public static async Task delete(InteractionContext ctx, [Option("Name", "A the exact name from the playlist to be deleted.")] string listname) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();
			if (await PlaylistCommands.CountNameEntriesAsync((long)ctx.User.Id, listname) == 0) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("There no playlist saved with that name!");
				await DelMssTimerAsync(20, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
				return ;
			}
			embed.WithColor(DiscordColor.Aquamarine);
			if (await PlaylistCommands.DeletePlaylistAsync((long)ctx.User.Id, listname) == true)
				embed.WithDescription("Playlist Deleted!");
			else
				embed.WithDescription("Something went wrong!");
			await DelMssTimerAsync(20, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())));
		}
		[SlashCommand("import", "Imports the selected playlist from the user.")]
		public static async Task import(InteractionContext ctx, [Option("index", "The index from the playlist you want to import.")] long playlistIndex = 0, [Option("User", "The user from which I should retrieve the list.")] DiscordUser? givenUser = null) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;
		
		// 1. Start
			DiscordUser dUser = ctx.User;
			if (givenUser != null)
				dUser = givenUser;
			long	entriesCount = await DBEngine.GetAllRowsCountAsync("data.cm_playlists", $"userid = {dUser.Id}");
			var		entriesValue = await PlaylistCommands.GetUserPlaylistsAsync((long)dUser.Id);
			if (!(playlistIndex >= 1 && playlistIndex <= entriesCount)) {
				var selectEmbed = new DiscordEmbedBuilder();
				selectEmbed.WithColor(DiscordColor.Aquamarine);
				selectEmbed.WithThumbnail(dUser.AvatarUrl);
				selectEmbed.WithTitle($"{dUser.Username}'s Playlists");
				if (entriesCount == -1 || entriesValue == null) {
					selectEmbed.WithColor(DiscordColor.Black);
					selectEmbed.WithDescription("Empty...");
					await DelMssTimerAsync(10, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(selectEmbed.Build())));
					return ;
				}
				string description = "";
				for (int i = 0; i < entriesValue.Length; i++) {
					Uri? uri = null;
					if (entriesValue[i][1].Contains("https://") == true || entriesValue[i][1].Contains("http://") == true)
						uri = new Uri(entriesValue[i][1]);
					if (uri != null)
						switch (uri.Host) { // Chooses favicon based on the plataform
							case ("youtube.com"):
							case ("www.youtube.com"):
								description += "<:YoutubeIcon:1269684532777320448> ";
							break;
							case ("soundcloud.com"):
								description += "<:SoundCloudIcon:1269685534737825822> ";
							break;
							case ("open.spotify.com"):
								description += "<:SpotifyIcon:1269685522528211004> ";
							break;
						}
					description += $"` {i + 1} ` -> {entriesValue[i][0]}\n";
				}
				selectEmbed.WithDescription(description);
				selectEmbed.WithFooter("Send the index from the playlist to be imported.");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(selectEmbed.Build()));
				DiscordMessage reply;
				do {
					var temp = await ctx.Channel.GetNextMessageAsync();
					reply = temp.Result;
				} while(reply.Author.Id != ctx.User.Id);
				playlistIndex = reply.Content.StoI() - 1;
				await reply.DeleteAsync();
				if (playlistIndex < 0 || playlistIndex > entriesValue.Length - 1) {
					var errEmbed = new DiscordEmbedBuilder() {
						Color = DiscordColor.Black,
						Description = "Invalid index."
					};
					await DelMssTimerAsync(10, await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(errEmbed.Build())));
					return ;
				}
			}
			else
				playlistIndex--;
			if (entriesValue != null) {
				var waitembed = new DiscordEmbedBuilder() {
					Color = DiscordColor.Aquamarine,
					Description = "Wait a second, I'm processing it."
				};
				var waitmss = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(waitembed.Build()));
				LavalinkLoadResult	searchQuery = await tools.node.Rest.GetTracksAsync(entriesValue[playlistIndex][1], LavalinkSearchType.Plain);
				var musicTracks = searchQuery.Tracks.ToArray();
				for (int i = 0; i < musicTracks.Length; i++)
					tools.queue.AddTrackToQueue(new ChariotTrack(musicTracks[i], ctx.User));
				var lastEmbed = new DiscordEmbedBuilder();
				lastEmbed.WithColor(tools.queue.Tracks[^1].Color);
				lastEmbed.WithDescription($"{tools.queue.Tracks[^1].Favicon}{ctx.User.Username} imported a playlist! {musicTracks.Length} new tracks!");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(lastEmbed.Build()));
				if (tools.conn.CurrentState.CurrentTrack == null)
					await tools.queue.Conn.PlayAsync(await tools.queue.UseNextTrackAsync());
				await Task.Delay(1000 * 30);
				await ctx.DeleteResponseAsync();
			}
			else
				await ctx.DeleteResponseAsync();
			return ;
		}
	
	// 1. Database ChariotMusicRelated

	// 3. Database Stuff
		public static async Task<bool>			ExportPlaylistAsync(long userid, string username, string listname, string listlink) {
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query = "INSERT INTO data.cm_playlists (userid, username, listname, listlink)\n"
									+ $"VALUES ({userid}, '{username}', '{listname}', '{listlink}')";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var reader = await cmd.ExecuteNonQueryAsync();
					}
				}
				return (true);
			} catch (Exception ex) {
				if (DBEngine.Debug == true)
					Program.WriteLine($"[->ImportPlaylistAsyncError\n{ex.ToString()}\n]");
				return (false);
			}
		}
		public static async Task<bool>			DeletePlaylistAsync(long userid, string listname) {
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query = "DELETE FROM data.cm_playlists\n"
									+ $"WHERE userid = {userid} AND listname = '{listname}'";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var reader = await cmd.ExecuteNonQueryAsync();
					}
				}
				return (true);
			} catch (Exception ex) {
				if (DBEngine.Debug == true)
					Program.WriteLine($"[->ImportPlaylistAsyncError\n{ex.ToString()}\n]");
				return (false);
			}
		}
		public static async Task<string[][]?>	GetUserPlaylistsAsync(long userid) {
			string[][]?	retObj = null;
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query = "SELECT d.listname, d.listlink FROM data.cm_playlists d \n"
									+ $"WHERE userid = {userid}\n"
									+ "ORDER BY id";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var reader = await cmd.ExecuteReaderAsync();
						await reader.ReadAsync();
						retObj = new string[await DBEngine.GetAllRowsCountAsync("data.cm_playlists", $"userid = {userid}")][]; // TODO: function to count rows so it does not go BOOM
						int i = -1;
						do {
							Program.WriteLine("ENTERING ROW " + (i + 1));
							retObj[++i] = new string[2]; // TODO: function to count rows so it does not go BOOM
							retObj[i][0] = await reader.GetFieldValueAsync<string>(0, CancellationToken.None);
							retObj[i][1] = await reader.GetFieldValueAsync<string>(1, CancellationToken.None);
						} while (await reader.ReadAsync() == true);
					}
				}
				Program.WriteLine("Returning");
				return (retObj);
			} catch(Exception ex) {
				if (DBEngine.Debug == true)
					Program.WriteLine($"[->ImportPlaylistAsyncError\n{ex.ToString()}\n]");
				return (null);
			}
		}
		public static async Task<string?>		ImportPlaylistAsync(long userid, int? listindex = 0) {
			string	listlink;
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query = "SELECT d.listlink\n"
									+ "FROM data.cm_playlists d\n"
									+ $"WHERE userid = {userid}\n"
									+ $"OFFSET {listindex}";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var reader = await cmd.ExecuteReaderAsync();
						await reader.ReadAsync();
						listlink = reader.GetString(0);
					}
				}
				return (listlink);
			} catch(Exception ex) {
				if (DBEngine.Debug == true)
					Program.WriteLine($"[->ImportPlaylistAsyncError\n{ex.ToString()}\n]");
				return (null);
			}
		}
		public static async Task<int>			CountNameEntriesAsync(long userid, string listname) {
			int count;
			try {
				using (var conn = new NpgsqlConnection(DBConfig.Conn)) {
					await conn.OpenAsync();
					string query = "SELECT COUNT (*)\n"
									+ "FROM data.cm_playlists\n"
									+ $"WHERE userid = {userid} AND listname = '{listname}'";
					using (var cmd = new NpgsqlCommand(query, conn)) {
						var reader = await cmd.ExecuteReaderAsync();
						await reader.ReadAsync();
						count = reader.GetInt32(0);
					}
				}
				return (count);
			} catch(Exception ex) {
				if (DBEngine.Debug == true)
					Program.WriteLine($"[->ImportPlaylistAsyncError\n{ex.ToString()}\n]");
				return (0);
			}
		}
	
	// E. Miscs
		private static DiscordEmbed[]			GetPlaylistEmbed(ChariotTrack[] tracks, DiscordUser user) {
			Program.WriteLine("GET Playlist QUEUE ENTER");
		// 0. Base Check
			if (tracks.Length == 0) {
				var	errembed = new DiscordEmbedBuilder();
				errembed.WithColor(DiscordColor.Gray);
				errembed.WithDescription("The playlist is empty...");
				return (new DiscordEmbed[1] {errembed.Build()});
			}
		// 1. Core
			var retEmbedArr = new DiscordEmbed[0];
			int i = 0;
			while (i < tracks.Length) {
				var	tempArr = new DiscordEmbed[retEmbedArr.Length + 1];
				for (int j = 0; j < retEmbedArr.Length; j++)
					tempArr[j] = retEmbedArr[j];
				retEmbedArr = tempArr;
				var	embed = new DiscordEmbedBuilder();
				embed.WithColor(DiscordColor.Black);
				if (retEmbedArr.Length == 1) {
					embed.WithTitle($"{user.Username}'s Playlist");
					embed.WithThumbnail(user.AvatarUrl);
				}
				string description = "";
				while (i < tracks.Length && description.Length < 3700) {
					Program.WriteLine($"Entry index [{i}]");
					description += $"{tracks[i].Favicon} ` {i + 1} ` -> {tracks[i].Title}\n";
					i++;
				}
				embed.WithDescription(description);
				retEmbedArr[^1] = embed.Build();
			}
		// 2. Return
			return (retEmbedArr);
		}
		private static async Task				DelMssTimerAsync(int seconds, DiscordMessage message) /* Deletes the given discord message past the given seconds */ {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
			return ;
		}
	}
}
