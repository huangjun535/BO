using LeagueSharp;
using LeagueSharp.Common;
using SebbyLib;
using System;
using System.Linq;

namespace SSSSSSSSivir
{
    class Sivir
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell E, Q, Qc, W, R;
        public float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadSivir()
        {
            Q = new Spell(SpellSlot.Q, 1200f);
            Qc = new Spell(SpellSlot.Q, 1200f);
            W = new Spell(SpellSlot.W, float.MaxValue);
            E = new Spell(SpellSlot.E, float.MaxValue);
            R = new Spell(SpellSlot.R, 25000f);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);
            Qc.SetSkillshot(0.25f, 90f, 1350f, true, SkillshotType.SkillshotLine);

            Config.SubMenu("Q技能").AddItem(new MenuItem("ComboQ", "连招总是使用Q", true));
            Config.SubMenu("Q技能").AddItem(new MenuItem("farmQ", "Lane clear Q", true).SetValue(true));
            Config.SubMenu("Q技能").AddItem(new MenuItem("jungleQ", "Jungle clear Q", true).SetValue(true));

            Config.SubMenu("W技能").AddItem(new MenuItem("ComboW", "连招 总是使用", true));
            Config.SubMenu("W技能").AddItem(new MenuItem("harasW", "Harras W", true).SetValue(true));
            Config.SubMenu("W技能").AddItem(new MenuItem("farmW", "Lane clear W", true).SetValue(true));
            Config.SubMenu("W技能").AddItem(new MenuItem("jungleW", "Jungle clear W", true).SetValue(true));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
            {
                for (int i = 0; i < 4; i++)
                {
                    var spell = enemy.Spellbook.Spells[i];
                    if (spell.SData.TargettingType != SpellDataTargetType.Self && spell.SData.TargettingType != SpellDataTargetType.SelfAndUnit)
                    {
                        if (spell.SData.TargettingType == SpellDataTargetType.Unit)
                            Config.SubMenu("E技能").SubMenu("自动格挡的技能").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell.SData.Name, spell.Name).SetValue(true));
                        else
                            Config.SubMenu("E技能").SubMenu("自动格挡的技能").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell.SData.Name, spell.Name).SetValue(false));
                    }
                }
            }
            Config.SubMenu("E技能").AddItem(new MenuItem("autoE", "Auto E", true).SetValue(true));
            Config.SubMenu("E技能").AddItem(new MenuItem("AGC", "AntiGapcloserE", true).SetValue(true));
            Config.SubMenu("E技能").AddItem(new MenuItem("Edmg", "血量低于%才用E", true).SetValue(new Slider(90, 100, 0)));

            Config.SubMenu("R技能").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu("杂项").SubMenu("自动骚扰英雄名单").AddItem(new MenuItem("haras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu("杂项").AddItem(new MenuItem("Mana", "清线最低蓝量", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("杂项").AddItem(new MenuItem("LCminions", "清线最小兵数", true).SetValue(new Slider(5, 10, 0)));

            Config.SubMenu("显示").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            SebbyLib.Orbwalking.AfterAttack += Orbwalker_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        public void Orbwalker_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (W.IsReady() && !Program.None && target.IsValidTarget())
            {
                var t = target as Obj_AI_Hero;
				if (t != null)
                {
                    if(Player.GetAutoAttackDamage(t) * 3 > t.Health - OktwCommon.GetIncomingDamage(t))
						W.Cast();
                    if (Program.Combo && Player.Mana > RMANA + WMANA)
                        W.Cast();
                    else if (Config.Item("harasW", true).GetValue<bool>() && !Player.UnderTurret(true) && Player.Mana > RMANA + WMANA + QMANA && Config.Item("haras" + t.ChampionName).GetValue<bool>())
                    {
                        W.Cast();
                    }
                }
                else
                {
                    var t2 = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
                    if (t2.IsValidTarget() && Config.Item("harasW", true).GetValue<bool>() && Config.Item("haras" + t2.ChampionName).GetValue<bool>() && !Player.UnderTurret(true) && Player.Mana > RMANA + WMANA + QMANA && t2.Distance(target.Position) < 500)
                    {
                        W.Cast();
                    }

                    if (target is Obj_AI_Minion && Program.LaneClear && Config.Item("farmW", true).GetValue<bool>() && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && !Player.UnderTurret(true))
                    {
                        var minions = Cache.GetMinions(target.Position, 500);
                        if (minions.Count >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!E.IsReady() || args.SData.IsAutoAttack() || Player.HealthPercent > Config.Item("Edmg", true).GetValue<Slider>().Value || !Config.Item("autoE", true).GetValue<bool>()
                || !sender.IsEnemy || sender.IsMinion || !sender.IsValid<Obj_AI_Hero>() || args.SData.Name.ToLower() == "tormentedsoil")
                return;

            if (Config.Item("spell" + args.SData.Name) != null && !Config.Item("spell" + args.SData.Name).GetValue<bool>())
                return;

            if (args.Target != null)
            {
                if (args.Target.IsMe)
                    E.Cast();
            }
            else if (OktwCommon.CanHitSkillShot(Player, args))
            {
                E.Cast();
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var Target = gapcloser.Sender;
            if (Config.Item("AGC", true).GetValue<bool>() && E.IsReady() && Target.IsValidTarget(5000))
                E.Cast();
            return;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
            }
           
            if (Program.LagFree(1) && Q.IsReady() && !Player.IsWindingUp)
            {
                LogicQ();
            }

            if (Program.LagFree(2) && R.IsReady() && Program.Combo && Config.Item("autoR", true).GetValue<bool>())
            {
                LogicR();
            }

            if (Program.LagFree(3) && Program.LaneClear)
            {
                Jungle();
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                var qDmg = OktwCommon.GetKsDamage(t,Q) * 1.9;
                if (SebbyLib.Orbwalking.InAutoAttackRange(t))
                    qDmg = qDmg + Player.GetAutoAttackDamage(t) * 3;
                if (qDmg > t.Health)
                    Q.Cast(t, true);
                else if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && Config.Item("haras" + t.ChampionName).GetValue<bool>() && !Player.UnderTurret(true))
                {
                     if (Player.Mana > Player.MaxMana * 0.9)
                        Program.CastSpell(Q, t);
                     else if (ObjectManager.Player.Mana > RMANA + WMANA + QMANA + QMANA)
                        Program.CastSpell(Qc, t);
                     else if (Player.Mana > RMANA + WMANA + QMANA + QMANA)
                     {
                         Q.CastIfWillHit(t, 2, true);
                         if(Program.LaneClear)
                             Program.CastSpell(Q, t);
                     }
                }
                if (Player.Mana > RMANA + WMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy);
                }
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var farmPosition = Q.GetLineFarmLocation(minionList, Q.Width);
                if (farmPosition.MinionsHit >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                    Q.Cast(farmPosition.Position);
            }
        }
        private void LogicR()
        {
            var t = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);
            if (Player.CountEnemiesInRange(800f) > 2)
                R.Cast();
            else if (t.IsValidTarget() && Orbwalker.GetTarget() == null && Program.Combo && Player.GetAutoAttackDamage(t) * 2 > t.Health && !Q.IsReady() && t.CountEnemiesInRange(800) < 3)
                R.Cast();
        }

        private void Jungle()
        {
            if ( Player.Mana > RMANA  + WMANA + RMANA )
            {
                var mobs = Cache.GetMinions(ObjectManager.Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>())
                    {
                        W.Cast();
                        return;
                    }
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>())
                    {
                        Q.Cast(mob);
                        return;
                    }
                }
            }
        }

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

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }
        }
    }
}
