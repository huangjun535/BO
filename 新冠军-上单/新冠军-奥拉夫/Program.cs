namespace YuLeOlaf
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using SharpDX.Direct3D9;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Color = SharpDX.Color;
    using Font = SharpDX.Direct3D9.Font;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class OlafAxe
    {
        public GameObject Object { get; set; }
        public float NetworkId { get; set; }
        public Vector3 AxePos { get; set; }
        public double ExpireTime { get; set; }
    }

    internal class Program
    {
        public static Obj_AI_Hero Player;
        private static readonly OlafAxe olafAxe = new OlafAxe();
        public static Font TextAxe, TextLittle;
        public static int LastTickTime;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, Q2, W, E, R;
        private static Items.Item itemYoumuu;
        public static SpellSlot IgniteSlot = SpellSlot.Unknown;
        private static Dictionary<string, Tuple<Items.Item, EnumItemType, EnumItemTargettingType>> ItemDb;
        public static Menu Menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.CharData.BaseSkinName != "Olaf")
                return;
			

            Player = ObjectManager.Player;

            spellsLoad();
            loadItems();

            Menu = new Menu("QQ群438230879", "QQ群438230879", true).SetFontStyle(FontStyle.Regular, SharpDX.Color.Yellow);

            Menu.AddSubMenu(new Menu("走砍设置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));
            Menu.SubMenu("Orbwalking").AddItem(new MenuItem("Flee.Active", "逃跑按键").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press))).Permashow();
            Menu.SubMenu("Orbwalking").AddItem(new MenuItem("Harass.UseQ.Toggle", "自动骚扰Q按键").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle))).Permashow();

            Menu.AddSubMenu(new Menu("Q设置", "QSETTINGS"));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("ASDASD", "连招模式"));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("UseQCombo", "使用!")).SetValue(true);
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("ASDAS2D", "骚扰模式"));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("UseQHarass", "使用!").SetValue(false));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("UseQ2Harass", "  短距离Q").SetValue(true));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("Harass.UseQ.MinMana", "  最低骚扰Q蓝量").SetValue(new Slider(30, 100, 0)));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("AaaSDASD", "清线模式"));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("UseQFarm", "使用!", true).SetValue(true));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("UseQFarmMinCount", "  最少命中小兵数", true).SetValue(new Slider(2, 5, 1)));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("UseQFarmMinMana", "  最低清线Q蓝量", true).SetValue(new Slider(30, 100, 0)));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("xfeffhg", "清野模式"));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("UseQJFarm", "使用!", true).SetValue(true));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("UseQJFarmMinMana", "   最低清野Q蓝量", true).SetValue(new Slider(30, 100, 0)));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("ASDXFEASD", "逃跑模式"));
            Menu.SubMenu("QSETTINGS").AddItem(new MenuItem("Flee.UseQ", "使用!").SetValue(false));

            Menu.AddSubMenu(new Menu("W设置", "WSETTINGS"));
            Menu.SubMenu("WSETTINGS").AddItem(new MenuItem("ASDASD", "连招模式"));
            Menu.SubMenu("WSETTINGS").AddItem(new MenuItem("UseQCombo", "连招总是使用!"));
            Menu.SubMenu("WSETTINGS").AddItem(new MenuItem("xfeffhg", "清野模式"));
            Menu.SubMenu("WSETTINGS").AddItem(new MenuItem("UseWJFarm", "使用!", true).SetValue(false));
            Menu.SubMenu("WSETTINGS").AddItem(new MenuItem("UseWJFarmMinMana", "  最低清野W蓝量", true).SetValue(new Slider(30, 100, 0)));

            Menu.AddSubMenu(new Menu("E设置", "ESETTINGS"));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("ASDASD", "连招模式"));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("UseQCombo", "连招总是使用!"));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("ASDAS2D", "骚扰模式"));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("UseEHarass", "使用!").SetValue(true));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("AaaSDASD", "清线模式"));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("UseEFarm", "使用!", true).SetValue(true));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("UseEFarmSet", "  清线模式:", true).SetValue(new StringList(new[] { "尾刀", "总是使用" }, 0)));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("UseEFarmMinHealth", "  最低清线E生命值", true).SetValue(new Slider(10, 100, 0)));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("xfeffhg", "清野模式"));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("UseEJFarm", "使用!", true).SetValue(false));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("UseEJFarmSet", "  清野模式:", true).SetValue(new StringList(new[] { "尾刀", "总是使用" }, 1)));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("UseEJFarmMinHealth", "  最低清野E生命值", true).SetValue(new Slider(10, 100, 0)));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("AxcrSDASD", "自动模式"));
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("Misc.AutoE", "使用!").SetValue(false));
            string[] strE = new string[1000 / 250];
            for (var i = 250; i <= 1000; i += 250)
            {
                strE[i / 250 - 1] = " " + i;
            }
            Menu.SubMenu("ESETTINGS").AddItem(new MenuItem("Misc.AutoE.Delay", "  自动E延迟:").SetValue(new StringList(strE, 0)));

            Menu.AddSubMenu(new Menu("R设置", "RSETTINGS"));
            Menu.SubMenu("RSETTINGS").AddItem(new MenuItem("Misc.AutoREvade", "使用R的时候禁止躲避").SetValue(true));
            Menu.SubMenu("RSETTINGS").AddItem(new MenuItem("Misc.AutoR", "团控时自动R").SetValue(false));

            Menu.AddSubMenu(new Menu("物品使用", "ItemUSING"));
            Menu.SubMenu("ItemUSING").AddItem(new MenuItem("Spells.ITWMS", "连招骚扰总是使用物品!"));
            Menu.SubMenu("ItemUSING").AddItem(new MenuItem("LaneClearUseItems", "清线自动使用物品").SetValue(true));
            Menu.SubMenu("ItemUSING").AddItem(new MenuItem("JungleFarmUseItems", "清野自动使用物品").SetValue(true));
            Menu.SubMenu("ItemUSING").AddItem(new MenuItem("Spells.Ignite", "自动点燃!").SetValue(true));

            Menu.AddSubMenu(new Menu("显示设置", "Drawings"));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw.QRange", "Q 范围").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw.Q2Range", "短 Q 范围").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw.ERange", "E 范围").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw.AxePosition", "斧头位置").SetValue(new StringList(new[] { "关闭", "线圈", "直线", "两者" }, 3)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw.AxeTime", "斧头时间").SetValue(true));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw.AAA", "击杀提示").SetValue(true));

            Menu.AddToMainMenu();

            textAxe();
            
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            Game.OnUpdate += Game_OnUpdate1;
        }

        private static void Game_OnUpdate1(EventArgs args)
        {
            if (Player.HasBuff("OlafRagnarok"))
            {
                if (Menu.Item("Misc.AutoREvade").GetValue<bool>())
                {
                    DisableEvade.Disable();
                }
            }

            if (Menu.Item("Misc.AutoREvade").GetValue<bool>() && DisableEvade.EvadeDisabled)
            {
                if (!Player.HasBuff("OlafRagnarok"))
                {
                    DisableEvade.Enable();
                }
            }
        }

        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.Name.Contains("axe_totem_team_id_green.troy"))
            {
                olafAxe.Object = obj;
                olafAxe.ExpireTime = Game.Time + 8;
                olafAxe.NetworkId = obj.NetworkId;
                olafAxe.AxePos = obj.Position;
            }
        }

        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.Name.Contains("axe_totem_team_id_green.troy"))
            {
                olafAxe.Object = null;
                LastTickTime = 0;
            }
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {GameOnOnGameLoad();
                    try
                    {
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            );
        }

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero)
            {
                foreach (var item in
                    ItemDb.Where(
                        i =>
                        i.Value.ItemType == EnumItemType.OnTarget
                        && i.Value.TargetingType == EnumItemTargettingType.EnemyHero && i.Value.Item.IsReady()))
                {
                    item.Value.Item.Cast();
                }

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && W.IsReady()
                    && args.Target.Health > Player.TotalAttackDamage * 2)
                {
                    W.Cast();
                }
            }
        }

        private static void CountAa()
        {
            int result = 0;

            foreach (var e in HeroManager.Enemies.Where(e => e.Distance(Player.Position) < Q.Range * 3 && !e.IsDead && e.IsVisible))
            {
                var getComboDamage = GetComboDamage(e);
                var str = " ";

                if (e.Health < getComboDamage + Player.TotalAttackDamage * 5)
                {
                    result = (int)Math.Ceiling((e.Health - getComboDamage) / Player.TotalAttackDamage) + 1;
                    if (e.Health < getComboDamage)
                    {
                        str = "Combo = Kill";
                    }
                    else
                    {
                        str = (getComboDamage > 0 ? "Combo " : "") + (result > 0 ? result + " x AA Damage = Kill" : "");
                    }
                }

                DrawText(
                    TextLittle,
                    str,
                    (int)e.HPBarPosition.X + 145,
                    (int)e.HPBarPosition.Y + 5,
                    result <= 4 ? Color.GreenYellow : Color.White);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            if (Menu.Item("Draw.AAA").GetValue<bool>())
                CountAa();

            var drawAxePosition = Menu.Item("Draw.AxePosition").GetValue<StringList>().SelectedIndex;
            if (olafAxe.Object != null)
            {
                var exTime = TimeSpan.FromSeconds(olafAxe.ExpireTime - Game.Time).TotalSeconds;
                var color = exTime > 4 ? System.Drawing.Color.Yellow : System.Drawing.Color.Red;
                switch (drawAxePosition)
                {
                    case 1:
                        Render.Circle.DrawCircle(olafAxe.Object.Position, 150, color, 6);
                        break;
                    case 2:
                        {
                            var line = new Geometry.Polygon.Line(
                                Player.Position,
                                olafAxe.AxePos,
                                Player.Distance(olafAxe.AxePos));
                            line.Draw(color, 2);
                        }
                        break;
                    case 3:
                        {
                            Render.Circle.DrawCircle(olafAxe.Object.Position, 150, color, 6);

                            var line = new Geometry.Polygon.Line(
                                Player.Position,
                                olafAxe.AxePos,
                                Player.Distance(olafAxe.AxePos));
                            line.Draw(color, 2);
                        }
                        break;


                }
            }

            if (Menu.Item("Draw.AxeTime").GetValue<bool>() && olafAxe.Object != null)
            {
                var time = TimeSpan.FromSeconds(olafAxe.ExpireTime - Game.Time);
                var pos = Drawing.WorldToScreen(olafAxe.AxePos);
                var display = string.Format("{0}:{1:D2}", time.Minutes, time.Seconds);

                Color vTimeColor = time.TotalSeconds > 4 ? Color.White : Color.Red;
                DrawText(TextAxe, display, (int)pos.X - display.Length * 3, (int)pos.Y - 65, vTimeColor);
            }

            foreach (var spell in SpellList)
            {
                var menuItem = Menu.Item("Draw." + spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color, 1);
                }
            }
            var Q2Range = Menu.Item("Draw.Q2Range").GetValue<Circle>();
            if (Q2Range.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Q2.Range, Q2Range.Color, 1);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo || !Player.HasBuff("Recall"))
            {
                if (Menu.Item("Harass.UseQ.Toggle").GetValue<KeyBind>().Active)
                {
                    CastQ();
                }
            }

            if (E.IsReady() && Menu.Item("Misc.AutoE").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    CastE(t);
                    //E.CastOnUnit(t);
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
                UseSpells();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                LaneClear();
                JungleFarm();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }


            if (Menu.Item("Flee.Active").GetValue<KeyBind>().Active)
                Flee();

            if (R.IsReady() && Menu.Item("Misc.AutoR").GetValue<bool>())
            {
                CastR();
            }
        }

        private static void Combo()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (!t.IsValidTarget())
                return;

            if (Menu.Item("UseQCombo").GetValue<bool>() && Q.IsReady() &&
                Player.Distance(t.ServerPosition) <= Q.Range)
            {
                PredictionOutput qPredictionOutput = Q.GetPrediction(t);
                var castPosition = qPredictionOutput.CastPosition.Extend(ObjectManager.Player.Position, -100);

                if (Player.Distance(t.ServerPosition) >= 300)
                {
                    Q.Cast(castPosition);
                }
                else
                {
                    Q.Cast(qPredictionOutput.CastPosition);
                }
            }

            if (E.IsReady() && Player.Distance(t.ServerPosition) <= E.Range)
            {
                CastE(t);
                //E.CastOnUnit(t);
            }

            if (W.IsReady() && Player.Distance(t.ServerPosition) <= 225f)
            {
                W.Cast();
            }

            CastItems(t);

            if (GetComboDamage(t) > t.Health && IgniteSlot != SpellSlot.Unknown
                && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                Player.Spellbook.CastSpell(IgniteSlot, t);
            }
        }

        private static void CastE(AttackableUnit t)
        {
            if (!E.IsReady() && !t.IsValidTarget(E.Range))
            {
                return;
            }

            foreach (var enemy in Helper.EnemyInfo.Where(
                x =>
                    !x.Player.IsDead &&
                    Environment.TickCount - x.LastSeen >=
                    (Menu.Item("Misc.AutoE.Delay").GetValue<StringList>().SelectedIndex + 1)*250 &&
                    x.Player.NetworkId == t.NetworkId).Select(x => x.Player).Where(enemy => enemy != null))
            {
                E.CastOnUnit(enemy);
            }

        }
        private static void CastQ()
        {
            if (!Q.IsReady())
                return;

            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget())
            {
                Vector3 castPosition;
                PredictionOutput qPredictionOutput = Q.GetPrediction(t);

                if (!t.IsFacing(Player) && t.Path.Count() >= 1) // target is running
                {
                    castPosition = Q.GetPrediction(t).CastPosition
                                   + Vector3.Normalize(t.ServerPosition - Player.Position) * t.MoveSpeed / 2;
                }
                else
                {
                    castPosition = qPredictionOutput.CastPosition.Extend(ObjectManager.Player.Position, -100);
                }

                Q.Cast(Player.Distance(t.ServerPosition) >= 350 ? castPosition : qPredictionOutput.CastPosition);
            }
        }

        private static void CastShortQ()
        {
            if (!Q.IsReady())
                return;

            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget() && Q.IsReady()
                && Player.Mana > Player.MaxMana / 100 * Menu.Item("Harass.UseQ.MinMana").GetValue<Slider>().Value
                && Player.Distance(t.ServerPosition) <= Q2.Range)
            {
                PredictionOutput q2PredictionOutput = Q2.GetPrediction(t);
                var castPosition = q2PredictionOutput.CastPosition.Extend(ObjectManager.Player.Position, -140);
                if (q2PredictionOutput.Hitchance >= HitChance.High) Q2.Cast(castPosition);
            }
        }

        private static void CastR()
        {
            BuffType[] buffList =
            {
                BuffType.Blind,
                BuffType.Charm,
                BuffType.Fear,
                BuffType.Knockback,
                BuffType.Knockup,
                BuffType.Taunt,
                BuffType.Slow,
                BuffType.Silence,
                BuffType.Disarm,
                BuffType.Snare
            };

            foreach (var b in buffList.Where(b => Player.HasBuffOfType(b)))
            {
                R.Cast();
            }
        }

        private static void Harass()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Menu.Item("UseQHarass").GetValue<bool>())
            {
                CastQ();
            }

            if (Menu.Item("UseQ2Harass").GetValue<bool>())
            {
                CastShortQ();
            }

            if (E.IsReady() && Menu.Item("UseEHarass").GetValue<bool>() && Player.Distance(t.ServerPosition) <= E.Range)
            {
                CastE(t);
                //E.CastOnUnit(t);
            }
        }

        private static void LaneClear()
        {
            var allMinions = MinionManager.GetMinions(
                Player.ServerPosition,
                Q.Range,
                MinionTypes.All,
                MinionTeam.Enemy,
                MinionOrderTypes.MaxHealth);

            if (allMinions.Count <= 0) return;

            if (Menu.Item("LaneClearUseItems").GetValue<bool>())
            {
                foreach (var item in from item in ItemDb
                                     where
                                         item.Value.ItemType == EnumItemType.AoE
                                         && item.Value.TargetingType == EnumItemTargettingType.EnemyObjects
                                     let iMinions = allMinions
                                     where
                                         item.Value.Item.IsReady()
                                         && iMinions[0].Distance(Player.Position) < item.Value.Item.Range
                                     select item)
                {
                    item.Value.Item.Cast();
                }
            }

            if (Menu.Item("UseQFarm", true).GetValue<bool>() && Q.IsReady()
                && Player.HealthPercent > Menu.Item("UseQFarmMinMana", true).GetValue<Slider>().Value)
            {
                var vParamQMinionCount = Menu.Item("UseQFarmMinCount", true).GetValue<Slider>().Value;

                var objAiHero = from x1 in ObjectManager.Get<Obj_AI_Minion>()
                                where x1.IsValidTarget() && x1.IsEnemy
                                select x1
                                    into h
                                orderby h.Distance(Player) descending
                                select h
                                        into x2
                                where x2.Distance(Player) < Q.Range - 20 && !x2.IsDead
                                select x2;

                var aiMinions = objAiHero as Obj_AI_Minion[] ?? objAiHero.ToArray();

                var lastMinion = aiMinions.First();

                var qMinions = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    Player.Distance(lastMinion.Position));

                if (qMinions.Count > 0)
                {
                    var locQ = Q.GetLineFarmLocation(qMinions, Q.Width);

                    if (qMinions.Count == qMinions.Count(m => Player.Distance(m) < Q.Range)
                        && locQ.MinionsHit >= vParamQMinionCount && locQ.Position.IsValid())
                    {
                        Q.Cast(lastMinion.Position);
                    }
                }
            }

            if (Menu.Item("UseEFarm", true).GetValue<bool>() && E.IsReady()
                && Player.HealthPercent > Menu.Item("UseEFarmMinHealth", true).GetValue<Slider>().Value)
            {
                var eMinions = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (eMinions.Count > 0)
                {
                    var eFarmSet = Menu.Item("UseEFarmSet", true).GetValue<StringList>().SelectedIndex;
                    switch (eFarmSet)
                    {
                        case 0:
                            {
                                if (eMinions[0].Health <= E.GetDamage(eMinions[0]))
                                {
                                    E.CastOnUnit(eMinions[0]);
                                }
                                break;
                            }
                        case 1:
                            {
                                E.CastOnUnit(eMinions[0]);
                                break;
                            }
                    }
                }
            }
        }

        private static void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0)
            {
                return;
            }

            var mob = mobs[0];

            if (Menu.Item("JungleFarmUseItems").GetValue<bool>())
            {
                foreach (var item in from item in ItemDb
                                     where
                                         item.Value.ItemType == EnumItemType.AoE
                                         && item.Value.TargetingType == EnumItemTargettingType.EnemyObjects
                                     let iMinions = mobs
                                     where item.Value.Item.IsReady() && iMinions[0].IsValidTarget(item.Value.Item.Range)
                                     select item)
                {
                    item.Value.Item.Cast();
                }
            }

            if (Menu.Item("UseQJFarm", true).GetValue<bool>() && Q.IsReady())
            {
                if (Player.Mana < Player.MaxMana / 100 * Menu.Item("UseQJFarmMinMana", true).GetValue<Slider>().Value) return;

                if (Q.IsReady()) Q.Cast(mob.Position - 20);
            }

            if (Menu.Item("UseWJFarm", true).GetValue<bool>() && W.IsReady())
            {
                if (Player.Mana < Player.MaxMana / 100 * Menu.Item("UseWJFarmMinMana", true).GetValue<Slider>().Value) return;

                if (mobs.Count >= 2 || mob.Health > Player.TotalAttackDamage * 2.5) W.Cast();
            }

            if (Menu.Item("UseEJFarm", true).GetValue<bool>() && E.IsReady())
            {
                if (Player.Health < Player.MaxHealth / 100 * Menu.Item("UseEJFarmMinHealth", true).GetValue<Slider>().Value) return;

                var vParamESettings = Menu.Item("UseEJFarmSet", true).GetValue<StringList>().SelectedIndex;
                switch (vParamESettings)
                {
                    case 0:
                        {
                            if (mob.Health <= Player.GetSpellDamage(mob, SpellSlot.E)) E.CastOnUnit(mob);
                            break;
                        }
                    case 1:
                        {
                            E.CastOnUnit(mob);
                            break;
                        }
                }
            }
        }

        private static void Flee()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (Menu.Item("Flee.UseQ").GetValue<bool>())
                if (Q.IsReady())
                {
                    CastQ();
                }
        }


        private static void UseSpells()
        {
            var t = TargetSelector.GetTarget(Program.Q.Range, TargetSelector.DamageType.Magical);

            if (!t.IsValidTarget())
                return;

            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                IgniteOnTarget(t);
            }
        }

        private static void IgniteOnTarget(Obj_AI_Hero t)
        {
            var range = 550f;
            var use = Menu.Item("Spells.Ignite").GetValue<bool>();
            if (use && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                t.Distance(ObjectManager.Player.Position) < range &&
                ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite) > t.Health)
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, t);
            }
        }

        private static void CastItems(Obj_AI_Hero t)
        {
            foreach (var item in ItemDb)
            {
                if (item.Value.ItemType == EnumItemType.AoE
                    && item.Value.TargetingType == EnumItemTargettingType.EnemyHero)
                {
                    if (t.IsValidTarget(item.Value.Item.Range) && item.Value.Item.IsReady()) item.Value.Item.Cast();
                }
                if (item.Value.ItemType == EnumItemType.Targeted
                    && item.Value.TargetingType == EnumItemTargettingType.EnemyHero)
                {
                    if (t.IsValidTarget(item.Value.Item.Range) && item.Value.Item.IsReady()) item.Value.Item.Cast(t);
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Base t)
        {
            var fComboDamage = 0d;

            if (Q.IsReady()) fComboDamage += Q.GetDamage(t);

            if (E.IsReady()) fComboDamage += E.GetDamage(t);

            if (Items.CanUseItem(3146)) fComboDamage += Player.GetItemDamage(t, Damage.DamageItems.Hexgun);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                fComboDamage += Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);
            }

            return (float)fComboDamage;
        }

        public static void DrawText(Font aFont, String aText, int aPosX, int aPosY, Color aColor)
        {
            aFont.DrawText(null, aText, aPosX + 2, aPosY + 2, aColor != Color.Black ? Color.Black : Color.White);
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }

        private struct Tuple<TA, TB, TC> : IEquatable<Tuple<TA, TB, TC>>
        {
            private readonly TA item;
            private readonly TB itemType;
            private readonly TC targetingType;

            public Tuple(TA pItem, TB pItemType, TC pTargetingType)
            {
                this.item = pItem;
                this.itemType = pItemType;
                this.targetingType = pTargetingType;
            }

            public TA Item
            {
                get { return this.item; }
            }

            public TB ItemType
            {
                get { return this.itemType; }
            }

            public TC TargetingType
            {
                get { return this.targetingType; }
            }

            public override int GetHashCode()
            {
                return this.item.GetHashCode() ^ this.itemType.GetHashCode() ^ this.targetingType.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || this.GetType() != obj.GetType())
                {
                    return false;
                }
                return this.Equals((Tuple<TA, TB, TC>)obj);
            }

            public bool Equals(Tuple<TA, TB, TC> other)
            {
                return other.item.Equals(item) && other.itemType.Equals(this.itemType)
                       && other.targetingType.Equals(this.targetingType);
            }
        }

        private enum EnumItemType
        {
            OnTarget,
            Targeted,
            AoE
        }

        private enum EnumItemTargettingType
        {
            Ally,
            EnemyHero,
            EnemyObjects
        }

        private static void loadItems()
        {
            itemYoumuu = new Items.Item(3142, 225f);

            ItemDb =
                new Dictionary<string, Tuple<Items.Item, EnumItemType, EnumItemTargettingType>>
                    {
                         {
                            "Tiamat",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new LeagueSharp.Common.Items.Item(3077, 450f),
                            EnumItemType.AoE,
                            EnumItemTargettingType.EnemyObjects)
                        },
                        {
                            "Bilge",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new Items.Item(3144, 450f),
                            EnumItemType.Targeted,
                            EnumItemTargettingType.EnemyHero)
                        },
                        {
                            "Blade",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new Items.Item(3153, 450f),
                            EnumItemType.Targeted,
                            EnumItemTargettingType.EnemyHero)
                        },
                        {
                            "Hydra",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new Items.Item(3074, 450f),
                            EnumItemType.AoE,
                            EnumItemTargettingType.EnemyObjects)
                        },
                        {
                            "Titanic Hydra Cleave",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new Items.Item(3748, Orbwalking.GetRealAutoAttackRange(null) + 65),
                            EnumItemType.OnTarget,
                            EnumItemTargettingType.EnemyHero)
                        },
                        {
                            "Randiun",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new Items.Item(3143, 490f),
                            EnumItemType.AoE,
                            EnumItemTargettingType.EnemyHero)
                        },
                        {
                            "Hextech",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new Items.Item(3146, 750f),
                            EnumItemType.Targeted,
                            EnumItemTargettingType.EnemyHero)
                        },
                        {
                            "Entropy",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new Items.Item(3184, 750f),
                            EnumItemType.Targeted,
                            EnumItemTargettingType.EnemyHero)
                        },
                        {
                            "Youmuu's Ghostblade",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new Items.Item(3142, Orbwalking.GetRealAutoAttackRange(null) + 65),
                            EnumItemType.AoE,
                            EnumItemTargettingType.EnemyHero)
                        },
                        {
                            "Sword of the Divine",
                            new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                            new Items.Item(3131, Orbwalking.GetRealAutoAttackRange(null) + 65),
                            EnumItemType.AoE,
                            EnumItemTargettingType.EnemyHero)
                        }
                    };
        }

        private static void textAxe()
        {
            TextAxe = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Segoe UI",
                    Height = 39,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural,
                });
            TextLittle = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Segoe UI",
                    Height = 15,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural,
                });
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;
            new Helper();
        }

        internal class EnemyHeros
        {
            public Obj_AI_Hero Player;
            public int LastSeen;

            public EnemyHeros(Obj_AI_Hero player)
            {
                Player = player;
            }
        }

        internal class Helper
        {
            public static List<EnemyHeros> EnemyInfo = new List<EnemyHeros>();

            public Helper()
            {
                var champions = ObjectManager.Get<Obj_AI_Hero>().ToList();

                EnemyInfo = HeroManager.Enemies.Select(e => new EnemyHeros(e)).ToList();

                Game.OnUpdate += Game_OnGameUpdate;
            }

            private void Game_OnGameUpdate(EventArgs args)
            {
                foreach (EnemyHeros enemyInfo in EnemyInfo)
                {
                    if (!enemyInfo.Player.IsVisible)
                        enemyInfo.LastSeen = Environment.TickCount;
                }
            }
        }

        private static void spellsLoad()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            Q2 = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R);
            Q.SetSkillshot(0.25f, 75f, 1500f, false, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.25f, 75f, 1600f, false, SkillshotType.SkillshotLine);
            SpellList.Add(Q);
            SpellList.Add(E);
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        }
    }
}
