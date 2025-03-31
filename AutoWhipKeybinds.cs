using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace auto_whipstacking
{
    public class AutoWhipKeybinds : ModSystem
    {
        // 控制自动切鞭功能的快捷键
        public static ModKeybind ToggleSwitchHotkey;

        // 控制自动切鞭是否启用
        public static bool SwitchingEnabled = true;

        public override void Load()
        {
            // 注册快捷键（在控件设置中显示为本地化名称）
            ToggleSwitchHotkey = KeybindLoader.RegisterKeybind(
                Mod,
                "ToggleSwitch",
                "None"
            );
        }

        public override void Unload()
        {
            ToggleSwitchHotkey = null;
        }

        public override void PostUpdateInput()
        {
            if (ToggleSwitchHotkey.JustPressed)
            {
                // 切换启用状态
                SwitchingEnabled = !SwitchingEnabled;

                // 使用静态字符串拼接以正确获取本地化
                string status = SwitchingEnabled ? "On" : "Off";
                string key = "Mods.auto_whipstacking.SwitchToggle." + status;

                Main.NewText(Language.GetTextValue(key));
            }
        }
    }
}
