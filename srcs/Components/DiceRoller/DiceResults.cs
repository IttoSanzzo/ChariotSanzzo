namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceResults {
		public class IterationResult {
			public int[][] NodeResults { get; set; } = [];
			public int TotalResult { get; set; } = 0;
		}
		public class DiceNode {
			public string NodeExpression { get; set; } = "";
			public char NodeOperator { get; set; } = '+';
			public string Type { get; set; } = "";
			public int AdvantageValue { get; set; } = 0;
			public int CountValue { get; set; } = 0;
			public int SideValue { get; set; } = 0;
		}

		public bool WasSuccess { get; set; } = false;
		public string ErrorMessage { get; set; } = "";
		public string FormattedExpression { get; set; } = "";
		public int MaxResultWidth { get; set; } = 1;
		public DiceNode[] DiceNodes { get; set; } = [];
		public IterationResult[] IterationResults { get; set; } = [];
	}
}
