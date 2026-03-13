using ChariotSanzzo.Components.AlbinaApi.Common;

namespace ChariotSanzzo.Components.AlbinaApi.DTOs {
	public class TraitEffectDto : IEffectDto {
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

	public enum TraitType {
		Unknown,
		Generic,
		Racial,
		Talent,
		Blessing,
		Unique,
	};
	public enum TraitSubType {
		Unknown,
	};

	public class TraitInfo : IGenericInfo {
		public string[]	Summary				{get; set;} = [];
		public string[]	Description		{get; set;} = [];
		public string[]	Miscellaneous	{get; set;} = [];
		public string[]	Requirements	{get; set;} = [];
	}

	public class TraitDto {
		public Guid										Id						{get; set;}
		public string									Slug					{get; set;} = null!;
		public string									Name					{get; set;} = null!;
		public TraitType							Type					{get; set;}
		public TraitSubType						SubType				{get; set;}
		public string									IconUrl				{get; set;} = string.Empty;
		public string									BannerUrl			{get; set;} = string.Empty;
		public DateTime								CreatedAt			{get; set;}
		public DateTime?							UpdatedAt			{get; set;}
		public TraitInfo							Info					{get; set;} = new();
		public TraitEffectDto[]				Effects				{get; set;} = [];
		public string									AlbinaVersion	{get; set;} = string.Empty;
	}
}