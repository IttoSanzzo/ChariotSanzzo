namespace ChariotSanzzo.Services.Commands.ChoiceProviders {
	public class PauseStateChoiceProvider : IIntChoiceProvider {
		public override (string, int)[] Options { get; } = [
			("Resume", 0),
			("Pause", 1),
			("Switch", 2),
		];
	}
}
