using ChariotSanzzo.Components.AlbinaApi.Common;

namespace ChariotSanzzo.Components.AlbinaApi.DTOs {
	public enum ItemType {
		Unknown,
		Armament,
		Focus,
		Shielding,
		Frame,
		Wearable,
		Accessory,
		Consumable,
		Tool,
		Miscellaneous,
		Special,
		Random
	};
	public enum ItemSubType {
		Unknown,
		Grimoire,
		Staff,
		Scepter,
		Wand,
		Orb,
		ShortBlade,
		Sword,
		Axe,
		Bow,
		Crossbow,
		Polearm,
		ConcussiveWeapon,
		FireWeapon,
		TetheredWeapon,
		BluntWeapon,
		LightShield,
		MediumShield,
		HeavyShield,
		LightFrame,
		MediumFrame,
		HeavyFrame,
		Relic,
		Potion,
		Container,
		Material,
		ThrownWeapon,
		Charm,
		Catalyst,
		Codex,
		Ring,
		Amulet,
		Scroll,
		Food,
		Tool,
		Key,
		Toy,
		Random,
	};

	public class ItemEffectDto : IEffectDto {
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

	public class ItemInfo : IGenericInfo {
		public string[]	Summary				{get; set;} = [];
		public string[]	Description		{get; set;} = [];
		public string[]	Miscellaneous	{get; set;} = [];
	}
	public class EquipmentStats {
		public string	Damage			{get; set;} = string.Empty;
		public string	Accuracy		{get; set;} = string.Empty;
		public string	Defense			{get; set;} = string.Empty;
		public string	DamageType	{get; set;} = string.Empty;
		public string	Range				{get; set;} = string.Empty;
	}
	public class ItemProperties {
		public string							Slot			{get; set;} = string.Empty;
		public string							Attribute	{get; set;} = string.Empty;
		public EquipmentStats			Stats			{get; set;} = new();
		public ExtrasTableItem[]	Extras		{get; set;} = [];
	}
	public class ItemDto {
		public Guid									Id						{get; set;}
		public string								Slug					{get; set;} = null!;
		public string								Name					{get; set;} = null!;
		public ItemType							Type					{get; set;}
		public ItemSubType					SubType				{get; set;}
		public string								IconUrl				{get; set;} = string.Empty;
		public string								BannerUrl			{get; set;} = string.Empty;
		public DateTime							CreatedAt			{get; set;}
		public DateTime?						UpdatedAt			{get; set;}
		public ItemInfo							Info					{get; set;} = new();
		public ItemProperties				Properties		{get; set;} = new();
		public ItemEffectDto[]			Effects		{get; set;} = [];
		public string								AlbinaVersion	{get; set;} = string.Empty;
	}
}