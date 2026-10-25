namespace ChariotSanzzo.Services.Commands.ChoiceProviders {
	public class LoopStateChoiceProvider : IIntChoiceProvider {
		public override (string, int)[] Options { get; } = [
			("None", 0),
			("Track", 1),
			("Queue", 2),
		];
	}
}
