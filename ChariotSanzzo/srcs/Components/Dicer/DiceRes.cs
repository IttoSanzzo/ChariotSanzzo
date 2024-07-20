using System.Data;
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

		// 2. Functions
		public void FmString() {
			string	ret = "";
			int	vanCount = this._dSet._dAdvan;
			if (vanCount >= 0)
				for (int i = 0; i < this._dSet._dCount; i++) {
					if (i > this._dSet._dCount - vanCount - 1) {
						ret += $"~~{this._rValues[i]}~~";
						this._rTotalV -= this._rValues[i];
					}
					else
						ret += $"{this._rValues[i]}";
					if (i < this._dSet._dCount - 1)
						ret += ", ";
				}
			else
				for (int i = 0; i < this._dSet._dCount; i++) {
					if (vanCount++ < 0) {
						ret += $"~~{this._rValues[i]}~~";
						this._rTotalV -= this._rValues[i];
					}
					else
						ret += $"{this._rValues[i]}";
					if (i < this._dSet._dCount - 1)
						ret += ", ";
				}
			if (this._dSet._dEquat != "") {
				string equation = $"{this._rTotalV}{this._dSet._dEquat}";
				string? res = new DataTable().Compute(equation, null).ToString();
				if (res != null)
					this._rTotalV = Int32.Parse(res);
			}
			string totalString = $"` {this._rTotalV}";
			if (this._dSet._dTimes > 1 && this._dSet._dEquat == "")
				for (int i = 0; i < (DigitsCount(this._dSet._dCount * this._dSet._dSides) - DigitsCount(this._rTotalV)); i++)
					totalString += " ";
			ret = totalString + " `<-- [" + ret + $"]{((this._dSet._dTimes <= 3) ? $" {this._dSet._dString}" : "")}";this._rString = ret;
		}

		// 3. Utils
		public void BubbleSort() {
			int	temp;

			for (int i = 0; i < this._dSet._dCount; i++)
				if (i < this._dSet._dCount - 1 && this._rValues[i] < this._rValues[i + 1]) {
					temp = this._rValues[i];
					this._rValues[i] = this._rValues[i + 1];
					this._rValues[i + 1] = temp;
					i = -1;
				}
		}
		public int DigitsCount(int nbr) {
			return ((int)Math.Floor(Math.Log10(Math.Abs(nbr)) + 1));
		}
	}
}
