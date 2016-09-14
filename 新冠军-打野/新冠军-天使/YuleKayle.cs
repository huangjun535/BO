namespace YuleKayle
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Linq;
    using YuLeLibrary;

    class Kayle
    {
        private Menu Menu = Program.Menu;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, W, E, R;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player;

        public void Load()
        {
            {
                Player = ObjectManager.Player;
                Q = new Spell(SpellSlot.Q, 670);
                W = new Spell(SpellSlot.W, 900);
                E = new Spell(SpellSlot.E, 660);
                R = new Spell(SpellSlot.R, 900);
            }

            {
                Menu.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "自动 Q", true).SetValue(true));
                Menu.SubMenu("Q 设置").AddItem(new MenuItem("jungleQ", "清野 Q", true).SetValue(true));
                Menu.SubMenu("Q 设置").AddItem(new MenuItem("autoharrasQ", "      骚扰Q名单", true));
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                {
                    Menu.SubMenu("Q 设置").AddItem(new MenuItem("harras" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));
                }
            }

            {
                Menu.SubMenu("W 设置").AddItem(new MenuItem("autoW", "自动 W", true).SetValue(true));
                Menu.SubMenu("W 设置").AddItem(new MenuItem("autoWspeed", "自动 W 加速", true).SetValue(true));
                Menu.SubMenu("W 设置").AddItem(new MenuItem("autoWWWWWWWW", "     自动W友军设置", true));
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly))
                {
                    Menu.SubMenu("W 设置").AddItem(new MenuItem("Wally" + ally.ChampionName, ally.ChampionName, true).SetValue(true));
                    Menu.SubMenu("W 设置").AddItem(new MenuItem("WallyHp" + ally.ChampionName, "治疗目标|触发血量比", true).SetValue(new Slider(40)));
                }
                Menu.SubMenu("W 设置").AddItem(new MenuItem("WallyMp", "治疗目标|自身最低蓝量比", true).SetValue(new Slider(40)));
            }

            {
                Menu.SubMenu("E 设置").AddItem(new MenuItem("autoE", "连招 E", true).SetValue(true));
                Menu.SubMenu("E 设置").AddItem(new MenuItem("harrasE", "骚扰 E", true).SetValue(true));
                Menu.SubMenu("E 设置").AddItem(new MenuItem("farmE", "清线 E", true).SetValue(true));
                Menu.SubMenu("E 设置").AddItem(new MenuItem("jungleE", "清野 E", true).SetValue(true));
            }

            {
                Menu.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
                Menu.SubMenu("R 设置").AddItem(new MenuItem("autoRRRRRRRRRRR", "     自动R友军设置", true));
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsAlly))
                {
                    Menu.SubMenu("R 设置").AddItem(new MenuItem("Rally" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));
                }
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var spell = enemy.Spellbook.Spells[i];
                        if (spell.SData.TargettingType != SpellDataTargetType.Self && spell.SData.TargettingType != SpellDataTargetType.SelfAndUnit)
                        {
                            Menu.SubMenu("R 设置").SubMenu("躲避技能").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell.SData.Name, spell.Name, true).SetValue(false));
                        }
                    }
                }
            }

            {
                Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
                Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
                Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
                Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
                new AutoWard().Load();
                new Tracker().Load();
            }

            {
                Menu.SubMenu("技能范围").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
                Menu.SubMenu("技能范围").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
                Menu.SubMenu("技能范围").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
                Menu.SubMenu("技能范围").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));
            }

            {
                Menu.AddItem(new MenuItem("ClearEnable", "清线技能开关(鼠标滑轮控制)", true).SetValue(true)).Permashow();
            }

            {
                Game.OnWndProc += Game_OnWndProc;
                Game.OnUpdate += Game_OnGameUpdate;
                Drawing.OnDraw += Drawing_OnDraw;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!R.IsReady() || sender.IsMinion || !sender.IsEnemy || args.SData.IsAutoAttack() || !Menu.Item("autoR", true).GetValue<bool>() || !sender.IsValid<Obj_AI_Hero>() || args.SData.Name.ToLower() == "tormentedsoil")
                return;

            if (Menu.Item("spell" + args.SData.Name, true) == null || !Menu.Item("spell" + args.SData.Name, true).GetValue<bool>())
                return;

            if (args.Target != null)
            {
                if (args.Target.IsAlly)
                {
                    var ally = args.Target as Obj_AI_Hero;

                    if (ally != null && Menu.Item("Rally" + ally.ChampionName).GetValue<bool>())
                        R.CastOnUnit(ally);
                }
            }
            else
            {
                foreach (var ally in Program.Allies.Where(ally => ally.IsValid && !ally.IsDead && ally.HealthPercent < 70 && Player.ServerPosition.Distance(ally.ServerPosition) < R.Range && Menu.Item("Rally" + ally.ChampionName).GetValue<bool>()))
                {
                    if (Common.CanHitSkillShot(ally, args))
                        R.CastOnUnit(ally);
                }
            }
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x20a)
            {
                Menu.Item("ClearEnable", true).SetValue(!Menu.Item("ClearEnable", true).GetValue<bool>());
            }
        }

        private void setbool()
        {
            AutoWard.Enable = Menu.GetBool("AutoWard");
            AutoWard.AutoBuy = Menu.GetBool("AutoBuy");
            AutoWard.AutoPink = Menu.GetBool("AutoPink");
            AutoWard.OnlyCombo = Menu.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Program.Combo;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            setbool();

            if (Program.LagFree(1))
            {
                SetMana();
                Jungle();
            }

            if (R.IsReady() && Menu.GetBool("autoR"))
                LogicR();
            if (Program.LagFree(2) && W.IsReady() && !Player.IsWindingUp && Menu.GetBool("autoW"))
                LogicW();
            if (Program.LagFree(3) && E.IsReady() && Menu.GetBool("autoE"))
                LogicE();
            if (Program.LagFree(4) && Q.IsReady() && !Player.IsWindingUp && Menu.GetBool("autoQ"))
                LogicQ();
        }

        private void LogicR()
        {
            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && !ally.IsDead && ally.HealthPercent < 50 && Player.ServerPosition.Distance(ally.ServerPosition) < R.Range && Menu.GetBool("Rally" + ally.ChampionName)))
            {
                double dmg = Common.GetIncomingDamage(ally, 1);
                var enemys = ally.CountEnemiesInRange(800);
               
                if (dmg == 0 && enemys == 0)
                    continue;

                enemys = (enemys == 0) ? 1 : enemys;

                if (ally.Health - dmg < enemys * ally.Level * 20)
                    R.CastOnUnit(ally);
            }
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (t.IsValidTarget())
            {
                if (Program.Combo)
                    Q.Cast(t);
                else if (Program.Farm && Menu.GetBool("harras" + t.ChampionName) && Player.Mana > RMANA + WMANA + QMANA + QMANA)
                    Q.Cast(t);
                else if (Player.Health < Player.Level * 40 && !W.IsReady() && !R.IsReady())
                    Q.Cast(t);
                else if (Common.GetKsDamage(t, Q) > t.Health)
                    Q.Cast(t);
            }
        }

        private void LogicW()
        {
            if (!Player.InFountain() && !Player.HasBuff("Recall") && !Player.IsRecalling())
            {
                if (Menu.GetBool("autoWspeed"))
                {
                    var t = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget())
                    {
                        if (Program.Combo && Player.Mana > WMANA + QMANA + EMANA && Player.Distance(t.Position) > Q.Range)
                            W.CastOnUnit(Player);
                    }
                }

                foreach (var ally in HeroManager.Allies.Where (ally => ally.IsValid && !ally.IsDead && Menu.GetBool("Wally" + ally.ChampionName) && Player.Distance(ally.Position) < W.Range))
                {
                    if (Player.ManaPercent >= Menu.GetSlider("WallyMp") && ally.HealthPercent <= Menu.GetSlider("WallyHp" + ally.ChampionName))
                    {
                        W.CastOnUnit(ally);
                    }
                }
            }
        }

        private void LogicE()
        {
            if(Program.Combo && Player.Mana > WMANA + EMANA && Player.CountEnemiesInRange(700) > 0)
                E.Cast();
            else if (Program.Farm && Menu.GetBool("harrasE") && Player.Mana > WMANA + EMANA + QMANA && Player.CountEnemiesInRange(500) > 0)
                E.Cast();
            else if (Program.LaneClear && Menu.GetBool("farmE") && Player.Mana > WMANA + EMANA + QMANA && FarmE() && Menu.GetBool("ClearEnable"))
                E.Cast();
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + RMANA + WMANA && Menu.GetBool("ClearEnable"))
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (E.IsReady() && Menu.GetBool("jungleE"))
                    {
                        E.Cast();
                        return;
                    }
                    if (Q.IsReady() && Menu.GetBool("jungleQ"))
                    {
                        Q.Cast(mob);
                        return;
                    }
                }
            }
        }

        private bool FarmE()
        {
            return (Cache.GetMinions(Player.ServerPosition, 600).Count > 0);
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;
            RMANA = 0;

            if (!Q.IsReady())
                QMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;

        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.GetBool("qRange") && Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }
            if (Menu.GetBool("wRange") && W.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }
            if (Menu.GetBool("eRange") && E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }
            if (Menu.GetBool("rRange") && R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }
    }
}
