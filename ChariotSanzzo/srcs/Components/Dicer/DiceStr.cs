using STPlib;

namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceStr {
		// 0. Member Variables
		public string	_lStr {get; set;}
		public bool		_sCheck {get; set;} = true;
		public DiceSet	_dSet {get; set;}

		// 1. Constructors
		public DiceStr(DiceSet dSet) {
			this._dSet = dSet;
			this._lStr = this._dSet._dString;
			if (CheckDString() == false)
				return ;
		}

		// 2. Checks
		public bool CheckDString() {
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
			/* Checks for Invalid Characters */
			else if (this._lStr.StrLimitChr("0123456789dD# +-*/aA") == false) {
				Console.Write("Other Char");
				this._sCheck = false;
			}
			/* Checks for Token Repetitions */
			else if (this._lStr.StrCountChr("dD") > 1
				|| this._lStr.StrCountChr("#") > 1
				|| this._lStr.StrCountChr("aA") > 1) {
				Console.Write("Token Count Error");
				this._sCheck = false;
			}
			/* End Return */
			if (this._sCheck)
				Console.WriteLine("All Good");
			else
				Console.WriteLine(" <- Error");
			return (this._sCheck);
		}
	}
}
