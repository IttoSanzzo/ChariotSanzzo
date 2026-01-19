namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceResults {
		public class IterationResult {
			public int[][] NodeResults { get; set; } = [];
			public int TotalResult { get; set; } = 0;
		}
		public class DiceNode {
			public char NodeOperator { get; set; } = '+';
			public string NodeExpression { get; set; } = "";
			public int AdvantageValue { get; set; } = 0;
			public string Type { get; set; } = "";
		}

		public bool WasSuccess { get; set; } = false;
		public string ErrorMessage { get; set; } = "";
		public string FormattedExpression { get; set; } = "";
		public int MaxResultWidth { get; set; } = 1;
		public DiceNode[] DiceNodes { get; set; } = [];
		public IterationResult[] IterationResults { get; set; } = [];
	}
}
