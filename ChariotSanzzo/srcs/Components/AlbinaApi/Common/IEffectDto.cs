namespace ChariotSanzzo.Components.AlbinaApi.Common {
	public interface IEffectDto {
		public Guid							Id						{get;}
		public string						Role					{get;}
		public string						Name					{get;}
		public AlbinaTextColor?	Color					{get;}
		public DateTime					CreatedAt			{get;}
		public DateTime?				UpdatedAt			{get;}
		public EffectContent[]	Contents			{get;}
		public string						AlbinaVersion	{get;}
		public int							Order					{get;}
	}
}