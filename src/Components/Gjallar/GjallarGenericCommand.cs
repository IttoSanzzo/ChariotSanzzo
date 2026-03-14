namespace ChariotSanzzo.Components.Gjallar {
	public class GjallarGenericCommand {
		public string Command { get; set; } = "";
		public string TargetBot { get; set; } = "";
		public string UserId { get; set; } = "";
		public string GuildId { get; set; } = "";
		public string? TrackUrl { get; set; } = null;
		public string? ChannelId { get; set; } = null;
		public int? TrackPosition { get; set; } = null;
		public long? SeekSeconds { get; set; } = null;
		public bool Priority { get; set; } = true;
	}
	public class GjallarPostBody {
		public string? TrackUrl { get; set; } = null;
		public string Command { get; set; } = "";
		public string UserId { get; set; } = "";
		public string Color { get; set; } = "";
		public string? Message { get; set; } = "";
		public string? ChannelId { get; set; } = null;
	}
	public class GjallarCallResult(string command, bool wasSuccess = false, string? errorMessage = null) {
		public string Command { get; set; } = command;
		public bool WasSuccess { get; set; } = wasSuccess;
		public string? ErrorMessage { get; set; } = errorMessage;
	}
}
