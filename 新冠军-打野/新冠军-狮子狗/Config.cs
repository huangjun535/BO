using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace Rengar
{
    public static class Config
    {
        public static Menu Menu, Modes, Draw, Magnet, Targetting, Combo, Harass, LaneClear, JungleClear, Auto, Assasinate;
        private static int _lastSwitchTick , _lastAssasinateSwitch;
        private static List<string> heroList = new List<string>();
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static void BadaoActivate()
        {
            Variables.Q = new Spell(SpellSlot.Q);
            Variables.W = new Spell(SpellSlot.W, 300);
            Variables.E = new Spell(SpellSlot.E, 1000);
            Variables.R = new Spell(SpellSlot.R);
            Variables.E.SetSkillshot(0.25f, 70, 1500, true, SkillshotType.SkillshotLine);
            Variables.E.MinHitChance = HitChance.Medium;
            Variables.E2 = new Spell(SpellSlot.E, 1000);
            Variables.E2.SetSkillshot(0.25f, 70, 1500, true, SkillshotType.SkillshotLine);
            Variables.E2.MinHitChance = HitChance.Medium;
            Variables.W.SetSkillshot(0.25f, 500, 2000, false, SkillshotType.SkillshotCircle);
            Variables.W.MinHitChance = HitChance.Medium;
            foreach (var spell in
                        Player.Spellbook.Spells.Where(
                          i =>
                                i.Name.ToLower().Contains("smite") &&
            (i.Slot == SpellSlot.Summoner1 || i.Slot == SpellSlot.Summoner2)))
            {
                Variables.Smite = spell.Slot;
            }



            Menu = new Menu("新冠军狮子狗Q群438230879", "YuleRengar", true);

            var orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            Variables.Orbwalker = new Rengar.Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);
            var ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            // Modes
            Modes = Menu.AddSubMenu(new Menu("连招模式", "连招模式"));

            // Combo
            Combo = Modes.AddSubMenu(new Menu("Combo", "Combo"));
            Variables.ComboSmite = Combo.AddItem(new MenuItem("comboUseSmite", "Use Smite").SetValue(true));
            Variables.ComboYoumuu = Combo.AddItem(new MenuItem("comboUseYoumuu", "大招开启幽梦").SetValue(true));
            Variables.ComboMode = Combo.AddItem(new MenuItem("comboMode", "Combo Mode").SetValue(new StringList(new[] { "Snare", "Burst", "Auto", "Always Q", "AP mode" }, 0)));
            Variables.ComboSwitchKey = Combo.AddItem(new MenuItem("ComboSwitch", "连招转换开关").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Variables.ComboResetAA = Combo.AddItem(new MenuItem("ComboResetAA", "总是用AQA重置普攻").SetValue(false));

            //Harass
            //Harass = Modes.AddSubMenu(new Menu("Harass", "Harass"));
            //Variables.HarassW = Harass.AddItem(new MenuItem("harassUseW", "Use W").SetValue(true));
            //Variables.HarassE = Harass.AddItem(new MenuItem("harassUseE", "Use E").SetValue(true));

            //Assasinate
            Assasinate = Modes.AddSubMenu(new Menu("刺杀", "Assasinate"));
            Variables.AssasinateInstruction = Assasinate.AddItem(new MenuItem("AssasinateInstruction", "只对左键选定的目标"));
            Variables.AssassinateKey = Assasinate.AddItem(new MenuItem("AssassinateKey", "刺杀按键").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            Variables.AssasinateSwitchKey = Assasinate.AddItem(new MenuItem("AssasinateSwitchKey", "Switch Target Key").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press)));
            foreach (var hero in HeroManager.Enemies)
            {
                heroList.Add(hero.ChampionName + "(" + hero.Name + ")");
            }
            Variables.AssasinateTarget = Assasinate.AddItem(new MenuItem("刺杀", "刺杀目标").SetValue(new StringList(heroList.ToArray())));

            //LaneClear
            LaneClear = Modes.AddSubMenu(new Menu("LaneClear", "清兵"));
            Variables.LaneQ = LaneClear.AddItem(new MenuItem("laneUseQ", "Use Q").SetValue(true));
            Variables.LaneW = LaneClear.AddItem(new MenuItem("laneUseW", "Use W").SetValue(true));
            Variables.LaneE = LaneClear.AddItem(new MenuItem("laneUseE", "Use E").SetValue(true));
            Variables.LaneTiamat = LaneClear.AddItem(new MenuItem("laneUseTiamat", "Use Tiamat/Hydra").SetValue(true));
            Variables.LaneSave = LaneClear.AddItem(new MenuItem("laneSave", "保留5层残暴值").SetValue(true));

            //JungleClear
            JungleClear = Modes.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            Variables.JungQ = JungleClear.AddItem(new MenuItem("jungUseQ", "Use Q").SetValue(true));
            Variables.JungW = JungleClear.AddItem(new MenuItem("jungUseW", "Use W").SetValue(true));
            Variables.JungE = JungleClear.AddItem(new MenuItem("jungUseE", "Use E").SetValue(true));
            Variables.JungTiamat = JungleClear.AddItem(new MenuItem("jungUseTiamat", "Use Tiamat/Hydra").SetValue(true));
            Variables.JungSave = JungleClear.AddItem(new MenuItem("jungSave", "保留5层残暴值").SetValue(true));

            //Auto
            Auto = Modes.AddSubMenu(new Menu("Auto", "Auto"));
            Variables.AutoWHeal = Auto.AddItem(new MenuItem("autoWHeal", "自动W，如果血量 <").SetValue(new Slider(20, 0, 100)));
            Variables.AutoEInterrupt = Auto.AddItem(new MenuItem("autoEInterrupt", "Interrupt with E").SetValue(true));
            Variables.AutoSmiteKS = Auto.AddItem(new MenuItem("autoSmiteKS", "自称惩戒抢BUFF").SetValue(true));
            Variables.AutoESK = Auto.AddItem(new MenuItem("autoEKS", "E Ks").SetValue(true));
            Variables.AutoWKS = Auto.AddItem(new MenuItem("autoWKS", "W Ks").SetValue(true));
            Variables.AutoSmiteSteal = Auto.AddItem(new MenuItem("autoSteal", "惩戒大小龙").SetValue(true));

            //drawing
            Draw = Menu.AddSubMenu(new Menu("显示设置", "显示设置"));
            Variables.DrawMode = Draw.AddItem(new MenuItem("drawMode", "Draw Mode").SetValue(true));
            Variables.DrawAssasinate = Draw.AddItem(new MenuItem("DrawAssasinate", "显示刺杀目标").SetValue(true));

            //magnet
            Magnet = Menu.AddSubMenu(new Menu("集中攻击", "集中攻击"));
            Magnet.AddItem(new MenuItem("MagnetInstruction", "集中攻击左键选择目标"));
            Variables.MagnetEnable = Magnet.AddItem(new MenuItem("magnetEnable", "Enable").SetValue(true));
            Variables.MagnetRange = Magnet.AddItem(new MenuItem("magnetRange", "选择目标距离").SetValue(new Slider(300, 150, 500)));

            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            ComboModeSwitch();
            AssasinateSwitch();
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Variables.DrawMode.GetValue<bool>())
            {
                var x = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(x[0], x[1], Color.White, Variables.ComboMode.GetValue<StringList>().SelectedValue);
            }
        }
        private static void ComboModeSwitch()
        {
            var comboMode = Variables.ComboMode.GetValue<StringList>().SelectedValue;
            var lasttime = Utils.GameTimeTickCount - _lastSwitchTick;
            if (!Variables.ComboSwitchKey.GetValue<KeyBind>().Active ||
                lasttime <= Game.Ping)
            {
                return;
            }

            switch (comboMode)
            {
                case "Snare":
                    Variables.ComboMode.SetValue(new StringList(new[] { "Snare", "Burst", "Auto", "Always Q", "AP mode" }, 1));
                    _lastSwitchTick = Utils.GameTimeTickCount + 300;
                    break;
                case "Burst":
                    Variables.ComboMode.SetValue(new StringList(new[] { "Snare", "Burst", "Auto", "Always Q", "AP mode" }, 2));
                    _lastSwitchTick = Utils.GameTimeTickCount + 300;
                    break;
                case "Auto":
                    Variables.ComboMode.SetValue(new StringList(new[] { "Snare", "Burst", "Auto", "Always Q", "AP mode" }, 3));
                    _lastSwitchTick = Utils.GameTimeTickCount + 300;
                    break;
                case "Always Q":
                    Variables.ComboMode.SetValue(new StringList(new[] { "Snare", "Burst", "Auto", "Always Q", "AP mode" }, 4));
                    _lastSwitchTick = Utils.GameTimeTickCount + 300;
                    break;
                case "AP mode":
                    Variables.ComboMode.SetValue(new StringList(new[] { "Snare", "Burst", "Auto", "Always Q", "AP mode" }, 0));
                    _lastSwitchTick = Utils.GameTimeTickCount + 300;
                    break;
            }
        }
        private static void AssasinateSwitch()
        {
            int TargetIndex = Variables.AssasinateTarget.GetValue<StringList>().SelectedIndex;
            int Index = Variables.AssasinateTarget.GetValue<StringList>().SList.Count() - 1;
            var lastTime = Utils.GameTimeTickCount - _lastAssasinateSwitch;
            if (!Variables.AssasinateSwitchKey.GetValue<KeyBind>().Active ||
                lastTime <= Game.Ping)
            {
                return;
            }
            int NextIndex = TargetIndex + 1 > Index ? 0 : TargetIndex + 1;
            Variables.AssasinateTarget.SetValue(new StringList(heroList.ToArray(),NextIndex));
            _lastAssasinateSwitch = Utils.GameTimeTickCount + 300;
        }
    }
}
