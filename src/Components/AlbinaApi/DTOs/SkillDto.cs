using ChariotSanzzo.Components.AlbinaApi.Common;

namespace ChariotSanzzo.Components.AlbinaApi.DTOs {
	public class SkillEffectDto : IEffectDto {
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

	public enum SkillType {
		Unknown,
		Generic,
		Common,
		Racial,
		Unique
	};
	public enum SkillSubType {
		Unknown,
		MajorAction,
		MinorAction,
		MajorReaction,
		MinorReaction
	};

	public class SkillInfo : IGenericInfo {
		public string[]	Summary				{get; set;} = [];
		public string[]	Description		{get; set;} = [];
		public string[]	Miscellaneous	{get; set;} = [];
	}
	public class SkillComponents {
		public string	Mana			{get; set;} = string.Empty;
		public string	Stamina		{get; set;} = string.Empty;
		public string	Time			{get; set;} = string.Empty;
		public string	Duration	{get; set;} = string.Empty;
		public string	Form			{get; set;} = string.Empty;
		public string	Range			{get; set;} = string.Empty;
		public string	Area			{get; set;} = string.Empty;
	}
	public class SkillProperties {
		public SkillComponents		Components	{get; set;} = new();
		public ExtrasTableItem[]	Extras			{get; set;} = [];
	}
	public class SkillDto {
		public Guid										Id						{get; set;}
		public string									Slug					{get; set;} = null!;
		public string									Name					{get; set;} = null!;
		public SkillType							Type					{get; set;}
		public SkillSubType						SubType				{get; set;}
		public string									IconUrl				{get; set;} = string.Empty;
		public string									BannerUrl			{get; set;} = string.Empty;
		public DateTime								CreatedAt			{get; set;}
		public DateTime?							UpdatedAt			{get; set;}
		public SkillInfo							Info					{get; set;} = new();
		public SkillProperties				Properties		{get; set;} = new();
		public SkillEffectDto[]				Effects				{get; set;} = [];
		public string									AlbinaVersion	{get; set;} = string.Empty;
	}
}