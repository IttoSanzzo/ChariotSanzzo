using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ChariotSanzzo.Components.MusicQueue {
	public class QueueCollection {
	// 0. Member Variables
		private int				_length {get; set;} = 0;
		private TrackQueue[]	_queues	{get; set;} = new TrackQueue[0];

	// 1. Constructors
		public QueueCollection() {
			Console.WriteLine("Queue Collection Constructed!\n\n");
		}

	// 2. Utils
		private void			CreateQueue(long serverId, LavalinkGuildConnection conn, DiscordChannel? chat) {
			Console.WriteLine($"CreateEntered {this._length}");
			if (QueueExist(serverId) == true)
				return ;
			TrackQueue[] temp = new TrackQueue[this._length + 1];
			int	i = -1;
			while (++i < this._length)
				temp[i] = this._queues[i];
			temp[i] = new TrackQueue(serverId, conn, chat);
			this._queues = temp;
			this._length += 1;
			Console.WriteLine($"CreateExit {this._length}");
		}
		public void			DropQueue(long serverId) {
			if (QueueExist(serverId) == false)
				return ;
			TrackQueue[] temp = new TrackQueue[this._length - 1];
			int	i = -1;
			while (++i < this._length)
				if (this._queues[i]._serverId != serverId)
					temp[i] = this._queues[i];
			this._queues = temp;
			this._length -= 1;
			Console.WriteLine($"DropExit {this._length}");
		}
		public TrackQueue	GetQueue(long serverId, LavalinkGuildConnection conn, DiscordChannel? chat) {
			Console.WriteLine("GetEntered");
			for (int i = 0; i < this._length; i++)
				if (this._queues[i]._serverId == serverId)
					return (this._queues[i]);
			this.CreateQueue(serverId, conn, chat);
			Console.WriteLine($"GetExit {this._length}\n\n");
			return (this._queues[this._length - 1]);
		}

	// 3. Utils
		public bool QueueExist(long serverId) {
			for (int i = 0; i < this._length; i++)
				if (this._queues[i]._serverId == serverId)
					return (true);
			Console.WriteLine($"{this._length} lenght: Queue does not already exist!");
			return (false);
		}
	}
}