namespace ChariotSanzzo.Components.DiceRoller {
	public partial class DiceExpression {
		private partial class DiceNode {
			private class DiceSet {
				private static Random Random { get; set; } = new Random();
				public int Count { get; set; } = 1;
				public int Sides { get; set; } = 1;
				private int SidesTarget { get; set; } = 1;
				public int Advantage { get; set; } = 0;

				public DiceSet(string setExpression) {
					int dPosition = setExpression.IndexOf('d', StringComparison.OrdinalIgnoreCase);
					int aPosition = setExpression.IndexOf('a', StringComparison.OrdinalIgnoreCase);

					if (dPosition > 0)
						this.Count = int.Parse(setExpression[..dPosition]);
					if (aPosition != -1) {
						if (int.TryParse(setExpression[(aPosition + 1)..], out var advantageValue))
							this.Advantage = advantageValue;
						else
							this.Advantage = 1;
						if (setExpression[aPosition] == 'a')
							this.Advantage *= -1;
						this.Sides = int.Parse(setExpression[(dPosition + 1)..aPosition]);
					} else
						this.Sides = int.Parse(setExpression[(dPosition + 1)..]);
					this.SidesTarget = this.Sides + 1;
				}
				public bool Validate() {
					if (this.Count <= 0
					|| this.Count > 1000
					|| this.Sides <= 0
					|| this.Sides > 1000)
						return (false);
					return (true);
				}
				public int[] DirectRollWithoutAdvantage() {
					int[] results = new int[this.Count];
					for (int index = 0; index < this.Count; ++index)
						results[index] = Random.Next(1, this.SidesTarget);
					return results;
				}
				public int[] DirectRollWithAdvantage() {
					int[] results = new int[this.Count];
					for (int index = 0; index < this.Count; ++index)
						results[index] = Random.Next(1, this.SidesTarget);
					if (Advantage > 0)
						return [.. results.OrderByDescending((x) => x).Take(this.Count - this.Advantage)];
					else
						return [.. results.OrderByDescending((x) => x).TakeLast(this.Count - Math.Abs(this.Advantage))];
				}
				public (int total, int[] results, int advantage) Roll() {
					int[] results = new int[this.Count];
					for (int index = 0; index < this.Count; ++index)
						results[index] = Random.Next(1, this.SidesTarget);
					results = [.. results.OrderByDescending((x) => x)];
					int total = 0;
					int targetIndex = this.Advantage > 0 ? this.Count - this.Advantage : this.Count;
					for (int index = this.Advantage < 0 ? Math.Abs(this.Advantage) : 0; index < targetIndex; ++index)
						total += results[index];
					return (total, results, this.Advantage);
				}
			}
		}
	}
}
