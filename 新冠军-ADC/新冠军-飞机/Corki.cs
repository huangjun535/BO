namespace YuLeCorki
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YuLeLibrary;

    class Corki
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 825);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 1230);
           
            Q.SetSkillshot(0.3f, 200f, 1000f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.2f, 40f, 2000f, true, SkillshotType.SkillshotLine);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "自动 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harassQ", "骚扰 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("Lane.UseQ", "清线Q最低命中数 >= ").SetValue(new StringList(new[] { "关闭", " >= 1", " >= 2", " >= 3" }, 2)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("drawKillableMinions", "显示 Q可击杀小兵", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("QMana", "清线最低蓝量", true).SetValue(new Slider(80, 100, 30)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("Jungle.UseQ", "清野Q模式").SetValue(new StringList(new[] { "关闭", "仅对大野怪使用", " >= 2", " >= 3" }, 1)));

            Config.SubMenu("W 设置").AddItem(new MenuItem("nktdE", "自动W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("fleew", "逃跑W", true).SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("harassE", "骚扰 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("Lane.UseE", "清线E最低命中数").SetValue(new StringList(new[] { "关闭", " >= 1", " >= 2", " >= 3", " >= 4", " >= 5", " >= 6" }, 2)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("EMana", "清线最低蓝量", true).SetValue(new Slider(80, 100, 30)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("Jungle.UseE", "清野E模式").SetValue(new StringList(new[] { "关闭", "仅对大野怪使用", " >= 2", " >= 3" }, 1)));

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rammo", "清线骚扰最低R层数", true).SetValue(new Slider(3, 6, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("minionR", "尝试R小兵溅射伤害", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Lane.UseR", "清线R最低命中数 >= ").SetValue(new StringList(new[] { "关闭", " >= 1", " >= 2", " >= 3" }, 3)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("RMana", "清线最低蓝量", true).SetValue(new Slider(80, 100, 30)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Jungle.UseR", "清野R模式").SetValue(new StringList(new[] { "关闭", "仅对大野怪使用", " >= 2", " >= 3" }, 1)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("ksR", "击杀 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("stealR", "偷野 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("useR", "手动R按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            Config.SubMenu("显示 设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示 设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示 设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示 设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Config.AddItem(new MenuItem("fleewcur", "逃跑时自动跟随鼠标移动", true).SetValue(true));
            Config.AddItem(new MenuItem("flee", "逃跑按键", true).SetValue(new KeyBind('Z', KeyBindType.Press)));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;

        }

        private void fleelogic()
        {
            if (Config.Item("flee", true).GetValue<KeyBind>().Active)
            {
                if (Config.Item("fleewcur", true).GetValue<bool>())
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }

                if (Config.Item("fleew", true).GetValue<bool>() && W.IsReady())
                {
                    W.Cast(Game.CursorPos);
                }
            }
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (E.IsReady() && Sheen() && args.Target.IsValid<Obj_AI_Hero>())
            {
                if(Program.Combo && Config.Item("autoE", true).GetValue<bool>() && Player.Mana > EMANA + RMANA)
                    E.Cast(args.Target.Position);
                if (Program.Farm && Config.Item("harassE", true).GetValue<bool>() && Player.Mana > EMANA + RMANA + QMANA)
                    E.Cast(args.Target.Position);
                if (!Q.IsReady() && !R.IsReady() && args.Target.Health < Player.FlatPhysicalDamageMod * 2)
                    E.Cast();
            }
        }

        private void setbool()
        {
            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Program.Combo;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            fleelogic();

            if (Program.LagFree(0))
            {
                SetMana();
                setbool();
                farm();
                KSR();
                StealR();
            }
            if (Program.LagFree(1) && Q.IsReady() && !Player.IsWindingUp && Sheen())
                LogicQ();
            if (Program.LagFree(2) && Program.Combo && W.IsReady())
                LogicW();
            if (Program.LagFree(4) && R.IsReady() && !Player.IsWindingUp && Sheen() && !Player.IsWindingUp)
                LogicR();
        }

        private void StealR()
        {
            if (Config.Item("stealR", true).GetValue<bool>() && R.IsReady())
            {
                var jMob = MinionManager.GetMinions(Player.ServerPosition, R.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.Health + (x.HPRegenRate / 2) <= R.GetDamage(x));

                if (R.CanCast(jMob))
                {
                    R.Cast(jMob);
                }

                var minion = MinionManager.GetMinions(Player.ServerPosition, R.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth) .FirstOrDefault( x => x.Health <= E.GetDamage(x) && (x.SkinName.ToLower().Contains("siege") || x.SkinName.ToLower().Contains("super")));

                if (R.IsReady() && R.CanCast(minion))
                {
                    R.Cast(minion);
                }
            }

        }

        private void KSR()
        {
            if (Config.Item("ksR", true).GetValue<bool>() && R.IsReady())
            {
                var target = HeroManager.Enemies.FirstOrDefault(x => !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield) && R.CanCast(x) && (x.Health + (x.HPRegenRate / 2)) <= R.GetDamage(x));

                if (R.IsReady() && R.CanCast(target))
                {
                    R.Cast(target);
                }
            }
        }

        private void LogicR()
        {
            float rSplash = 150;
            if (bonusR)
            {
                rSplash = 300;
            }
            
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget())
            {
                var rDmg = Common.GetKsDamage(t,R);
                var qDmg = Q.GetDamage(t);
                if (rDmg * 2> t.Health)
                    CastR(R, t);
                else if (t.IsValidTarget(Q.Range) && qDmg + rDmg > t.Health)
                    CastR(R, t);
                if (Player.Spellbook.GetSpell(SpellSlot.R).Ammo > 1)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(R.Range) && enemy.CountEnemiesInRange(rSplash) > 1))
                        t = enemy;

                    if (Program.Combo && Player.Mana > RMANA * 3 )
                    {
                        CastR(R, t);
                    }
                    else if (Program.Farm && Player.Mana > RMANA + EMANA + QMANA + WMANA && Player.Spellbook.GetSpell(SpellSlot.R).Ammo >= Config.Item("Rammo", true).GetValue<Slider>().Value)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(R.Range)))
                            CastR(R, enemy);
                    }

                    if (!Program.None && Player.Mana > RMANA + QMANA + EMANA)
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(R.Range) && !Common.CanMove(enemy)))
                            CastR(R, t);
                    }
                }
            }
        }

        private void CastR(Spell R , Obj_AI_Hero t)
        {
            Program.CastSpell(R, t);
            if (Config.Item("minionR", true).GetValue<bool>())
            {
                // collision + predictio R
                var poutput = R.GetPrediction(t);
                var col = poutput.CollisionObjects.Count(ColObj => ColObj.IsEnemy && ColObj.IsMinion && !ColObj.IsDead);

                //hitchance
                var prepos = Prediction.GetPrediction(t, 0.4f);

                if (col == 0 && (int)prepos.Hitchance < 5)
                    return;

                float rSplash = 140;
                if (bonusR)
                    rSplash = 290f;
                
                var minions = Cache.GetMinions(Player.ServerPosition, R.Range - rSplash);
                foreach (var minion in minions.Where(minion => minion.Distance(poutput.CastPosition) < rSplash))
                {
                    R.Cast(minion);
                    return;
                }
            }
        }

        private void LogicW()
        {
            var dashPosition = Player.Position.Extend(Game.CursorPos, W.Range);

            var t = TargetSelector.GetTarget(Q.Range * 2, TargetSelector.DamageType.Physical);

            if(t.IsHPBarRendered)
            {
                if (Game.CursorPos.Distance(Player.Position) > Player.AttackRange + Player.BoundingRadius * 2 && Program.Combo && Config.Item("nktdE", true).GetValue<bool>() && Player.Mana > RMANA + WMANA - 10)
                {
                    W.Cast(dashPosition);
                }
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (Program.Combo && Config.Item("autoQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && Config.Item("harassQ", true).GetValue<bool>()&& Player.Mana > RMANA + EMANA + WMANA + RMANA)
                    Program.CastSpell(Q, t);
                else
                {
                    var qDmg = Common.GetKsDamage(t, Q);
                    var rDmg = R.GetDamage(t);
                    if (qDmg > t.Health)
                        Q.Cast(t);
                    else if (rDmg + qDmg > t.Health && Player.Mana > RMANA + QMANA)
                        Program.CastSpell(Q, t);
                    else if (rDmg + 2 * qDmg > t.Health && Player.Mana > QMANA + RMANA * 2)
                        Program.CastSpell(Q, t);
                }

                if (!Program.None && Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !Common.CanMove(enemy)))
                        Q.Cast(enemy, true, true);
                }
            }
        }

        public void farm()
        {
            if (Program.LaneClear && !Player.IsWindingUp && Sheen())
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionTeam.Neutral);
                if (mobs.Count > 0 && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                {
                    int jungleQValue = Config.Item("Jungle.UseQ").GetValue<StringList>().SelectedIndex;

                    if (jungleQValue != 0 && W.IsReady())
                    {
                        Obj_AI_Base jungleMobs = GetMobs(Q.Range, jungleQValue != 1 ? MobTypes.All : MobTypes.BigBoys, jungleQValue != 1 ? jungleQValue : 1);

                        if (jungleMobs != null)
                        {
                            Q.Cast(jungleMobs);
                        }
                    }

                    int jungleEValue = Config.Item("Jungle.UseE").GetValue<StringList>().SelectedIndex;

                    if (W.IsReady() && jungleEValue != 0)
                    {
                        Obj_AI_Base jungleMobs = GetMobs(E.Range, jungleEValue != 1 ? MobTypes.All : MobTypes.BigBoys, jungleEValue != 1 ? jungleEValue : 1);

                        if (jungleMobs != null)
                        {
                            E.Cast();
                        }
                    }

                    int jungleRValue = Config.Item("Jungle.UseR").GetValue<StringList>().SelectedIndex;

                    if (jungleRValue != 0 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo > Config.Item("Rammo", true).GetValue<Slider>().Value)
                    {
                        Obj_AI_Base jungleMobs = GetMobs(R.Range, jungleRValue != 1 ? MobTypes.All : MobTypes.BigBoys, jungleRValue != 1 ? jungleRValue : 1);

                        if (jungleMobs != null)
                        {
                            R.Cast(jungleMobs);
                        }
                    }
                }

                {
                    var minions = Cache.GetMinions(Player.ServerPosition, Q.Range);

                    int laneQValue = Config.Item("Lane.UseQ").GetValue<StringList>().SelectedIndex;
                    if (laneQValue != 0 && Q.IsReady() && Player.ManaPercent > Config.Item("QMana", true).GetValue<Slider>().Value)
                    {
                        var qfarm = Q.GetCircularFarmLocation(minions, Q.Width);
                        if (qfarm.MinionsHit >= laneQValue)
                        {
                            Q.Cast(qfarm.Position);
                            return;
                        }
                    }

                    int laneEValue = Config.Item("Lane.UseE").GetValue<StringList>().SelectedIndex;
                    if (laneEValue != 0 && E.IsReady() && Player.ManaPercent > Config.Item("EMana", true).GetValue<Slider>().Value)
                    {
                        int minCount = minions.Count();
                        if (minCount >= laneEValue)
                        {
                            E.Cast();
                            return;
                        }
                    }

                    int laneRValue = Config.Item("Lane.UseR").GetValue<StringList>().SelectedIndex;

                    if (R.IsReady() && laneRValue != 0)
                    {
                        if (Player.Spellbook.GetSpell(SpellSlot.R).Ammo >= Config.Item("Rammo", true).GetValue<Slider>().Value && Player.ManaPercent > Config.Item("RMana", true).GetValue<Slider>().Value)
                        {
                            var rfarm = R.GetCircularFarmLocation(minions, 100);

                            if (rfarm.MinionsHit >= laneRValue)
                            {
                                R.Cast(rfarm.Position);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private bool Sheen()
        {
            var target = Orbwalker.GetTarget();

            if (target.IsValidTarget() &&  Player.HasBuff("sheen") )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool bonusR { get { return Player.HasBuff("corkimissilebarragecounterbig"); } }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        public static void drawText(string msg, Vector3 Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - 200, color, msg);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if(Config.Item("drawKillableMinions", true).GetValue<bool>() && Q.IsReady() && !Program.Combo)
            {
                foreach (Obj_AI_Base m in MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range).Where(x => E.CanCast(x) && x.Health < Q.GetDamage(x)))
                {
                    Render.Circle.DrawCircle(m.Position, (float)(m.BoundingRadius * 2), System.Drawing.Color.DarkBlue, 5);
                }
            }

            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
            }
            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }
            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }

        public enum MobTypes
        {
            All,
            BigBoys
        }

        public static Obj_AI_Base GetMobs(float spellRange, MobTypes mobTypes = MobTypes.All, int minMobCount = 1)
        {
            List<Obj_AI_Base> mobs = MinionManager.GetMinions(
                spellRange + 200,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (mobs == null) return null;

            if (mobTypes == MobTypes.BigBoys)
            {
                Obj_AI_Base oMob = (from fMobs in mobs
                                    from fBigBoys in
                                        new[]
                                        {
                            "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red",
                            "SRU_Krug", "SRU_Dragon", "SRU_Baron", "Sru_Crab"
                                        }
                                    where fBigBoys == fMobs.SkinName
                                    select fMobs).FirstOrDefault();

                if (oMob != null)
                {
                    if (oMob.IsValidTarget(spellRange))
                    {
                        return oMob;
                    }
                }
            }
            else if (mobs.Count >= minMobCount)
            {
                return mobs[0];
            }

            return null;
        }
    }
}
