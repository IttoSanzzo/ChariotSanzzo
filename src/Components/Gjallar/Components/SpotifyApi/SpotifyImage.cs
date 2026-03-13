using Newtonsoft.Json;

namespace ChariotSanzzo.Components.Gjallar {
	public class SpotifyImage {
		// JM. Json Member Variables
		[JsonProperty("height")]
		public int Height { get; set; }
		[JsonProperty("url")]
		public string? Url { get; set; } = null;
		[JsonProperty("width")]
		public int Width { get; set; }
	}
}
