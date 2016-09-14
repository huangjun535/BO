namespace YuLeLucian
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Linq;
    using YuLeLibrary;

    internal class Lucian
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;

        private Spell E, Q, Q1, R, R1, W, W1;

        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private bool passRdy = false;
        private float castR = Game.Time;
        public Obj_AI_Hero Player {get { return ObjectManager.Player; }}
        public static dash Dash;

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 675f);
            Q1 = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W, 1100);
            E = new Spell(SpellSlot.E, 475f);
            R = new Spell(SpellSlot.R, 1200f);
            R1 = new Spell(SpellSlot.R, 1200f);

            Q1.SetSkillshot(0.40f, 10f, float.MaxValue, true, SkillshotType.SkillshotLine);
            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.30f, 80f, 1600f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.1f, 110, 2800, true, SkillshotType.SkillshotLine);
            R1.SetSkillshot(0.1f, 110, 2800, false, SkillshotType.SkillshotLine);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "连招Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harasQ", "骚扰Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("farmQ", "清线Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("JCQ", "清野Q", true).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "自动 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("farmW", "清线 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("JCW", "清野 W", true).SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("slowE", "自动E|身上有减速Buff", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("JCE", "清野 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("DashMode", "突进模式", true).SetValue(new StringList(new[] { "鼠标位置", "侧面", "安全距离" }, 2)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("EnemyCheck", "敌人数>= 禁止突进 ", true).SetValue(new Slider(3, 0, 5)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("AAcheck", "仅在AA范围内突进", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("GapcloserMode", "反突模式", true).SetValue(new StringList(new[] { "鼠标位置", "安全距离", "关闭" }, 1)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("ASCWSWD", "    反突列表", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("E 设置").AddItem(new MenuItem("EGCchampion" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));
            Dash = new dash(E);

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("useR", "手动R按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Config.SubMenu("杂项设置").AddItem(new MenuItem("Mana", "清线最低蓝量", true).SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("杂项设置").AddItem(new MenuItem("ManaJG", "清野最低蓝量", true).SetValue(new Slider(30, 100, 0)));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动自动插眼", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Spellbook.OnCastSpell +=Spellbook_OnCastSpell;
            Obj_AI_Base.OnDoCast += Obj_AI_Base_OnDoCast;
        }

        private void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spellName = args.SData.Name;

            if (!sender.IsMe || !Orbwalking.IsAutoAttack(spellName))
                return;

            if (args.Target is Obj_AI_Hero)
            {
                var target = (Obj_AI_Base)args.Target;

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && target.IsValid)
                {
                    Utility.DelayAction.Add(5, () => OnDoCastDelayed(args));
                }
            }
            if (args.Target is Obj_AI_Minion)
            {
                var target = (Obj_AI_Base)args.Target;

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && target.IsValid)
                {
                    Utility.DelayAction.Add(5, () => OnDoCastDelayed(args));
                }
            }
        }

        private void OnDoCastDelayed(GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target is Obj_AI_Hero)
            {
                var target = (Obj_AI_Base)args.Target;
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && target.IsValid)
                {
                    LogicE();

                    if (Q.IsReady() && !E.IsReady() && Config.GetBool("autoQ") && !passRdy)
                        Q.Cast(target);

                    if (!E.IsReady() && (!Q.IsReady() || (Q.IsReady() && !Config.GetBool("autoQ"))) && Config.GetBool("autoW") && W.IsReady() && !passRdy)
                        W.Cast(target.Position);
                }
            }
            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Player.ManaPercent >= Config.GetSlider("ManaJG"))
                {
                    var Mobs = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                    if (Mobs[0].IsValid && Mobs.Count != 0)
                    {
                        if (E.IsReady() && !passRdy && Config.GetBool("JCE"))
                            E.Cast(Player.Position.Extend(Game.CursorPos, 70));

                        if (Q.IsReady() && (!E.IsReady() || (E.IsReady() && !Config.GetBool("JCE"))) && Config.GetBool("JCQ") && !passRdy)
                            Q.Cast(Mobs[0]);

                        if (!E.IsReady() && !Q.IsReady() && Config.GetBool("JCW") && W.IsReady() && !passRdy)
                            W.Cast(Mobs[0].Position);
                    }
                }
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E)
            {
                passRdy = true;
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "LucianW" || args.SData.Name == "LucianE" || args.SData.Name == "LucianQ")
                {
                    passRdy = true;
                }
                else
                    passRdy = false;

                if (args.SData.Name == "LucianR")
                    castR = Game.Time;
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
			
            if (R1.IsReady() && Game.Time - castR > 5 && Config.Item("useR", true).GetValue<KeyBind>().Active)
            {
				if (Player.IsChannelingImportantSpell() && (int)(Game.Time * 10) % 2 == 0)
				{
				    Console.WriteLine("chaneling");
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
				
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget(R1.Range))
                {
                    R1.Cast(t);
                    return;
                }
            }
            if (Program.LagFree(0))
            {
                SetMana();
                
            }
            if (Program.LagFree(1) && Q.IsReady() && !passRdy && !SpellLock )
                LogicQ();
            if (Program.LagFree(2) && W.IsReady() && !passRdy && !SpellLock && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
            if (Program.LagFree(3) && E.IsReady())
                LogicE();
            if (Program.LagFree(4))
            {
                if (R.IsReady() && Game.Time - castR > 5 && Config.Item("autoR", true).GetValue<bool>())
                    LogicR();

                if (!passRdy && !SpellLock)
                farm();
            }

            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Program.Combo;
        }

        private double AaDamage(Obj_AI_Hero target)
        {
            if (Player.Level > 12)
                return Player.GetAutoAttackDamage(target) * 1.3;
            else if (Player.Level > 6)
                return Player.GetAutoAttackDamage(target) * 1.4;
            else if (Player.Level > 0)
                return Player.GetAutoAttackDamage(target) * 1.5;
            return 0;
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var t1 = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget(Q.Range))
            {
                if (Common.GetKsDamage(t, Q) + AaDamage(t) > t.Health)
                    Q.Cast(t);
                else if (Program.Farm && Config.Item("harasQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA + EMANA + WMANA)
                    Q.Cast(t);
            }
            else if (Program.Farm && Config.Item("harasQ", true).GetValue<bool>() && t1.IsValidTarget(Q1.Range) && Player.Distance(t1.ServerPosition) > Q.Range + 100)
            {
                if (Program.Farm && Player.Mana < RMANA + QMANA + EMANA + WMANA )
                    return;
                var prepos = Prediction.GetPrediction(t1, Q1.Delay); 
                if ((int)prepos.Hitchance < 5)
                    return;
                var distance = Player.Distance(prepos.CastPosition);
                var minions = Cache.GetMinions(Player.ServerPosition, Q.Range);
                
                foreach (var minion in minions.Where(minion => minion.IsValidTarget(Q.Range)))
                {
                    if (prepos.CastPosition.Distance(Player.Position.Extend(minion.Position, distance)) < 25)
                    {
                        Q.Cast(minion);
                        return;
                    }
                }
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (Orbwalking.InAutoAttackRange(t))
                    W.Collision=false;
                else
                    W.Collision=true;

                var qDmg = Q.GetDamage(t);
                var wDmg = Common.GetKsDamage(t, W);

                if (Orbwalking.InAutoAttackRange(t))
                {
                    qDmg += (float)AaDamage(t);
                    wDmg += (float)AaDamage(t);
                }

                if (wDmg > t.Health) 
                    Program.CastSpell(W, t);
                else if (wDmg + qDmg > t.Health && Q.IsReady() && Player.Mana > RMANA + WMANA + QMANA)
                    Program.CastSpell(W, t);

                var orbT = Orbwalker.GetTarget() as Obj_AI_Hero;
                if (orbT == null)
                {
                    return;
                }
                else if (orbT.IsValidTarget())
                {
                    t = orbT;
                }


                if (Program.Farm && Config.Item("harras" + t.ChampionName).GetValue<bool>() && !Player.UnderTurret(true) && Player.Mana > Player.MaxMana * 0.8 && Player.Mana > RMANA + WMANA + EMANA + QMANA + WMANA)
                    Program.CastSpell(W, t);
                else if (Program.Farm && Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !Common.CanMove(enemy)))
                        W.Cast(enemy, true);
                }
            }
        }
        
        private void LogicR()
        {
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget(R.Range) && t.CountAlliesInRange(500) == 0 && Common.ValidUlt(t) && !Orbwalking.InAutoAttackRange(t))
            {
                var rDmg = R.GetDamage(t,1) * (10 + 5 * R.Level);

                var tDis = Player.Distance(t.ServerPosition);
                if (rDmg * 0.8 > t.Health && tDis < 700 && !Q.IsReady())
                    R.Cast(t, true, true);
                else if (rDmg * 0.7 > t.Health && tDis < 800)
                    R.Cast(t, true, true);
                else if (rDmg * 0.6 > t.Health && tDis < 900)
                    R.Cast(t, true, true);
                else if (rDmg * 0.5 > t.Health && tDis < 1000)
                    R.Cast(t, true, true);
                else if (rDmg * 0.4 > t.Health && tDis < 1100)
                    R.Cast(t, true, true);
                else if (rDmg * 0.3 > t.Health && tDis < 1200)
                    R.Cast(t, true, true);
                return;
            }
        }

        private void LogicE()
        {
            if (Player.Mana < RMANA + EMANA || !Config.Item("autoE", true).GetValue<bool>())
                return;

            if (HeroManager.Enemies.Any(target => target.IsValidTarget(270) && target.IsMelee))
            {
                var dashPos = Dash.CastDash(true);
                if (!dashPos.IsZero)
                {
                    E.Cast(dashPos);
                }
            }
            else
            {
                if (!Program.Combo || passRdy || SpellLock)
                    return;

                var dashPos = Dash.CastDash();
                if (!dashPos.IsZero)
                {
                    E.Cast(dashPos);
                }
            }
        }


        public void farm()
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value)
                {
                    var minions = Cache.GetMinions(Player.ServerPosition, Q1.Range);
                    if (Q.IsReady() && Config.Item("farmQ", true).GetValue<bool>())
                    {
                        foreach (var minion in minions)
                        {
                            var poutput = Q1.GetPrediction(minion);
                            var col = poutput.CollisionObjects;
                            
                            if (col.Count() > 2)
                            {
                                var minionQ = col.First();
                                if (minionQ.IsValidTarget(Q.Range))
                                {
                                    Q.Cast(minion);
                                    return;
                                }
                            }
                        }
                    }
                    if (W.IsReady() && Config.Item("farmW", true).GetValue<bool>())
                    {
                        var Wfarm = W.GetCircularFarmLocation(minions, 150);
                        if (Wfarm.MinionsHit > 3 )
                            W.Cast(Wfarm.Position);
                    }
                }
            }
        }


        private bool SpellLock
        {
            get
            {
                if (Player.HasBuff("lucianpassivebuff"))
                    return true;
                else
                    return false;
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
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q1.Range, System.Drawing.Color.Cyan, 1);
            }

            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }

            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Orange, 1);
            }

            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }
    }
    internal class dash
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Menu Config = Program.Config;
        private static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private static Spell DashSpell;

        public dash(Spell qwer)
        {
            DashSpell = qwer;

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (DashSpell.IsReady() && Config.Item("EGCchampion" + gapcloser.Sender.ChampionName, true).GetValue<bool>())
            {
                int GapcloserMode = Config.Item("GapcloserMode", true).GetValue<StringList>().SelectedIndex;
                if (GapcloserMode == 0)
                {
                    var bestpoint = Player.Position.Extend(Game.CursorPos, DashSpell.Range);
                    if (IsGoodPosition(bestpoint))
                        DashSpell.Cast(bestpoint);
                }
                else if (GapcloserMode == 1)
                {
                    var points = Common.CirclePoints(10, DashSpell.Range, Player.Position);
                    var bestpoint = Player.Position.Extend(gapcloser.Sender.Position, -DashSpell.Range);
                    int enemies = bestpoint.CountEnemiesInRange(DashSpell.Range);
                    foreach (var point in points)
                    {
                        int count = point.CountEnemiesInRange(DashSpell.Range);
                        if (count < enemies)
                        {
                            enemies = count;
                            bestpoint = point;
                        }
                        else if (count == enemies && Game.CursorPos.Distance(point) < Game.CursorPos.Distance(bestpoint))
                        {
                            enemies = count;
                            bestpoint = point;
                        }
                    }
                    if (IsGoodPosition(bestpoint))
                        DashSpell.Cast(bestpoint);
                }
            }
        }

        public Vector3 CastDash(bool asap = false)
        {
            int DashMode = Config.Item("DashMode", true).GetValue<StringList>().SelectedIndex;

            Vector3 bestpoint = Vector3.Zero;
            if (DashMode == 0)
            {
                bestpoint = Player.Position.Extend(Game.CursorPos, DashSpell.Range);
            }
            else if (DashMode == 1)
            {
                var orbT = Orbwalker.GetTarget();
                if (orbT != null && orbT is Obj_AI_Hero)
                {
                    Vector2 start = Player.Position.To2D();
                    Vector2 end = orbT.Position.To2D();
                    var dir = (end - start).Normalized();
                    var pDir = dir.Perpendicular();

                    var rightEndPos = end + pDir * Player.Distance(orbT);
                    var leftEndPos = end - pDir * Player.Distance(orbT);

                    var rEndPos = new Vector3(rightEndPos.X, rightEndPos.Y, Player.Position.Z);
                    var lEndPos = new Vector3(leftEndPos.X, leftEndPos.Y, Player.Position.Z);

                    if (Game.CursorPos.Distance(rEndPos) < Game.CursorPos.Distance(lEndPos))
                    {
                        bestpoint = Player.Position.Extend(rEndPos, DashSpell.Range);
                    }
                    else
                    {
                        bestpoint = Player.Position.Extend(lEndPos, DashSpell.Range);
                    }
                }
            }
            else if (DashMode == 2)
            {
                var points = Common.CirclePoints(15, DashSpell.Range, Player.Position);
                bestpoint = Player.Position.Extend(Game.CursorPos, DashSpell.Range);
                int enemies = bestpoint.CountEnemiesInRange(350);
                foreach (var point in points)
                {
                    int count = point.CountEnemiesInRange(350);
                    if (!InAARange(point))
                        continue;
                    if (point.UnderAllyTurret())
                    {
                        bestpoint = point;
                        enemies = count - 1;
                    }
                    else if (count < enemies)
                    {
                        enemies = count;
                        bestpoint = point;
                    }
                    else if (count == enemies && Game.CursorPos.Distance(point) < Game.CursorPos.Distance(bestpoint))
                    {
                        enemies = count;
                        bestpoint = point;
                    }
                }
            }

            if (bestpoint.IsZero)
                return Vector3.Zero;

            var isGoodPos = IsGoodPosition(bestpoint);

            if (asap && isGoodPos)
            {
                return bestpoint;
            }
            else if (isGoodPos && InAARange(bestpoint))
            {
                return bestpoint;
            }
            return Vector3.Zero;
        }

        public bool InAARange(Vector3 point)
        {
            if (!Config.Item("AAcheck", true).GetValue<bool>())
                return true;
            else if (Orbwalker.GetTarget() != null && Orbwalker.GetTarget().Type == GameObjectType.obj_AI_Hero)
            {
                return point.Distance(Orbwalker.GetTarget().Position) < Player.AttackRange;
            }
            else
            {
                return point.CountEnemiesInRange(Player.AttackRange) > 0;
            }
        }

        public bool IsGoodPosition(Vector3 dashPos)
        {
            float segment = DashSpell.Range / 5;
            for (int i = 1; i <= 5; i++)
            {
                if (Player.Position.Extend(dashPos, i * segment).IsWall())
                    return false;
            }

            if (dashPos.UnderTurret(true))
                return false;

            var enemyCheck = Config.Item("EnemyCheck", true).GetValue<Slider>().Value;
            var enemyCountDashPos = dashPos.CountEnemiesInRange(600);

            if (enemyCheck > enemyCountDashPos)
                return true;

            var enemyCountPlayer = Player.CountEnemiesInRange(400);

            if (enemyCountDashPos <= enemyCountPlayer)
                return true;

            return false;
        }
    }
}