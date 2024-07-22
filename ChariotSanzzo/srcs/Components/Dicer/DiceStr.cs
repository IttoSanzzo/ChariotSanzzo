#define BANANADA


using STPlib;


namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceStr {
		// -1. Enums
		public enum TokenI : short {
			Hash = 0,
			Dd = 1,
			Aa = 2,
			Math = 3
		}

		// 0. Member Variables
		public bool		_sCheck {get; set;} = true;
		private string	_lStr {get; set;}
		private int[][]	_token {get; set;} = new int[4][];
		private DiceSet	_dSet {get; set;}

		// 1. Constructors
		public DiceStr(DiceSet dSet) {
			this._dSet = dSet;
			this._lStr = this._dSet._dString;
			if (this.CheckDString() == false)
				return ;
			this.GetValues();
		}

		// 2. Checks
		private bool CheckDString() {
			this._sCheck = true;
			Console.Write("Check Try -> ");
			/* Defaults for Manual Incriptions */
			if (this._dSet._bManual == true) {
				Console.WriteLine("Manual Mode");
				return (this._sCheck);
			}
			/* Checks for Empty or Length Limite */
			else if (this._lStr == "" || this._lStr.Length > 40) {
				Console.Write("Empty or Big");
				this._sCheck = false;
			}
			this._lStr = this._lStr.RemoveChr(' ');
			Console.Write($"=>{this._lStr}<=");
			/* Checks for Invalid Characters */
			if (this._lStr.StrLimitChrSetOrDigit("dD#+-*/aA") == false) {
				Console.Write("Other Char");
				this._sCheck = false;
			}
			/* Checks and Count Tokens */
			else if (this.CountTokens() == false) {
				Console.Write("Token Count Error");
				this._sCheck = false;
			}
			/* Checks Token Order */
			else if (this.CheckTokenOrder() == false) {
				Console.Write("Token Order Error");
				this._sCheck = false;
			}
			/* End Return */
			if (this._sCheck) Console.WriteLine("All Good"); else Console.WriteLine(" <- Error");
			return (this._sCheck);
		}
		private bool CountTokens() {
			this._token[(short)TokenI.Dd] = this._lStr.StrCountChrFirstIndex("Dd");
			if (this._token[(short)TokenI.Dd][0] != 1)
				return (false);
			this._token[(short)TokenI.Hash] = this._lStr.StrCountChrFirstIndex("#");
			if (this._token[(short)TokenI.Hash][0] > 1)
				return (false);
			this._token[(short)TokenI.Aa] = this._lStr.StrCountChrFirstIndex("Aa");
			if (this._token[(short)TokenI.Aa][0] > 1)
				return (false);
			this._token[(short)TokenI.Math] = this._lStr.StrCountChrFirstIndex("+-*/");
			if (this._token[(short)TokenI.Math][0] > 1)
				this._token[(short)TokenI.Math][0] = 1;
			return (true);
		}
		private bool CheckTokenOrder() {
			if ((this._token[(short)TokenI.Hash][0] == 1 && (this._token[(short)TokenI.Hash][1] > this._token[(short)TokenI.Dd][1]))
				|| (this._token[(short)TokenI.Aa][0] == 1 && (this._token[(short)TokenI.Aa][1] < this._token[(short)TokenI.Dd][1])))
				return (false);
			if (this._token[(short)TokenI.Math][0] == 1 && this._token[(short)TokenI.Aa][0] == 1 && (this._token[(short)TokenI.Math][1] < this._token[(short)TokenI.Aa][1]))
				return (false);
			if (this._token[(short)TokenI.Math][0] == 1 && this._token[(short)TokenI.Math][1] < this._token[(short)TokenI.Dd][1])
				return (false);
			if (this._token[(short)TokenI.Hash][0] == 1 && this._token[(short)TokenI.Hash][1] == 0 || this._lStr[0] == '0')
				return (false);
			return (true);
		}

		// 3. Get Values
		private void GetValues() {
			int	advanType = 1;
			if (this._token[(short)TokenI.Aa][0] == 1 && this._lStr[this._token[(short)TokenI.Aa][1]] == 'a')
				advanType = -1;
			if (this._token[(short)TokenI.Hash][0] == 1) {
				this._dSet._dTimes = this._lStr.Substring(0, this._token[(short)TokenI.Hash][1]).StoI();
				this._lStr = this._lStr.Remove(0, this._token[(short)TokenI.Hash][1] + 1);
			}
			else
				this._dSet._dTimes = 1;
			if (char.IsDigit(this._lStr[0])) {
				int	i = -1;
				while (++i < this._lStr.Length)
					if (char.IsDigit(this._lStr[i]) == false)
						break ;
				this._dSet._dCount = this._lStr.Substring(0, i).StoI();
				this._lStr = this._lStr.Remove(0, i + 1);
			} else {
				this._dSet._dCount = 1;
				this._lStr = this._lStr.Remove(0, 1);
			}
			if (char.IsDigit(this._lStr[0])) {
				int	i = -1;
				while (++i < this._lStr.Length)
					if (char.IsDigit(this._lStr[i]) == false)
						break ;
				this._dSet._dSides = this._lStr.Substring(0, i).StoI();
				if (this._token[(short)TokenI.Aa][0] == 1 || this._token[(short)TokenI.Math][0] == 1)
					this._lStr = this._lStr.Remove(0, i + this._token[(short)TokenI.Aa][0]);
			} else {
				this._sCheck = false;
				return ;
			}
			if (this._token[(short)TokenI.Aa][0] == 1) {
				int	i = -1;
				while (++i < this._lStr.Length)
					if (char.IsDigit(this._lStr[i]) == false)
						break ;
				this._dSet._dAdvan = this._lStr.Substring(0, i).StoI() * advanType;
				Console.WriteLine($"AdvanType -> {advanType == 1}");
				if (this._token[(short)TokenI.Math][0] == 1)
					this._lStr = this._lStr.Remove(0, i);	
			}
			if (this._token[(short)TokenI.Math][0] == 1)
				this._dSet._dEquat = new string(this._lStr);
			else
				this._dSet._dEquat = "";
			Console.Write($"|_dTime = {this._dSet._dTimes}|\n" +
							$"|_dCount = {this._dSet._dCount}|\n" +
							$"|_dCount = {this._dSet._dCount}|\n" +
							$"|_dMath = " + this._dSet._dEquat + "|\n" +
							$"|_dSides = {this._dSet._dSides}|\n");
			Console.WriteLine($"|=>{this._lStr}<=|");
			return ;
		}
	}
}
