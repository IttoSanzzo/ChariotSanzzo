using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ChariotSanzzo.Components.MusicQueue {
	public class QueueCollection {
	// 0. Member Variables
		private int				_length {get; set;} = 0;
		private TrackQueue[]	_queues	{get; set;} = new TrackQueue[0];

	// 1. Constructors
		public QueueCollection() {
			Program.WriteLine("Queue Collection Constructed!\n\n");
		}

	// 2. Utils
		public void			CreateQueue(ulong serverId, LavalinkGuildConnection conn, DiscordChannel? chat) {
			if (QueueExist(serverId) == true)
				return ;
			TrackQueue[] temp = new TrackQueue[this._length + 1];
			int	i = -1;
			while (++i < this._length)
				temp[i] = this._queues[i];
			temp[i] = new TrackQueue(serverId, conn, chat, this);
			this._queues = temp;
			this._length += 1;
		}
		public void			DropQueue(ulong serverId) {
			if (QueueExist(serverId) == false)
				return ;
			TrackQueue[] temp = new TrackQueue[this._length - 1];
			int	i = -1;
			while (++i < this._length)
				if (this._queues[i]._serverId != serverId)
					temp[i] = this._queues[i];
			this._queues = temp;
			this._length -= 1;
		}
		public TrackQueue	GetQueue(ulong serverId, LavalinkGuildConnection conn, DiscordChannel? chat) {
			for (int i = 0; i < this._length; i++)
				if (this._queues[i]._serverId == serverId)
					return (this._queues[i]);
			this.CreateQueue(serverId, conn, chat);
			return (this._queues[this._length - 1]);
		}
		public TrackQueue?	GetQueueUnsafe(ulong serverId) {
			for (int i = 0; i < this._length; i++)
				if (this._queues[i]._serverId == serverId)
					return (this._queues[i]);
			return (null);
		}

	// 3. Utils
		public bool QueueExist(ulong serverId) {
			for (int i = 0; i < this._length; i++)
				if (this._queues[i]._serverId == serverId)
					return (true);
			return (false);
		}
	}
}