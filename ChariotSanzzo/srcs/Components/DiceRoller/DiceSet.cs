using ChariotSanzzo.Database;
using DSharpPlus.Entities;
using STPlib;

namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceSet {
		// 0. Member Variables
		private static Random	_rRandom {get; set;} = new Random();
		public string			_dString {get; set;} = "";
		public string			_dEquat {get; set;} = "";
		public int				_dTimes {get; set;} = 0;
		public int				_dCount {get; set;} = 0;
		public int				_dSides {get; set;} = 0;
		public int				_dAdvan {get; set;} = 0;
		public bool				_bManual {get; set;} = false;
		private bool			_rError {get; set;} = false;
		private DiscordEmbed?	_embed {get; set;} = null;
		private DiceStr?		_cStr {get; set;} = null;
		private DiceRes[]?		_dResul {get; set;} = null;

		// 1. Constructors
		public		DiceSet(int dTimes, int dCount, int dSides, int dAdvan, string dEquat) {
			this._bManual = true;
			this._dTimes = dTimes;
			this._dCount = dCount;
			this._dSides = dSides;
			this._dAdvan = dAdvan;
			this._dEquat = dEquat;
			this.FormStringFromParams();
			this.CheckDice();
			this.RunDice();
			this.GenFinalEmbed();
		}
		public		DiceSet(string dLine) {
			this._dString = dLine;
			this.CheckDice();
			this.FormStringFromParams();
			this.RunDice();
			this.GenFinalEmbed();
		}

		// 2. Checking & Running
		public bool		CheckDice() {
			if (this._cStr == null)
				this._cStr = new DiceStr(this);
			return (this._cStr._sCheck);
		}
		private bool	RunDice() {
			if ((this._dTimes <= 0 || this._dTimes > 200)
				|| (this._dCount <= 0 || this._dCount > 1000)
				|| (this._dSides <= 0 || this._dSides > 1000)
				|| (this._dTimes * this._dCount) > 10000) {
				this._rError = true;
				return (false);
			}
			this._dResul = new DiceRes[this._dTimes];
			for (int i = 0; i < this._dTimes; i++)
				this._dResul[i] = new DiceRes(this);
			return (true);
		}

		// 3. Returning
		private void		GenFinalEmbed() {
			var embed = new DiscordEmbedBuilder();
			if (this._rError || this._cStr != null && this._cStr._sCheck == false) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription($"{((this._dTimes > 200 || this._dCount > 1000 || this._dSides > 1000 || (this._dTimes * this._dCount) > 10000) ?
										"That dice set is WAAAY too big." : "Dice Set was invalid.")}");
				this._embed = embed.Build();
				return ;
			}
			string	context = "";
			if (this._dResul != null)
				for (int i = 0; i < this._dTimes; i++) {
					context += this._dResul[i]._rString;
					if (i < this._dTimes - 1)
						context += "\n";
				}
			else
				context = "So... what happened? No results found.";
			if (context.Length > 4000)
				context = "That Dice Set was too big for embeds.";
			embed.WithColor(DiscordColor.DarkBlue);
			embed.WithDescription(context);
			if (this._dTimes > 3)
				embed.WithTitle($"Dice Set..: {this._dString}");
			this._embed = embed.Build();
		}
		public DiscordEmbed	GetEmbed() {
			if (this._embed != null)
				return (this._embed);
			var	embed = new DiscordEmbedBuilder() {
						Color = DiscordColor.Red,
						Description = "No embed to retrieve."};
			return (embed.Build());
		}
		public string		GetFinalString() {
			string	context = "";

			if (this._dResul != null)
				for (int i = 0; i < this._dTimes; i++) {
					context += this._dResul[i]._rString;
					if (this._dTimes > 3)
						context += $"Dice Set..: {this._dString}";
					if (i < this._dTimes - 1)
						context += "\n";
				}
			else if ((this._dTimes * this._dCount) > 30000)
				context = "That dice set is WAAAY too big.";
			else
				context = "So... what happened?";
			if (context.Length > 1950)
				context = "That Dice Set was too big for text messages.";
			return (context);
		}

		// 4. Utils
		public DiscordEmbed	GetSetConfigEmbed() {
			var embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Gold,
				Title = "Dice Set:",
				Description = $"String.......: {this._dString}\n" +
								$"Equation..: {this._dEquat}\n" +
								$"Times.......: {this._dTimes}\n" +
								$"Count.......: {this._dCount}\n" +
								$"Sides........: {this._dSides}\n" +
								$"Removes..: {this._dAdvan}\n"};
			return (embed.Build());
		}
		private string		FormStringFromParams() {
			this._dString = "";
			if (this._dTimes > 1)
				this._dString += $"{this._dTimes}#";
			if (this._dCount > 1)
				this._dString += $"{this._dCount}";
			this._dString += $"d{this._dSides}";
			if (this._dAdvan > 0)
				this._dString += $"A{this._dAdvan}";
			else if (this._dAdvan < 0)
				this._dString += $"a{this._dAdvan * -1}";
			if (this._dEquat != "")
				this._dString += $" ({this._dEquat})";
			return (this._dString);
		}
		public int			TriggerNatDice() {
			if (this._cStr == null || this._cStr._sCheck == false || this._rError == true)
				return (0);
			if (this._dTimes == 1 && ((this._dCount - int.Abs(this._dAdvan)) == 1)
				&& (this._dResul != null && (this._dResul[0]._rValues[0] == 1 || this._dResul[0]._rValues[0] == 20)))
				return (this._dResul[0]._rValues[0]);
			return (0);
		}
		public async Task<DiscordEmbed>	GetEventEmbed(int diceres, string name) {
			DBEngine	engine = new DBEngine();
			DBFanfare	dicef = await engine.GetDiceFanfareAsync(diceres);
			string[]	mssprts = dicef._message.Split('&');
			if (mssprts.Length != 1) {
					dicef._message = mssprts[0];
					dicef._message += name;
					dicef._message += mssprts[1];
			}
			var	embed = new DiscordEmbedBuilder() {
					Color = DiscordColor.Black,
					Title = dicef._message,
					ImageUrl = dicef._glink
					};
			return (embed.Build());
		}
	}
}
