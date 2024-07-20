using DSharpPlus.Entities;

namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceSet {
		// 0. Member Variables
		public string		_dString {get; set;} = "";
		public string		_dEquat {get; set;} = "";
		public int			_dTimes {get; set;} = 0;
		public int			_dCount {get; set;} = 0;
		public int			_dSides {get; set;} = 0;
		public int			_dAdvan {get; set;} = 0;
		public DiceRes[]?	_dResul {get; set;} = null;

		// 1. Constructors
		public DiceSet() {}
		public DiceSet(string dLine) {
			this._dString = FmDiceString(dLine);
			this._dEquat = FmDiceEquation();
			this._dTimes = FmDiceTimes();
			this._dCount = FmDiceCount();
			this._dSides = FmDiceSides();
			this._dAdvan = FmDiceAdvantage();
		}

		// 2. Form Dice Functions
		string FmDiceString(string line) {
			string	ret;
			ret = "";
			return (ret);
		}
		string FmDiceEquation() {
			this._dEquat = "";
			return (this._dEquat);
		}
		int FmDiceTimes() {
			this._dTimes = 0;
			return (this._dTimes);
		}
		int FmDiceCount() {
			this._dCount = 0;
			return (this._dCount);
		}
		int FmDiceSides() {
			this._dSides = 0;
			return (this._dSides);
		}
		int FmDiceAdvantage() {
			this._dAdvan = 0;
			return (this._dAdvan);
		}
	
		// 3. Running
		public bool RunDice() {
			if (this._dTimes == 0 || this._dCount == 0 || this._dSides == 0
				|| (this._dTimes * this._dCount) > 30000)
				return (false);
			this._dResul = new DiceRes[this._dTimes];
			for (int i = 0; i < this._dTimes; i++)
				this._dResul[i] = new DiceRes(this);
			return (true);
		}

		// 4. Returning
		public DiscordEmbed GetFinalEmbed() {
			string	context = "";

			if (this._dResul != null)
				for (int i = 0; i < this._dTimes; i++) {
					context += this._dResul[i]._rString;
					if (i < this._dTimes - 1)
						context += "\n";
				}
			else if ((this._dTimes * this._dCount) > 30000)
				context = "That dice set is WAAAY too big.";
			else
				context = "So... what happened?";
			if (context.Length > 4000)
				context = "That Dice Set was too big for embeds.";
			var embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.DarkBlue,
				Description = context};
			return (embed.Build());
		}
		public string GetFinalString() {
			string	context = "";

			if (this._dResul != null)
				for (int i = 0; i < this._dTimes; i++) {
					context += this._dResul[i]._rString;
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
		public DiscordEmbed GetSetConfigEmbed() {
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
	}
}
