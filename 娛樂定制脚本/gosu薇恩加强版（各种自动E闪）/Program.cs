#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LeagueSharp.SDK.Utils;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;


#endregion

namespace gosu
{
    public class Vayne
    {
        public static Spell E;
        public static Spell Q;
        public static Spell R;

        private static Orbwalking.Orbwalker orbwalker;

        private static Menu menu;

        private static Dictionary<string, SpellSlot> spellData;

        private static Obj_AI_Hero tar;
        public const string ChampName = "Vayne";
        public static Obj_AI_Hero Player;
        private static BuffType[] buffs;
        private static Spell cleanse;
        private static Spell flash;
        private static Menu Itemsmenu;
        private static Menu qmenu;
        private static Menu emenu;
        private static Menu rmenu;
        private static Menu botrk;
        private static Menu qss;



        /* Asuna VayneHunter Copypasta */
        private static readonly Vector2 DragPos = new Vector2(11514, 4462);

        private static float LastMoveC;

        private static void TumbleHandler()
        {

            if (Player.Position.X < 12000 || Player.Position.X > 12070 || Player.Position.Y < 4800 ||
            Player.Position.Y > 4872)
            {
                MoveToLimited(new Vector2(12050, 4827).To3D());
            }
            else
            {
                MoveToLimited(new Vector2(12050, 4827).To3D());
                Q.Cast(DragPos, true);
            }
        }

        private static void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - LastMoveC < 80)
            {
                return;
            }

