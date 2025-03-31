// AutoWhipPlayer.cs - 稳定版 + 起手武器限制 + 防闪现攻击

using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;

namespace auto_whipstacking
{
    public class AutoWhipPlayer : ModPlayer
    {
        private List<Item> currentMainWhips = new();
        private int mainWhipIndex = 0;
        private int mainWhipTimer = 0;
        private bool isInSubWhipState = false;

        private int initialWeaponType = -1;
        private bool wasAttackingLastFrame = false;

        private int switchCooldown = 0;

        public override void PostUpdate()
        {
            if (Main.playerInventory || !AutoWhipKeybinds.SwitchingEnabled)
                return;

            var config = ModContent.GetInstance<AutoWhipConfig>();
            if (!config.EnableAutoSwitch || config.MainWhips.Count == 0)
                return;

            if (switchCooldown > 0)
            {
                switchCooldown--;
                return;
            }

            var heldItem = Player.HeldItem;
            if (heldItem == null || heldItem.damage <= 0 || heldItem.useStyle <= ItemUseStyleID.None)
                return;

            bool isAttacking = Main.mouseLeft && Main.hasFocus;

            if (!isAttacking)
            {
                if (wasAttackingLastFrame && initialWeaponType != -1 && Player.HeldItem.type != initialWeaponType)
                {
                    int index = FindItemIndex(initialWeaponType);
                    if (index >= 0)
                    {
                        Player.selectedItem = index;
                        Player.SetDummyItemTime(1);
                        switchCooldown = 10;
                        if (config.LogEnabled)
                            Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.RestoreInitialWeapon"));
                    }
                }
                wasAttackingLastFrame = false;
                return;
            }

            if (!wasAttackingLastFrame)
            {
                wasAttackingLastFrame = true;
                initialWeaponType = heldItem.type;

                // ✅ 起手武器必须是主鞭或副鞭，否则不启动自动切鞭逻辑
                bool isMainWhip = config.MainWhips.Any(w => w.Type == heldItem.type);
                bool isSubWhip = config.WhipBuffPairs.Any(p => p.WhipItem.Type == heldItem.type);
                if (!isMainWhip && !isSubWhip)
                {
                    wasAttackingLastFrame = false;
                    return;
                }

                mainWhipTimer = 0;
                UpdateCurrentMainWhips(config);
            }
            else if (!isInSubWhipState)
            {
                mainWhipTimer++;
            }

            var missingBuffs = GetMissingBuffPairs(config);
            if (missingBuffs.Count > 0)
            {
                var bestSub = Player.inventory
                    .Where(i => i != null && !i.IsAir && missingBuffs.Any(p => p.WhipItem.Type == i.type))
                    .OrderByDescending(i => i.damage)
                    .FirstOrDefault();

                if (bestSub != null && heldItem.type != bestSub.type)
                {
                    int index = FindItemIndex(bestSub.type);
                    if (index >= 0)
                    {
                        Player.selectedItem = index;
                        Player.SetDummyItemTime(1);
                        switchCooldown = 10;
                        isInSubWhipState = true;
                        if (config.LogEnabled)
                            Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.SwitchToSubWhip") + bestSub.Name);
                    }
                }
                return;
            }

            if (isInSubWhipState)
            {
                isInSubWhipState = false;
                if (currentMainWhips.Count > 0)
                {
                    int index = FindItemIndex(currentMainWhips[mainWhipIndex].type);
                    if (index >= 0)
                    {
                        Player.selectedItem = index;
                        Player.SetDummyItemTime(1);
                        switchCooldown = 10;
                        if (config.LogEnabled)
                            Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.SwitchToMainWhip") + Player.inventory[index].Name);
                    }
                }
                return;
            }

            if (mainWhipTimer >= config.MainWhipDuration && currentMainWhips.Count > 1)
            {
                mainWhipTimer = 0;
                mainWhipIndex = (mainWhipIndex + 1) % currentMainWhips.Count;
                var next = currentMainWhips[mainWhipIndex];

                if (heldItem.type != next.type)
                {
                    int index = FindItemIndex(next.type);
                    if (index >= 0)
                    {
                        Player.selectedItem = index;
                        Player.SetDummyItemTime(1);
                        switchCooldown = 10;
                        if (config.LogEnabled)
                            Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.SwitchToMainWhip") + next.Name);
                    }
                }
            }
        }

        private void UpdateCurrentMainWhips(AutoWhipConfig config)
        {
            currentMainWhips = Player.inventory
                .Where(i => i != null && !i.IsAir && config.MainWhips.Any(m => m.Type == i.type))
                .OrderByDescending(i => i.damage)
                .ToList();
            mainWhipIndex = 0;
        }

        private int FindItemIndex(int type)
        {
            return System.Array.FindIndex(Player.inventory, i => i != null && i.type == type);
        }

        private List<WhipBuffPair> GetMissingBuffPairs(AutoWhipConfig config)
        {
            return config.WhipBuffPairs
                .Where(p => Player.inventory.Any(i => i != null && !i.IsAir && i.type == p.WhipItem.Type))
                .Where(p => !Player.HasBuff(p.Buff.Type))
                .ToList();
        }
    }
}
