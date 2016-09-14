namespace YuLeLeblanc
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using YuLeLibrary;

    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R, R2;
        public static Menu Menu;
        public static int Rstate, Wstate, Ecol;
        public static int LastChain = 0;
        public static int LastChainM = 0;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad1(EventArgs args)
        {
            if (Player.ChampionName != "Leblanc")
                return;

            Q = new Spell(SpellSlot.Q, 710);
            W = new Spell(SpellSlot.W, 750);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R);
            R2 = new Spell(SpellSlot.R, 950);
            if (Rstate == 1)
                R = new Spell(SpellSlot.R, Q.Range);
            if (Rstate == 2)
            {
                R = new Spell(SpellSlot.R, W.Range);
                R.SetSkillshot(0, 70, 1500, false, SkillshotType.SkillshotLine);
            }

            //Q.SetSkillshot(300, 50, 2000, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 70, 1600, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0, 70, 1500, false, SkillshotType.SkillshotLine);

            Menu = new Menu("QQ群：438230879", "QQ群：438230879", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Chartreuse);

            var orbwalkerMenu = new Menu("走砍设置", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            var combomenu = Menu.AddSubMenu(new Menu("连招设置", "Combo"));
            {
                combomenu.AddItem(new MenuItem("Use Q Combo", "使用 Q").SetValue(true));
                combomenu.AddItem(new MenuItem("Use W Combo", "使用 W").SetValue(true));
                combomenu.AddItem(new MenuItem("Use W Combo Gap", "使用W突进").SetValue(true));
                combomenu.AddItem(new MenuItem("Use E Combo", "使用 E").SetValue(true));
                combomenu.AddItem(new MenuItem("Use R Combo", "使用 R").SetValue(true));
                combomenu.AddItem(new MenuItem("force focus selected", "集中攻击选择目标").SetValue(false));
                combomenu.AddItem(new MenuItem("if selected in :", "选择目标距离自身<= ").SetValue(new Slider(1000, 1000, 1500)));
            }

            var harassmenu = Menu.AddSubMenu(new Menu("骚扰设置", "Harass"));
            {
                harassmenu.AddItem(new MenuItem("Use Q Harass", "使用 Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("Use W Harass", "使用 W").SetValue(true));
                harassmenu.AddItem(new MenuItem("Use W Back Harass", "使用 W 返回").SetValue(true));
                harassmenu.AddItem(new MenuItem("HarassMana", "自身最低蓝量比").SetValue(new Slider(50, 100, 0)));
            }


            var laneclearmenu = Menu.AddSubMenu(new Menu("清线设置", "Lane Clear"));
            {
                laneclearmenu.AddItem(new MenuItem("LaneClearUseQ", "使用 Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("LaneClearUseW", "使用 W").SetValue(false));
                laneclearmenu.AddItem(new MenuItem("LaneClearMana", "自身最低蓝量比").SetValue(new Slider(50, 100, 0)));
            }

            var jungleclearmenu = Menu.AddSubMenu(new Menu("清野设置", "Jungle Clear"));
            {
                jungleclearmenu.AddItem(new MenuItem("JungleFarmUseQ", "使用 Q").SetValue(true));
                jungleclearmenu.AddItem(new MenuItem("JungleFarmUseW", "使用 W").SetValue(true));
                jungleclearmenu.AddItem(new MenuItem("JungleFarmUseE", "使用 E").SetValue(true));
                jungleclearmenu.AddItem(new MenuItem("JungleFarmMana", "自身最低蓝量比").SetValue(new Slider(20, 100, 0)));
            }

            var twochainmenu = Menu.AddSubMenu(new Menu("双重锁链", "Two Chains"));
            {
                twochainmenu.AddItem(new MenuItem("Two Chains Active", "使用按键").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                twochainmenu.AddItem(new MenuItem("Only On Selected Target", "仅对选择目标使用").SetValue(true));
            }

            var autowardmenu = Menu.AddSubMenu(new Menu("自动眼位", "Auto Ward"));
            {
                autowardmenu.AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
                autowardmenu.AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
                autowardmenu.AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
                autowardmenu.AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
                new AutoWard().Load();
                new Tracker().Load();
            }

            var skinmenu = Menu.AddSubMenu(new Menu("换肤设置", "Skin Chance"));
            {
                skinmenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                skinmenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "1", "2", "3", "4", "5", "6", "7", "8" })));
            }

            var drawmenu = Menu.AddSubMenu(new Menu("显示设置", "Drawings"));
            {
                drawmenu.AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
                drawmenu.AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
                drawmenu.AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
                drawmenu.AddItem(new MenuItem("wqRange", "W + Q 范围", true).SetValue(true));
            }

            Menu.AddItem(new MenuItem("WFlee", "逃跑按键").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            if (Menu.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Menu.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
            }

            if (Menu.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Gray, 1, 1);
            }

            if (Menu.Item("wqRange", true).GetValue<bool>())
            {
                Utility.DrawCircle(ObjectManager.Player.Position, W.Range + Q.Range, System.Drawing.Color.Red, 1, 1);
            }
        }

        public static bool WgapCombo { get { return Menu.Item("Use W Combo Gap").GetValue<bool>(); } }


        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            setbool();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (Menu.Item("Two Chains Active").GetValue<KeyBind>().Active)
            {
                Obj_AI_Hero target = null;
                if (Menu.Item("Only On Selected Target").GetValue<bool>())
                {
                    target = TargetSelector.GetSelectedTarget();
                }
                else
                    target = TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget() && Orbwalking.InAutoAttackRange(target))
                {
                    TwoChainsActive(target);
                }
                else
                {
                    TwoChainsActive(null);
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (ObjectManager.Player.ManaPercent > Menu.Item("HarassMana").GetValue<Slider>().Value)
                {
                    if (Menu.Item("Use Q Harass").GetValue<bool>())
                    {
                        useQ();
                    }
                    if (Menu.Item("Use W Harass").GetValue<bool>())
                    {
                        useWH();
                    }
                    if (Menu.Item("Use W Back Harass").GetValue<bool>())
                    {
                        useWBH();
                    }
                }
            }
            CheckR();
            CheckW();

            if (Orbwalker.ActiveMode== Orbwalking.OrbwalkingMode.LaneClear)
            {
                LC();
                JC();
            }
            if (Menu.Item("WFlee").GetValue<KeyBind>().Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                if (W.IsReady() && !LeBlancStillJumped)
                {
                    W.Cast(Game.CursorPos);
                }

                if(R.IsReady() && Rstate == 2)
                {
                    R.Cast(Game.CursorPos);
                }
            }
        }

        private static void LC()
        {
            if (!Orbwalking.CanMove(40))
                return;

            if (!LeBlancStillJumped)
            {
                return;
            }

            if (ObjectManager.Player.ManaPercent < Menu.Item("LaneClearMana").GetValue<Slider>().Value)
            {
                return;
            }
            var useW = Menu.Item("LaneClearUseW").GetValue<bool>();

            var xUseQ = Menu.Item("LaneClearUseQ").GetValue<bool>();
            if (Q.IsReady() && xUseQ)
            {
                var minionsQ = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                foreach (Obj_AI_Base vMinion in
                    from vMinion in minionsQ
                    let vMinionQDamage = ObjectManager.Player.GetSpellDamage(vMinion, SpellSlot.Q)
                    where vMinion.Health <= vMinionQDamage && vMinion.Health > ObjectManager.Player.GetAutoAttackDamage(vMinion)
                    select vMinion)
                {
                    Q.CastOnUnit(vMinion);
                }

                foreach (var minion in minionsQ.Where(m => LeagueSharp.Common.HealthPrediction.GetHealthPrediction(m, (int)(ObjectManager.Player.AttackCastDelay * 1000), Game.Ping / 2 - 100) < 0) .Where(m => m.Health <= Q.GetDamage(m)))
                {
                    Q.CastOnUnit(minion);
                }
            }

            var rangedMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 20);
            if (!useW || !W.IsReady())
                return;

            var minionsW = W.GetCircularFarmLocation(rangedMinionsW, W.Width * 0.75f);

            if (minionsW.MinionsHit < 2 || !W.IsInRange(minionsW.Position.To3D()))
                return;

            W.Cast(minionsW.Position);
        }

        public static bool LeBlancStillJumped
        {
            get
            {
                return !W.IsReady() || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "leblancslidereturn";
            }
        }

        private static void JC()
        {
            if (ObjectManager.Player.ManaPercent < Menu.Item("JungleFarmMana").GetValue<Slider>().Value)
            {
                return;
            }
            var useQ = Menu.Item("JungleFarmUseQ").GetValue<bool>();
            var useW = Menu.Item("JungleFarmUseW").GetValue<bool>();
            var useE = Menu.Item("JungleFarmUseE").GetValue<bool>();

            var mobs = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0)
                return;
            var mob = mobs[0];
            if (useQ && Q.IsReady())
                Q.CastOnUnit(mob);

            if (useW && W.IsReady() && mobs.Count >= 2 && !LeBlancStillJumped)
                W.Cast(mob.Position);

            if (useE && E.IsReady())
                E.Cast(mob);
        }

        private static void setbool()
        {
            AutoWard.Enable = Menu.GetBool("AutoWard");
            AutoWard.AutoBuy = Menu.GetBool("AutoBuy");
            AutoWard.AutoPink = Menu.GetBool("AutoPink");
            AutoWard.OnlyCombo = Menu.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;

            if (Menu.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, Menu.Item("SkinSelect").GetValue<StringList>().SelectedIndex);
            }
            else if (!Menu.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, 0);
            }
        }

        public static bool Selected()
        {
            if (!Menu.Item("force focus selected").GetValue<bool>())
            {
                return false;
            }
            else
            {
                var target = TargetSelector.GetSelectedTarget();
                float a = Menu.Item("if selected in :").GetValue<Slider>().Value;
                if (target == null || target.IsDead || target.IsZombie)
                {
                    return false;
                }
                else
                {
                    if (Player.Distance(target.Position) > a)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        public static void useQ()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

            }
            else
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {Game_OnGameLoad1(args);
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


        public static void useE()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget(E.Range))
                {
                    CastE(target);
                }

            }
            else
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical) ??
                             TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (target != null && target.IsValidTarget(E.Range))
                {
                    CastE(target);
                }
            }
        }
        public static void useW()
        {
            if (Menu.Item("Use W Combo").GetValue<bool>())
            {
                if (Selected())
                {
                    var target = TargetSelector.GetSelectedTarget();
                    if (target != null && target.IsValidTarget(W.Range))
                    {
                        CastW(target);
                    }

                }
                else
                {
                    var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                    if (target != null && target.IsValidTarget(W.Range))
                    {
                        CastW(target);
                    }
                }
            }
        }

        public static void useWH()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget(W.Range))
                {
                    CastW(target);
                }

            }
            else
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                if (target != null && target.IsValidTarget(W.Range))
                {
                    CastW(target);
                }
            }
        }

        public static void useWBH()
        {
            if (Wstate == 2)
                W.Cast();
        }

        public static void useR()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    CastR(target);
                }

            }
            else
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (target != null && target.IsValidTarget(E.Range))
                {
                    CastR(target);
                }
            }
        }

        public static void CastR(Obj_AI_Base target)
        {
            if (R.IsReady())
            {
                if (Rstate == 1)
                {
                    if (target.IsValidTarget(R.Range))
                    {
                        R.Cast(target);
                    }
                }
                if (Rstate == 2)
                {
                    var t = Prediction.GetPrediction(target, 400).CastPosition;
                    float x = target.MoveSpeed;
                    float y = x * 400 / 1000;
                    var pos = target.Position;
                    if (target.Distance(t) <= y)
                    {
                        pos = t;
                    }
                    if (target.Distance(t) > y)
                    {
                        pos = target.Position.Extend(t, y - 50);
                    }
                    if (Player.Distance(pos) <= 600)
                    {
                        R.Cast(pos);
                    }
                    if (Player.Distance(pos) > 600)
                    {
                        if (target.Distance(t) > y)
                        {
                            var pos2 = target.Position.Extend(t, y);
                            if (Player.Distance(pos2) <= 600)
                            {
                                R.Cast(pos2);
                            }
                            else
                            {
                                var prediction = R.GetPrediction(target);
                                if (prediction.Hitchance >= HitChance.High)
                                {
                                    var pos3 = prediction.CastPosition;
                                    var pos4 = Player.Position.Extend(pos3, 600);
                                    R.Cast(pos4);
                                }
                            }
                        }

                    }
                }
            }
        }

        public static void CastW(Obj_AI_Base target)
        {
            if (!W.IsReady() || Wstate != 1)
                return;
            var t = Prediction.GetPrediction(target, 400).CastPosition;
            float x = target.MoveSpeed;
            float y = x * 400 / 1000;
            var pos = target.Position;
            if (target.Distance(t) <= y)
            {
                pos = t;
            }
            if (target.Distance(t) > y)
            {
                pos = target.Position.Extend(t, y - 50);
            }
            if (Player.Distance(pos) <= 600)
            {
                W.Cast(pos);
            }
            if (Player.Distance(pos) > 600)
            {
                if (target.Distance(t) > y)
                {
                    var pos2 = target.Position.Extend(t, y);
                    if (Player.Distance(pos2) <= 600)
                    {
                        W.Cast(pos2);
                    }
                    else
                    {
                        var prediction = W.GetPrediction(target);
                        if (prediction.Hitchance >= HitChance.High)
                        {
                            var pos3 = prediction.CastPosition;
                            var pos4 = Player.Position.Extend(pos3, 600);
                            W.Cast(pos4);
                        }
                    }

                }
            }
        }

        public static void CastE(Obj_AI_Base target)
        {
            if (E.IsReady() && !Player.IsDashing())
            {
                if (!R.IsReady())
                { E.Cast(target); }
                if (R.IsReady() && Rstate == 4)
                { E.Cast(target); }
            }
        }

        public static void CheckE(Obj_AI_Base target)
        {
            if (E.IsReady())
            {
                var prediction = E.GetPrediction(target);
                if (prediction.Hitchance == HitChance.Collision)
                {
                    Ecol = 1;
                }
                else
                {
                    Ecol = 0;
                }
            }
            if (!E.IsReady())
            {
                Ecol = 0;
            }
        }
        public static void CheckR()
        {
            string x = Player.Spellbook.GetSpell(SpellSlot.R).Name;          
            if (x == "LeblancChaosOrbM")
                Rstate = 1;
            if (x == "LeblancSlideM")
                Rstate = 2;
            if (x == "LeblancSoulShackleM")
                Rstate = 3;
            if (x == "LeblancSlideReturnM")
            {
                Rstate = 4;
            }
            if (Rstate == 1)
                R = new Spell(SpellSlot.R, Q.Range);
            if (Rstate == 2)
            {
                R = new Spell(SpellSlot.R, W.Range);
                R.SetSkillshot(0, 70, 1500, false, SkillshotType.SkillshotLine);
            }
        }
        public static void CheckW()
        {
            string x = Player.Spellbook.GetSpell(SpellSlot.W).Name;
            if (x == "LeblancSlideReturn")
            {
                Wstate = 2;
            }
            else
                Wstate = 1;
        }

        public static void Combo()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                CheckE(target);
                float a = Player.Distance(target.Position);
                if (a > Q.Range && a <= 1200)
                {
                    if (WgapCombo && R.IsReady() && Rstate != 4 && W.IsReady() 
                        && Wstate != 2 && Menu.Item("Use W Combo").GetValue<bool>())
                    {
                        W.Cast(Player.Position.Extend(target.Position, 600));
                    }
                }
                else if (a <= Q.Range)
                {
                    if (Ecol == 1)
                    {
                        if (W.IsReady() && Wstate != 2)
                        {
                            useW();
                            useQ();
                            useR();
                            useE();
                        }
                        if (!W.IsReady() && Wstate == 1 && R.IsReady() && Rstate == 2)
                        {
                            useR();
                            useQ();
                            useE();
                            useW();
                        }
                        else
                        {
                            useQ();
                            useR();
                            useE();
                            useW();
                        }
                    }
                    if (Ecol == 0)
                    {
                        useQ();
                        useR();
                        useE();
                        if (!(R.IsReady() && Rstate == 1))
                            useW();
                    }
                }
            }
            else
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget())
                {
                    CheckE(target);
                    if (Ecol == 1)
                    {
                        if (W.IsReady() && Wstate != 2)
                        {
                            useW();
                            useQ();
                            useR();
                            useE();
                        }
                        if (!W.IsReady() && Wstate == 1 && R.IsReady() && Rstate == 2)
                        {
                            useR();
                            useQ();
                            useE();
                            useW();
                        }
                        else
                        {
                            useQ();
                            useR();
                            useE();
                            useW();
                        }
                    }
                    if (Ecol == 0)
                    {
                        useQ();
                        useR();
                        useE();
                        if (!(R.IsReady() && Rstate == 1))
                            useW();
                    }
                }
                else
                {
                    var target1 = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Magical);
                    if (target1 != null)
                    {
                        if (WgapCombo && R.IsReady() && Rstate != 4 && W.IsReady() 
                            && Wstate != 2 && Menu.Item("Use W Combo").GetValue<bool>())
                        {
                            W.Cast(Player.Position.Extend(target1.Position, 600));
                        }
                    }
                }
            }
        }

        public static void TwoChainsActive(Obj_AI_Hero target)
        {
            R2.SetSkillshot(0.25f, 70, 1600, true, SkillshotType.SkillshotLine);
            Orbwalking.Orbwalk(target, Game.CursorPos, 90, 50);
            if (target != null && target.IsValidTarget(Program.Q.Range) && (Program.Rstate != 3 || !R2.IsReady()))
            {
                Program.Q.Cast(target);
            }
            if (target != null && target.IsValidTarget(Program.E.Range) && Program.E.IsReady() && !ObjectManager.Player.IsDashing()
                 && !target.HasBuff("LeblancSoulShackleM") && Environment.TickCount - LastChainM >= 1500 + Game.Ping)
            {
                Program.E.Cast(target);
                LastChain = Environment.TickCount;
            }
            if (target != null && target.IsValidTarget(Program.E.Range) && Program.R.IsReady() && !ObjectManager.Player.IsDashing() && Program.Rstate == 3
                && !target.HasBuff("LeblancSoulShackle") && Environment.TickCount - LastChain >= 1500 + Game.Ping)
            {
                R2.Cast(target);
                LastChainM = Environment.TickCount;
            }
            if (target.IsValidTarget(Program.W.Range) && Program.Menu.Item("Use W Combo").GetValue<bool>() && (Program.Rstate != 3 || !R2.IsReady()))
            {
                Program.CastW(target);
            }
        }


        public static void useQE()
        {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget() && !target.IsZombie)
                {
                    if( Player.Distance(target.Position) <= Q.Range)
                    {
                        Q.Cast(target);
                    }
                    if (Player.Distance(target.Position) <= E.Range)
                    {
                        E.Cast(target);
                    }
                } 
        }
    }
}
