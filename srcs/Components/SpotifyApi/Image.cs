using Newtonsoft.Json;

namespace ChariotSanzzo.Components.SpotifyApi {
	public class Image {
	// JM. Json Member Variables
		[JsonProperty("height")]
		public int		Height	{get; set;}
		[JsonProperty("url")]
		public string?	Url		{get; set;} = null;
		[JsonProperty("width")]
		public int		Width	{get; set;}
	}
}
