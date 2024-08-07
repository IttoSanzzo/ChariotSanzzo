using Newtonsoft.Json;

namespace ChariotSanzzo.Components.SpotifyApi {
	public class Image {
		[JsonProperty("height")]
		public int		_height {get; set;}
		[JsonProperty("url")]
		public string?	_url {get; set;} = null;
		[JsonProperty("width")]
		public int		_width {get; set;}
	}
}
