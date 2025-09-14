namespace ChariotSanzzo.Database {
	public class DBFanfare {
	// M. Member Variables
		public string	Message	{get; set;} = "Not Found...";
		public string	GifLink	{get; set;} = "https://ih1.redbubble.net/image.2563904783.2932/raf,360x360,075,t,fafafa:ca443f4786.jpg";

	// C. Constructors
		public	DBFanfare() {}
		public	DBFanfare(string message, string glink) {
			this.Message = message;
			this.GifLink = glink;
		}
	}
}
