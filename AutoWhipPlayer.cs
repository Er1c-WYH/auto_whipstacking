using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using System;

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
        private bool pendingReturnToInitialWeapon = false;

        private int switchCooldown = 0;

        private Dictionary<int, int> debuffWeaponTimers = new();
        private bool isInDebuffState = false;
        private int returnWeaponType = -1;
        private int returnFromSubWhipType = -1;
        private int savedMainWhipTimer = 0;

        private Dictionary<int, int> itemIndexCache = new();

        private bool wasPlayerInventoryOpenLastFrame = false; // 用于判断“刚关闭背包”

        public override void PostUpdate()
        {
            var config = ModContent.GetInstance<AutoWhipConfig>();
            itemIndexCache.Clear();

            if (!config.EnableAutoSwitch || !AutoWhipKeybinds.SwitchingEnabled)
                return;

            if (!Main.playerInventory && wasPlayerInventoryOpenLastFrame)
            {
                bool isValidHeld = Player.HeldItem != null && !Player.HeldItem.IsAir && Player.HeldItem.damage > 0;

                if ((Player.selectedItem >= 10 && !isValidHeld) ||
                    (Player.selectedItem >= 10 && isValidHeld && Player.HeldItem.type != initialWeaponType))
                {
                    if (initialWeaponType <= 0) return;

                    int index = FindItemIndex(initialWeaponType);
                    if (index >= 0 && index < 10 && !Player.inventory[index].IsAir)
                    {
                        Player.selectedItem = index;
                        Player.SetDummyItemTime(1);
                        if (config.LogEnabled)
                            Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.RestoreInitialWeapon"));
                    }
                    else
                    {
                        Player.selectedItem = 0;
                    }
                }
            }

            bool enableMainWhip = config.EnableMainWhip;
            bool enableSubWhip = config.EnableSubWhip;
            bool enableDebuffWeapon = config.EnableDebuffWeapon;

            // ✅ 更安全的攻击判断
            bool isAttacking = Player.controlUseItem && Main.hasFocus &&
                               Player.HeldItem != null && !Player.HeldItem.IsAir &&
                               Player.HeldItem.damage > 0 && Player.HeldItem.useStyle > ItemUseStyleID.None;

            if (pendingReturnToInitialWeapon && Player.itemAnimation <= 1 && Player.itemTime <= 1)
            {
                if (initialWeaponType > 0)
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
                pendingReturnToInitialWeapon = false;
            }

            if (Main.playerInventory)
            {
                wasAttackingLastFrame = false;
                pendingReturnToInitialWeapon = false;
                wasPlayerInventoryOpenLastFrame = true;
                return;
            }

            var validDebuffWeapons = config.DebuffWeapons
                .Where(d => d.Weapon.Type > 0 && Player.inventory.Any(i => i != null && !i.IsAir && i.type == d.Weapon.Type))
                .ToList();

            if (debuffWeaponTimers.Count != validDebuffWeapons.Count)
            {
                debuffWeaponTimers.Clear();
                foreach (var debuff in validDebuffWeapons)
                    debuffWeaponTimers[debuff.Weapon.Type] = 0;
            }

            foreach (var debuff in validDebuffWeapons)
                debuffWeaponTimers[debuff.Weapon.Type]++;

            if (isInDebuffState)
            {
                if (Player.itemAnimation <= 1 && Player.itemTime <= 1)
                {
                    isInDebuffState = false;
                    mainWhipTimer = savedMainWhipTimer;

                    int index = FindItemIndex(returnWeaponType);
                    if (index >= 0)
                    {
                        Player.selectedItem = index;
                        Player.SetDummyItemTime(1);
                        switchCooldown = 10;
                        if (config.LogEnabled)
                            Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.RestoreAfterDebuffWeapon") + Player.inventory[index].Name);
                    }
                }
                wasPlayerInventoryOpenLastFrame = false;
                return;
            }

            if (!isAttacking)
            {
                if (wasAttackingLastFrame && initialWeaponType != -1 && Player.HeldItem.type != initialWeaponType)
                {
                    if (Player.itemAnimation <= 1 && Player.itemTime <= 1)
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
                    else
                    {
                        pendingReturnToInitialWeapon = true;
                    }
                }

                wasAttackingLastFrame = false;
                wasPlayerInventoryOpenLastFrame = false;
                return;
            }

            var heldItem = Player.HeldItem;
            if (heldItem == null || heldItem.IsAir || heldItem.damage <= 0 || heldItem.useStyle <= ItemUseStyleID.None)
            {
                wasPlayerInventoryOpenLastFrame = false;
                return;
            }

            bool isMainWhip = config.MainWhips.Any(w => w.Type == heldItem.type);
            bool isSubWhip = config.WhipBuffPairs.Any(p => p.WhipItem.Type == heldItem.type);

            if (!enableMainWhip && !isMainWhip && !isSubWhip)
            {
                wasAttackingLastFrame = false;
                wasPlayerInventoryOpenLastFrame = false;
                return;
            }

            if (!wasAttackingLastFrame)
            {
                wasAttackingLastFrame = true;
                pendingReturnToInitialWeapon = false;

                // ✅ 只有快捷栏里的武器才记录为初始主武器
                if (Player.selectedItem >= 0 && Player.selectedItem < 10)
                {
                    initialWeaponType = heldItem.type;
                }

                if (!isMainWhip && !isSubWhip)
                {
                    wasAttackingLastFrame = false;
                    wasPlayerInventoryOpenLastFrame = false;
                    return;
                }

                mainWhipTimer = 0;
                UpdateCurrentMainWhips(config);
            }
            else if (!isInSubWhipState)
            {
                mainWhipTimer++;
            }

            if (enableDebuffWeapon && (isMainWhip || isSubWhip))
            {
                var readyList = validDebuffWeapons
                    .Where(d => debuffWeaponTimers[d.Weapon.Type] >= d.Interval * 60)
                    .OrderByDescending(d => Player.inventory.FirstOrDefault(i => i.type == d.Weapon.Type)?.damage ?? 0)
                    .ToList();

                if (readyList.Count > 0)
                {
                    var selected = readyList[0];
                    int index = FindItemIndex(selected.Weapon.Type);
                    if (index >= 0)
                    {
                        returnWeaponType = heldItem.type;
                        savedMainWhipTimer = mainWhipTimer;

                        Player.selectedItem = index;
                        Player.SetDummyItemTime(1);
                        isInDebuffState = true;
                        debuffWeaponTimers[selected.Weapon.Type] = 0;
                        if (config.LogEnabled)
                            Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.SwitchToDebuffWeapon") + Player.inventory[index].Name);
                        wasPlayerInventoryOpenLastFrame = false;
                        return;
                    }
                }
            }

            if (enableSubWhip)
            {
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
                            if (index >= 0 && Player.itemAnimation <= 1 && Player.itemTime <= 1)
                            {
                                returnFromSubWhipType = heldItem.type;
                                Player.selectedItem = index;
                                Player.SetDummyItemTime(1);
                                switchCooldown = 10;
                                isInSubWhipState = true;
                                if (config.LogEnabled)
                                    Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.SwitchToSubWhip") + bestSub.Name);
                            }
                        }
                    wasPlayerInventoryOpenLastFrame = false;
                    return;
                }
            }
            else
            {
                isInSubWhipState = false;
            }

            if (isInSubWhipState)
            {
                if (Player.itemAnimation <= 1 && Player.itemTime <= 1)
                {
                    isInSubWhipState = false;
                    int index = FindItemIndex(returnFromSubWhipType);
                    if (index >= 0)
                    {
                        Player.selectedItem = index;
                        Player.SetDummyItemTime(1);
                        switchCooldown = 10;
                        if (config.LogEnabled)
                            Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.SwitchToMainWhip") + Player.inventory[index].Name);
                    }
                }
                wasPlayerInventoryOpenLastFrame = false;
                return;
            }

            if (enableMainWhip && !isInSubWhipState && mainWhipTimer >= config.MainWhipDuration && currentMainWhips.Count > 1)
            {
                if (Player.itemAnimation <= 1 && Player.itemTime <= 1)
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

            // ✅ 最后更新“上一帧背包状态”
            wasPlayerInventoryOpenLastFrame = Main.playerInventory;
        }

        private void UpdateCurrentMainWhips(AutoWhipConfig config)
        {
            currentMainWhips = Player.inventory
                .Where(i => i != null && !i.IsAir && config.MainWhips.Any(m => m.Type == i.type))
                .GroupBy(i => i.type) // 按类型分组
                .Select(g => g.First()) // 每种只取一个
                .OrderByDescending(i => i.damage)
                .ToList();

            mainWhipIndex = 0;
        }

        private int FindItemIndex(int type)
        {
            if (itemIndexCache.TryGetValue(type, out int cached))
                return cached;

            int index = Array.FindIndex(Player.inventory, i => i != null && i.type == type);
            if (index >= 0) itemIndexCache[type] = index;
            return index;
        }

        private List<WhipBuffPair> GetMissingBuffPairs(AutoWhipConfig config)
        {
            return config.WhipBuffPairs
                .Where(p => Player.inventory.Any(i => i != null && !i.IsAir && i.type == p.WhipItem.Type))
                .Where(p =>
                {
                    if (!Player.HasBuff(p.Buff.Type))
                        return true;

                    if (config.UseBuffTimeThreshold)
                    {
                        for (int i = 0; i < Player.buffType.Length; i++)
                        {
                            if (Player.buffType[i] == p.Buff.Type)
                            {
                                return Player.buffTime[i] < config.BuffTimeThreshold;
                            }
                        }
                    }

                    return false;
                })
                .ToList();
        }
    }
}
