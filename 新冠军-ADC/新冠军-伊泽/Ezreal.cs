using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace YuLeEzreal
{
    class Ezreal
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        public float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        Vector3 CursorPosition = Vector3.Zero;
        public double lag = 0;
        public double WCastTime = 0;
        public double QCastTime = 0;
        public float DragonDmg = 0;
        public double DragonTime = 0;
        public bool Esmart = false;
        public double OverKill = 0;
        public double OverFarm = 0;
        public double diag = 0;
        public double diagF = 0;
        public int Muramana = 3042;
        public int Tear = 3070;
        public int Manamune = 3004;
        public double NotTime = 0;

        public static Core.DashLogic Dash;

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 1170);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 3000f);
            
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("AutoQ", "自动 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("HarassQ", "骚扰 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("HarassMana", "骚扰最低蓝量", true).SetValue(new Slider(30, 100, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("farmQ", "清线 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("FQ", "仅Q攻击范围外的小兵", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("Mana", "清线最低蓝量", true).SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("LCP", "快速清线模式", true).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("wPush", "W ally (push tower)", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("harrasW", "Harass W", true).SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("smartE", "手动E按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space
            Config.SubMenu("E 设置").AddItem(new MenuItem("smartEW", "手动EW按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space
            Config.SubMenu("E 设置").AddItem(new MenuItem("EKsCombo", "连招E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("EAntiMelee", "反突进E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("autoEgrab", "高级反突进", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("GapcloserMode", "反突进模式", true).SetValue(new StringList(new[] { "Game Cursor", "安全位置", "Disable" }, 1)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("DashMode", "突进模式", true).SetValue(new StringList(new[] { "Game Cursor", "Side", "Safe position" }, 2)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("EnemyCheck", "敌人数量检测", true).SetValue(new Slider(3, 5, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("WallCheck", "墙检测", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("TurretCheck", "防御塔检测", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("AAcheck", "只在普攻范围内", true).SetValue(true));
            Dash = new Core.DashLogic(E);

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rcc", "R cc", true).SetValue(false));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Raoe", "R AOE", true).SetValue(new Slider(3, 5, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rjungle", "R Jungle stealer", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rally", "Ally stealer", true).SetValue(false));
            Config.SubMenu("R 设置").AddItem(new MenuItem("useR", "Semi-manual cast R key", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rturrent", "塔下不R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("MaxRangeR", "Max R range", true).SetValue(new Slider(3000, 5000, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("MinRangeR", "Min R range", true).SetValue(new Slider(900, 5000, 0)));

            new BaseUlt().Init();

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" })));
            }

            Config.SubMenu("杂项设置").AddItem(new MenuItem("apEz", "AP模式", true).SetValue(false));
            Config.SubMenu("杂项设置").AddItem(new MenuItem("stack", "满篮自动叠女神", true).SetValue(true));

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q range", true).SetValue(true));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));


            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += afterAttack;
            Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
        }

        private void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if(sender.IsMe && Config.Item("autoEgrab", true).GetValue<bool>() && E.IsReady())
            {
                if(args.Buff.Name == "ThreshQ" || args.Buff.Name == "rocketgrab2")
                {
                    var dashPos = Dash.CastDash(true);
                    if (!dashPos.IsZero)
                    {
                        E.Cast(dashPos);
                    }
                    else
                    {
                        E.Cast(Game.CursorPos);
                    }
                }
            }
        }

        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (W.IsReady() && Config.Item("wPush", true).GetValue<bool>() && target.IsValid<Obj_AI_Turret>() && Player.Mana > RMANA + EMANA + QMANA + WMANA + WMANA + RMANA)
            {
                foreach (var ally in HeroManager.Allies)
                {
                    if (!ally.IsMe && ally.IsAlly && ally.Distance(Player.Position) < 600)
                        W.Cast(ally);
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
            }
            if (R.IsReady() && Config.Item("Rjungle", true).GetValue<bool>())
            {
                KsJungle();
            }
            else
                DragonTime = 0;

            if (E.IsReady())
            {
                if (Program.LagFree(0))
                    LogicE();

                if (Config.Item("smartE", true).GetValue<KeyBind>().Active)
                    Esmart = true;
                if (Config.Item("smartEW", true).GetValue<KeyBind>().Active && W.IsReady())
                {
                    CursorPosition = Game.CursorPos;
                    W.Cast(CursorPosition);
                }
                if (Esmart && Player.Position.Extend(Game.CursorPos, E.Range).CountEnemiesInRange(500) < 4)
                    E.Cast(Player.Position.Extend(Game.CursorPos, E.Range), true);
                
                if (!CursorPosition.IsZero)
                    E.Cast(Player.Position.Extend(CursorPosition, E.Range), true);
            }
            else
            {
                CursorPosition = Vector3.Zero;
                Esmart = false;
            }

            if (Q.IsReady())
                LogicQ();

            if (Program.LagFree(3) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();

            if ( R.IsReady())
            {
                if (Config.Item("useR", true).GetValue<KeyBind>().Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget())
                        R.Cast(t, true, true);
                }

                if (Program.LagFree(4))
                    LogicR();
            }
        }

        private void LogicQ()
        {
            if (Program.LagFree(1))
            {
                if (!Orbwalking.CanMove(50))
                    return;
                bool cc = !Program.None && Player.Mana > RMANA + QMANA + EMANA;
                bool harass = Program.Farm && Config.Item("HarassQ", true).GetValue<bool>() && Player.ManaPercent > Config.Item("HarassMana", true).GetValue<Slider>().Value;

                if (Program.Combo && Player.Mana > RMANA + QMANA)
                {
                    var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                    if (t.IsValidTarget())
                        Program.CastSpell(Q, t);
                }

                foreach (var t in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range)).OrderBy(t => t.Health))
                {
                    var qDmg = SebbyLib.OktwCommon.GetKsDamage(t, Q);
                    var wDmg = W.GetDamage(t);
                    if (qDmg + wDmg > t.Health)
                    {
                        Program.CastSpell(Q, t);
                        OverKill = Game.Time;
                        return;
                    }

                    if (cc && !CanMove(t))
                        Q.Cast(t);

                    if (harass)
                        Program.CastSpell(Q, t);
                }
            }
            else if (Program.LagFree(2))
            {
                if (Farm && Player.Mana > QMANA)
                {
                    farmQ();
                    lag = Game.Time;
                }
                else if (Config.Item("stack", true).GetValue<bool>() && Utils.TickCount - Q.LastCastAttemptT > 4000 && !Player.HasBuff("Recall") && Player.Mana > Player.MaxMana * 0.95 && Program.None && (Items.HasItem(Tear) || Items.HasItem(Manamune)))
                {
                    Q.Cast(Player.Position.Extend(Game.CursorPos, 500));
                }
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Program.Combo && Player.Mana > RMANA + WMANA + EMANA)
                    Program.CastSpell(W, t);
                else if (Program.Farm && Config.Item("harrasW", true).GetValue<bool>() && (Player.Mana > Player.MaxMana * 0.8 || Config.Item("apEz", true).GetValue<bool>()) && Player.ManaPercent > Config.Item("HarassMana", true).GetValue<Slider>().Value)
                    Program.CastSpell(W, t);
                else
                {
                    var qDmg = Q.GetDamage(t);
                    var wDmg = SebbyLib.OktwCommon.GetKsDamage(t, W);
                    if (wDmg > t.Health)
                    {
                        Program.CastSpell(W, t);
                        OverKill = Game.Time;
                    }
                    else if (wDmg + qDmg > t.Health && Q.IsReady())
                    {
                        Program.CastSpell(W, t);
                    }
                }

                if (!Program.None && Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !CanMove(enemy)))
                        W.Cast(enemy, true);
                }
            }
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);
            
            if (Config.Item("EAntiMelee", true).GetValue<bool>())
            { 
                if (HeroManager.Enemies.Any(target => target.IsValidTarget(1000) && target.IsMelee && Player.Distance(Prediction.GetPrediction(target, 0.2f).CastPosition) < 250))
                {
                    var dashPos = Dash.CastDash(true);
                    if (!dashPos.IsZero)
                    {
                        E.Cast(dashPos);
                    }
                }
            }

            if (t.IsValidTarget() && Program.Combo && Config.Item("EKsCombo", true).GetValue<bool>() && Player.HealthPercent > 40 && t.Distance(Game.CursorPos) + 300 < t.Position.Distance(Player.Position) && !Orbwalking.InAutoAttackRange(t) && !Player.UnderTurret(true) && (Game.Time - OverKill > 0.3) )
            {
                var dashPosition = Player.Position.Extend(Game.CursorPos, E.Range);

                if (dashPosition.CountEnemiesInRange(900) < 3)
                {
                    var dmgCombo = 0f;
                    
                    if (t.IsValidTarget(950))
                    {
                        dmgCombo = (float)Player.GetAutoAttackDamage(t) + E.GetDamage(t);
                    }

                    if (Q.IsReady() && Player.Mana > QMANA + EMANA && Q.WillHit(dashPosition, Q.GetPrediction(t).UnitPosition))
                        dmgCombo = Q.GetDamage(t);

                    if (W.IsReady() && Player.Mana > QMANA + EMANA + WMANA )
                    {
                        dmgCombo += W.GetDamage(t);
                    }

                    if (dmgCombo > t.Health && SebbyLib.OktwCommon.ValidUlt(t))
                    {
                        E.Cast(dashPosition);
                        OverKill = Game.Time;
                    }
                }
            }
        }

        public bool CanMove(Obj_AI_Hero target)
        {
            if (target.MoveSpeed < 50 || target.IsStunned ||
                target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Fear) || 
                target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) || 
                target.HasBuff("Recall") ||
                target.HasBuffOfType(BuffType.Knockback) || target.HasBuffOfType(BuffType.Charm) || 
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) || 
                (target.IsChannelingImportantSpell() && !target.IsMoving))
            {
                return false;
            }
            else
                return true;
        }

        private void LogicR()
        {
            if (Player.UnderTurret(true) && Config.Item("Rturrent", true).GetValue<bool>())
                return;

            if (Config.Item("autoR", true).GetValue<bool>() && Player.CountEnemiesInRange(800) == 0 && Game.Time - OverKill > 0.6)
            {
                R.Range = Config.Item("MaxRangeR", true).GetValue<Slider>().Value;
                foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) && SebbyLib.OktwCommon.ValidUlt(target)))
                {
                    double predictedHealth = target.Health - SebbyLib.OktwCommon.GetIncomingDamage(target);

                    if (Config.Item("Rcc", true).GetValue<bool>() && target.IsValidTarget(Q.Range + E.Range) && target.Health < Player.MaxHealth && !CanMove(target))
                    {
                        R.Cast(target, true, true);
                    }

                    double Rdmg = R.GetDamage(target);

                    if (Rdmg > predictedHealth)
                        Rdmg = getRdmg(target);

                    if (Rdmg > predictedHealth && target.CountAlliesInRange(500) == 0 && Player.Distance(target) > Config.Item("MinRangeR", true).GetValue<Slider>().Value)
                    {
                        Program.CastSpell(R,target);
                    }
                    if (Program.Combo && Player.CountEnemiesInRange(1200) == 0)
                    {
                        R.CastIfWillHit(target, Config.Item("Raoe", true).GetValue<Slider>().Value, true);
                    }
                }
            }
        }

        private bool DashCheck(Vector3 dash)
        {
            if (!dash.UnderTurret(true) || Program.Combo)
                return true;
            else
                return false;
        }

        private double getRdmg(Obj_AI_Base target)
        {
            var rDmg = R.GetDamage(target);
            var dmg = 0;
            PredictionOutput output = R.GetPrediction(target);
            Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();
            direction.Normalize();
            List<Obj_AI_Hero> enemies = HeroManager.Enemies.Where(x =>x.IsValidTarget()).ToList();
            foreach (var enemy in enemies)
            {
                PredictionOutput prediction = R.GetPrediction(enemy);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - Player.ServerPosition;
                Vector3 w = predictedPosition - Player.ServerPosition;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = Player.ServerPosition + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (R.Width + 100 + enemy.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                    dmg++;
            }
            var allMinionsR = SebbyLib.Cache.GetMinions(ObjectManager.Player.ServerPosition, R.Range);
            foreach (var minion in allMinionsR)
            {
                PredictionOutput prediction = R.GetPrediction(minion);
                Vector3 predictedPosition = prediction.CastPosition;
                Vector3 v = output.CastPosition - Player.ServerPosition;
                Vector3 w = predictedPosition - Player.ServerPosition;
                double c1 = Vector3.Dot(w, v);
                double c2 = Vector3.Dot(v, v);
                double b = c1 / c2;
                Vector3 pb = Player.ServerPosition + ((float)b * v);
                float length = Vector3.Distance(predictedPosition, pb);
                if (length < (R.Width + 100 + minion.BoundingRadius / 2) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                    dmg++;
            }
            //if (Config.Item("debug", true).GetValue<bool>())
            //    Game.PrintChat("R collision" + dmg);
            if (dmg == 0)
                return rDmg;
            else if (dmg > 7)
                return rDmg * 0.7;
            else
                return rDmg - (rDmg * 0.1 * dmg);

        }

        private float GetPassiveTime()
        {
            return
                ObjectManager.Player.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name == "ezrealrisingspellforce")
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        private bool Farm
        {
            get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit); }
        }

        public void farmQ()
        {
            if (Program.LaneClear)
            {
                var mobs = SebbyLib.Cache.GetMinions(Player.ServerPosition, 800, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    Q.Cast(mob.Position);
                }
            }

            if (!Orbwalking.CanMove(50) || (Orbwalker.ShouldWait() && Orbwalking.CanAttack()))
            {
                return;
            }

            var minions = SebbyLib.Cache.GetMinions(Player.ServerPosition, Q.Range);
            int orbTarget = 0;

            if (Orbwalker.GetTarget() != null)
                orbTarget = Orbwalker.GetTarget().NetworkId;

            if (Config.Item("FQ", true).GetValue<bool>())
            {
                foreach (var minion in minions.Where(minion => minion.IsValidTarget() && orbTarget != minion.NetworkId && minion.HealthPercent < 70 && !Orbwalker.InAutoAttackRange(minion) && minion.Health < Q.GetDamage(minion)))
                {
                    if (Q.Cast(minion) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
            }

            if (Config.Item("farmQ", true).GetValue<bool>() && Program.LaneClear && !Orbwalking.CanAttack() && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value)
            {
                var LCP = Config.Item("LCP", true).GetValue<bool>();
                var PT = Game.Time - GetPassiveTime() > -1.5 || !E.IsReady();

                foreach (var minion in minions.Where(minion => Orbwalker.InAutoAttackRange(minion)))
                {
                    
                    var hpPred = SebbyLib.HealthPrediction.GetHealthPrediction(minion, 300);
                    if (hpPred < 20)
                        continue;
                    
                    var qDmg = Q.GetDamage(minion);
                    if (hpPred < qDmg && orbTarget != minion.NetworkId)
                    {
                        if (Q.Cast(minion) == Spell.CastStates.SuccessfullyCasted)
                            return; 
                    }
                    else if (PT || LCP)
                    {
                        if (minion.HealthPercent > 80)
                        {
                            if (Q.Cast(minion) == Spell.CastStates.SuccessfullyCasted)
                                return;
                        }
                    }
                }
            }
        }

        private void KsJungle()
        {
            var mobs = SebbyLib.Cache.GetMinions(Player.ServerPosition, float.MaxValue, MinionTeam.Neutral);
            foreach (var mob in mobs)
            {
                if (mob.Health == mob.MaxHealth)
                    continue;
                if (((mob.SkinName.ToLower().Contains("dragon"))
                    || (mob.SkinName == "SRU_Baron")
                    || (mob.SkinName == "SRU_Red")
                    || (mob.SkinName == "SRU_Blue"))
                    && (mob.CountAlliesInRange(1000) == 0 || Config.Item("Rally", true).GetValue<bool>())
                    && mob.Distance(Player.Position) > 1000
                    )
                {
                    if (DragonDmg == 0)
                        DragonDmg = mob.Health;

                    if (Game.Time - DragonTime > 3)
                    {
                        if (DragonDmg - mob.Health > 0)
                        {
                            DragonDmg = mob.Health;
                        }
                        DragonTime = Game.Time;
                    }
                    else
                    {
                        var DmgSec = (DragonDmg - mob.Health) * (Math.Abs(DragonTime - Game.Time) / 3);
                        //Program.debug("DS  " + DmgSec);
                        if (DragonDmg - mob.Health > 0)
                        {
                            
                            var timeTravel = GetUltTravelTime(Player, R.Speed, R.Delay, mob.Position);
                            var timeR = (mob.Health - R.GetDamage(mob)) / (DmgSec / 3);
                            //Program.debug("timeTravel " + timeTravel + "timeR " + timeR + "d " + R.GetDamage(mob));
                            if (timeTravel > timeR)
                                R.Cast(mob.Position);
                        }
                        else
                            DragonDmg = mob.Health;
                        //Program.debug("" + GetUltTravelTime(ObjectManager.Player, R.Speed, R.Delay, mob.Position));
                    }
                }
            }
        }

        private float GetUltTravelTime(Obj_AI_Hero source, float speed, float delay, Vector3 targetpos)
        {
            float distance = Vector3.Distance(source.ServerPosition, targetpos);
            float missilespeed = speed;

            return (distance / missilespeed + delay);
        }

        private void SetMana()
        {
            if (Player.HealthPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
            }
            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }
            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }
    }
}
