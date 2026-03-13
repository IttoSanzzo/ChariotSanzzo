using ChariotSanzzo.Components.AlbinaApi.Common;

namespace ChariotSanzzo.Components.AlbinaApi.DTOs {
	public enum MasteryType {
		Unknown,
		Expertise,
		Proficiency,
		Knowledge,
		Craft
	};
	public enum MasterySubType {
		Unknown,
		Agility,
		Intelligence,
		Strength,
		Constitution,
		Technique,
		Charisma,
		Wisdom,
		Singular,
		Multiple,
		General,
		Fighter,
		Production,
		Combatant,
		Armed,
		Armored,
		Navy,
		Defensive,
		Focus,
		CombatStyle,
		Tool
	}

public class MasteryEffectDto : IEffectDto {
		public Guid							Id						{get; set;} = Guid.Empty;
		public string						Role					{get; set;} = null!;
		public string						Name					{get; set;} = null!;
		public AlbinaTextColor?	Color					{get; set;} = null;
		public DateTime					CreatedAt			{get; set;}
		public DateTime?				UpdatedAt			{get; set;}
		public EffectContent[]	Contents			{get; set;} = [];
		public string						AlbinaVersion	{get; set;} = string.Empty;
		public int							Order					{get; set;} = 0;
	}

	public class MasteryInfo : IGenericInfo {
		public string[]	Summary				{get; set;} = [];
		public string[]	Description		{get; set;} = [];
		public string[]	Miscellaneous	{get; set;} = [];
	}
	public class MasteryDto {
		public Guid									Id						{get; set;}
		public string								Slug					{get; set;} = null!;
		public string								Name					{get; set;} = null!;
		public MasteryType					Type					{get; set;}
		public MasterySubType				SubType				{get; set;}
		public string								IconUrl				{get; set;} = string.Empty;
		public string								BannerUrl			{get; set;} = string.Empty;
		public DateTime							CreatedAt			{get; set;}
		public DateTime?						UpdatedAt			{get; set;}
		public MasteryInfo					Info					{get; set;} = new();
		public MasteryEffectDto[]		Effects				{get; set;} = [];
		public string								AlbinaVersion	{get; set;} = string.Empty;
	}
}