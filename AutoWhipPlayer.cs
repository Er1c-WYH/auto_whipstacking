// AutoWhipPlayer.cs - 主/副武器 + Debuff 武器完整自动切换逻辑（仅攻击状态触发）

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

        private int switchCooldown = 0;

        private Dictionary<int, int> debuffWeaponTimers = new();
        private bool isInDebuffState = false;
        private int returnWeaponType = -1;

        public override void PostUpdate()
        {
            var config = ModContent.GetInstance<AutoWhipConfig>();

            if (!config.EnableAutoSwitch || !AutoWhipKeybinds.SwitchingEnabled)
                return;

            bool isAttacking = Main.mouseLeft && Main.hasFocus;

            // 初始化计时器（按需）
            if (debuffWeaponTimers.Count != config.DebuffWeapons.Count)
            {
                debuffWeaponTimers.Clear();
                foreach (var debuff in config.DebuffWeapons)
                {
                    debuffWeaponTimers[debuff.Weapon.Type] = 0;
                }
            }

            if (!isAttacking)
            {
                // 攻击松开时，处理恢复
                if (isInDebuffState)
                {
                    isInDebuffState = false;
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
                return; // 玩家未攻击，不处理任何切换
            }

            var heldItem = Player.HeldItem;
            if (heldItem == null || heldItem.damage <= 0 || heldItem.useStyle <= ItemUseStyleID.None)
                return;

            bool isMainWhip = config.MainWhips.Any(w => w.Type == heldItem.type);
            bool isSubWhip = config.WhipBuffPairs.Any(p => p.WhipItem.Type == heldItem.type);

            if (!wasAttackingLastFrame)
            {
                wasAttackingLastFrame = true;
                initialWeaponType = heldItem.type;

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

            // 攻击中，更新计时器
            foreach (var key in debuffWeaponTimers.Keys.ToList())
            {
                if (debuffWeaponTimers[key] < int.MaxValue)
                    debuffWeaponTimers[key]++;
            }

            // 仅当当前手持主/副武器时，才触发 Debuff 武器切换
            if (isMainWhip || isSubWhip)
            {
                var readyList = config.DebuffWeapons
                    .Where(d =>
                        Player.inventory.Any(i => i != null && !i.IsAir && i.type == d.Weapon.Type) &&
                        debuffWeaponTimers.TryGetValue(d.Weapon.Type, out int t) &&
                        t >= d.Interval * 60)
                    .OrderByDescending(d => Player.inventory.First(i => i.type == d.Weapon.Type).damage)
                    .ToList();

                if (readyList.Count > 0)
                {
                    var selected = readyList[0];
                    int index = FindItemIndex(selected.Weapon.Type);
                    if (index >= 0)
                    {
                        returnWeaponType = Player.HeldItem.type;
                        Player.selectedItem = index;
                        Player.SetDummyItemTime(1);
                        isInDebuffState = true;
                        debuffWeaponTimers[selected.Weapon.Type] = 0;

                        if (config.LogEnabled)
                            Main.NewText(Language.GetTextValue("Mods.auto_whipstacking.SwitchToDebuffWeapon") + Player.inventory[index].Name);
                        return;
                    }
                }
            }

            if (Main.playerInventory || !AutoWhipKeybinds.SwitchingEnabled)
                return;

            if (!config.EnableAutoSwitch || config.MainWhips.Count == 0)
                return;

            if (switchCooldown > 0)
            {
                switchCooldown--;
                return;
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
            return Array.FindIndex(Player.inventory, i => i != null && i.type == type);
        }

        private List<WhipBuffPair> GetMissingBuffPairs(AutoWhipConfig config)
        {
            return config.WhipBuffPairs
                .Where(p => Player.inventory.Any(i => i != null && !i.IsAir && i.type == p.WhipItem.Type))
                .Where(p =>
                {
                    // 如果没有 buff，肯定是缺失
                    if (!Player.HasBuff(p.Buff.Type))
                        return true;

                    // 如果启用了时间阈值判断，则根据剩余时间判断是否“视为缺失”
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