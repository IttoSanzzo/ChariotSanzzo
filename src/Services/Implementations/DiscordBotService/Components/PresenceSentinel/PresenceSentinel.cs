using System.Text;
using System.Text.Json;
using ChariotSanzzo.Services.Components;
using ChariotSanzzo.Utils;
using DSharpPlus;

namespace ChariotSanzzo.Services {
	public class UserPresenceState(ulong userId) {
		public ulong UserId { get; set; } = userId;
		public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
		public Dictionary<string, object> Attributes { get; } = [];

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

	public class PresenceRegistry(DiscordClient client) {
		private readonly DiscordClient Client = client;
		public readonly Dictionary<ulong, UserPresenceState> Users = [];

		public async Task<UserPresenceState> GetOrCreate(ulong userId) {
			if (!Users.TryGetValue(userId, out var state)) {
				state = new UserPresenceState(userId);
				var user = await Client.GetUserAsync(userId);
				if (user != null) {
					state.Set("user.username", user.Username);
					state.Set("user.avatarUrl", user.AvatarUrl);
				}
				Users[userId] = state;
			}
			return state;
		}
	}
	public static class PresenceSentinel {
		public static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };
		private static DiscordClient Client = null!;
		public static PresenceRegistry Registry { get; set; } = null!;
		private static readonly List<IPresenceTracker> Trackers = [];
		private static VoicePresenceResolver VoiceResolver = null!;

		public static void AddTracker(IPresenceTracker tracker) {
			Trackers.Add(tracker);
		}
		public static void Initialize(DiscordClient client) {
			Client = client;
			Registry = new(Client);
			VoiceResolver = new(Registry, Client);

			var trackerTypes = typeof(Program).Assembly
					.GetTypes()
					.Where(t =>
						!t.IsAbstract &&
						!t.IsInterface &&
						typeof(IPresenceTracker).IsAssignableFrom(t)
					);
			foreach (var trackerType in trackerTypes) {
				if (Activator.CreateInstance(trackerType) is IPresenceTracker tracker) {
					Trackers.Add(tracker);
				} else throw new Exception("Failed to initiallize a IPresenceTracker.");
			}
		}
		public static async Task ForceStalkEverythingForUserAsync(ulong userId) {
			await ForceResolveVoiceAsync(userId);
		}
		public static async Task<string> GetUserPresenceJsonStringAsync(ulong userId) {
			var state = await Registry.GetOrCreate(userId);
			var nested = state.Attributes.ToNestedJson();
			return JsonSerializer.Serialize(nested, SerializerOptions);
		}
		public static async Task<string> GetForceStalkeUserJsonStringAsync(ulong userId) {
			await ForceStalkEverythingForUserAsync(userId);
			return await GetUserPresenceJsonStringAsync(userId);
		}
		public static async Task PostPresenceUpdate(ulong userId) {
			await Program.HttpClient.PostAsync(
				LinkData.GetChariotApiFullAddress($"/live/users/{userId}/presence-sentinel-socket"),
				new StringContent(
					await GetUserPresenceJsonStringAsync(userId),
					Encoding.UTF8,
					"application/json"
				)
			);
		}
		public static async Task<UserPresenceState?> ForceResolveVoiceAsync(ulong userId) {
			return await VoiceResolver.ForceResolveUserVoiceStateAsync(userId);
		}
	}
}
