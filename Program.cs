using ChariotSanzzo.Infrastructure;
using ChariotSanzzo.Infrastructure.Config;
using DotNetEnv;

namespace ChariotSanzzo {
	public class Program {
		public static HttpClient HttpClient { get; set; } = new();
		public static bool IsInDocker { get; set; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));
		public static WebApplication App { get; set; } = null!;

		static void Main(string[] args) {
			if (Program.IsInDocker)
				Env.Load();
			else
				Env.Load(".env");
			var SafeStartToken = Environment.GetEnvironmentVariable("SAFE_START_TOKEN") ?? throw new Exception("No safe start token received");
			if (SafeStartToken != "SafeStart") {
				Console.WriteLine("Not initalized by Core, aborting...");
				return;
			}
			ConfigLoader.Load();

			App = AppFactory.CreateApp(args);
			Program.ColorWriteLine(ConsoleColor.Blue, $"{DiscordBotConfig.Name} is up!");
			App.Run();
		}

		public static void ColorWriteLine(ConsoleColor color, string text) {
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write($"{DiscordBotConfig.Name}: ");
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ResetColor();
		}
		public static void WriteLine(string text) {
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write($"{DiscordBotConfig.Name}: ");
			Console.ResetColor();
			Console.WriteLine(text);
		}
		public static void Write(string text) {
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write($"{DiscordBotConfig.Name}: ");
			Console.ResetColor();
			Console.Write(text);
		}
		public static void WriteException(Exception ex) {
			Program.ColorWriteLine(ConsoleColor.Yellow, ex.ToString());
		}
	}
}
