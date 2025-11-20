namespace ChariotSanzzo.Components.PresenceSentinel {
	public interface IPresenceTracker {
		Task InitializeAsync(PresenceRegistry registry);
	}
}