namespace YuleJinx
{
    using FontColor = SharpDX.Color;
    using Color = System.Drawing.Color;
    using FontStyle = System.Drawing.FontStyle;
    using System;
    using LeagueSharp;
    using LeagueSharp.Common;
    using System.Collections.Generic;
    using System.Linq;
    using SharpDX;
    using SebbyLib;
    using Orbwalking = SebbyLib.Orbwalking;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    class Program
    {
        public static Menu Menu;
        public static Obj_AI_Hero Player;
        public static Spell Q, W, E, R, DrawSpell;
        public static Orbwalking.Orbwalker Orbwalker;
        public static float DrawSpellTime = 0, DragonDmg = 0, lag = 0, LatFocusTime = Game.Time;
        public static double DragonTime = 0;
        public static bool HaveBigGun => Player.HasBuff("JinxQ");

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            try
            {
                if (ObjectManager.Player.ChampionName != "Jinx")
                    return;

                Player = ObjectManager.Player;

                new AutoUlt();

                LoadMenu();

                LoadSpells();

                LoadEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in OnGameLoad " + ex);
            }
        }

        private static void LoadEvents()
        {
            try
            {
                Game.OnUpdate += OnUpdate;
                AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
                Drawing.OnDraw += OnDraw;
                Orbwalking.BeforeAttack += BeforeAttack;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Events Loading " + ex);
            }
        }

        #region

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            try
            {
                if (!Q.IsReady())
                    return;

                if (!(args.Target is Obj_AI_Hero))
                    return;

                var t = (Obj_AI_Hero)args.Target;

                if (HaveBigGun && t.IsValidTarget())
                {
                    var RealDistance = Player.ServerPosition.Distance(Prediction.GetPrediction(t, 0.05f).CastPosition) + Player.BoundingRadius + t.BoundingRadius;

                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Menu.Item("ComboQ").GetValue<bool>())
                    {
                        if (RealDistance < (650f + Player.BoundingRadius + t.BoundingRadius))
                        {
                            if(Player.Mana < R.ManaCost + 20 || Player.GetAutoAttackDamage(t) * 3 < t.Health)
                            {
                                Q.Cast();
                            }
                        }
                    }
                    else if((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) && Menu.Item("HarassQ").GetValue<bool>())
                    {
                        if((RealDistance > (670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level) || RealDistance < (650f + Player.BoundingRadius + t.BoundingRadius) || Player.Mana < R.ManaCost + E.ManaCost + W.ManaCost + W.ManaCost))
                        {
                            Q.Cast();
                        }
                    }
                }

