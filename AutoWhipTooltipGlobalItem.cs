using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace auto_whipstacking
{
    /// <summary>
    /// 给配置中出现的主鞭 / 副鞭 / Debuff 武器增加提示 Tooltip。
    /// </summary>
    public class AutoWhipTooltipGlobalItem : GlobalItem
    {
        // 让所有物品共用一个实例就够了（只读配置），避免额外开销
        public override bool InstancePerEntity => false;

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            // 跳过空气物品
            if (item == null || item.IsAir)
                return;

            var config = ModContent.GetInstance<AutoWhipConfig>();
            if (config == null)
                return;

            // 如果你希望“关闭总开关时不显示提示”，可以解除下面这行注释：
            // if (!config.EnableAutoSwitch)
            //     return;

            bool isMainWhip   = IsMainWhip(config, item.type);
            bool isSubWhip    = IsSubWhip(config, item.type);
            bool isDebuffWeap = IsDebuffWeapon(config, item.type);

            // 没有出现在任何一个列表里，就不用加提示
            if (!isMainWhip && !isSubWhip && !isDebuffWeap)
                return;

            // 主鞭提示
            if (isMainWhip)
            {
                var line = new TooltipLine(Mod, "AutoWhip_MainWhip",
                    Language.GetTextValue("Mods.auto_whipstacking.Tooltip.MainWhip"))
                {
                    OverrideColor = new Color(120, 190, 255) // 浅蓝色
                };
                tooltips.Add(line);
            }

            // 副鞭提示
            if (isSubWhip)
            {
                var line = new TooltipLine(Mod, "AutoWhip_SubWhip",
                    Language.GetTextValue("Mods.auto_whipstacking.Tooltip.SubWhip"))
                {
                    OverrideColor = new Color(150, 230, 150) // 浅绿色
                };
                tooltips.Add(line);
            }

            // Debuff 武器提示
            if (isDebuffWeap)
            {
                var line = new TooltipLine(Mod, "AutoWhip_DebuffWeapon",
                    Language.GetTextValue("Mods.auto_whipstacking.Tooltip.DebuffWeapon"))
                {
                    OverrideColor = new Color(230, 120, 120) // 带点警示感的红色
                };
                tooltips.Add(line);
            }
        }

        // ======== 辅助判定方法，统一做空指针 & 合法性检查 ========

        private static bool IsMainWhip(AutoWhipConfig config, int itemType)
        {
            if (config.MainWhips == null)
                return false;

            // MainWhips 里是 ItemDefinition
            return config.MainWhips.Any(m => m != null && m.Type == itemType);
        }

        private static bool IsSubWhip(AutoWhipConfig config, int itemType)
        {
            if (config.WhipBuffPairs == null)
                return false;

            // 副鞭条目：要求条目非空、WhipItem 非空且 Type > 0
            return config.WhipBuffPairs.Any(p =>
                p != null &&
                p.WhipItem != null &&
                p.WhipItem.Type > 0 &&
                p.WhipItem.Type == itemType);
        }

        private static bool IsDebuffWeapon(AutoWhipConfig config, int itemType)
        {
            if (config.DebuffWeapons == null)
                return false;

            // Debuff 武器条目：要求条目非空、Weapon 非空且 Type > 0
            return config.DebuffWeapons.Any(d =>
                d != null &&
                d.Weapon != null &&
                d.Weapon.Type > 0 &&
                d.Weapon.Type == itemType);
        }
    }
}
