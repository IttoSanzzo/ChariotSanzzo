using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using Gjallarhorn.Components;

namespace Gjallarhorn.Events {
	public static class ChariotConn {
		// -1. Struct
			public struct t_tools {
				public ulong					serverId	{get; set;}
				public LavalinkExtension		llInstace	{get; set;}
				public LavalinkNodeConnection	node		{get; set;}
				public LavalinkGuildConnection	conn		{get; set;}
			}

		// 0. Members Variables
		static ulong	_GjallarhornId	{get; set;} = 1273070668451418122;

		// 1. Core Event

	// Temporary
		
	// Miscs
		private static async Task	DelMssTimerAsync(int seconds, DiscordMessage message) /* Deletes the given discord message past the given seconds */ {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
			return ;
		}
	}
}
