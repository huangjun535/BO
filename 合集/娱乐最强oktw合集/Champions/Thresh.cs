using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SebbyLib;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Thresh
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Epush, Q, R, W;
        private static Obj_AI_Base Marked;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 1075);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 480);
            R = new Spell(SpellSlot.R, 400);
            Epush = new Spell(SpellSlot.E, 450);

            Q.SetSkillshot(0.5f, 70, 1900f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.2f, 10, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 50, 2000, false, SkillshotType.SkillshotLine);
            Epush.SetSkillshot(0f, 50, float.MaxValue, false, SkillshotType.SkillshotLine);

            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("ts", "Use common TargetSelector", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("ts1", "ON - only one target"));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("ts2", "OFF - all grab-able targets"));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("qCC", "Auto Q cc", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("qDash", "Auto Q dash", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("minGrab", "Min range grab", true).SetValue(new Slider(250, 125, (int)Q.Range)));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("maxGrab", "Max range grab", true).SetValue(new Slider((int)Q.Range, 125, (int)Q.Range)));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("Q option").SubMenu("Grab").AddItem(new MenuItem("grab" + enemy.ChampionName, enemy.ChampionName).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("GapQ", "OnEnemyGapcloser Q",true)).SetValue(true);

            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("Wdmg", "W dmg % hp", true).SetValue(new Slider(10, 100, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW3", "Auto W shield big dmg", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW2", "Auto W if Q succesfull", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW4", "Auto W vs Blitz Hook", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW5", "Auto W if jungler pings", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW6", "Auto W on gapCloser", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("autoW7", "Auto W on Slows/Stuns", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W option").AddItem(new MenuItem("wCount", "Auto W if x enemies near ally", true).SetValue(new Slider(3, 0, 5)));

            Config.SubMenu(Player.ChampionName).SubMenu("E option").AddItem(new MenuItem("autoE", "Auto E", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E option").AddItem(new MenuItem("pushE", "Auto push", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E option").AddItem(new MenuItem("inter", "OnPossibleToInterrupt" , true)).SetValue(true);
            Config.SubMenu(Player.ChampionName).SubMenu("E option").AddItem(new MenuItem("Gap", "OnEnemyGapcloser", true)).SetValue(true);
            Config.SubMenu(Player.ChampionName).SubMenu("E option").AddItem(new MenuItem("Emin", "Min pull range E", true).SetValue(new Slider(200, 0, (int)E.Range)));

            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("rCount", "Auto R if x enemies in range", true).SetValue(new Slider(2, 0, 5)));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("rKs", "R ks", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("comboR", "always R in combo", true).SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw when skill rdy", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("AACombo", "Disable AA if can use E", true).SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
            Obj_AI_Base.OnBuffRemove += Obj_AI_Base_OnBuffRemove;
            
            Game.OnPing += Game_OnPing;
        }

        void Game_OnPing(GamePingEventArgs args)
        {
            //if jungler pings OnMyWay
            //give him lantern for easy gank
            if(!Config.Item("autoW5",true).GetValue<bool>())return;
            var jungler = args.Source as Obj_AI_Hero;
            if (jungler != null && jungler.Distance(Player.Position)<=W.Range+500)
            {
                if (jungler.Spellbook.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("smite") ||
                    jungler.Spellbook.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("smite"))
                {
                    if (args.PingType == PingCategory.OnMyWay)
                    {
                        int random = new Random().Next(350, 750); // so it wont happen too fast
                        Utility.DelayAction.Add(random,() => CastW(args.Position.To3D()));
                    }

                }
            }
        }


        private void Obj_AI_Base_OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsEnemy && args.Buff.Name == "ThreshQ")
            {
                Marked = null;
            }
        }

        private void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsEnemy && args.Buff.Name == "ThreshQ")
            {
                Marked = sender;
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && Config.Item("inter", true).GetValue<bool>() && sender.IsValidTarget(E.Range))
            {
                E.Cast(sender.ServerPosition);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if(gapcloser.Sender.IsAlly)return;

            if (Config.Item("autoW6",true).GetValue<bool>())
            {
                var allyHero =
                    HeroManager.Allies.Where(ally => ally.Distance(Player) <= W.Range + 550 && !ally.IsMe)
                        .OrderBy(ally => ally.Distance(gapcloser.End))
                        .FirstOrDefault();
                if (allyHero != null)
                {
                    CastW(allyHero.Position);
                }
            }
            if (E.IsReady() && Config.Item("Gap", true).GetValue<bool>() && gapcloser.Sender.IsValidTarget(E.Range) && !Marked.IsValidTarget())
            {
                E.Cast(gapcloser.Sender);
            }
            else if (Q.IsReady() && Config.Item("GapQ", true).GetValue<bool>() && gapcloser.Sender.IsValidTarget(Q.Range))
            {
                Q.Cast(gapcloser.Sender);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {

            if (Program.Combo && Config.Item("AACombo", true).GetValue<bool>())
            {
                if (!E.IsReady())
                    SebbyLib.Orbwalking.Attack = true;

                else
                    SebbyLib.Orbwalking.Attack = false;
            }
            else
                SebbyLib.Orbwalking.Attack = true;

            if (Marked.IsValidTarget())
            {
                if (Program.Combo)
                {
                    if (OktwCommon.GetPassiveTime(Marked, "ThreshQ") < 0.3)
                        Q.Cast();

                    if (W.IsReady() && Config.Item("autoW2", true).GetValue<bool>())
                    {
                        var allyW = Player;
                        foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && !ally.IsDead && Player.Distance(ally.ServerPosition) < W.Range + 500))
                        {
                            if (Marked.Distance(ally.ServerPosition) > 800 && Player.Distance(ally.ServerPosition) > 600)
                            {
                                CastW(Prediction.GetPrediction(ally, 1f).CastPosition);
                            }
                        }
                    }
                }
            }
            else
            {
                if (Program.LagFree(1) && Q.IsReady())
                    LogicQ();

                if (Program.LagFree(2) && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                    LogicE();
            }

            if (Program.LagFree(3) && W.IsReady())
                LogicW();
            if (Program.LagFree(4) && R.IsReady())
                LogicR();
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget()  && OktwCommon.CanMove(t) && !Marked.IsValidTarget())
            {
                
                if (Program.Combo)
                {
                    if (Player.Distance(t) > Config.Item("Emin", true).GetValue<Slider>().Value)
                    CastE(false, t);
                }
                else if (Config.Item("pushE", true).GetValue<bool>())
                {
                    CastE(true, t);
                }
            }
        }

        private void CastE(bool push, Obj_AI_Base target)
        {
            if (push)
            {
                var eCastPosition = E.GetPrediction(target).CastPosition;
                E.Cast(eCastPosition);
            }
            else
            {
                var eCastPosition = Epush.GetPrediction(target).CastPosition;
                var distance = Player.Distance(eCastPosition);
                var ext = Player.Position.Extend(eCastPosition, -distance);
                E.Cast(ext);
            }
        }

        private void LogicQ()
        {
            float maxGrab = Config.Item("maxGrab", true).GetValue<Slider>().Value;
            float minGrab = Config.Item("minGrab", true).GetValue<Slider>().Value;

            if (Program.Combo && Config.Item("ts", true).GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(maxGrab, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget(maxGrab) && !t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) && Config.Item("grab" + t.ChampionName).GetValue<bool>() && Player.Distance(t.ServerPosition) > minGrab)
                    Program.CastSpell(Q, t);
            }

            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget(maxGrab) && Config.Item("grab" + t.ChampionName).GetValue<bool>() && Player.Distance(t.ServerPosition) > minGrab))
            {
                if (!t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) )
                {
                    if (Program.Combo && !Config.Item("ts", true).GetValue<bool>())
                        Program.CastSpell(Q, t);

                    if (Config.Item("qCC", true).GetValue<bool>())
                    {
                        if (!OktwCommon.CanMove(t))
                            Q.Cast(t);

                        Q.CastIfHitchanceEquals(t, HitChance.Immobile);
                    }
                    if (Config.Item("qDash", true).GetValue<bool>())
                    {
                        Q.CastIfHitchanceEquals(t, HitChance.Dashing);
                    }
                }
            }
        }

        private void LogicR()
        {

            var rCountOut = Player.CountEnemiesInRange(R.Range);
            var rCountIn = Player.CountEnemiesInRange(R.Range - 180);

            if (rCountOut < rCountIn)
                return;

            if (rCountOut >= Config.Item("rCount", true).GetValue<Slider>().Value && Config.Item("rCount", true).GetValue<Slider>().Value > 0)
                R.Cast();

            if (Config.Item("comboR", true).GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget() && ((Player.UnderTurret(false) && !Player.UnderTurret(true)) || Program.Combo))
                {
                    if (Player.Distance(t.ServerPosition) > Player.Distance(t.Position))
                        R.Cast();
                }
            }
        }

        private void LogicW()
        {
            if (Config.Item("autoW4", true).GetValue<bool>())
            {
                var saveAlly = HeroManager.Allies.FirstOrDefault(ally => ally.HasBuff("rocketgrab2") && !ally.IsMe);
                if (saveAlly != null)
                {
                    var blitz = saveAlly.GetBuff("rocketgrab2").Caster;
                    if (Player.Distance(blitz.Position) <= W.Range + 550 && W.IsReady())
                    {

                        CastW(blitz.Position);
                    }
                }
            }

            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && !ally.IsDead && Player.Distance(ally) < W.Range + 400))
            {
                if (Config.Item("autoW7", true).GetValue<bool>() && !ally.IsMe)
                {
                    if (ally.Distance(Player) <= W.Range)
                    {
                        if (ally.IsStunned || ally.IsRooted)
                        {
                            W.Cast(ally.Position);
                        }
                    }
                }

                int nearEnemys = ally.CountEnemiesInRange(900);

                if (nearEnemys >= Config.Item("wCount", true).GetValue<Slider>().Value && Config.Item("wCount", true).GetValue<Slider>().Value > 0)
                    CastW(W.GetPrediction(ally).CastPosition);

                if (Config.Item("autoW", true).GetValue<bool>() && Player.Distance(ally) < W.Range + 100)
                {
                    double dmg = OktwCommon.GetIncomingDamage(ally);
                    if (dmg == 0)
                        continue;

                    int sensitivity = 20;

                    double HpPercentage = (dmg * 100) / ally.Health;
                    double shieldValue = 20 + (Player.Level * 20) + (0.4 * Player.FlatMagicDamageMod);

                    nearEnemys = (nearEnemys == 0) ? 1 : nearEnemys;

                    if (dmg > shieldValue && Config.Item("autoW3", true).GetValue<bool>())
                        W.Cast(W.GetPrediction(ally).CastPosition);
                    else if (dmg > 100 + Player.Level * sensitivity)
                        W.Cast(W.GetPrediction(ally).CastPosition);
                    else if (ally.Health - dmg < nearEnemys * ally.Level * sensitivity)
                        W.Cast(W.GetPrediction(ally).CastPosition);
                    else if (HpPercentage >= Config.Item("Wdmg", true).GetValue<Slider>().Value)
                        W.Cast(W.GetPrediction(ally).CastPosition);
                }
            }
        }

        private void CastW(Vector3 pos)
        {
            if (Player.Distance(pos) < W.Range)
                W.Cast(pos);
            else
                W.Cast(Player.Position.Extend(pos, W.Range));
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (Q.IsReady())
                        Utility.DrawCircle(Player.Position, (float)Config.Item("maxGrab", true).GetValue<Slider>().Value, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, (float)Config.Item("maxGrab", true).GetValue<Slider>().Value, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (E.IsReady())
                        Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
            }

            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (R.IsReady())
                        Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
                }
                else
                    Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }
    }
}