            LastMoveC = Environment.TickCount;
            Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }

        /* End Asuna VayneHunter Copypasta */

        public static void Main(string[] args)
        {
     
                var Z = Assembly.GetExecutingAssembly().GetName().Version;
                using (var zyz = new WebClient())
                {
                    var ZF =
                        zyz.DownloadString(
                            "https://raw.githubusercontent.com/Zzihao/Zyz/master/Z-vayne/AssemblyInfo.cs");
                    var CF =
                        new Regex(@"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]").Match
                            (ZF);
                    if (!CF.Success)
                    {
                        return;
                    }
                    var GV =
                        new System.Version(
                            $"{CF.Groups[1]}.{CF.Groups[2]}.{CF.Groups[3]}.{CF.Groups[4]}");
                    if (GV != Z)
                    {
                        Game.PrintChat("<font color=\"#FFFF00\"><b>姝ょ増鏈凡琚仠鐢紒璇疯繘VIP缇わ紒</b></font><font color='#FF6666'><b>&#50;&#49;&#53;&#50;&#50;&#54;&#48;&#56;&#54;</b></font><font color=\"#FFFF00\"><b>浣跨敤鏂扮増鏈紒</b></font>");
                        Game.PrintChat("<font color=\"#FFFF00\"><b>姝ょ増鏈凡琚仠鐢紒璇疯繘VIP缇わ紒</b></font><font color='#FF6666'><b>&#50;&#49;&#53;&#50;&#50;&#54;&#48;&#56;&#54;</b></font><font color=\"#FFFF00\"><b>浣跨敤鏂扮増鏈紒</b></font>");
                        Game.PrintChat("<font color=\"#FFFF00\"><b>姝ょ増鏈凡琚仠鐢紒璇疯繘VIP缇わ紒</b></font><font color='#FF6666'><b>&#50;&#49;&#53;&#50;&#50;&#54;&#48;&#56;&#54;</b></font><font color=\"#FFFF00\"><b>浣跨敤鏂扮増鏈紒</b></font>");
                        Game.PrintChat("<font color=\"#FFFF00\"><b>姝ょ増鏈凡琚仠鐢紒璇疯繘VIP缇わ紒</b></font><font color='#FF6666'><b>&#50;&#49;&#53;&#50;&#50;&#54;&#48;&#56;&#54;</b></font><font color=\"#FFFF00\"><b>浣跨敤鏂扮増鏈紒</b></font>");
                        Game.PrintChat("<font color=\"#FFFF00\"><b>姝ょ増鏈凡琚仠鐢紒璇疯繘VIP缇わ紒</b></font><font color='#FF6666'><b>&#50;&#49;&#53;&#50;&#50;&#54;&#48;&#56;&#54;</b></font><font color=\"#FFFF00\"><b>浣跨敤鏂扮増鏈紒</b></font>");
                        Game.PrintChat("<font color=\"#FFFF00\"><b>姝ょ増鏈凡琚仠鐢紒璇疯繘VIP缇わ紒</b></font><font color='#FF6666'><b>&#50;&#49;&#53;&#50;&#50;&#54;&#48;&#56;&#54;</b></font><font color=\"#FFFF00\"><b>浣跨敤鏂扮増鏈紒</b></font>");
                    }

                    else
                    {
                        CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
                    }
                }
            
   
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
     
            Player = ObjectManager.Player;
            //Utils.PrintMessage("Vayne loaded");
            if (Player.ChampionName != ChampName) return;
            spellData = new Dictionary<string, SpellSlot>();
            //Game.PrintChat("Riven");
            menu = new Menu("GosuVN（各种E闪）", "GosuVN（各种E闪）", true).SetFontStyle(System.Drawing.FontStyle.Regular, Color.Red);
            //Orbwalker
            menu.AddSubMenu(new Menu("走砍设置", "走砍设置"));
            orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("走砍设置"));


            qmenu = menu.AddSubMenu(new Menu("闪避突袭", "闪避突袭"));
            qmenu.AddItem(new MenuItem("UseQC", "用Q连招").SetValue(true));
            qmenu.AddItem(new MenuItem("hq", "用Q骚扰").SetValue(true));
            qmenu.AddItem(new MenuItem("restrictq", "限制Q的使用?").SetValue(true));
            qmenu.AddItem(new MenuItem("UseQJ", "用Q补兵").SetValue(true));
            qmenu.AddItem(new MenuItem("Junglemana", "用Q补兵的最低蓝量").SetValue(new Slider(60, 1, 100)));
            qmenu.AddItem(
              new MenuItem("aaqaa", "AQEA(瞬间3环)").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            emenu = menu.AddSubMenu(new Menu("恶魔审判", "恶魔审判"));
            emenu.AddItem(new MenuItem("UseEC", "用E连招").SetValue(true));
           // emenu.AddItem(new MenuItem("Emod", "E的模式（0为新，1为旧）"))
             //   .SetValue(new Slider(0, 0, 1))
               // .SetTooltip("0更准确但使用频率低，1更强势但准确率低", Color.Yellow);
            emenu.AddItem(new MenuItem("he", "用E骚扰").SetValue(true));
            emenu.AddItem(new MenuItem("Int_E", "使用E打断技能").SetValue(true));
            emenu.AddItem(new MenuItem("Gap_E", "使用E反突进").SetValue(true));
            emenu.AddItem(new MenuItem("PushDistance", "E的推墙距离").SetValue(new Slider(425, 475, 300)));

            emenu.AddItem(new MenuItem("EF", "E闪").SetValue(true).SetFontStyle(System.Drawing.FontStyle.Regular, Color.Pink));
            emenu.AddItem(
              new MenuItem("EFS", "手动E闪").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            //  emenu.AddItem(
            //    new MenuItem("UseEaa", "AA之后使用E").SetValue(
            //      new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            rmenu = menu.AddSubMenu(new Menu("终极时刻", "终极时刻"));
            rmenu.AddItem(new MenuItem("useR", "用R连招").SetValue(true));
            rmenu.AddItem(new MenuItem("enemys", "如果周围敌人 >=").SetValue(new Slider(2, 1, 5)));

            rmenu.AddItem(new MenuItem("RQdisAA", "若隐身时不AA", false).SetValue((new KeyBind('Z', KeyBindType.Toggle))));

            Itemsmenu = menu.AddSubMenu(new Menu("物品使用", "物品使用"));
            Itemsmenu.AddItem(new MenuItem("BOTRK", "使用破败").SetValue(true));
            botrk = Itemsmenu.AddSubMenu(new Menu("破败设定", "使用破败"));
            botrk.AddItem(new MenuItem("myhp", "如果生命百分比低于 < %")).SetValue(new Slider(100, 0, 100));
            botrk.AddItem(new MenuItem("theirhp", "如果敌人生命百分比低于 < %")).SetValue(new Slider(100, 0, 100));
            Itemsmenu.AddItem(new MenuItem("Ghostblade", "使用幽梦").SetValue(true));
          //  Itemsmenu.AddItem(new MenuItem("QSS", "使用净化物品").SetValue(true));
        //    qss = menu.SubMenu("净化/水银").AddSubMenu(new Menu("解控设定", "使用净化物品"));

            menu.AddItem(new MenuItem("Emod", "E的模式").SetValue(new StringList(new[] {  "综合","准确", "激进" })));

            menu.AddItem(new MenuItem("walltumble", "翻墙"))
              .SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Press));


            menu.AddItem(new MenuItem("娱乐VIP群：215226086", "娱乐VIP群：215226086"));
            menu.AddItem(new MenuItem("娱乐", "娱乐VIP群：215226086"));

     //       buffs = new[]
       //                 {
         //                   BuffType.Blind, BuffType.Charm, BuffType.CombatDehancer, BuffType.Fear, BuffType.Flee,
           //                 BuffType.Knockback, BuffType.Knockup, BuffType.Polymorph, BuffType.Silence,
             //               BuffType.Snare, BuffType.Stun, BuffType.Suppression, BuffType.Taunt
               //         };

