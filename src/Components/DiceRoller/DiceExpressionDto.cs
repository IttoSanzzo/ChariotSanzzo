namespace ChariotSanzzo.Components.DiceRoller {
	public class DiceExpressionDto {
		public string DiceExpression { get; set; } = "";

		public DiceExpression ToDiceExpression() {
			return new DiceExpression(this.DiceExpression);
		}
	}
}
