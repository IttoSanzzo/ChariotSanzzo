using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Database {
	public class DBFanfare {
		// 0. Member Variables
		public string	_message {get; set;} = "Not Found...";
		public string	_glink {get; set;} = "https://ih1.redbubble.net/image.2563904783.2932/raf,360x360,075,t,fafafa:ca443f4786.jpg";

		// 1. Condtructors
		public	DBFanfare() {}
		public	DBFanfare(string message, string glink) {
			this._message = message;
			this._glink = glink;
		}
	}
}
