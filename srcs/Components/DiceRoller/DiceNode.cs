namespace ChariotSanzzo.Components.DiceRoller {
	public partial class DiceExpression {
		public enum DiceNodeType {
			Undefined,
			DiceSet,
			Constant
		}
		private enum DiceNodeOperator {
			None,
			Sum,
			Sub,
			Mul,
			Div
		}

		private partial class DiceNode {
			public DiceNodeType Type { get; set; } = DiceNodeType.Undefined;
			public DiceNodeOperator Operator { get; set; } = DiceNodeOperator.None;
			public string NodeString { get; set; } = "";
			private DiceSet Set { get; set; } = null!;
			public int Constant { get; set; } = 0;

			public DiceNode(string nodeString) {
				(this.Operator, this.NodeString) = nodeString[0] switch {
					'+' => (DiceNodeOperator.Sum, nodeString[1..]),
					'-' => (DiceNodeOperator.Sub, nodeString[1..]),
					'*' => (DiceNodeOperator.Mul, nodeString[1..]),
					'/' => (DiceNodeOperator.Div, nodeString[1..]),
					_ => (DiceNodeOperator.None, nodeString)
				};
				if (nodeString.Contains('d', StringComparison.OrdinalIgnoreCase)) {
					this.Type = DiceNodeType.DiceSet;
					this.Set = new(this.NodeString);
				} else {
					this.Type = DiceNodeType.Constant;
					this.Constant = int.Parse(this.NodeString);
				}
			}
			public bool Validate() {
				if (this.Type == DiceNodeType.DiceSet)
					return this.Set.Validate();
				return true;
			}
			public int GetAdvantageValue() {
				if (this.Type == DiceNodeType.DiceSet)
					return this.Set.Advantage;
				return 0;
			}
			public char GetOperatorSymbol() {
				return OperatorSymbol(this.Operator);
			}
			public string GetFormatedNodeString() {
				if (this.Operator == DiceNodeOperator.None)
					return this.NodeString;
				return $"{OperatorSymbol(this.Operator)} {this.NodeString}";
			}
			private string GetFormatedNodeStringWithResults(int[] results, int advantage, bool withExpressionString = true) {
				string resultsString = advantage switch {
					> 0 => $"[{String.Join(", ", results.Select((value, index) => index < results.Length - advantage ? $"{value}" : $"~~{value}~~"))}]",
					< 0 => $"[{String.Join(", ", results.Select((value, index) => index < Math.Abs(advantage) ? $"~~{value}~~" : $"{value}"))}]",
					_ => $"[{String.Join(", ", results)}]"
				};
				if (this.Operator == DiceNodeOperator.None)
					return $"{resultsString}{(withExpressionString ? ($" {this.NodeString}") : "")}";
				return $"{OperatorSymbol(this.Operator)} {resultsString}{(withExpressionString ? ($" {this.NodeString}") : "")}";
			}
			public (int result, int[] results) Execute() {
				if (this.Type == DiceNodeType.Constant)
					return (this.Constant, [this.Constant]);
				var (total, results, _) = this.Set.Roll();
				return (total, results);
			}
			public (int result, DiceNodeOperator nodeOperator, string message) ExecuteWithMessage(bool withExpressionString = true) {
				if (this.Type == DiceNodeType.Constant)
					return (this.Constant, this.Operator, this.GetFormatedNodeString());
				var (total, results, advantage) = this.Set.Roll();
				return (total, this.Operator, $"{this.GetFormatedNodeStringWithResults(results, advantage, withExpressionString)}");
			}
			public int GetMaxValue() {
				if (this.Type == DiceNodeType.Constant)
					return this.Constant;
				return this.Set.Count * this.Set.Sides;
			}
		}

		private static char OperatorSymbol(DiceNodeOperator nodeOperator) =>
		nodeOperator switch {
			DiceNodeOperator.Sum => '+',
			DiceNodeOperator.Sub => '-',
			DiceNodeOperator.Mul => '*',
			DiceNodeOperator.Div => '/',
			_ => '?',
		};
	}
}