//            for (int i = 0; i < buffs.Length; i++)
  //          {
    //            qss.AddItem(new MenuItem(buffs[i].ToString(), buffs[i].ToString()).SetValue(true));
      //      }

            Q = new Spell(SpellSlot.Q, 0f);
            R = new Spell(SpellSlot.R, float.MaxValue);
            E = new Spell(SpellSlot.E, float.MaxValue);
            flash = new Spell(ObjectManager.Player.GetSpellSlot("SummonerFlash"), 425f);


            var cde = ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("SummonerBoost"));
            if (cde != null)
            {
                if (cde.Slot != SpellSlot.Unknown) //trees
                {
                    cleanse = new Spell(cde.Slot);
                }
            }

            E.SetTargetted(0.25f, 2200f);
     //       Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;

            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;

            Game.PrintChat("<font color='#003399'><b>鏈€鏂版渶寮烘杩庡姞鍏ワ紒</b></font><font color='#FFFF00'><b>Z瀛楀彿鑴氭湰瀹氬埗缇わ細264791942</b></font>");
            menu.AddToMainMenu();
        }



        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(350) && emenu.Item("Gap_E").GetValue<bool>())
            {
                E.Cast(gapcloser.Sender);
            }
        }



        private static void Interrupter2_OnInterruptableTarget(
            Obj_AI_Hero unit,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && unit.IsValidTarget(550) && emenu.Item("Int_E").GetValue<bool>())
            {
                E.Cast(unit);
            }
        }

  /*      public static void Game_ProcessSpell(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name.ToLower() == "zedult" && args.Target.IsMe)
            {
                if (Items.CanUseItem(3140))
                {
                    Utility.DelayAction.Add(1000, () => Items.UseItem(3140));
                }
                else if (Items.CanUseItem(3139))
                {
                    Utility.DelayAction.Add(1000, () => Items.UseItem(3139));
                }
                else if (cleanse != null && cleanse.IsReady())
                {
                    Utility.DelayAction.Add(1000, () => cleanse.Cast());
                }
            }
        }
        */
        /*   private static void Usepotion()
           {
               var iusehppotion = menu.Item("usehppotions").GetValue<bool>();
               var iusepotionhp = Player.Health
                                  <= (Player.MaxHealth * (menu.Item("usepotionhp").GetValue<Slider>().Value) / 100);
               var iusemppotion = menu.Item("usemppotions").GetValue<bool>();
               var iusepotionmp = Player.Mana
                                  <= (Player.MaxMana * (menu.Item("usepotionmp").GetValue<Slider>().Value) / 100);
               if (Player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

               if (Utility.CountEnemiesInRange(800) > 0)
               {
                   if (iusepotionhp && iusehppotion
                       && !(ObjectManager.Player.HasBuff("RegenerationPotion")
                            || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                            || ObjectManager.Player.HasBuff("ItemCrystalFlask")
                            || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                            || ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")))
                   {
                       if (Items.HasItem(2010) && Items.CanUseItem(2010))
                       {
                           Items.UseItem(2010);
                       }

                       if (Items.HasItem(2003) && Items.CanUseItem(2003))
                       {
                           Items.UseItem(2003);
                       }

                       if (Items.HasItem(2031) && Items.CanUseItem(2031))
                       {
                           Items.UseItem(2031);
                       }

                       if (Items.HasItem(2032) && Items.CanUseItem(2032))
                       {
                           Items.UseItem(2032);
                       }

                       if (Items.HasItem(2033) && Items.CanUseItem(2033))
                       {
                           Items.UseItem(2033);
                       }
                   }

                   if (iusepotionmp && iusemppotion
                       && !(ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")
                            || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                            || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                            || ObjectManager.Player.HasBuff("ItemCrystalFlask")))
                   {
                       if (Items.HasItem(2041) && Items.CanUseItem(2041))
                       {
                           Items.UseItem(2041);
                       }

                       if (Items.HasItem(2010) && Items.CanUseItem(2010))
                       {
                           Items.UseItem(2010);
                       }

                       if (Items.HasItem(2032) && Items.CanUseItem(2032))
                       {
                           Items.UseItem(2032);
                       }

                       if (Items.HasItem(2033) && Items.CanUseItem(2033))
                       {
                           Items.UseItem(2033);
                       }
                   }
               }
           }
           药品使用 暂时不需要*/

        /* private static void Farm()
         {
             var mob =
                 MinionManager.GetMinions(
                     Player.ServerPosition,
                     E.Range,
                     MinionTypes.All,
                     MinionTeam.Neutral,
                     MinionOrderTypes.MaxHealth).FirstOrDefault();
             var Minions = MinionManager.GetMinions(Player.Position.Extend(Game.CursorPos, Q.Range), Player.AttackRange, MinionTypes.All);
             var useQ = qmenu.Item("UseQJ").GetValue<bool>();

             int countMinions = 0;
             foreach (var minions in Minions.Where(minion => minion.Health < Player.GetAutoAttackDamage(minion) || minion.Health < Q.GetDamage(minion)))
             {
                 countMinions++;
             }

             if (countMinions >= 2 && useQ && Q.IsReady() && Minions != null)
                 Q.Cast(Player.Position.Extend(Game.CursorPos, Q.Range/2));

             if (useQ && Q.IsReady() && Orbwalking.InAutoAttackRange(mob) && mob != null)
             {
                 Q.Cast(Game.CursorPos);
             }
         }*/

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var RQdisAA = menu.Item("RQdisAA").IsActive();
            if (RQdisAA && Player.HasBuff("vaynetumblefade"))
                args.Process = false;
        }

        public static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;

            if (orbwalker.ActiveMode.ToString() == "LaneClear"
                && 100 * (Player.Mana / Player.MaxMana) > qmenu.Item("Junglemana").GetValue<Slider>().Value)
            {
                var mob =
                    MinionManager.GetMinions(
                        Player.ServerPosition,
                        E.Range,
                        MinionTypes.All,
                        MinionTeam.Neutral,
                        MinionOrderTypes.MaxHealth).FirstOrDefault();
                var Minions = MinionManager.GetMinions(
                    Player.Position.Extend(Game.CursorPos, Q.Range),
                    Player.AttackRange,
                    MinionTypes.All);
                var useQ = qmenu.Item("UseQJ").GetValue<bool>();
                int countMinions = 0;
                foreach (var minions in
                    Minions.Where(
                        minion =>
                        minion.Health < Player.GetAutoAttackDamage(minion)
                        || minion.Health < Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) || minion.Health < Q.GetDamage(minion)))
                {
                    countMinions++;
                }

                if (countMinions >= 2 && useQ && Q.IsReady() && Minions != null) Q.Cast(Player.Position.Extend(Game.CursorPos, Q.Range / 2));

                if (useQ && Q.IsReady() && Orbwalking.InAutoAttackRange(mob) && mob != null)
                {
                    Q.Cast(Game.CursorPos);
                }
            }

            if (!(target is Obj_AI_Hero)) return;

            tar = (Obj_AI_Hero)target;

            if (menu.Item("aaqaa").GetValue<KeyBind>().Active)
            {
                if (Q.IsReady())
                {
                    Q.Cast(Game.CursorPos);

                }
                E.Cast(tar);
                Orbwalking.Orbwalk(TargetSelector.GetTarget(625, TargetSelector.DamageType.Physical), Game.CursorPos);
            }

            if (orbwalker.ActiveMode.ToString() == "Combo")
            {
                if (Itemsmenu.Item("BOTRK").GetValue<bool>()
                    && (tar.Health <= tar.MaxHealth * botrk.Item("theirhp").GetValue<Slider>().Value / 100)
                    || (Player.Health <= Player.MaxHealth * botrk.Item("myhp").GetValue<Slider>().Value / 100))
                {
                    //Game.PrintChat("in");
                    if (Items.CanUseItem(3153))
                    {
                        Items.UseItem(3153, tar);
                    }
                    else if (Items.CanUseItem(3144))
                    {
                        {
                            Items.UseItem(3144, tar);
                        }
                    }
                }

                if (Itemsmenu.Item("Ghostblade").GetValue<bool>() && tar.IsValidTarget(800))
                {
                    if (Items.CanUseItem(3142))
                    {
                        Items.UseItem(3142);
                    }
                }
            }

            /*  感觉这功能有点弱智，先放着。 
                if (emenu.Item("UseEaa").GetValue<KeyBind>().Active)
                {
                    E.Cast((Obj_AI_Base)target);
                    emenu.Item("UseEaa").SetValue<KeyBind>(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle));
                }
            */

            if (Q.IsReady()
                && ((orbwalker.ActiveMode.ToString() == "Combo" && qmenu.Item("UseQC").GetValue<bool>())
                    || (orbwalker.ActiveMode.ToString() == "Mixed" && qmenu.Item("hq").GetValue<bool>())))
            {
                if (qmenu.Item("restrictq").GetValue<bool>())
                {
                    var after = ObjectManager.Player.Position
                                + Normalize(Game.CursorPos - ObjectManager.Player.Position) * 300;
                    //Game.PrintChat("After: {0}", after);
                    var disafter = Vector3.DistanceSquared(after, tar.Position);
                    //Game.PrintChat("DisAfter: {0}", disafter);
                    //Game.PrintChat("first calc: {0}", (disafter) - (630*630));
                    if ((disafter < 630 * 630) && disafter > 150 * 150)
                    {
                        Q.Cast(Game.CursorPos);
                    }

                    if (Vector3.DistanceSquared(tar.Position, ObjectManager.Player.Position) > 630 * 630
                        && disafter < 630 * 630)
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }
                else
                {
                    Q.Cast(Game.CursorPos);
                }
                //Q.Cast(Game.CursorPos);
            }
        }


        public static Vector3 Normalize(Vector3 A)
        {
            double distance = Math.Sqrt(A.X * A.X + A.Y * A.Y);
            return new Vector3(new Vector2((float)(A.X / distance)), (float)(A.Y / distance));
        }

   

        public static void Game_OnGameUpdate(EventArgs args)
        {

            if (menu.Item("useR").GetValue<bool>() && R.IsReady()
                && ObjectManager.Player.CountEnemiesInRange(1000) >= menu.Item("enemys").GetValue<Slider>().Value
                && orbwalker.ActiveMode.ToString() == "Combo")
            {
                R.Cast();
            }


            if (menu.Item("walltumble").GetValue<KeyBind>().Active)
            {
                TumbleHandler();
            }


            if (menu.Item("aaqaa").GetValue<KeyBind>().Active)
            {
                Orbwalking.Orbwalk(TargetSelector.GetTarget(625, TargetSelector.DamageType.Physical), Game.CursorPos);
            }

            if (menu.Item("EFS").GetValue<KeyBind>().Active)
            {
                Orbwalking.Orbwalk(TargetSelector.GetTarget(625, TargetSelector.DamageType.Physical), Game.CursorPos);
            }

        /*    if (Itemsmenu.Item("QSS").GetValue<bool>())
            {
                for (int i = 0; i < buffs.Length; i++)
                {
                    if (ObjectManager.Player.HasBuffOfType(buffs[i]) && qss.Item(buffs[i].ToString()).GetValue<bool>())
                    {
                        if (Items.CanUseItem(3140))
                        {
                            Items.UseItem(3140);
                        }
                        else if (Items.CanUseItem(3139))
                        {
                            Items.UseItem(3140);
                        }
                        else if (cleanse != null && cleanse.IsReady())
                        {
                            cleanse.Cast();
                        }
                    }
                }
            }
            */
            if (!E.IsReady()) return;
                           //(orbwalker.ActiveMode.ToString() != "Combo" || !menu.Item("UseEC").GetValue<bool>()) &&
                          //!menu.Item("UseET").GetValue<KeyBind>().Active)) return;
            var dashPosition = Player.Position.Extend(Game.CursorPos, Q.Range);

            if ((orbwalker.ActiveMode.ToString() == "Combo" && emenu.Item("UseEC").GetValue<bool>()) || (orbwalker.ActiveMode.ToString() == "Mixed" && emenu.Item("he").GetValue<bool>()))
                switch (menu.Item("Emod").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        foreach (var En in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(550f) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)))
                        {            
                                var EPred = E.GetPrediction(En);
                                int pushDist = emenu.Item("PushDistance").GetValue<Slider>().Value;
                                var FinalPosition = EPred.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -pushDist).To3D();

                                for (int i = 1; i < pushDist; i += (int)En.BoundingRadius)
                                {
                                    Vector3 loc3 = EPred.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D();

                                    if (loc3.IsWall() || AsunasAllyFountain(FinalPosition))
                                        E.Cast(En);
                                }   
                        }
                        break;

                    case 1:
                        foreach (var target2 in ObjectManager.Get<Obj_AI_Hero>().Where(target2 => target2.IsValidTarget(550f) && target2.Path.Count() < 2))
                        {
                            var prepos = E.GetPrediction(target2);

                            float pushDistance = 470;

                            if (Player.ServerPosition != ObjectManager.Player.ServerPosition)
                                pushDistance = 420;

                            int radius = 250;
                            var start2 = target2.ServerPosition;
                            var end2 = prepos.CastPosition.Extend(ObjectManager.Player.ServerPosition, -pushDistance);

                            Vector2 start = start2.To2D();
                            Vector2 end = end2.To2D();
                            var dir = (end - start).Normalized();
                            var pDir = dir.Perpendicular();

                            var rightEndPos = end + pDir * radius;
                            var leftEndPos = end - pDir * radius;


                            var rEndPos = new Vector3(rightEndPos.X, rightEndPos.Y, ObjectManager.Player.Position.Z);
                            var lEndPos = new Vector3(leftEndPos.X, leftEndPos.Y, ObjectManager.Player.Position.Z);


                            var step = start2.Distance(rEndPos) / 10;
                            for (var i = 0; i < 10; i++)
                            {
                                var pr = start2.Extend(rEndPos, step * i);
                                var pl = start2.Extend(lEndPos, step * i);
                                if (pr.IsWall() && pl.IsWall())
                                    E.Cast(target2);
                            }
                        }
                        break;

                    case 2:
                        foreach (var hero in from hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(550f) && !hero.HasBuffOfType(BuffType.SpellShield))
                                             let prediction = E.GetPrediction(hero)
                                             where NavMesh.GetCollisionFlags(
                                                 prediction.UnitPosition.To2D()
                                                     .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                                         -emenu.Item("PushDistance").GetValue<Slider>().Value)
                                                     .To3D())
                                                 .HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(
                                                     prediction.UnitPosition.To2D()
                                                         .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                                             -(emenu.Item("PushDistance").GetValue<Slider>().Value / 2))
                                                         .To3D())
                                                     .HasFlag(CollisionFlags.Wall)
                                             select hero)
                        {
                            E.Cast(hero);
                        }
                        break;
                }
       
            if (ObjectManager.Player.IsDead)
                    {
                        return;
                    }

                    if (flash.IsReady() && orbwalker.ActiveMode.ToString() == "Combo" && menu.Item("EF").GetValue<bool>()
                        || menu.Item("EFS").GetValue<KeyBind>().Active && flash.IsReady())
                    {
                        foreach (var target in LeagueSharp.SDK.GameObjects.EnemyHeroes.Where(
                               t =>
                                   t.IsValidTarget(E.Range) &&
                                   !Invulnerable.Check(t, DamageType.Magical, false) &&
                                   !t.IsValidTarget(LeagueSharp.SDK.GameObjects.Player.BoundingRadius) &&
                                   LeagueSharp.SDK.GameObjects.Player.Distance(LeagueSharp.SDK.GameObjects.Player.ServerPosition.Extend(t.ServerPosition, flash.Range)) >
                                       LeagueSharp.SDK.GameObjects.Player.Distance(t) + t.BoundingRadius))
                        {
                            for (var i = 1; i < 10; i++)
                            {
                                if ((!target.IsDashing()
                                        ? (target.ServerPosition - Vector3.Normalize(target.ServerPosition - LeagueSharp.SDK.GameObjects.Player.ServerPosition) * (float)(i * 42.5)).IsWall()
                                        : true) &&
                                    (E.GetPrediction(target).UnitPosition - Vector3.Normalize(target.ServerPosition - LeagueSharp.SDK.GameObjects.Player.ServerPosition) * (float)(i * 42.5)).IsWall() &&

                                    (!target.IsDashing()
                                        ? (target.ServerPosition - Vector3.Normalize(target.ServerPosition - LeagueSharp.SDK.GameObjects.Player.ServerPosition) * i * 44).IsWall()
                                        : true) &&
                                    (E.GetPrediction(target).UnitPosition - Vector3.Normalize(target.ServerPosition - LeagueSharp.SDK.GameObjects.Player.ServerPosition) * i * 44).IsWall())
                                {
                                    E.CastOnUnit(target);
                                    flash.Cast(ObjectManager.Player.ServerPosition.Extend(target.ServerPosition, flash.Range));
                                }

                                /* 第一版
                 * foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(
                   t =>
                   t.IsValidTarget(E.Range)&&
                        !t.HasBuffOfType(BuffType.SpellShield) &&
                       !t.IsValidTarget(ObjectManager.Player.BoundingRadius) &&
                       ObjectManager.Player.Distance(ObjectManager.Player.ServerPosition.Extend(t.ServerPosition, flash.Range)) >
                           ObjectManager.Player.Distance(t) + t.BoundingRadius))
                {
                        for (var i = 1; i < 10; i++)
                        {
                            if ((!target.IsDashing()
                                    ? (target.ServerPosition - Vector3.Normalize(target.ServerPosition - ObjectManager.Player.ServerPosition) * (float)(i * 42.5)).IsWall()
                                    : true) &&
                                (E.GetPrediction(target).UnitPosition - Vector3.Normalize(target.ServerPosition - ObjectManager.Player.ServerPosition) * (float)(i * 42.5)).IsWall() &&

                                (!target.IsDashing()
                                    ? (target.ServerPosition - Vector3.Normalize(target.ServerPosition - ObjectManager.Player.ServerPosition) * i * 44).IsWall()
                                    : true) &&
                                (E.GetPrediction(target).UnitPosition - Vector3.Normalize(target.ServerPosition - ObjectManager.Player.ServerPosition) * i * 44).IsWall())
                            {
                                E.CastOnUnit(target);
                               flash.Cast(ObjectManager.Player.ServerPosition.Extend(target.ServerPosition, flash.Range));
                            }
                            */
                            }
                        }
                    }
                }

        private static bool AsunasAllyFountain(Vector3 position)
        {
            float fountainRange = 750;
            var map = LeagueSharp.Common.Utility.Map.GetMap();
            if (map != null && map.Type == LeagueSharp.Common.Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return
                ObjectManager.Get<GameObject>().Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly).Any(spawnPoint => Vector2.Distance(position.To2D(), spawnPoint.Position.To2D()) < fountainRange);
        }
    }
    }





