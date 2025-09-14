using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ChariotSanzzo.Components.MusicComponent {
	public class QueueCollection {
	// M. Member Variables
		internal int					Length	{get; set;} = 0;
		internal TrackQueue[]	Queues	{get; set;} = new TrackQueue[0];

	// C. Constructors
		public QueueCollection() {
			Program.WriteLine($"Queue Collection Constructed!");
		}

	// 0. Core
		public void					CreateQueue(ulong serverId, DiscordMember owner, LavalinkGuildConnection conn, DiscordChannel? chat) {
			if (QueueExist(serverId) == true)
				return ;
			TrackQueue[] temp = new TrackQueue[this.Length + 1];
			int	i = -1;
			while (++i < this.Length)
				temp[i] = this.Queues[i];
			temp[i] = new TrackQueue(serverId, owner, conn, chat, this);
			this.Queues = temp;
			this.Length += 1;
		}
		public void					DropQueue(ulong serverId) {
			if (QueueExist(serverId) == false)
				return ;
			TrackQueue[] temp = new TrackQueue[this.Length - 1];
			int	i = -1;
			while (++i < this.Length)
				if (this.Queues[i].ServerId != serverId)
					temp[i] = this.Queues[i];
			this.Queues = temp;
			this.Length -= 1;
		}
		public TrackQueue		GetQueue(ulong serverId, DiscordMember owner, LavalinkGuildConnection conn, DiscordChannel? chat) {
			for (int i = 0; i < this.Length; i++)
				if (this.Queues[i].ServerId == serverId)
					return (this.Queues[i]);
			this.CreateQueue(serverId, owner, conn, chat);
			return (this.Queues[this.Length - 1]);
		}
		public TrackQueue?	GetQueueUnsafe(ulong serverId) {
			for (int i = 0; i < this.Length; i++)
				if (this.Queues[i].ServerId == serverId)
					return (this.Queues[i]);
			return (null);
		}
		
	// U. Utils
		public bool					QueueExist(ulong serverId) {
			for (int i = 0; i < this.Length; i++)
				if (this.Queues[i].ServerId == serverId)
					return (true);
			return (false);
		}
	}
}