#define BANANADA


using STPlib;


namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceStr {
	// -1. Extras
		public enum TokenI : short {
			Hash = 0,
			Dd = 1,
			Aa = 2,
			Math = 3
		}

	// M. Member Variables
		public bool		SCheck	{get; set;} = true;
		private string	LStr	{get; set;}
		private int[][]	Token	{get; set;} = new int[4][];
		private DiceSet	DSet	{get; set;}

	// C. Constructors
		public DiceStr(DiceSet dSet) {
			this.DSet = dSet;
			this.LStr = this.DSet.DString;
			if (this.DSet.DManual == true)
				return ;
			if (this.CheckDString() == false)
				return ;
			this.GetValues();
		}

	// 0. Checks
		private bool CheckDString() {
			/* Checks for Empty or Length Limite */
			if (this.LStr == "" || this.LStr.Length > 40)
				this.SCheck = false;
			/* Removes Spaces from the Input */
			this.LStr = this.LStr.RemoveChr(' ');
			/* Checks for Invalid Characters */
			if (this.LStr.StrLimitChrSetOrDigit("dD#+-*/aA") == false)
				this.SCheck = false;
			/* Checks and Count Tokens */
			else if (this.CountTokens() == false)
				this.SCheck = false;
			/* Checks Token Order */
			else if (this.CheckTokenOrder() == false)
				this.SCheck = false;
			/* End Return */
			return (this.SCheck);
		}
		private bool CountTokens() {
			this.Token[(short)TokenI.Dd] = this.LStr.StrCountChrFirstIndex("Dd");
			if (this.Token[(short)TokenI.Dd][0] != 1)
				return (false);
			this.Token[(short)TokenI.Hash] = this.LStr.StrCountChrFirstIndex("#");
			if (this.Token[(short)TokenI.Hash][0] > 1)
				return (false);
			this.Token[(short)TokenI.Aa] = this.LStr.StrCountChrFirstIndex("Aa");
			if (this.Token[(short)TokenI.Aa][0] > 1)
				return (false);
			this.Token[(short)TokenI.Math] = this.LStr.StrCountChrFirstIndex("-+*/");
			if (this.Token[(short)TokenI.Math][0] > 1)
				this.Token[(short)TokenI.Math][0] = 1;
			return (true);
		}
		private bool CheckTokenOrder() {
			if ((this.Token[(short)TokenI.Hash][0] == 1 && (this.Token[(short)TokenI.Hash][1] > this.Token[(short)TokenI.Dd][1]))
				|| (this.Token[(short)TokenI.Aa][0] == 1 && (this.Token[(short)TokenI.Aa][1] < this.Token[(short)TokenI.Dd][1])))
				return (false);
			if (this.Token[(short)TokenI.Math][0] == 1 && this.Token[(short)TokenI.Aa][0] == 1 && (this.Token[(short)TokenI.Math][1] < this.Token[(short)TokenI.Aa][1]))
				return (false);
			if (this.Token[(short)TokenI.Math][0] == 1 && this.Token[(short)TokenI.Math][1] < this.Token[(short)TokenI.Dd][1])
				return (false);
			if (this.Token[(short)TokenI.Hash][0] == 1 && this.Token[(short)TokenI.Hash][1] == 0 || this.LStr[0] == '0')
				return (false);
			return (true);
		}

	// G. Gets
		private void GetValues() {
			int	advanType = 1;
			if (this.Token[(short)TokenI.Aa][0] == 1 && this.LStr[this.Token[(short)TokenI.Aa][1]] == 'a')
				advanType = -1;
			if (this.Token[(short)TokenI.Hash][0] == 1) {
				this.DSet.DTimes = this.LStr.Substring(0, this.Token[(short)TokenI.Hash][1]).StoI();
				this.LStr = this.LStr.Remove(0, this.Token[(short)TokenI.Hash][1] + 1);
			}
			else
				this.DSet.DTimes = 1;
			if (char.IsDigit(this.LStr[0])) {
				int	i = -1;
				while (++i < this.LStr.Length)
					if (char.IsDigit(this.LStr[i]) == false)
						break ;
				this.DSet.DCount = this.LStr.Substring(0, i).StoI();
				this.LStr = this.LStr.Remove(0, i + 1);
			} else {
				this.DSet.DCount = 1;
				this.LStr = this.LStr.Remove(0, 1);
			}
			if (char.IsDigit(this.LStr[0])) {
				int	i = -1;
				while (++i < this.LStr.Length)
					if (char.IsDigit(this.LStr[i]) == false)
						break ;
				this.DSet.DSides = this.LStr.Substring(0, i).StoI();
				if (this.Token[(short)TokenI.Aa][0] == 1 || this.Token[(short)TokenI.Math][0] == 1)
					this.LStr = this.LStr.Remove(0, i + this.Token[(short)TokenI.Aa][0]);
			} else {
				this.SCheck = false;
				return ;
			}
			if (this.Token[(short)TokenI.Aa][0] == 1) {
				int	i = -1;
				while (++i < this.LStr.Length)
					if (char.IsDigit(this.LStr[i]) == false)
						break ;
				this.DSet.DAdvan = this.LStr.Substring(0, i).StoI() * advanType;
				if (this.Token[(short)TokenI.Math][0] == 1)
					this.LStr = this.LStr.Remove(0, i);	
			}
			if (this.Token[(short)TokenI.Math][0] == 1) {
				this.DSet.DEquat = new string(this.LStr);
				if (this.DSet.DEquat.StrLimitChrSetOrDigit("+-*/") == false)
					this.SCheck = false;
			}
			else
				this.DSet.DEquat = "";
			return ;
		}
	}
}
