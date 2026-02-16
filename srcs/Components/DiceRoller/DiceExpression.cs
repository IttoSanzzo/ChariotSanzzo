using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Components.DiceRoller {
	public partial class DiceExpression {
		private static int MaxExpressionLength { get; set; } = 50;
		private string ExpressionString { get; set; } = null!;
		private string ExpressionStringFormatted { get; set; } = "";
		public bool IsValid { get; private set; } = false;
		public int MaxResult { get; private set; } = 0;
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
			if (this.Nodes.Count > 20 || this.Nodes.Any((node) => node.Validate() == false)) {
				this.IsValid = false;
				return;
			}
			this.MaxResult = this.GenMaxResult();
			if (this.MaxResult > 1000000) {
				this.IsValid = false;
				return;
			}
			if (this.Iterations > 1) {
				this.ExpressionStringFormatted = $"{this.Iterations}#";
				foreach (var node in this.Nodes)
					this.ExpressionStringFormatted += $" {node.GetFormatedNodeString()}";
			} else
				this.ExpressionStringFormatted = string.Join(" ", this.Nodes.Select((node) => node.GetFormatedNodeString()));
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
			int maxResultWidth = this.MaxResult.ToString().Length;
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
					finalMessageBuilder.Append($"` {(this.Iterations == 1 ? total : total.ToString().PadRight(maxResultWidth, ' '))} ` ⟵ {nodeString.ToString()}");
				else finalMessageBuilder.Append($"\n` {(this.Iterations == 1 ? total : total.ToString().PadRight(maxResultWidth, ' '))} ` ⟵ {nodeString.ToString()}");
				if (finalMessageBuilder.Length > 4090) {
					this.IsValid = false;
					DiscordEmbedBuilder error = new() {
						Color = DiscordColor.Red,
						Description = "Dice Result was too large."
					};
					return (false, error.Build());
				}
			}
			embed.Description = finalMessageBuilder.ToString();
			return (true, embed.Build());
		}
		public DiceResults Roll() {
			var results = new DiceResults();
			if (IsValid == false) {
				results.WasSuccess = false;
				results.ErrorMessage = "Dice Expression Was Not Valid.";
				return results;
			}
			results.FormattedExpression = this.ExpressionStringFormatted;
			int maxResultWidth = this.MaxResult.ToString().Length;
			results.MaxResultWidth = maxResultWidth;

			results.DiceNodes = [.. Nodes.Select((node) => new DiceResults.DiceNode() {
				NodeExpression = node.GetFormatedNodeString(),
				NodeOperator = node.GetOperatorSymbol(),
				AdvantageValue = node.GetAdvantageValue(),
				CountValue = node.GetCountValue(),
				SideValue = node.GetSideValue(),
				Type = ((node.Type == DiceNodeType.DiceSet) ? "DiceSet" : "Constant"),
			})];
			results.IterationResults = [.. new int[Iterations].Select((_) => {
				var iterationResult = new DiceResults.IterationResult {
					TotalResult = 0,
					NodeResults = new int[Nodes.Count][]
				};
				for (int nodeIndex = 0; nodeIndex < Nodes.Count; ++nodeIndex) {
					var (rollResult, rollResults) = Nodes[nodeIndex].Execute();
					iterationResult.TotalResult = Nodes[nodeIndex].Operator switch {
						DiceNodeOperator.Sum => iterationResult.TotalResult + rollResult,
						DiceNodeOperator.Sub => iterationResult.TotalResult - rollResult,
						DiceNodeOperator.Mul => iterationResult.TotalResult * rollResult,
						DiceNodeOperator.Div => iterationResult.TotalResult / rollResult,
						_ => iterationResult.TotalResult + rollResult
					};
					iterationResult.NodeResults[nodeIndex] = rollResults;
				}
				return iterationResult;
			})];
			results.WasSuccess = true;
			return results;
		}

		public int GenMaxResult() {
			int max = 0;
			foreach (var node in this.Nodes) {
				max = node.Operator switch {
					DiceNodeOperator.Sum => max + node.GetMaxValue(),
					DiceNodeOperator.Sub => max - node.GetMaxValue(),
					DiceNodeOperator.Mul => max * node.GetMaxValue(),
					DiceNodeOperator.Div => max / node.GetMaxValue(),
					_ => max + node.GetMaxValue()
				};
			}
			return max;
		}

		[GeneratedRegex(@"\s")]
		private static partial Regex AllSpacesRegex();
		[GeneratedRegex(@"(?=[+\-*/])")]
		private static partial Regex ArithmeticSeparatorsRegex();
		[GeneratedRegex(@"^(?:\d+#)?(?:\d+|(?:\d*)[dD]\d+(?:[aA]\d*)?)(?:[+\-*/](?:\d+|(?:\d*)[dD]\d+(?:[aA]\d*)?))*$")]
		private static partial Regex DiceExpressionRegex();
	}
}
