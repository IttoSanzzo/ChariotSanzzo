using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands.ChoiceProviders {
	public abstract class IIntChoiceProvider : IChoiceProvider {
		public abstract (string, int)[] Options { get; }
		private IReadOnlyList<DiscordApplicationCommandOptionChoice> ChoiceOptions =>
		[.. Options.Select((option) => new DiscordApplicationCommandOptionChoice(option.Item1, option.Item2))];

		public ValueTask<IEnumerable<DiscordApplicationCommandOptionChoice>> ProvideAsync(CommandParameter parameter) =>
			ValueTask.FromResult<IEnumerable<DiscordApplicationCommandOptionChoice>>(ChoiceOptions);
	}
}
