using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

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
        [DefaultValue(false)]
        public bool UseBuffTimeThreshold;

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.BuffTimeThreshold.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.BuffTimeThreshold.Tooltip")]
        [Range(1, 600)]
        [DefaultValue(60)]
        public int BuffTimeThreshold;

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.MainWhips.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.MainWhips.Tooltip")]
        public List<ItemDefinition> MainWhips = new();

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.MainWhipDuration.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.MainWhipDuration.Tooltip")]
        [DefaultValue(60)]
        [Range(1, 300)]
        public int MainWhipDuration;

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.WhipBuffPairs.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.WhipBuffPairs.Tooltip")]
        public List<WhipBuffPair> WhipBuffPairs = new();

        [LabelKey("$Mods.auto_whipstacking.AutoWhipConfig.LogEnabled.Label")]
        [TooltipKey("$Mods.auto_whipstacking.AutoWhipConfig.LogEnabled.Tooltip")]
        [DefaultValue(false)]
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
}
