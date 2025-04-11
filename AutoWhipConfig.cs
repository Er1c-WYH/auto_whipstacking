using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using Terraria.ID;

namespace auto_whipstacking
{
    public class AutoWhipConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [LabelKey("$Mods.auto_whipstacking.Config.EnableAutoSwitch.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.EnableAutoSwitch.Tooltip")]
        [DefaultValue(true)]
        public bool EnableAutoSwitch { get; set; } // 总开关

        [LabelKey("$Mods.auto_whipstacking.Config.LogEnabled.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.LogEnabled.Tooltip")]
        [DefaultValue(false)]
        public bool LogEnabled { get; set; } // 日志开关

        [Header("$Mods.auto_whipstacking.Config.Headers.MainWhips")] // 主鞭头部

        [LabelKey("$Mods.auto_whipstacking.Config.MainWhipDuration.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.MainWhipDuration.Tooltip")]
        [DefaultValue(60)]
        [Range(1, 300)]
        public int MainWhipDuration { get; set; } // 主鞭使用时长

        [LabelKey("$Mods.auto_whipstacking.Config.MainWhips.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.MainWhips.Tooltip")]
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

        [Header("$Mods.auto_whipstacking.Config.Headers.SubWhips")] // 副鞭头部

        [LabelKey("$Mods.auto_whipstacking.Config.EnableSubWhip.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.EnableSubWhip.Tooltip")]
        [DefaultValue(true)]
        public bool EnableSubWhip { get; set; } // 副鞭开关

        [LabelKey("$Mods.auto_whipstacking.Config.UseBuffTimeThreshold.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.UseBuffTimeThreshold.Tooltip")]
        [DefaultValue(false)]
        public bool UseBuffTimeThreshold { get; set; } // 启用buff时间阈值判断

        [LabelKey("$Mods.auto_whipstacking.Config.BuffTimeThreshold.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.BuffTimeThreshold.Tooltip")]
        [Range(1, 600)]
        [DefaultValue(30)]
        public int BuffTimeThreshold { get; set; } // buff时间阈值

        [LabelKey("$Mods.auto_whipstacking.Config.WhipBuffPairs.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.WhipBuffPairs.Tooltip")]
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

        [Header("$Mods.auto_whipstacking.Config.Headers.DebuffWeapons")] // debuff武器头部

        [LabelKey("$Mods.auto_whipstacking.Config.EnableDebuffWeapon.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.EnableDebuffWeapon.Tooltip")]
        [DefaultValue(true)]
        public bool EnableDebuffWeapon { get; set; } // debuff武器开关

        [LabelKey("$Mods.auto_whipstacking.Config.DebuffWeapons.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.DebuffWeapons.Tooltip")]
        public List<DebuffWeaponConfig> DebuffWeapons { get; set; } = new()
        {
            new()
            {
                Weapon = new ItemDefinition(ItemID.ApprenticeStaffT3),
                Interval = 8
            },
        };
    }

    public class WhipBuffPair
    {
        [LabelKey("$Mods.auto_whipstacking.Config.WhipBuffPair.WhipItem.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.WhipBuffPair.WhipItem.Tooltip")]
        public ItemDefinition WhipItem;

        [LabelKey("$Mods.auto_whipstacking.Config.WhipBuffPair.Buff.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.WhipBuffPair.Buff.Tooltip")]
        public BuffDefinition Buff;
    }

    public class DebuffWeaponConfig
    {
        [LabelKey("$Mods.auto_whipstacking.Config.DebuffWeapon.Weapon.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.DebuffWeapon.Weapon.Tooltip")]
        public ItemDefinition Weapon;

        [LabelKey("$Mods.auto_whipstacking.Config.DebuffWeapon.Interval.Label")]
        [TooltipKey("$Mods.auto_whipstacking.Config.DebuffWeapon.Interval.Tooltip")]
        [Range(1, 60)]
        [DefaultValue(5)]
        public int Interval;
    }
}
