namespace ChariotSanzzo.Services.Commands.ChoiceProviders {
	public class TrueOrFalseChoiceProvider : IIntChoiceProvider {
		public override (string, int)[] Options { get; } = [
			("True", 1),
			("False", 0),
		];
	}
}