                if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && !HaveBigGun && Menu.Item("LaneClearQ").GetValue<bool>())
                {
                    if(Player.Mana > R.ManaCost + E.ManaCost + W.ManaCost + 30)
                    {
                        var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, (670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level));
                        foreach (var minion in allMinionsQ.Where(minion => args.Target.NetworkId != minion.NetworkId && minion.Distance(args.Target.Position) < 200 && (5 - Q.Level) * Player.GetAutoAttackDamage(minion) < args.Target.Health && (5 - Q.Level) * Player.GetAutoAttackDamage(minion) < minion.Health))
                        {
                            Q.Cast();
                        }
                    }
                }

                if (Menu.Item("TsMode").GetValue<StringList>().SelectedIndex != 0 || !Menu.Item("ExtraFocus").GetValue<bool>() || !(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
                {
                    return;
                }

                if (args.Target is Obj_AI_Hero)
                {
                    var newTarget = (Obj_AI_Hero)args.Target;
                    var forceFocusEnemy = newTarget;
                    var aaRange = Player.AttackRange + Player.BoundingRadius + 350;

                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(aaRange)))
                    {
                        if (enemy.Health / Player.GetAutoAttackDamage(enemy) + 1 < forceFocusEnemy.Health / Player.GetAutoAttackDamage(forceFocusEnemy))
                        {
                            forceFocusEnemy = enemy;
                        }
                    }

                    if (forceFocusEnemy.NetworkId != newTarget.NetworkId && Game.Time - LatFocusTime < 2)
                    {
                        args.Process = false;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Before Attack Events " + ex);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            try
            {
                if (Player.IsDead)
                    return;

                if (Q.IsReady() && Menu.Item("QDraw").GetValue<Circle>().Active)
                    Render.Circle.DrawCircle(Player.Position, Player.AttackRange, Menu.Item("QDraw").GetValue<Circle>().Color);

                if (W.IsReady() && Menu.Item("WDraw").GetValue<Circle>().Active)
                    Render.Circle.DrawCircle(Player.Position, W.Range, Menu.Item("WDraw").GetValue<Circle>().Color);

                if (E.IsReady() && Menu.Item("EDraw").GetValue<Circle>().Active)
                    Render.Circle.DrawCircle(Player.Position, E.Range, Menu.Item("EDraw").GetValue<Circle>().Color);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in On Draw " + ex);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsMinion)
                    return;

                if (!E.IsReady())
                    return;

                if(sender.IsEnemy)
                {
                    if(Menu.Item("ProtectE").GetValue<bool>())
                    {
                        if(sender.IsValidTarget(E.Range))
                        {
                            if(ShouldUseE(args.SData.Name))
                            {
                                E.Cast(sender.ServerPosition, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in On Process Spell Cast " + ex);
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser g)
        {
            if (!E.IsReady())
                return;

            if(Menu.Item("AntiE").GetValue<bool>())
            {
                if(g.Sender.IsValidTarget(E.Range))
                {
                    if (E.GetPrediction(g.Sender).Hitchance >= HitChance.VeryHigh)
                    {
                        E.Cast(g.Sender, true);
                    }
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                if (Player.IsDead)
                    return;

                TSMode();

                QLogic();

                WLogic();

                ELogic();

                ToggleRLogic();

                StealJungleRLogic();

                AutoRLogic();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in On Update " + ex);
            }
        }

        private static void TSMode()
        {
            try
            {
                var orbT = Orbwalker.GetTarget();

                if (orbT != null && orbT.Type == GameObjectType.obj_AI_Hero)
                {
                    var bestTarget = (Obj_AI_Hero)orbT;
                    var hitToBestTarget = bestTarget.Health / Player.GetAutoAttackDamage(bestTarget);

                    if (Menu.Item("TsMode").GetValue<StringList>().SelectedIndex == 0)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget() && Orbwalker.InAutoAttackRange(enemy)))
                        {
                            if (enemy.Health / Player.GetAutoAttackDamage(enemy) < hitToBestTarget)
                            {
                                bestTarget = enemy;
                            }
                        }
                    }
                    else
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget() && Orbwalker.InAutoAttackRange(enemy)))
                        {

                            if (enemy.Health / Player.GetAutoAttackDamage(enemy) < 3)
                            {
                                bestTarget = enemy;
                                break;
                            }
                            if (Menu.Item("TsModePriority" + enemy.ChampionName).GetValue<Slider>().Value > Menu.Item("TsModePriority" + bestTarget.ChampionName).GetValue<Slider>().Value)
                            {
                                bestTarget = enemy;
                            }

                        }
                    }
                    if (bestTarget.NetworkId != orbT.NetworkId)
                    {
                        Orbwalker.ForceTarget(bestTarget);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in TS Mode " + ex);
            }
        }

        private static void QLogic()
        {
            try
            {
                if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) && (Game.Time - lag > 0.1) && !HaveBigGun && !Player.IsWindingUp && Orbwalking.CanAttack() && Player.Mana > R.ManaCost + W.ManaCost + E.ManaCost + 10 && Menu.Item("HarassQ").GetValue<bool>())
                {
                    foreach (var minion in MinionManager.GetMinions(670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level + 30).Where(minion => !Orbwalking.InAutoAttackRange(minion) && minion.Health < Player.GetAutoAttackDamage(minion) * 1.2 && (650f + Player.BoundingRadius + minion.BoundingRadius) < (Player.ServerPosition.Distance(Prediction.GetPrediction(minion, 0.05f).CastPosition) + Player.BoundingRadius + minion.BoundingRadius) && (670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level) < (Player.ServerPosition.Distance(Prediction.GetPrediction(minion, 0.05f).CastPosition) + Player.BoundingRadius + minion.BoundingRadius)))
                    {
                        Orbwalker.ForceTarget(minion);
                        Q.Cast();
                        return;
                    }
                    lag = Game.Time;
                }

                var t = TargetSelector.GetTarget((670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level) + 60, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    if (!HaveBigGun && (!Orbwalking.InAutoAttackRange(t) || t.CountEnemiesInRange(250) > 2) && Orbwalker.GetTarget() == null)
                    {
                        var distance = Player.ServerPosition.Distance(Prediction.GetPrediction(t, 0.05f).CastPosition) + Player.BoundingRadius + t.BoundingRadius;

                        if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && (Player.Mana > R.ManaCost + W.ManaCost + 10 || Player.GetAutoAttackDamage(t) * 3 > t.Health))
                        {
                            Q.Cast();
                        }
                        else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Menu.Item("HarassQ").GetValue<bool>())
                        {
                            if(!Player.IsWindingUp)
                                if(Orbwalking.CanAttack())
                                    if(!Player.UnderTurret(true))
                                        if(Player.Mana > R.ManaCost + W.ManaCost + E.ManaCost + 20)
                                            if(distance < (670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level) + t.BoundingRadius + Player.BoundingRadius)
                                                Q.Cast();
                        }
                    }
                }
                else if (!HaveBigGun && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Player.Mana > R.ManaCost + W.ManaCost + 20 && Player.CountEnemiesInRange(2000) > 0)
                {
                    Q.Cast();
                }
                else if (HaveBigGun && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Player.Mana < R.ManaCost + W.ManaCost + 20)
                {
                    Q.Cast();
                }
                else if (HaveBigGun && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Player.CountEnemiesInRange(2000) == 0)
                {
                    Q.Cast();
                }
                else if (HaveBigGun && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit))
                {
                    Q.Cast();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Q Logic " + ex);
            }
        }

        private static void AutoRLogic()
        {
            try
            {
                if (!R.IsReady())
                    return;

                if (Menu.Item("AutoREnable").GetValue<bool>())
                {
                    bool cast = false;

                    foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget() && CanKill(target)))
                    {
                        float predictedHealth = target.Health + target.HPRegenRate * 2;

                        var Rdmg = R.GetDamage(target, 1);

                        if (Rdmg > predictedHealth)
                        {
                            cast = true;

                            PredictionOutput output = R.GetPrediction(target);

                            Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();

                            direction.Normalize();

                            List<Obj_AI_Hero> enemies = HeroManager.Enemies.Where(x => x.IsEnemy && x.IsValidTarget()).ToList();

                            foreach (var enemy in enemies)
                            {
                                if (enemy.SkinName == target.SkinName || !cast)
                                    continue;

                                PredictionOutput prediction = R.GetPrediction(enemy);

                                Vector3 predictedPosition = prediction.CastPosition;
                                Vector3 v = output.CastPosition - Player.ServerPosition;
                                Vector3 w = predictedPosition - Player.ServerPosition;

                                double c1 = Vector3.Dot(w, v);
                                double c2 = Vector3.Dot(v, v);
                                double b = c1 / c2;

                                Vector3 pb = Player.ServerPosition + ((float)b * v);

                                float length = Vector3.Distance(predictedPosition, pb);

                                if (length < (R.Width + 150 + enemy.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                                    cast = false;
                            }

                            if (cast && (Player.ServerPosition.Distance(target.ServerPosition) + Player.BoundingRadius + target.BoundingRadius) > (670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level) + 300 + target.BoundingRadius && target.CountAlliesInRange(600) == 0 && Player.CountEnemiesInRange(400) == 0)
                            {
                                List<Vector2> waypoints = target.GetWaypoints();

                                if ((Player.Distance(waypoints.Last().To3D()) - Player.Distance(target.Position)) > 400)
                                    SpellCast(R, target);
                            }

                            else if (cast && target.CountEnemiesInRange(200) > 2 && (Player.ServerPosition.Distance(target.ServerPosition) + Player.BoundingRadius + target.BoundingRadius) > (670f + Player.BoundingRadius + 25 * Player.Spellbook.GetSpell(SpellSlot.Q).Level) + 200 + target.BoundingRadius)
                            {
                                R.Cast(target, true, true);
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Auto R Logic " + ex);
            }
        }

        private static void StealJungleRLogic()
        {
            try
            {
                if (!R.IsReady())
                    return;

                var mobs = MinionManager.GetMinions(Player.ServerPosition, float.MaxValue, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

                foreach (var mob in mobs)
                {
                    if (mob.Health < mob.MaxHealth && ((mob.SkinName == "SRU_Dragon" && Menu.Item("Rdragon", true).GetValue<bool>()) || (mob.SkinName == "SRU_Baron" && Menu.Item("Rbaron", true).GetValue<bool>()))  && mob.CountAlliesInRange(1000) == 0 && mob.Distance(Player.Position) > 1000)
                    {
                        if (DragonDmg == 0)
                            DragonDmg = mob.Health;

                        if (Game.Time - DragonTime > 4)
                        {
                            if (DragonDmg - mob.Health > 0)
                            {
                                DragonDmg = mob.Health;
                            }

                            DragonTime = Game.Time;
                        }
                        else
                        {
                            var DmgSec = (DragonDmg - mob.Health) * (Math.Abs(DragonTime - Game.Time) / 4);

                            if (DragonDmg - mob.Health > 0)
                            {
                                var timeTravel = GetUltTravelTime(Player, R.Speed, R.Delay, mob.Position);
                                var timeR = (mob.Health - Player.CalcDamage(mob, Damage.DamageType.Physical, (250 + (100 * R.Level)) + Player.FlatPhysicalDamageMod + 300)) / (DmgSec / 4);

                                if (timeTravel > timeR)
                                    R.Cast(mob.Position);
                            }
                            else
                            {
                                DragonDmg = mob.Health;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Steal Jungle R Logic " + ex);
            }
        }

        private static void ToggleRLogic()
        {
            try
            {
                if (!R.IsReady())
                    return;

                if (Menu.Item("Togglerkey").GetValue<KeyBind>().Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget())
                    {
                        if (Menu.Item("TogglerMode").GetValue<StringList>().SelectedIndex == 0)
                        {
                            if (t.HealthPercent < 50)
                                R.Cast(t);
                        }
                        else
                        {
                            R.CastIfWillHit(t, 2);
                            R.Cast(t, true, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Toggle R Logic " + ex);
            }
        }

        private static void WLogic()
        {
            try
            {
                if (!W.IsReady())
                    return;

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Menu.Item("ComboW").GetValue<bool>())
                {
                    var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

                    if (target == null)
                        return;

                    float distance = Player.Position.Distance(target.Position);

                    if (distance >= 550)
                        if (target.IsValidTarget(W.Range))
                            SpellCast(W, target);
                }

                if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Menu.Item("HarassW").GetValue<bool>()) || Menu.Item("HarassAutoW").GetValue<KeyBind>().Active)
                {
                    if (Player.ManaPercent < Menu.Item("HarassWManaPer").GetValue<Slider>().Value)
                        return;

                    if (Player.UnderTurret(true))
                        return;

                    var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

                    if (target == null)
                        return;

                    float distance = Player.Position.Distance(target.Position);

                    if (distance >= 500)
                        if (Menu.Item("HarassWList" + target.ChampionName).GetValue<bool>())
                            if (target.IsValidTarget(W.Range))
                                if (W.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
                                    W.Cast(target, true);
                }

                if (Menu.Item("AutoWKS").GetValue<bool>())
                {
                    var e = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

                    if (e.IsValidTarget() && e.Distance(Player.Position) > 500)
                        if (GetKillStealDamage(e, W) > e.Health)
                            if (CanKill(e))
                                if(Player.Position.Distance(e.Position) >= 600)
                                    SpellCast(W, e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Auto W Logic " + ex);
            }
        }

        private static void ELogic()
        {
            try
            {
                if (!E.IsReady())
                    return;

                if (Player.Mana < (E.ManaCost + R.ManaCost + W.ManaCost))
                    return;

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

                    if (t.IsValidTarget(E.Range) && E.GetPrediction(t).CastPosition.Distance(t.Position) > 200 && (int)E.GetPrediction(t).Hitchance == 5)
                    {
                        if (t.HasBuffOfType(BuffType.Slow) || CountEnemiesInRangeDeley(E.GetPrediction(t).CastPosition, 250, E.Delay) > 1)
                        {
                            SpellCast(E, t);
                        }
                        else
                        {
                            if (E.GetPrediction(t).CastPosition.Distance(t.Position) > 200)
                            {
                                if (Player.Position.Distance(t.ServerPosition) > Player.Position.Distance(t.Position))
                                {
                                    if (t.Position.Distance(Player.ServerPosition) < t.Position.Distance(Player.Position))
                                        SpellCast(E, t);
                                }
                                else
                                {
                                    if (t.Position.Distance(Player.ServerPosition) > t.Position.Distance(Player.Position))
                                        SpellCast(E, t);
                                }
                            }
                        }
                    }
                }

                List<Obj_AI_Hero> Enemies = ObjectManager.Get<Obj_AI_Hero>().Where(e => e.IsEnemy && e.IsValidTarget()).ToList();

                foreach (var e in Enemies)
                {
                    if (Menu.Item("StunE").GetValue<bool>())
                    {
                        if (e.HasBuffOfType(BuffType.Stun))
                        {
                            if (e.IsValidTarget(E.Range))
                            {
                                if (E.GetPrediction(e).Hitchance >= HitChance.VeryHigh)
                                {
                                    E.Cast(e, true);
                                }
                            }
                        }
                    }

                    if (Menu.Item("SlowE").GetValue<bool>())
                    {
                        if (e.HasBuffOfType(BuffType.Slow))
                        {
                            if (e.IsValidTarget(E.Range))
                            {
                                if (E.GetPrediction(e).Hitchance >= HitChance.VeryHigh)
                                {
                                    E.Cast(e, true);
                                }
                            }
                        }
                    }

                    if (Menu.Item("ImmE").GetValue<bool>())
                    {
                        if (!CanMove(e))
                        {
                            if (E.GetPrediction(e).Hitchance >= HitChance.VeryHigh)
                            {
                                E.Cast(e, true);
                            }
                        }
                        else
                        {
                            E.CastIfHitchanceEquals(e, HitChance.Immobile, true);
                        }
                    }
                }

                if (Menu.Item("TelE").GetValue<bool>())
                {
                    foreach (var Object in ObjectManager.Get<Obj_AI_Base>().Where(Obj => Obj.IsEnemy && Obj.Distance(Player.ServerPosition) < E.Range && (Obj.HasBuff("teleport_target") || Obj.HasBuff("Pantheon_GrandSkyfall_Jump"))))
                    {
                        E.Cast(Object.Position, true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Auto E Logic " + e);
            }
        }

        #endregion

        #region

        public static bool ShouldUseE(string SpellName)
        {
            switch (SpellName)
            {
                case "ThreshQ":
                    return true;
                case "KatarinaR":
                    return true;
                case "AlZaharNetherGrasp":
                    return true;
                case "GalioIdolOfDurand":
                    return true;
                case "LuxMaliceCannon":
                    return true;
                case "MissFortuneBulletTime":
                    return true;
                case "RocketGrabMissile":
                    return true;
                case "CaitlynPiltoverPeacemaker":
                    return true;
                case "EzrealTrueshotBarrage":
                    return true;
                case "InfiniteDuress":
                    return true;
                case "VelkozR":
                    return true;
            }
            return false;
        }

        public static void GameOnOnGameLoad(EventArgs args)
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

        private static float GetUltTravelTime(Obj_AI_Hero source, float speed, float delay, Vector3 targetpos)
        {
            float distance = Vector3.Distance(source.ServerPosition, targetpos);
            float missilespeed = speed;
            if (source.ChampionName == "Jinx" && distance > 1350)
            {
                const float accelerationrate = 0.3f;
                var acceldifference = distance - 1350f;
                if (acceldifference > 150f) 
                    acceldifference = 150f;
                var difference = distance - 1500f;
                missilespeed = (1350f * speed + acceldifference * (speed + accelerationrate * acceldifference) + difference * 2200f) / distance;
            }
            return (distance / missilespeed + delay);
        }

        public static void SpellCast(Spell spell , Obj_AI_Base e)
        {
            SebbyLib.Prediction.SkillshotType CoreType2 = SebbyLib.Prediction.SkillshotType.SkillshotLine;
            bool aoe2 = false;

            if (spell.Type == SkillshotType.SkillshotCircle)
            {
                CoreType2 = SebbyLib.Prediction.SkillshotType.SkillshotCircle;
                aoe2 = true;
            }

            if (spell.Width > 80 && !spell.Collision)
                aoe2 = true;

            var predInput2 = new SebbyLib.Prediction.PredictionInput
            {
                Aoe = aoe2,
                Collision = spell.Collision,
                Speed = spell.Speed,
                Delay = spell.Delay,
                Range = spell.Range,
                From = Player.ServerPosition,
                Radius = spell.Width,
                Unit = e,
                Type = CoreType2
            };
            var poutput2 = SebbyLib.Prediction.Prediction.GetPrediction(predInput2);

            if (spell.Speed != float.MaxValue && OktwCommon.CollisionYasuo(Player.ServerPosition, poutput2.CastPosition))
                return;

            if (poutput2.Hitchance >= SebbyLib.Prediction.HitChance.VeryHigh)
                spell.Cast(poutput2.CastPosition);
            else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= SebbyLib.Prediction.HitChance.High)
            {
                spell.Cast(poutput2.CastPosition);
            }
        }

        private static bool CanMove(Obj_AI_Hero e)
        {
            if (e.HasBuffOfType(BuffType.Stun) || e.HasBuffOfType(BuffType.Snare) || e.HasBuffOfType(BuffType.Knockup) ||
                e.HasBuffOfType(BuffType.Charm) || e.HasBuffOfType(BuffType.Fear) || e.HasBuffOfType(BuffType.Knockback) ||
                e.HasBuffOfType(BuffType.Taunt) || e.HasBuffOfType(BuffType.Suppression) ||
                e.IsStunned || e.IsChannelingImportantSpell() || e.MoveSpeed < 50f)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool CanKill(Obj_AI_Base e)
        {
            if (e.HasBuffOfType(BuffType.PhysicalImmunity) || e.HasBuffOfType(BuffType.SpellImmunity) || e.IsZombie
                || e.IsInvulnerable || e.HasBuffOfType(BuffType.Invulnerability) || e.HasBuffOfType(BuffType.SpellShield)
                || e.HasBuff("deathdefiedbuff") || e.HasBuff("Undying Rage") || e.HasBuff("Chrono Shift"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static float GetKillStealDamage(Obj_AI_Base e, Spell spell)
        {
            var dmg = spell.GetDamage(e);

            if (Player.HasBuff("summonerexhaust"))
                dmg = dmg * 0.6f;

            if (e.HasBuff("ferocioushowl"))
                dmg = dmg * 0.7f;

            if (e is Obj_AI_Hero)
            {
                var champion = (Obj_AI_Hero)e;

                if (champion.ChampionName == "Blitzcrank" && !champion.HasBuff("BlitzcrankManaBarrierCD") && !champion.HasBuff("ManaBarrier"))
                {
                    dmg -= champion.Mana / 2f;
                }
            }

            var extraHP = e.Health - LeagueSharp.Common.HealthPrediction.GetHealthPrediction(e, 500);

            dmg += extraHP;
            dmg -= e.HPRegenRate;
            dmg -= e.PercentLifeStealMod * 0.005f * e.FlatPhysicalDamageMod;

            return dmg;
        }

        public static int CountEnemiesInRangeDeley(Vector3 position, float range, float delay)
        {
            int count = 0;

            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget()))
            {
                Vector3 prepos = Prediction.GetPrediction(t, delay).CastPosition;

                if (position.Distance(prepos) < range)
                    count++;
            }

            return count;
        }

        #endregion

        private static void LoadMenu()
        {
            try
            {
                Menu = new Menu("VIP金克丝Q群438230879", "Yulejinx", true).SetFontStyle(FontStyle.Regular, FontColor.Orange);
                
                Menu.AddSubMenu(new Menu("目标选择", "TargetSelcet")).SetFontStyle(FontStyle.Regular, FontColor.Red);
                Menu.SubMenu("TargetSelcet").AddItem(new MenuItem("TsMode", "目标选择模式").SetValue(new StringList(new[] { "快速击杀", "自动设置", "正常模式" }, 0)));
                Menu.SubMenu("TargetSelcet").AddItem(new MenuItem("ExtraFocus", "自动锁定目标").SetValue(Player.IsMelee));
                Menu.SubMenu("TargetSelcet").AddItem(new MenuItem("ExtraPriority", "      优先设置"));
                int i = 5;
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy).OrderBy(enemy => enemy.MaxHealth / Player.GetAutoAttackDamage(enemy)))
                {
                    Menu.SubMenu("TargetSelcet").AddItem(new MenuItem("TsModePriority" + enemy.ChampionName, enemy.ChampionName).SetValue(new Slider(i, 0, 5))).DontSave();
                    i--;
                }

                var orbwalkerMenu = new Menu("走砍设置", "走砍设置").SetFontStyle(FontStyle.Regular, FontColor.Red);
                Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
                Menu.AddSubMenu(orbwalkerMenu);

                Menu.AddSubMenu(new Menu("Q技能设置", "QSetting")).SetFontStyle(FontStyle.Regular, FontColor.Red);
                Menu.SubMenu("QSetting").AddItem(new MenuItem("ComboQMenu", "   连招Q设置"));
                Menu.SubMenu("QSetting").AddItem(new MenuItem("ComboQ", "连招Q").SetValue(true));
                Menu.SubMenu("QSetting").AddItem(new MenuItem("HarassQMenu", "   骚扰Q设置"));
                Menu.SubMenu("QSetting").AddItem(new MenuItem("HarassQ", "骚扰Q").SetValue(true));
                Menu.SubMenu("QSetting").AddItem(new MenuItem("LaneClearQMenu", "   清线Q设置"));
                Menu.SubMenu("QSetting").AddItem(new MenuItem("LaneClearQ", "清线Q").SetValue(true));

                Menu.AddSubMenu(new Menu("W技能设置", "WSetting")).SetFontStyle(FontStyle.Regular, FontColor.Red);
                Menu.SubMenu("WSetting").AddItem(new MenuItem("ComboWMenu", "   连招W设置"));
                Menu.SubMenu("WSetting").AddItem(new MenuItem("ComboW", "连招W").SetValue(true));
                Menu.SubMenu("WSetting").AddItem(new MenuItem("HarassWMenu", "   骚扰W设置"));
                Menu.SubMenu("WSetting").AddItem(new MenuItem("HarassW", "骚扰W").SetValue(true));
                Menu.SubMenu("WSetting").AddItem(new MenuItem("HarassAutoW", "骚扰自动W开关").SetValue(new KeyBind('U', KeyBindType.Toggle))).Permashow(true, null, FontColor.Red);
                Menu.SubMenu("WSetting").AddItem(new MenuItem("HarassWList", "骚扰W英雄列表"));
                foreach (var e in HeroManager.Enemies)
                {
                    Menu.SubMenu("WSetting").AddItem(new MenuItem("HarassWList" + e.ChampionName, e.ChampionName).SetValue(true));
                }
                Menu.SubMenu("WSetting").AddItem(new MenuItem("HarassWManaPer", "骚扰W最低蓝量比").SetValue(new Slider(40)));
                Menu.SubMenu("WSetting").AddItem(new MenuItem("AutoWMenu", "   自动W设置"));
                Menu.SubMenu("WSetting").AddItem(new MenuItem("AutoWKS", "击杀W").SetValue(true)).Permashow(true, null, FontColor.YellowGreen);

                Menu.AddSubMenu(new Menu("E技能设置", "ESetting")).SetFontStyle(FontStyle.Regular, FontColor.Red);
                Menu.SubMenu("ESetting").AddItem(new MenuItem("ComboEMenu", "   连招E设置"));
                Menu.SubMenu("ESetting").AddItem(new MenuItem("ComboE", "连招E").SetValue(true));
                Menu.SubMenu("ESetting").AddItem(new MenuItem("AutoEMenu", "   自动E设置"));
                Menu.SubMenu("ESetting").AddItem(new MenuItem("SlowE", "减速").SetValue(true));
                Menu.SubMenu("ESetting").AddItem(new MenuItem("StunE", "眩晕").SetValue(true));
                Menu.SubMenu("ESetting").AddItem(new MenuItem("TelE", "传送").SetValue(true));
                Menu.SubMenu("ESetting").AddItem(new MenuItem("ImmE", "无法移动").SetValue(true));
                Menu.SubMenu("ESetting").AddItem(new MenuItem("AntiE", "反突进").SetValue(true));
                Menu.SubMenu("ESetting").AddItem(new MenuItem("ProtectE", "自保E").SetValue(true));

                Menu.AddSubMenu(new Menu("大招设置", "ult")).SetFontStyle(FontStyle.Regular, FontColor.Red);
                Menu.SubMenu("ult").AddItem(new MenuItem("ToggleRMenu", "   手动R设置"));
                Menu.SubMenu("ult").AddItem(new MenuItem("Togglerkey", "手动R按键").SetValue(new KeyBind('T', KeyBindType.Press)));
                Menu.SubMenu("ult").AddItem(new MenuItem("TogglerMode", "手动R目标").SetValue(new StringList(new[] { "低血量", "AOE" }, 0)));
                Menu.SubMenu("ult").AddItem(new MenuItem("JungleRMenu", "   偷野R设置"));
                Menu.SubMenu("ult").AddItem(new MenuItem("Rjungle", "大招偷野(仅在有视野)", true).SetValue(true)).Permashow(true, null, FontColor.Sienna);
                Menu.SubMenu("ult").AddItem(new MenuItem("Rdragon", "偷小龙", true).SetValue(true));
                Menu.SubMenu("ult").AddItem(new MenuItem("Rbaron", "偷大龙", true).SetValue(true));
                Menu.SubMenu("ult").AddItem(new MenuItem("AutoRMenu", "   自动R设置"));
                Menu.SubMenu("ult").AddItem(new MenuItem("AutoREnable", "自动大招").SetValue(true)).Permashow(true, null, FontColor.PowderBlue);
                Menu.SubMenu("ult").AddItem(new MenuItem("BaseUlt", "基地大招").SetValue(true)).Permashow(true, null, FontColor.RosyBrown);

                Menu.AddSubMenu(new Menu("显示设置", "Drawing")).SetFontStyle(FontStyle.Regular, FontColor.Red);
                Menu.SubMenu("Drawing").AddItem(new MenuItem("QDraw", "Q 范围").SetValue(new Circle(true, Color.WhiteSmoke)));
                Menu.SubMenu("Drawing").AddItem(new MenuItem("WDraw", "W 范围").SetValue(new Circle(true, Color.RosyBrown)));
                Menu.SubMenu("Drawing").AddItem(new MenuItem("EDraw", "E 范围").SetValue(new Circle(true, Color.Linen)));
                Menu.SubMenu("Drawing").AddItem(new MenuItem("BaseUltDraw", "显示回城通知").SetValue(true));

                Menu.AddToMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Menu Loading " + ex);
            }
        }

        private static void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1490f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 2500f);

            W.SetSkillshot(0.6f, 75f, 3300f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(1.2f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.7f, 140f, 1500f, false, SkillshotType.SkillshotLine);
        }
    }
}
