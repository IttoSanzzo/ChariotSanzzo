using System.Text;
using DSharpPlus.Entities;

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

		public async Task<(bool wasSuccess, DiscordEmbed embed)> ToDiscordEmbedAsync(ulong? userId) {
			DiscordEmbedBuilder embed = new();
			if (userId != null) {
				var user = await Program.Client!.GetUserAsync((ulong)userId);
				if (user != null) {
					embed.WithFooter(user.Username, user.AvatarUrl);
				}
			}

			if (WasSuccess == false) {
				embed.Description = ErrorMessage;
				embed.Color = DiscordColor.Red;
				return (false, embed.Build());
			}
			embed.Color = DiscordColor.DarkBlue;

			if (this.IterationResults.Length > 1) {
				embed.Title = $"{this.IterationResults.Length}#";
				foreach (var node in this.DiceNodes)
					embed.Title += $" {node.NodeExpression}";
			}

			int maxResultWidth = this.MaxResultWidth;
			StringBuilder finalMessageBuilder = new();

			for (int index = 0; index < this.IterationResults.Length; ++index) {
				StringBuilder nodeString = new();
				for (int nodeIndex = 0; nodeIndex < this.DiceNodes.Length; ++nodeIndex) {
					if (nodeIndex != 0)
						nodeString.Append($" {this.DiceNodes[nodeIndex].NodeOperator} ");
					if (this.DiceNodes[nodeIndex].Type == "Constant") {
						nodeString.Append($" {this.IterationResults[index].NodeResults[nodeIndex][0]}");
					} else {
						nodeString.Append(GetFormatedNodeStringResults(this.IterationResults[index].NodeResults[nodeIndex], this.DiceNodes[nodeIndex].AdvantageValue));
						nodeString.Append($" {this.DiceNodes[nodeIndex].NodeExpression}");
					}
				}
				int total = this.IterationResults[index].TotalResult;
				if (index == 0)
					finalMessageBuilder.Append($"` {(this.IterationResults.Length == 1 ? total : total.ToString().PadRight(maxResultWidth, ' '))} ` ⟵ {nodeString.ToString()}");
				else finalMessageBuilder.Append($"\n` {(this.IterationResults.Length == 1 ? total : total.ToString().PadRight(maxResultWidth, ' '))} ` ⟵ {nodeString.ToString()}");
				if (finalMessageBuilder.Length > 4090) {
					embed.WithColor(DiscordColor.Red);
					embed.WithDescription("Dice Result was too large.");
					return (true, embed.Build());
				}
			}
			embed.Description = finalMessageBuilder.ToString();
			return (true, embed.Build());
		}
		private static string GetFormatedNodeStringResults(int[] results, int advantage) {
			return advantage switch {
				> 0 => $"[{String.Join(", ", results.Select((value, index) => index < results.Length - advantage ? $"{value}" : $"~~{value}~~"))}]",
				< 0 => $"[{String.Join(", ", results.Select((value, index) => index < Math.Abs(advantage) ? $"~~{value}~~" : $"{value}"))}]",
				_ => $"[{String.Join(", ", results)}]"
			};
		}
	}
}
