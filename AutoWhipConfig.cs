using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ID;

namespace auto_whipstacking
{
    public class AutoWhipConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.EnableAutoSwitch.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.EnableAutoSwitch.Tooltip")]
        [DefaultValue(true)]
        public bool EnableAutoSwitch;

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.UseBuffTimeThreshold.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.UseBuffTimeThreshold.Tooltip")]
        [DefaultValue(true)]
        public bool UseBuffTimeThreshold;

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.BuffTimeThreshold.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.BuffTimeThreshold.Tooltip")]
        [Range(1, 600)]
        [DefaultValue(30)]
        public int BuffTimeThreshold;

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.MainWhips.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.MainWhips.Tooltip")]
        public List<ItemDefinition> MainWhips { get; set; } = new()
        {
            new(ItemID.BlandWhip),
            new(ItemID.ThornWhip),
            new(ItemID.BoneWhip),
            new(ItemID.FireWhip),
            new(ItemID.CoolWhip),
            new(ItemID.SwordWhip),
            new(ItemID.MaceWhip),
            new(ItemID.ScytheWhip),
            new(ItemID.RainbowWhip),
        };

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.MainWhipDuration.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.MainWhipDuration.Tooltip")]
        [DefaultValue(60)]
        [Range(1, 300)]
        public int MainWhipDuration;

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.WhipBuffPairs.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.WhipBuffPairs.Tooltip")]
        public List<WhipBuffPair> WhipBuffPairs { get; set; } = new()
        {
            new()
            {
                WhipItem = new ItemDefinition(ItemID.ThornWhip),
                Buff = new BuffDefinition(BuffID.ThornWhipPlayerBuff)
            },
            new()
            {
                WhipItem = new ItemDefinition(ItemID.CoolWhip),
                Buff = new BuffDefinition(BuffID.CoolWhipPlayerBuff)
            },
            new()
            {
                WhipItem = new ItemDefinition(ItemID.SwordWhip),
                Buff = new BuffDefinition(BuffID.SwordWhipPlayerBuff)
            },
            new()
            {
                WhipItem = new ItemDefinition(ItemID.ScytheWhip),
                Buff = new BuffDefinition(BuffID.ScytheWhipPlayerBuff)
            },
        };

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.DebuffWeapons.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.DebuffWeapons.Tooltip")]
        public List<DebuffWeaponConfig> DebuffWeapons { get; set; } = new()
        {
            new()
            {
                Weapon = new ItemDefinition(ItemID.ApprenticeStaffT3),
                Interval = 8
            },
        };

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.LogEnabled.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.LogEnabled.Tooltip")]
        [DefaultValue(true)]
        public bool LogEnabled;
    }

    public class WhipBuffPair
    {
        [LabelKey("$Mods.auto_whipstacking.WhipBuffPair.WhipItemID.Label")]
        [TooltipKey("$Mods.auto_whipstacking.WhipBuffPair.WhipItemID.Tooltip")]
        public ItemDefinition WhipItem;

        [LabelKey("$Mods.auto_whipstacking.WhipBuffPair.BuffID.Label")]
        [TooltipKey("$Mods.auto_whipstacking.WhipBuffPair.BuffID.Tooltip")]
        public BuffDefinition Buff;
    }

    public class DebuffWeaponConfig
    {
        [LabelKey("$Mods.auto_whipstacking.DebuffWeaponConfig.Weapon.Label")]
        [TooltipKey("$Mods.auto_whipstacking.DebuffWeaponConfig.Weapon.Tooltip")]
        public ItemDefinition Weapon;

        [LabelKey("$Mods.auto_whipstacking.DebuffWeaponConfig.Interval.Label")]
        [TooltipKey("$Mods.auto_whipstacking.DebuffWeaponConfig.Interval.Tooltip")]
        [Range(1, 60)]
        [DefaultValue(5)]
        public int Interval;
    }
}
