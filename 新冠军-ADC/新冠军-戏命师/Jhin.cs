namespace YuLeJhin
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using YuLeLibarary;
    using YuLeLibrary;
    using SebbyLib;

    class Jhin
    {
        private Menu Config = Program.Config;
        public static SebbyLib.Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private bool Ractive = false;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private Vector3 rPosLast;
        private Obj_AI_Hero rTargetLast;
        private Vector3 rPosCast;

        private Items.Item FarsightOrb = new Items.Item(3342, 4000f), ScryingOrb = new Items.Item(3363, 3500f);

        private static string[] Spells =
        {
            "katarinar","drain","consume","absolutezero", "staticfield","reapthewhirlwind","jinxw","jinxr","shenstandunited","threshe","threshrpenta","threshq","meditate","caitlynpiltoverpeacemaker", "volibearqattack",
            "cassiopeiapetrifyinggaze","ezrealtrueshotbarrage","galioidolofdurand","luxmalicecannon", "missfortunebullettime","infiniteduress","alzaharnethergrasp","lucianq","velkozr","rocketgrabmissile"
        };

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 2500);
            E = new Spell(SpellSlot.E, 760);
            R = new Spell(SpellSlot.R, 3500);

            W.SetSkillshot(0.75f, 40, 10000, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(1f, 120, 1600, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.24f, 80, 5000, false, SkillshotType.SkillshotLine);

            Config.SubMenu("Q技能设置").AddItem(new MenuItem("autoQ", "自动Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("harrasQ", "骚扰Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("Qminion", "Q清兵", true).SetValue(true));

            Config.SubMenu("W技能设置").AddItem(new MenuItem("autoW", "自动W", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("autoWcombo", "只在连招自动W", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("harrasW", "骚扰W", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("Wstun", "W只击晕标记的目标", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("Waoe", "W两个以上敌人", true).SetValue(false));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("autoWcc", "自动W被控制或者标记敌人", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("MaxRangeW", "W最大范围", true).SetValue(new Slider(2500, 2500, 0)));

            Config.SubMenu("E技能设置").AddItem(new MenuItem("autoE", "自动E被控制目标", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("bushE", "自动E爆发", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("Espell", "E法术检测", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("EmodeCombo", "E连招模式", true).SetValue(new StringList(new[] { "always", "落点预判", "disable" }, 1)));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("Eaoe", "X个敌人自动E", true).SetValue(new Slider(3, 5, 0)));
            Config.SubMenu("E技能设置").SubMenu("E Gap Closer").AddItem(new MenuItem("EmodeGC", "Gap Closer position mode", true).SetValue(new StringList(new[] { "敌人的位置", "我的预判位置" }, 0)));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu("E技能设置").SubMenu("E Gap Closer").AddItem(new MenuItem("EGCchampion" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));

            Config.SubMenu("R技能设置").AddItem(new MenuItem("autoR", "R开关", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rvisable", "如果敌人不可见不开枪", true).SetValue(false));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rks", "自动R如果3下能击杀", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("useR", "半自动R开关", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space
            Config.SubMenu("R技能设置").AddItem(new MenuItem("MaxRangeR", "R最大范围", true).SetValue(new Slider(3000, 3500, 0)));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("MinRangeR", "R最小范围", true).SetValue(new Slider(1000, 3500, 0)));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rsafe", "R安全检测", true).SetValue(new Slider(1000, 2000, 0)));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("trinkiet", "Auto blue trinkiet", true).SetValue(true));

            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu("骚扰设置").AddItem(new MenuItem("harras" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu("清线清野").AddItem(new MenuItem("farmQ", "Q清线", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("farmW", "Lane clear W", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("farmE", "E清线", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("Mana", "LaneClear Mana", true).SetValue(new Slider(40, 100, 0)));
            Config.SubMenu("清线清野").AddItem(new MenuItem("LCminions", "LaneClear minimum minions", true).SetValue(new Slider(3, 10, 0)));
            Config.SubMenu("清线清野").AddItem(new MenuItem("jungleE", "Jungle clear E", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("jungleQ", "Jungle clear Q", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("jungleW", "W清野", true).SetValue(true));


            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "西部牛仔" })));
            }

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(true));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRangeMini", "R 范围(小地图)", true).SetValue(true));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }
        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R)
            {
                if (Config.Item("trinkiet", true).GetValue<bool>() && !IsCastingR)
                {
                    if (Player.Level < 9)
                        ScryingOrb.Range = 2500;
                    else
                        ScryingOrb.Range = 3500;

                    if (ScryingOrb.IsReady())
                        ScryingOrb.Cast(rPosLast);
                    if (FarsightOrb.IsReady())
                        FarsightOrb.Cast(rPosLast);
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name.ToLower() == "jhinr")
            {
                rPosCast = args.End;
            }
            if (!E.IsReady() || sender.IsMinion || !sender.IsEnemy || !Config.Item("Espell", true).GetValue<bool>() || !sender.IsValid<Obj_AI_Hero>() || !sender.IsValidTarget(E.Range))
                return;

            var foundSpell = Spells.Find(x => args.SData.Name.ToLower() == x);
            if (foundSpell != null)
            {
                E.Cast(sender.Position);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && Player.Mana > RMANA + WMANA)
            {
                var t = gapcloser.Sender;
                if (t.IsValidTarget(W.Range) && Config.Item("EGCchampion" + t.ChampionName, true).GetValue<bool>())
                {
                    if (Config.Item("EmodeGC", true).GetValue<StringList>().SelectedIndex == 0)
                        E.Cast(gapcloser.End);
                    else
                        E.Cast(Player.ServerPosition);
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

            if (Program.LagFree(1) && R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();

            if (IsCastingR)
            {
                OktwCommon.blockMove = true;
                OktwCommon.blockAttack = true;
                SebbyLib.Orbwalking.Attack = false;
                SebbyLib.Orbwalking.Move = false;
                return;
            }
            else
            {
                OktwCommon.blockMove = false;
                OktwCommon.blockAttack = false;
                SebbyLib.Orbwalking.Attack = true;
                SebbyLib.Orbwalking.Move = true;
            }


            if (Program.LagFree(4) && E.IsReady())
                LogicE();

            if (Program.LagFree(2) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();

            if (Program.LagFree(3) && W.IsReady() && !Player.IsWindingUp && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
        }

        private void LogicR()
        {
            if (!IsCastingR)
                R.Range = Config.Item("MaxRangeR", true).GetValue<Slider>().Value;
            else
                R.Range = 3500;

            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                rPosLast = R.GetPrediction(t).CastPosition;
                if (Config.Item("useR", true).GetValue<KeyBind>().Active && !IsCastingR)
                {
                    R.Cast(rPosLast);
                    rTargetLast = t;
                }

                if (!IsCastingR && Config.Item("Rks", true).GetValue<bool>()
                    && GetRdmg(t) * 4 > t.Health && t.CountAlliesInRange(700) == 0 && Player.CountEnemiesInRange(Config.Item("Rsafe", true).GetValue<Slider>().Value) == 0
                    && Player.Distance(t) > Config.Item("MinRangeR", true).GetValue<Slider>().Value
                    && !Player.UnderTurret(true) && OktwCommon.ValidUlt(t) && !OktwCommon.IsSpellHeroCollision(t, R))
                {
                    R.Cast(rPosLast);
                    rTargetLast = t;
                }
                if (IsCastingR)
                {
                    if (InCone(t.ServerPosition))
                        R.Cast(t);
                    else
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(R.Range) && InCone(t.ServerPosition)).OrderBy(enemy => enemy.Health))
                        {
                            R.Cast(t);
                            rPosLast = R.GetPrediction(enemy).CastPosition;
                            rTargetLast = enemy;
                        }
                    }
                }
            }
            else if (IsCastingR && rTargetLast != null && !rTargetLast.IsDead)
            {
                if (!Config.Item("Rvisable", true).GetValue<bool>() && InCone(rTargetLast.Position) && InCone(rPosLast))
                    R.Cast(rPosLast);
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var wDmg = GetWdmg(t);
                if (wDmg > t.Health - OktwCommon.GetIncomingDamage(t))
                    Program.CastSpell(W, t);

                if (Config.Item("autoWcombo", true).GetValue<bool>() && !Program.Combo)
                    return;

                if (Player.CountEnemiesInRange(400) > 1 || Player.CountEnemiesInRange(250) > 0)
                    return;

                if (t.HasBuff("jhinespotteddebuff") || !Config.Item("Wstun", true).GetValue<bool>())
                {
                    if (Player.Distance(t) < Config.Item("MaxRangeW", true).GetValue<Slider>().Value)
                    {
                        if (Program.Combo && Player.Mana > RMANA + WMANA)
                            Program.CastSpell(W, t);
                        else if (Program.Farm && Config.Item("harrasW", true).GetValue<bool>() && Config.Item("harras" + t.ChampionName).GetValue<bool>() && Player.Mana > RMANA + WMANA + QMANA + WMANA)
                            Program.CastSpell(W, t);
                    }
                }


                if (!Program.None && Player.Mana > RMANA + WMANA)
                {
                    if (Config.Item("Waoe", true).GetValue<bool>())
                        W.CastIfWillHit(t, 2);
                    if (Config.Item("autoWcc", true).GetValue<bool>())
                    {
                        foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && (!OktwCommon.CanMove(enemy) || enemy.HasBuff("jhinespotteddebuff"))))
                            W.Cast(enemy);
                    }
                }
            }
            if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmW", true).GetValue<bool>() && Player.Mana > RMANA + WMANA)
            {
                var minionList = SebbyLib.Cache.GetMinions(Player.ServerPosition, W.Range);
                var farmPosition = W.GetLineFarmLocation(minionList, W.Width);

                if (farmPosition.MinionsHit >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                    W.Cast(farmPosition.Position);
            }
        }

        private void LogicE()
        {
            if (Config.Item("autoE", true).GetValue<bool>())
            {
                var trapPos = OktwCommon.GetTrapPos(E.Range);
                if (!trapPos.IsZero)
                    E.Cast(trapPos);

                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                    E.Cast(enemy);
            }

            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() && Config.Item("EmodeCombo", true).GetValue<StringList>().SelectedIndex != 2)
            {
                if (Program.Combo && !Player.IsWindingUp)
                {
                    if (Config.Item("EmodeCombo", true).GetValue<StringList>().SelectedIndex == 1)
                    {
                        if (E.GetPrediction(t).CastPosition.Distance(t.Position) > 100)
                        {
                            if (Player.Position.Distance(t.ServerPosition) > Player.Position.Distance(t.Position))
                            {
                                if (t.Position.Distance(Player.ServerPosition) < t.Position.Distance(Player.Position))
                                    Program.CastSpell(E, t);
                            }
                            else
                            {
                                if (t.Position.Distance(Player.ServerPosition) > t.Position.Distance(Player.Position))
                                    Program.CastSpell(E, t);
                            }
                        }
                    }
                    else
                    {
                        Program.CastSpell(E, t);
                    }
                }

                E.CastIfWillHit(t, Config.Item("Eaoe", true).GetValue<Slider>().Value);
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmE", true).GetValue<bool>())
            {
                var minionList = SebbyLib.Cache.GetMinions(Player.ServerPosition, E.Range);
                var farmPosition = E.GetCircularFarmLocation(minionList, E.Width);

                if (farmPosition.MinionsHit >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                    E.Cast(farmPosition.Position);
            }
        }

        private void LogicQ()
        {
            var torb = Orbwalker.GetTarget();

            if (torb == null || torb.Type != GameObjectType.obj_AI_Hero)
            {
                if (Config.Item("Qminion", true).GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(Q.Range + 300, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget())
                    {

                        var minion = SebbyLib.Cache.GetMinions(Prediction.GetPrediction(t, 0.1f).CastPosition, 300).Where(minion2 => minion2.IsValidTarget(Q.Range)).OrderBy(x => x.Distance(t)).FirstOrDefault();
                        if (minion.IsValidTarget())
                        {
                            if (t.Health < GetQdmg(t))
                                Q.CastOnUnit(minion);
                            if (Program.Combo && Player.Mana > RMANA + EMANA)
                                Q.CastOnUnit(minion);
                            else if (Program.Farm && Config.Item("harrasQ", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA && Config.Item("harras" + t.ChampionName).GetValue<bool>())
                                Q.CastOnUnit(minion);
                        }
                    }
                }

            }
            else if (!SebbyLib.Orbwalking.CanAttack() && !Player.IsWindingUp)
            {
                var t = torb as Obj_AI_Hero;
                if (t.Health < GetQdmg(t) + GetWdmg(t))
                    Q.CastOnUnit(t);
                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Q.CastOnUnit(t);
                else if (Program.Farm && Config.Item("harrasQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA + WMANA + EMANA && Config.Item("harras" + t.ChampionName).GetValue<bool>())
                    Q.CastOnUnit(t);
            }
            if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>())
            {
                var minionList = SebbyLib.Cache.GetMinions(Player.ServerPosition, Q.Range);

                if (minionList.Count >= Config.Item("LCminions", true).GetValue<Slider>().Value)
                {
                    var minionAttack = minionList.FirstOrDefault(x => Q.GetDamage(x) > SebbyLib.HealthPrediction.GetHealthPrediction(x, 300));
                    if (minionAttack.IsValidTarget())
                        Q.CastOnUnit(minionAttack);
                }

            }
        }

        private bool InCone(Vector3 Position)
        {
            var range = R.Range;
            var angle = 70f * (float)Math.PI / 180;
            var end2 = rPosCast.To2D() - Player.Position.To2D();
            var edge1 = end2.Rotated(-angle / 2);
            var edge2 = edge1.Rotated(angle);

            var point = Position.To2D() - Player.Position.To2D();
            if (point.Distance(new Vector2(), true) < range * range && edge1.CrossProduct(point) > 0 && point.CrossProduct(edge2) > 0)
                return true;

            return false;
        }

        private void Jungle()
        {
            if (Program.LaneClear)
            {
                var mobs = SebbyLib.Cache.GetMinions(Player.ServerPosition, Q.Range, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>())
                    {
                        W.Cast(mob.ServerPosition);
                        return;
                    }
                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>())
                    {
                        E.Cast(mob.ServerPosition);
                        return;
                    }
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>())
                    {
                        Q.CastOnUnit(mob);
                        return;
                    }
                }
            }
        }

        private bool IsCastingR { get { return R.Instance.Name == "JhinRShot"; } }

        private double GetRdmg(Obj_AI_Base target)
        {
            var damage = (-25 + 75 * R.Level + 0.2 * Player.FlatPhysicalDamageMod) * (1 + (100 - target.HealthPercent) * 0.02);

            return Player.CalcDamage(target, Damage.DamageType.Physical, damage);
        }

        private double GetWdmg(Obj_AI_Base target)
        {
            var damage = 55 + W.Level * 35 + 0.7 * Player.FlatPhysicalDamageMod;

            return Player.CalcDamage(target, Damage.DamageType.Physical, damage);
        }

        private double GetQdmg(Obj_AI_Base target)
        {
            var damage = 35 + Q.Level * 25 + 0.4 * Player.FlatPhysicalDamageMod;

            return Player.CalcDamage(target, Damage.DamageType.Physical, damage);
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = WMANA - Player.PARRegenRate * W.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (Config.Item("rRangeMini", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Aqua, 1, 20, true);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
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
    }
}
