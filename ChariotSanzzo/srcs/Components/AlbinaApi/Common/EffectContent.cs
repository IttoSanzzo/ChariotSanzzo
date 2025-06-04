namespace ChariotSanzzo.Components.AlbinaApi.Common {
	public enum EffectContentType {
		Unknown,
		Text,
		Callout,
		Quote,
		Table
	}
	public class EffectContent {
		public EffectContentType	Type			{get; set;}
		public AlbinaTextColor?		Color			{get; set;} = null;
		public string							Value			{get; set;} = null!;
		public string[][]?				TableData	{get; set;} = null;
	}
}