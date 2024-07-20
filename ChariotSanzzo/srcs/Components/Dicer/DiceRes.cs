using DSharpPlus.Entities;

namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceRes {
		// 0. Member Variables
		private static Random	_rRandom {get; set;} = new Random();
		public string	_rString {get; set;} = "";
		public int[]	_rValues {get; set;} = {};
		public int		_rTotalV {get; set;} = 0;
		public DiceSet	_dSet {get; set;}

		// 1. Contructor
		public DiceRes(DiceSet dSet) {
			this._dSet = dSet;
			this._rValues = new int[this._dSet._dCount];
			for (int i = 0; i < this._dSet._dCount; i++) {
				this._rValues[i] = DiceRes._rRandom.Next(1, this._dSet._dSides + 1);
				this._rTotalV += this._rValues[i];
			}
			// TODO _rTotalV Manipulation
			// this._rTotalV = this._rTotalV + this._dSet._dEquat;
			this.BubbleSort();
			this.FmString();
			// Console.WriteLine(this._rString);
		}

		// 2. Utils
		public void BubbleSort() {
			int	temp;

			for (int i = 0; i < this._dSet._dCount; i++)
				if (i < this._dSet._dCount - 1 && this._rValues[i] < this._rValues[i + 1]) {
					temp = this._rValues[i];
					this._rValues[i] = this._rValues[i + 1];
					this._rValues[i + 1] = temp;
					i = 0;
				}
		}
		public void FmString() {
			string	ret = $"` {this._rTotalV} `<-- [";
			for (int i = 0; i < this._dSet._dCount; i++) {
				ret += $"{this._rValues[i]}";
				if (i < this._dSet._dCount - 1)
					ret += ", ";
			}
			ret += $"] {this._dSet._dString}";
			this._rString = ret;
		}
	}
}
