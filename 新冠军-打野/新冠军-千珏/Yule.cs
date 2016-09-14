namespace KKKKKKKindred
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SebbyLib;

    class Kindred
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;

        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static dash Dash;

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 340);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 550);

            Config.SubMenu("Q设置").AddItem(new MenuItem("autoQ", "Auto Q", true).SetValue(true));
            Config.SubMenu("Q设置").AddItem(new MenuItem("farmQ", "Lane clear Q", true).SetValue(true));
            Config.SubMenu("Q设置").AddItem(new MenuItem("jungleQ", "Jungle clear Q", true).SetValue(true));
            Config.SubMenu("Q设置").AddItem(new MenuItem("GAPSASA", "     突进设置", true));
            Dash = new dash(Q);

            Config.SubMenu("W设置").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            Config.SubMenu("W设置").AddItem(new MenuItem("harrasW", "Harass W", true).SetValue(true));
            Config.SubMenu("W设置").AddItem(new MenuItem("farmW", "Lane clear W", true).SetValue(true));
            Config.SubMenu("W设置").AddItem(new MenuItem("jungleW", "Jungle clear W", true).SetValue(true));

            Config.SubMenu("E设置").AddItem(new MenuItem("autoE", "Auto E", true).SetValue(true));
            Config.SubMenu("E设置").AddItem(new MenuItem("harrasE", "Harass E", true).SetValue(true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
            {
                Config.SubMenu("E设置").AddItem(new MenuItem("EuseList", "    使用E对象", true));
                Config.SubMenu("E设置").AddItem(new MenuItem("Euse" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));
            }
            Config.SubMenu("E设置").AddItem(new MenuItem("farmE", "Lane clear E", true).SetValue(true));
            Config.SubMenu("E设置").AddItem(new MenuItem("jungleE", "Jungle clear E", true).SetValue(true));

            Config.SubMenu("R设置").AddItem(new MenuItem("autoR", "Auto R", true).SetValue(true));
            Config.SubMenu("R设置").AddItem(new MenuItem("Renemy", "如果有X个敌人不使用R", true).SetValue(new Slider(4, 5, 0)));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsAlly))
            {
                Config.SubMenu("R设置").AddItem(new MenuItem("RuseList", "    使用R对象", true));
                Config.SubMenu("R设置").AddItem(new MenuItem("Ruse" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));
            }

            Config.SubMenu("杂项").AddItem(new MenuItem("haras", "       骚扰名单"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
            {
                Config.SubMenu("杂项").AddItem(new MenuItem("haras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));
            }
            Config.SubMenu("杂项").AddItem(new MenuItem("Leasd", "       清线清野"));
            Config.SubMenu("杂项").AddItem(new MenuItem("ClearEnable", "清线技能开关(鼠标滑轮控制)", true).SetValue(true)).Permashow();
            Config.SubMenu("杂项").AddItem(new MenuItem("Mana", "LaneClear Mana", true).SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("杂项").AddItem(new MenuItem("LCminions", "LaneClear minimum minions", true).SetValue(new Slider(3, 10, 0)));
            Config.SubMenu("杂项").AddItem(new MenuItem("SDASD", "       预判模式"));
            Config.SubMenu("杂项").AddItem(new MenuItem("PredictionMODE", "Prediction MODE", true).SetValue(new StringList(new[] { "Common prediction", "神预判" }, 1)));
            Config.SubMenu("杂项").AddItem(new MenuItem("HitChance", "Hit Chance", true).SetValue(new StringList(new[] { "Very High", "High", "Medium" }, 0)));
            Config.SubMenu("杂项").AddItem(new MenuItem("XWARD", "       自动插眼"));
            new ward().Load();
            new tracker().Load();
            Config.SubMenu("杂项").AddItem(new MenuItem("ZDASD", "       逃跑设置"));
            Config.SubMenu("杂项").AddItem(new MenuItem("fleekey", "逃跑按键", true).SetValue(new KeyBind('Z', KeyBindType.Press)));
            Config.SubMenu("杂项").AddItem(new MenuItem("fleeq", "使用Q", true).SetValue(true));
            Config.SubMenu("杂项").AddItem(new MenuItem("fleew", "使用W", true).SetValue(true));

            Config.SubMenu("显示").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu("显示").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu("显示").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu("显示").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            SebbyLib.Orbwalking.AfterAttack += Orbwalker_AfterAttack;
            Game.OnWndProc += Game_OnWndProc;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x20a)
            {
                Config.Item("ClearEnable", true).SetValue(!Config.Item("ClearEnable", true).GetValue<bool>());
            }
        }

        private bool boollc
        {
            get
            {
                return Config.Item("ClearEnable", true).GetValue<bool>();
            }
        }

        public void Orbwalker_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (target.Type != GameObjectType.obj_AI_Hero)
                return;

            if (Program.Combo && Player.Mana > RMANA + QMANA && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
            {
                var t = target as Obj_AI_Hero;
                if (t.IsValidTarget())
                {
                    var dashPos = Dash.CastDash();
                    if (!dashPos.IsZero && dashPos.CountEnemiesInRange(500) > 0)
                    {
                        Q.Cast(dashPos);
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }

            if (Program.LagFree(1) && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();

            if (Program.LagFree(2) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>() )
                LogicW();

            if (Program.LagFree(3) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();

            if (R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();

            if (Config.Item("fleekey", true).GetValue<KeyBind>().Active)
            {
                flee();
            }
        }

        private void flee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (Config.Item("fleeq", true).GetValue<bool>() && Q.IsReady() && Config.Item("fleew", true).GetValue<bool>() && W.IsReady())
            {
                if (W.IsReady() && Q.IsReady())
                {
                    W.Cast();
                }
                else if (!W.IsReady() && Q.IsReady())
                {
                    Q.Cast(Game.CursorPos);
                }
            }
            else if (Config.Item("fleeq", true).GetValue<bool>() && Q.IsReady())
            {
                Q.Cast(Game.CursorPos);
            }
        }

        private void LogicQ()
        {
            if (Program.Combo && Player.Mana > RMANA + QMANA)
            {
                if (Orbwalker.GetTarget() != null)
                    return;
                var dashPos = Dash.CastDash();
                if (!dashPos.IsZero && dashPos.CountEnemiesInRange(500) > 0)
                {
                    Q.Cast(dashPos);
                }
            }
            if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA && boollc)
            {
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, 400);
                if (allMinionsQ.Count >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                    Q.Cast(Game.CursorPos);
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(650, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() && !Q.IsReady())
            {
                if (Program.Combo && Player.Mana > RMANA + WMANA)
                    W.Cast();
                else if (Program.Farm && Config.Item("harrasW", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA && Config.Item("haras" + t.ChampionName).GetValue<bool>())
                    W.Cast();
            }
            var tks = TargetSelector.GetTarget(1600, TargetSelector.DamageType.Physical);
            if (tks.IsValidTarget())
            {
                if (W.GetDamage(tks) * 3 > tks.Health - OktwCommon.GetIncomingDamage(tks))
                    W.Cast();
            }

            if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmW", true).GetValue<bool>() && Player.Mana > RMANA + WMANA && boollc)
            {
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, 600);
                if (allMinionsQ.Count >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                    W.Cast();
            }
        }

        private void LogicE()
        {
            var torb = Orbwalker.GetTarget();
            if (torb == null || torb.Type != GameObjectType.obj_AI_Hero)
                return;
            else
            {
                var t = torb as Obj_AI_Hero;

                if (t.IsValidTarget(E.Range))
                {
                    if (!Config.Item("Euse" + t.ChampionName, true).GetValue<bool>())
                        return;
                    if (Program.Combo && Player.Mana > RMANA + EMANA)
                        E.CastOnUnit(t);
                    else if (Program.Farm && Config.Item("harrasE", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA && Config.Item("haras" + t.ChampionName).GetValue<bool>())
                        E.CastOnUnit(t);
                }
            }
        }

        private void LogicR()
        {
            var rEnemy = Config.Item("Renemy", true).GetValue<Slider>().Value;
            
            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && !ally.IsDead && ally.HealthPercent < 70 && Player.Distance(ally.ServerPosition) < R.Range && Config.Item("Ruse" + ally.ChampionName, true).GetValue<bool>() ))
            {
                double dmg = OktwCommon.GetIncomingDamage(ally, 1);
                var enemys = ally.CountEnemiesInRange(900);

                if (dmg == 0 && enemys == 0)
                    continue;

                if (ally.CountEnemiesInRange(500) < rEnemy)
                {
                    if (ally.Health - dmg < enemys * ally.Level * 15)
                        R.CastOnUnit(ally);
                    else if (ally.Health - dmg < ally.Level * 15)
                        R.CastOnUnit(ally);
                }
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && boollc)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>())
                    {
                        E.Cast(mob);
                        return;
                    }
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>())
                    {
                        Q.Cast(Game.CursorPos);
                        return;
                    }
                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>())
                    {
                        W.Cast();
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
            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }
            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }
            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }
    }
}
