namespace ChariotSanzzo.Components.AlbinaApi.DTOs {
	public enum RaceType {
		Unknown,
		Human,
		Fey,
		Demon,
		Dragon,
		Seirei
	};
		public enum RaceSubType {
		Unknown,
		Primary,
		Secundary
	};

	public class RaceInfo {
		public string[]	Introduction	{get; set;} = [];
		public string[]	Personality		{get; set;} = [];
		public string[]	Culture				{get; set;} = [];
		public string[]	Miscellaneous	{get; set;} = [];
		public string[]	Groups				{get; set;} = [];
		public string[]	Relations			{get; set;} = [];
		public string[]	Description		{get; set;} = [];
		public string[]	Images				{get; set;} = [];
	}
	public class RaceParameters {
		public int	Vitality			{get; set;} = 0;
		public int	Vigor					{get; set;} = 0;
		public int	Manapool			{get; set;} = 0;
		public int	PhysicalPower	{get; set;} = 0;
		public int	MagicalPower	{get; set;} = 0;
	}
	public class RaceGenerals {
		public string	Height		{get; set;} = string.Empty;
		public string	Weight		{get; set;} = string.Empty;
		public string	Longevity	{get; set;} = string.Empty;
		public string	Speed			{get; set;} = string.Empty;
		public string	Language	{get; set;} = string.Empty;
	}

	public class RaceDto {
		public Guid									Id						{get; set;}
		public string								Slug					{get; set;} = null!;
		public string								Name					{get; set;} = null!;
		public RaceType							Type					{get; set;}
		public RaceSubType					SubType				{get; set;}
		public string								IconUrl				{get; set;} = string.Empty;
		public string								BannerUrl			{get; set;} = string.Empty;
		public DateTime							CreatedAt			{get; set;}
		public DateTime?						UpdatedAt			{get; set;}
		public RaceInfo							Info					{get; set;} = new();
		public RaceParameters				Parameters		{get; set;} = new();
		public RaceGenerals					Generals			{get; set;} = new();
		public List<string>					Traits				{get; set;} = [];
		public List<string>					Skills				{get; set;} = [];
		public string								AlbinaVersion	{get; set;} = string.Empty;
	}
}