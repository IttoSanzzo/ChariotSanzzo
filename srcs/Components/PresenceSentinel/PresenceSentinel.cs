using System.Text;
using System.Text.Json;
using DSharpPlus;
using STPlib;

namespace ChariotSanzzo.Components.PresenceSentinel {
	public class UserPresenceState(ulong userId) {
		public ulong											UserId		 { get; set; } = userId;
		public DateTime										UpdatedAt		{get; private set;} = DateTime.UtcNow;
		public Dictionary<string, object>	Attributes	{get;} = [];

		public void Set(string key, object value) {
			Attributes[key] = value;
			UpdatedAt = DateTime.UtcNow;
		}
		public T? Get<T>(string key) {
			if (Attributes.TryGetValue(key, out var val))
				return (T)val;
			return default;
		}
	}

	public class PresenceRegistry {
		public readonly Dictionary<ulong, UserPresenceState>	Users = [];

		public UserPresenceState	GetOrCreate(ulong userId) {
			if (!Users.TryGetValue(userId, out var state)) {
				state = new UserPresenceState(userId);
				Users[userId] = state;
			}
			return state;
		}
	}

	public class PresenceSentinel {
		private static readonly string ChariotApiFullAddress	= Environment.GetEnvironmentVariable("CHARIOT_API_FULL_ADDRESS") ?? throw new InvalidOperationException("CHARIOT_API_FULL_ADDRESS not set");
		public readonly JsonSerializerOptions	SerializerOptions = new() {WriteIndented= false};
		public PresenceRegistry	Registry	{get; set;} = new();
		private readonly List<IPresenceTracker>	Trackers = [];
		private VoicePresenceResolver	VoiceResolver = null!;

		public void				AddTracker(IPresenceTracker tracker) {
			Trackers.Add(tracker);
		}
		public async Task	InitializeAsync(DiscordClient client) {
			this.VoiceResolver = new(Registry, client);
			foreach (var tracker in Trackers)
				await tracker.InitializeAsync(Registry);
		}
		public void			 	ForceStalkEverythingForUserAsync(ulong userId) {
			this.ForceResolveVoiceAsync(userId);
		}
		public string			GetUserPresenceJsonStringAsync(ulong userId) {
			var state = this.Registry.GetOrCreate(userId);
			var nested  = state.Attributes.ToNestedJson();
			return JsonSerializer.Serialize(nested, this.SerializerOptions);
		}
		public string			GetForceStalkeUserJsonStringAsync(ulong userId) {
			this.ForceStalkEverythingForUserAsync(userId);
			return GetUserPresenceJsonStringAsync(userId);
		}
		public async Task	PostPresenceUpdate(ulong userId) {
			await Program.HttpClient.PostAsync(
				$"{ChariotApiFullAddress}/live/users/{userId}/presence-sentinel-socket",
				new StringContent(
					this.GetUserPresenceJsonStringAsync(userId),
					Encoding.UTF8,
					"application/json"
				)
			);
		}
		public UserPresenceState? ForceResolveVoiceAsync(ulong userId) {
			return VoiceResolver.ForceResolveUserVoiceStateAsync(userId);
		}
	}
}