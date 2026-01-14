using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Components.DiceRoller {
	public partial class DiceExpression {
		private static int MaxExpressionLength { get; set; } = 40;
		private string ExpressionString { get; set; } = null!;
		public bool IsValid { get; private set; } = false;
		public string ValidationErrorMessage { get; private set; } = "";
		private List<DiceNode> Nodes { get; set; } = [];
		private int Iterations { get; set; } = 1;

		public DiceExpression(string expression) {
			if (expression.Length > MaxExpressionLength)
				return;
			this.ExpressionString = AllSpacesRegex().Replace(expression, "");
			this.IsValid = ValidateExpressionString(this.ExpressionString);
			if (this.IsValid == false)
				return;

			if (this.ExpressionString.Contains('#')) {
				int hashtagPosition = this.ExpressionString.IndexOf('#');
				this.Iterations = int.Parse(this.ExpressionString[..hashtagPosition]);
				if (this.Iterations > 200) {
					this.IsValid = false;
					return;
				}
				this.Nodes = [.. ArithmeticSeparatorsRegex().Split(this.ExpressionString[(hashtagPosition + 1)..]).Select((rawNodeString) => new DiceNode(rawNodeString))];
			} else
				this.Nodes = [.. ArithmeticSeparatorsRegex().Split(this.ExpressionString).Select((rawNodeString) => new DiceNode(rawNodeString))];
			if (this.Nodes.Any((node) => node.Validate() == false))
				this.IsValid = false;
		}
		public static bool ValidateExpressionString(string expression) {
			if (
				expression.Contains('d', StringComparison.OrdinalIgnoreCase) == false ||
				DiceExpressionRegex().IsMatch(expression) == false
			)
				return false;
			return true;
		}
		public static bool ValidateExpressionStringWithSpaces(string expressionWithSpaces) {
			string expression = AllSpacesRegex().Replace(expressionWithSpaces, "");
			if (
				expression.Contains('d', StringComparison.OrdinalIgnoreCase) == false ||
				DiceExpressionRegex().IsMatch(expression) == false
			)
				return false;
			return true;
		}
		public (bool wasSuccess, DiscordEmbed embed) RollForDiscord() {
			DiscordEmbedBuilder embed = new();
			if (IsValid == false) {
				embed.Description = "Dice Expression Was Not Valid.";
				embed.Color = DiscordColor.Red;
				return (false, embed.Build());
			}
			embed.Color = DiscordColor.DarkBlue;
			if (this.Iterations > 1) {
				embed.Title = $"{this.Iterations}#";
				foreach (var node in this.Nodes)
					embed.Title += $" {node.GetFormatedNodeString()}";
			}
			StringBuilder finalMessageBuilder = new();
			for (int index = 0; index < this.Iterations; ++index) {
				StringBuilder nodeString = new();
				int total = 0;
				foreach (var node in this.Nodes) {
					var (result, nodeOperator, message) = node.ExecuteWithMessage(this.Iterations == 1);
					total = nodeOperator switch {
						DiceNodeOperator.Sum => total + result,
						DiceNodeOperator.Sub => total - result,
						DiceNodeOperator.Mul => total * result,
						DiceNodeOperator.Div => total / result,
						_ => total + result
					};
					if (nodeOperator == DiceNodeOperator.None)
						nodeString.Append(message);
					else
						nodeString.Append($" {message}");
				}
				if (index == 0)
					finalMessageBuilder.Append($"` {total}{(this.Iterations != 1 && total < 10 ? " " : "")} ` ⟵ {nodeString.ToString()}");
				else finalMessageBuilder.Append($"\n` {total}{(this.Iterations != 1 && total < 10 ? " " : "")} ` ⟵ {nodeString.ToString()}");
			}
			embed.Description = finalMessageBuilder.ToString();
			return (true, embed.Build());
		}

		[GeneratedRegex(@"\s")]
		private static partial Regex AllSpacesRegex();
		[GeneratedRegex(@"(?=[+\-*/])")]
		private static partial Regex ArithmeticSeparatorsRegex();
		[GeneratedRegex(@"^(?:\d+#)?(?:\d+|(?:\d*)[dD]\d+(?:[aA]\d*)?)(?:[+\-*/](?:\d+|(?:\d*)[dD]\d+(?:[aA]\d*)?))*$")]
		private static partial Regex DiceExpressionRegex();
	}
}
