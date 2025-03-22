using ChariotSanzzo.Database;
using DSharpPlus.Entities;
using STPlib;

namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceSet {
	// M. Member Variables
		private static Random	Random	{get; set;} = new Random();
		public string			DString	{get; set;} = "";
		public string			DEquat	{get; set;} = "";
		public int				DTimes	{get; set;} = 0;
		public int				DCount	{get; set;} = 0;
		public int				DSides	{get; set;} = 0;
		public int				DAdvan	{get; set;} = 0;
		public bool				DManual	{get; set;} = false;
		private bool			RError	{get; set;} = false;
		private DiscordEmbed?	Embed	{get; set;} = null;
		private DiceStr?		CStr	{get; set;} = null;
		private DiceRes[]?		DResul	{get; set;} = null;

	// C. Constructors
		public		DiceSet(int dTimes, int dCount, int dSides, int dAdvan, string dEquat) {
			this.DManual = true;
			this.DTimes = dTimes;
			this.DCount = dCount;
			this.DSides = dSides;
			this.DAdvan = dAdvan;
			this.DEquat = dEquat;
			this.FormStringFromParams();
			this.CheckDice();
			this.RunDice();
			this.GenFinalEmbed();
		}
		public		DiceSet(string dLine) {
			this.DString = dLine;
			this.CheckDice();
			this.FormStringFromParams();
			this.RunDice();
			this.GenFinalEmbed();
		}

	// 0. Checking & Running
		public bool		CheckDice() {
			if (this.CStr == null)
				this.CStr = new DiceStr(this);
			return (this.CStr.SCheck);
		}
		private bool	RunDice() {
			if ((this.DTimes <= 0 || this.DTimes > 200)
				|| (this.DCount <= 0 || this.DCount > 1000)
				|| (this.DSides <= 0 || this.DSides > 1000)
				|| (this.DTimes * this.DCount) > 10000) {
				this.RError = true;
				return (false);
			}
			this.DResul = new DiceRes[this.DTimes];
			for (int i = 0; i < this.DTimes; i++)
				this.DResul[i] = new DiceRes(this);
			return (true);
		}

	// 1. Returning
		private void		GenFinalEmbed() {
			var embed = new DiscordEmbedBuilder();
			if (this.RError || this.CStr != null && this.CStr.SCheck == false) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription($"{((this.DTimes > 200 || this.DCount > 1000 || this.DSides > 1000 || (this.DTimes * this.DCount) > 10000) ?
										"That dice set is WAAAY too big." : "Dice Set was invalid.")}");
				this.Embed = embed.Build();
				return ;
			}
			string	context = "";
			if (this.DResul != null)
				for (int i = 0; i < this.DTimes; i++) {
					context += this.DResul[i].RString;
					if (i < this.DTimes - 1)
						context += "\n";
				}
			else
				context = "So... what happened? No results found.";
			if (context.Length > 4000)
				context = "That Dice Set was too big for embeds.";
			embed.WithColor(DiscordColor.DarkBlue);
			embed.WithDescription(context);
			if (this.DTimes > 3)
				embed.WithTitle($"Dice Set..: {this.DString}");
			this.Embed = embed.Build();
		}
		public DiscordEmbed	GetEmbed() {
			if (this.Embed != null)
				return (this.Embed);
			var	embed = new DiscordEmbedBuilder() {
						Color = DiscordColor.Red,
						Description = "No embed to retrieve."};
			return (embed.Build());
		}
		public string		GetFinalString() {
			string	context = "";

			if (this.DResul != null)
				for (int i = 0; i < this.DTimes; i++) {
					context += this.DResul[i].RString;
					if (this.DTimes > 3)
						context += $"Dice Set..: {this.DString}";
					if (i < this.DTimes - 1)
						context += "\n";
				}
			else if ((this.DTimes * this.DCount) > 30000)
				context = "That dice set is WAAAY too big.";
			else
				context = "So... what happened?";
			if (context.Length > 1950)
				context = "That Dice Set was too big for text messages.";
			return (context);
		}

	// U. Utils
		public DiscordEmbed	GetSetConfigEmbed() {
			var embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Gold,
				Title = "Dice Set:",
				Description = $"String.......: {this.DString}\n" +
								$"Equation..: {this.DEquat}\n" +
								$"Times.......: {this.DTimes}\n" +
								$"Count.......: {this.DCount}\n" +
								$"Sides........: {this.DSides}\n" +
								$"Removes..: {this.DAdvan}\n"};
			return (embed.Build());
		}
		private string		FormStringFromParams() {
			this.DString = "";
			if (this.DTimes > 1)
				this.DString += $"{this.DTimes}#";
			if (this.DCount > 1)
				this.DString += $"{this.DCount}";
			this.DString += $"d{this.DSides}";
			if (this.DAdvan > 0)
				this.DString += $"A{this.DAdvan}";
			else if (this.DAdvan < 0)
				this.DString += $"a{this.DAdvan * -1}";
			if (this.DEquat != "")
				this.DString += $" ({this.DEquat})";
			return (this.DString);
		}
		public int			TriggerNatDice() {
			if (this.CStr == null || this.CStr.SCheck == false || this.RError == true)
				return (0);
			if (this.DTimes == 1 && ((this.DCount - int.Abs(this.DAdvan)) == 1)
				&& (this.DResul != null && (this.DResul[0].RValues[0] == 1 || this.DResul[0].RValues[0] == 20)))
				return (this.DResul[0].RValues[0]);
			return (0);
		}
		public async Task<DiscordEmbed>	GetEventEmbed(int diceres, string name) {
			DBEngine	engine = new DBEngine();
			DBFanfare	dicef = await engine.GetDiceFanfareAsync(diceres);
			string[]	mssprts = dicef.Message.Split('&');
			if (mssprts.Length != 1) {
					dicef.Message = mssprts[0];
					dicef.Message += name;
					dicef.Message += mssprts[1];
			}
			var	embed = new DiscordEmbedBuilder() {
					Color = DiscordColor.Black,
					Title = dicef.Message,
					ImageUrl = dicef.GifLink
					};
			return (embed.Build());
		}
	}
}
