using ChariotSanzzo.Components.AlbinaApi.Common;

namespace ChariotSanzzo.Components.AlbinaApi.DTOs {
	public class SpellEffectDto : IEffectDto {
		public Guid							Id						{get; set;}
		public string						Role					{get; set;} = null!;
		public string						Name					{get; set;} = null!;
		public AlbinaTextColor?	Color					{get; set;} = null;
		public DateTime					CreatedAt			{get; set;}
		public DateTime?				UpdatedAt			{get; set;}
		public EffectContent[]	Contents			{get; set;} = [];
		public string						AlbinaVersion	{get; set;} = string.Empty;
		public int							Order					{get; set;} = 0;
	}

	public enum SpellType {
		Unknown,
	};
	public enum SpellSubType {
		Unknown,
	};

	public class SpellInfo : IGenericInfo {
		public string[]	Summary				{get; set;} = [];
		public string[]	Description		{get; set;} = [];
		public string[]	Miscellaneous	{get; set;} = [];
	}
	public class SpellComponents {
		public string	Mana			{get; set;} = string.Empty;
		public string	Stamina		{get; set;} = string.Empty;
		public string	Time			{get; set;} = string.Empty;
		public string	Duration	{get; set;} = string.Empty;
		public string	Form			{get; set;} = string.Empty;
		public string	Range			{get; set;} = string.Empty;
		public string	Area			{get; set;} = string.Empty;
	}
	public class SpellProperties {
		public SpellComponents		Components	{get; set;} = new();
		public ExtrasTableItem[]	Extras			{get; set;} = [];
		public string[]						Chants			{get; set;} = [];
	}
	public class SpellDto {
		public Guid										Id							{get; set;}
		public string									Slug						{get; set;} = null!;
		public string									Name						{get; set;} = null!;
		public SpellType							Type						{get; set;}
		public SpellSubType						SubType					{get; set;}
		public string									IconUrl					{get; set;} = string.Empty;
		public string									BannerUrl				{get; set;} = string.Empty;
		public DateTime								CreatedAt				{get; set;}
		public DateTime?							UpdatedAt				{get; set;}
		public SpellInfo							Info						{get; set;} = new();
		public SpellProperties				Properties			{get; set;} = new();
		public int										DomainLevel			{get; set;}
		public SpellDomain[]					SpellDomains		{get; set;} = [];
		public MagicAttribute[]				MagicAttributes	{get; set;} = [];
		public SpellEffectDto[]				Effects					{get; set;} = [];
		public string									AlbinaVersion		{get; set;} = string.Empty;
	}
}