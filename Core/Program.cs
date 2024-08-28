using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace core {
	internal class Program {
	// 0. Member Variables
		private static bool		_testing			{get; set;} = false;
		private static int		_CurrentPID			{get; set;} = Environment.ProcessId;
		private static Process?	_Lavalink			{get; set;} = null;
		private static Process	_Gjallarhorn		{get; set;} = new Process();
		private static string	_ExecArgs			{get; set;} = "";
		private static Process	_ChariotSanzzo		{get; set;} = new Process();
	// 0.1 Config Member Variables
		private static string	_ArgLavalink		{get; set;} = "true";

	// 1. Main
		static async Task<int> Main(string[] args) {
		// -1. Testing SafeStart
			if (args.Length < 1 || args[0] != "SafeStart") {
				Program.ColorWriteLine(ConsoleColor.Red, "Not initalized using the correct Start.sh, aborting...");
				return (0);
			}
		// 0. Testing Grounds

			if (Program._testing == true)
				return (0);
		// 1. Starting
			Program.ColorWriteLine(ConsoleColor.Green, $"Starting...");
			Console.CancelKeyPress += Program.SigIntHandler;
			var thread = new Thread(new ThreadStart(SocketHandler));
			thread.Start();
			Program._ExecArgs = $"SafeStart {Program._ArgLavalink}";
		
		// 2. Start Processes
			Program._Lavalink = Program.LaunchLavalink().Result;
			Program._Gjallarhorn = Program.LaunchModule("Gjallarhorn", Program._ExecArgs);
			Program._ChariotSanzzo = Program.LaunchModule("ChariotSanzzo", Program._ExecArgs);

		// 3. Checks
			await Task.Delay(1000 * 7);
			if (Program._ChariotSanzzo.HasExited == true || Program._Gjallarhorn.HasExited == true)
				Program.Exit(2, "Error: Couldn't start properly.");

		// E. Closing
			Program.ColorWriteLine(ConsoleColor.Green, $"Running!");
			await Task.Delay(-1);
			return (0);
		}
		private static Process	StartProcess(string command, string path, string exec_args = "") {
			Process proc = new System.Diagnostics.Process();
			proc.StartInfo.FileName = command;
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.CreateNoWindow = false;
			proc.StartInfo.RedirectStandardOutput = false;
			proc.StartInfo.WorkingDirectory = $"../{path}/";
			proc.StartInfo.Arguments = exec_args;
			if (proc.Start() == false)
				Program.Exit(1, $"Aborting: {command} failed to run!");
			return (proc);
		}
		private static Process	LaunchModule(string path, string exec_args = "") {
			Process proc = new System.Diagnostics.Process();
			proc.StartInfo.FileName = "dotnet";
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.CreateNoWindow = false;
			proc.StartInfo.RedirectStandardOutput = false;
			proc.StartInfo.WorkingDirectory = $"../{path}/";
			proc.StartInfo.Arguments = $"run --project ../{path}/ {Program._CurrentPID} {exec_args}";
			if (proc.Start() == false)
				Program.Exit(1, $"Aborting: {path} failed to run!");
			return (proc);
		}
		private static async Task<Process?>	LaunchLavalink() {
			Process? temp = null;
			if (Program._ArgLavalink == "true") {
				temp = StartProcess("pm2", "Lavalink", "del all");
				temp.WaitForExit();
				temp = StartProcess("pm2", "Lavalink", "start ./Start.sh -n LAVA0");
				await Task.Delay(1000);
				Program.ColorWriteLine(ConsoleColor.Green, "Waiting Lavalink to init...");
				await Task.Delay(3000);
			}
			return (temp);
		}
		private static void		Exit(int code, string mss) {
			if (code == 0)
				Program.ColorWriteLine(ConsoleColor.Green, mss);
			else
				Program.ColorWriteLine(ConsoleColor.Red, mss);
			try {
				Program._Gjallarhorn?.Kill(true);
				Program._ChariotSanzzo?.Kill(true);
				if (Program._ArgLavalink == "true") {
					var temp = StartProcess("pm2", "Lavalink", "del all");
					temp.WaitForExit();
				}
			} catch (Exception ex) {
				Program.ColorWriteLine(ConsoleColor.Gray, ex.ToString());
			}
			Environment.Exit(code);
		}

		// 3. Misc
		private static async void SocketHandler() {
		// 0. Setting Up
			string		hostName		= Dns.GetHostName();
			IPHostEntry	localhost		= Dns.GetHostEntryAsync(hostName).Result;
			IPAddress	localIpAddress	= localhost.AddressList[0];
			IPEndPoint	ipEndPoint		= new(localIpAddress, 11365);

		// 1. Building Socket
			using Socket listener = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listener.Bind(ipEndPoint);
			listener.Listen(100);
			while (true) {
				Socket handler = listener.AcceptAsync().Result;

			// 2. Running
				string response = "";
				while (true) {
					byte[]	buffer = new byte[1024];
					int		received = await handler.ReceiveAsync(buffer, SocketFlags.None);
					response = Encoding.UTF8.GetString(buffer, 0, received);
					if (response.Contains("<|EOM|>") == true) {
						response = response.Replace("<|EOM|>", "");
						Program.ColorWriteLine(ConsoleColor.Magenta, $"Socket: Message received \"{response}\"");
						await handler.SendAsync(Encoding.UTF8.GetBytes("<|ACK|>"));
					}
					break;
				}
				await Task.Delay(1000);
				switch (response) {
					case ("Stop ChariotSanzzo"):
						Program.Exit(0, "\rExiting via Stop Command!");
					break;
					case ("Restart ChariotSanzzo"):
						Program._ChariotSanzzo.Kill(true);
						Program._ChariotSanzzo = Program.LaunchModule("ChariotSanzzo", Program._ExecArgs);
					break;
				}
			}
		}
		private static void SigIntHandler(object? sender, ConsoleCancelEventArgs ctx) {
			Program.Exit(0, "\rExiting via SigInt!");
		}
		private static void	ColorWriteLine(ConsoleColor color, string text) {
			Console.ForegroundColor = color;
			Console.WriteLine("Core: " + text);
			Console.ResetColor();
		}
	}
}