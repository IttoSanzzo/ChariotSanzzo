using System.Data;

namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceRes {
	// M. Member Variables
		private static Random	Random	{get; set;} = new Random();
		public string			RString	{get; set;} = "";
		public int[]			RValues	{get; set;} = {};
		public int				RTotalV	{get; set;} = 0;
		public DiceSet			DSet		{get; set;}

	// C. Contructor
		public DiceRes(DiceSet dSet) {
			this.DSet = dSet;
			this.RValues = new int[this.DSet.DCount];
			for (int i = 0; i < this.DSet.DCount; i++) {
				this.RValues[i] = DiceRes.Random.Next(1, this.DSet.DSides + 1);
				this.RTotalV += this.RValues[i];
			}
			Array.Sort(this.RValues);
			Array.Reverse(this.RValues);
			this.FmString();
		}

	// 0. Core Functions
		public void	FmString() {
			string	ret = "";
			int	vanCount = this.DSet.DAdvan;
			if (vanCount >= 0)
				for (int i = 0; i < this.DSet.DCount; i++) {
					if (i > this.DSet.DCount - vanCount - 1) {
						ret += $"~~{this.RValues[i]}~~";
						this.RTotalV -= this.RValues[i];
					}
					else
						ret += $"{this.RValues[i]}";
					if (i < this.DSet.DCount - 1)
						ret += ", ";
				}
			else
				for (int i = 0; i < this.DSet.DCount; i++) {
					if (vanCount++ < 0) {
						ret += $"~~{this.RValues[i]}~~";
						this.RTotalV -= this.RValues[i];
					}
					else
						ret += $"{this.RValues[i]}";
					if (i < this.DSet.DCount - 1)
						ret += ", ";
				}
			if (this.DSet.DEquat != "") {
				string equation = $"{this.RTotalV}{this.DSet.DEquat}";
				string? res = new DataTable().Compute(equation, null).ToString();
				if (res != null)
					this.RTotalV = Int32.Parse(res);
			}
			string totalString = $"` {this.RTotalV}";
			if (this.DSet.DTimes > 1 && this.DSet.DEquat == "")
				for (int i = 0; i < (DigitsCount(this.DSet.DCount * this.DSet.DSides) - DigitsCount(this.RTotalV)); i++)
					totalString += " ";
			ret = totalString + " `<-- [" + ret + $"]{((this.DSet.DTimes <= 3) ? $" {this.DSet.DString}" : "")}";
			this.RString = ret;
		}

	// U. Utils
		public int	DigitsCount(int nbr) {
			return ((int)Math.Floor(Math.Log10(Math.Abs(nbr)) + 1));
		}
	}
}
