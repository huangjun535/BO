namespace YuLeCaitlyn
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YuLeLibrary;

    class Caitlyn
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, Qc, R, W;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private int castW;

        private float QCastTime = 0;

        public Obj_AI_Hero Player { get { return ObjectManager.Player; }}
        public Obj_AI_Hero LastW = ObjectManager.Player;

        private static string[] Spells =
        {
            "katarinar","drain","consume","absolutezero", "staticfield","reapthewhirlwind","jinxw","jinxr","shenstandunited","threshe","threshrpenta","threshq","meditate","caitlynpiltoverpeacemaker", "volibearqattack",
            "cassiopeiapetrifyinggaze","ezrealtrueshotbarrage","galioidolofdurand","luxmalicecannon", "missfortunebullettime","infiniteduress","alzaharnethergrasp","lucianq","velkozr","rocketgrabmissile"
        };

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 1250f);
            Qc = new Spell(SpellSlot.Q, 1250f);
            W = new Spell(SpellSlot.W, 800f);
            E = new Spell(SpellSlot.E, 770f);
            R = new Spell(SpellSlot.R, 3000f);


            Q.SetSkillshot(0.65f, 60f, 2200f, false, SkillshotType.SkillshotLine);
            Qc.SetSkillshot(0.65f, 60f, 2200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(1.5f, 20f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.30f, 70f, 2000f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 200f, 1500f, false, SkillshotType.SkillshotCircle);

            Config.SubMenu("Q技能设置").AddItem(new MenuItem("autoQ2", "自动Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("autoQ", "自动 Q|穿透", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("Qaoe", "自动 Q|群体伤害", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("Qslow", "自动 Q|敌人有负面状态", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("farmQ", "清线 Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("Mana", "清线 Q 最低蓝量比", true).SetValue(new Slider(80, 100, 30)));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("LCminions", "清线 Q 最低命中敌人数", true).SetValue(new Slider(2, 10, 0)));

            Config.SubMenu("W技能设置").AddItem(new MenuItem("WCombo", "连招预判W", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("autoW", "自动 W|控制", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("telE", "自动W |TP位置", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("bushW", "自动 W|敌人进草丛", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("bushW2", "自动 W|塔下放夹子", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("Wspell", "自动 W| 打断某些技能", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("WmodeGC", "自动 W|反突进模式", true).SetValue(new StringList(new[] { "落点位置", "我的预判" }, 0)));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu("W技能设置").AddItem(new MenuItem("WGCchampion" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));

            Config.SubMenu("E技能设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("Ehitchance", "自动 E|突进", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("useE", "自动 E|突进按键", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("EQks", "自动 E + Q + AA 击杀敌人", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("harrasEQ", "自动 WEQ 骚扰敌人", true).SetValue(true));
            Config.SubMenu("E技能设置").AddItem(new MenuItem("EmodeGC", "自动 E|反突模式", true).SetValue(new StringList(new[] { "落点位置", "鼠标位置", "敌方预判" }, 2)));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu("E技能设置").AddItem(new MenuItem("EGCchampion" + enemy.ChampionName, enemy.ChampionName, true).SetValue(true));

            Config.SubMenu("R技能设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rturrent", "自动 R|禁止在塔下使用", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rcol", "自动 R|目标附近检测碰撞距离(防止有人能挡)", true).SetValue(new Slider(400, 1000, 1)));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("Rrange", "自动 R|最低释放距离", true).SetValue(new Slider(1000, 1500, 1)));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("useR", "手动 R按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("自动眼位", "自动眼位"));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "抵抗军天使", "荒野治安官", "古墓丽影", "冰原狙击", "女警狙击", "铁血狙击手", "玉净夜叉" })));
            }

            Config.SubMenu("显示范围").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示范围").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示范围").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示范围").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;

        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.W)
            {
                if (ObjectManager.Get<Obj_GeneralParticleEmitter>().Any(obj => obj.IsValid && obj.Position.Distance(args.EndPosition) < 300 && obj.Name.ToLower().Contains("yordleTrap_idle_green.troy".ToLower()) ))
                    args.Process = false;
            }
            if (args.Slot == SpellSlot.E && Player.Mana > RMANA + WMANA)
            {
                W.Cast(Player.Position.Extend(args.EndPosition, Player.Distance(args.EndPosition) + 50));
                Utility.DelayAction.Add(10, () => E.Cast(args.EndPosition));
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && (args.SData.Name == "CaitlynPiltoverPeacemaker" || args.SData.Name == "CaitlynEntrapment"))
            {
                QCastTime = Game.Time;
            }

            if (!W.IsReady() || sender.IsMinion || !sender.IsEnemy || !Config.Item("Wspell", true).GetValue<bool>() || !sender.IsValid<Obj_AI_Hero>() || !sender.IsValidTarget(W.Range))
                return;

            var foundSpell = Spells.Find(x => args.SData.Name.ToLower() == x);
            if (foundSpell != null)
            {
                W.Cast(sender.Position);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if ( Player.Mana > RMANA + WMANA)
            {
                var t = gapcloser.Sender;
                if (E.IsReady() && t.IsValidTarget(E.Range) && Config.Item("EGCchampion" + t.ChampionName, true).GetValue<bool>())
                {
                    if (Config.Item("EmodeGC", true).GetValue<StringList>().SelectedIndex == 0)
                        E.Cast(gapcloser.End);
                    else if (Config.Item("EmodeGC", true).GetValue<StringList>().SelectedIndex == 1)
                        E.Cast(Game.CursorPos);
                    else
                        E.Cast(t.ServerPosition);
                }
                else if (W.IsReady() && t.IsValidTarget(W.Range) && Config.Item("WGCchampion" + t.ChampionName, true).GetValue<bool>())
                {
                    if (Config.Item("WmodeGC", true).GetValue<StringList>().SelectedIndex == 0)
                        W.Cast(gapcloser.End);
                    else
                        W.Cast(Player.ServerPosition);
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsRecalling())
                return;

            if (Config.Item("useR", true).GetValue<KeyBind>().Active && R.IsReady())
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    R.CastOnUnit(t);
            }



            if (Program.LagFree(0))
            {
                SetMana();
                R.Range = (500 * R.Level) + 1500;
            }
            
            if (Program.LagFree(1) && E.IsReady() && Orbwalking.CanMove(40))
                LogicE();
            if (Program.LagFree(2) && W.IsReady() && Orbwalking.CanMove(40))
                LogicW();
            if (Program.LagFree(3) && Q.IsReady() && Orbwalking.CanMove(40) && Config.Item("autoQ2", true).GetValue<bool>())
                LogicQ();
            if (Program.LagFree(4) && R.IsReady() && Config.Item("autoR", true).GetValue<bool>() && !ObjectManager.Player.UnderTurret(true) && Game.Time - QCastTime > 1)
                LogicR();
            return;
        }

        private void LogicR()
        {
            bool cast = false;

            if (Player.UnderTurret(true) && Config.Item("Rturrent", true).GetValue<bool>())
                return;


            foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) && Player.Distance(target.Position) > Config.Item("Rrange", true).GetValue<Slider>().Value && target.CountEnemiesInRange(Config.Item("Rcol", true).GetValue<Slider>().Value) == 1 && target.CountAlliesInRange(500) == 0 && Common.ValidUlt(target) ))
            {
                if (Common.GetKsDamage(target, R) > target.Health )
                {
                    cast = true;
                    PredictionOutput output = R.GetPrediction(target);
                    Vector2 direction = output.CastPosition.To2D() - Player.Position.To2D();
                    direction.Normalize();
                    List<Obj_AI_Hero> enemies = HeroManager.Enemies.Where(x => x.IsValidTarget()).ToList();

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
                        if (length < (Config.Item("Rcol", true).GetValue<Slider>().Value + enemy.BoundingRadius) && Player.Distance(predictedPosition) < Player.Distance(target.ServerPosition))
                            cast = false;
                    }
                    if (cast)
                        R.CastOnUnit(target);
                }
            }
        }

        private void LogicW()
        {
            if (Player.Mana > RMANA + WMANA)
            {
                if (W.IsReady() && Config.Item("WCombo", true).GetValue<bool>() && !ObjectManager.Player.Spellbook.IsAutoAttacking && Program.Combo)
                {
                    var targetw = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                    if (targetw.IsValidTarget(W.Range))
                    {
                        var prediction =
                            Prediction.GetPrediction(
                                new PredictionInput
                                {
                                    Unit = targetw,
                                    Delay = W.Delay,
                                    Radius = W.Width,
                                    Speed = W.Speed,
                                    Range = W.Range
                                });
                        if (targetw.IsMelee && targetw.IsFacing(ObjectManager.Player) && targetw.Distance(ObjectManager.Player) < 300 && Environment.TickCount - castW > 1300)
                        {
                            W.Cast(ObjectManager.Player);
                            castW = Environment.TickCount;
                        }

                        if (prediction.Hitchance >= HitChance.VeryHigh && targetw.IsFacing(ObjectManager.Player)
                            && Environment.TickCount - castW > 1300)
                        {
                            W.Cast(prediction.CastPosition);
                            castW = Environment.TickCount;
                        }

                        if (!targetw.IsFacing(ObjectManager.Player) && Environment.TickCount - castW > 2000)
                        {
                            var vector = targetw.ServerPosition - ObjectManager.Player.Position;
                            var Behind = W.GetPrediction(targetw).CastPosition + Vector3.Normalize(vector) * 100;
                            W.Cast(Behind);
                            castW = Environment.TickCount;
                        }
                    }
                } 

                if (Program.Combo)
                    return;
                if (Config.Item("autoW", true).GetValue<bool>())
                { 
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !Common.CanMove(enemy) && !enemy.HasBuff("caitlynyordletrapinternal")))
                    {
                        if (Utils.TickCount - W.LastCastAttemptT > 1000)
                        {
                            W.Cast(enemy);
                            LastW = enemy;
                        }
                        else if (LastW.NetworkId != enemy.NetworkId)
                        {
                            W.Cast(enemy);
                            LastW = enemy;
                        }
                    }
                }
                
                if (Config.Item("telE", true).GetValue<bool>())
                {
                    var trapPos = GetTrapPos(W.Range);
                    if (!trapPos.IsZero)
                        W.Cast(trapPos);
                }
                if((int)(Game.Time * 10) % 2 == 0 && Config.Item("bushW2", true).GetValue<bool>())
                {
                    if (Player.Spellbook.GetSpell(SpellSlot.W).Ammo == new int[]{0,3,3,4,4,5}[W.Level] && Player.CountEnemiesInRange(1000) == 0)
                    {
                        var points = Common.CirclePoints(8, W.Range, Player.Position);
                        foreach (var point in points)
                        {
                            if (NavMesh.IsWallOfGrass(point, 0) || point.UnderTurret(true))
                            {
                                if (!Common.CirclePoints(8, 150, point).Any(x => x.IsWall()))
                                {
                                    W.Cast(point);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        public Vector3 GetTrapPos(float range)
        {
            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValid && enemy.Distance(Player.ServerPosition) < range && (enemy.HasBuff("zhonyasringshield") || enemy.HasBuff("BardRStasis"))))
            {
                return enemy.Position;
            }

            foreach (var obj in ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(obj => obj.IsValid && obj.Position.Distance(Player.Position) < range))
            {
                var name = obj.Name.ToLower();

                if (name.Contains("GateMarker_red.troy".ToLower()) || name.Contains("global_ss_teleport_target_red.troy".ToLower())
                    || name.Contains("R_indicator_red.troy".ToLower()))
                    return obj.Position;
            }

            return Vector3.Zero;
        }

        private void LogicQ()
        {
            if (Program.Combo && Player.IsWindingUp)
                return;
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget(Q.Range))
            {
                if (GetRealDistance(t) > bonusRange() + 250 && !Orbwalking.InAutoAttackRange(t) && Common.GetKsDamage(t, Q) > t.Health && Player.CountEnemiesInRange(400) == 0)
                {
                    Program.CastSpell(Q, t);
             }
                else if (Program.Combo && Player.Mana > RMANA + QMANA + EMANA + 10 && Player.CountEnemiesInRange(bonusRange() + 100 + t.BoundingRadius) == 0 && !Config.Item("autoQ", true).GetValue<bool>())
                    Program.CastSpell(Q, t);
                if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + QMANA && Player.CountEnemiesInRange(400) == 0)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && (!Common.CanMove(enemy) || enemy.HasBuff("caitlynyordletrapinternal"))))
                        Q.Cast(enemy, true);
                    if (Player.CountEnemiesInRange(bonusRange()) == 0)
                    {
                        if (t.HasBuffOfType(BuffType.Slow) && Config.Item("Qslow", true).GetValue<bool>())
                            Q.Cast(t);
                        if(Config.Item("Qaoe", true).GetValue<bool>())
                            Q.CastIfWillHit(t, 2, true);
                    }
                }
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var farmPosition = Q.GetLineFarmLocation(minionList, Q.Width);
                if (farmPosition.MinionsHit > Config.Item("LCminions", true).GetValue<Slider>().Value)
                    Q.Cast(farmPosition.Position);
            }
        }

        private void LogicE()
        {
            if (Program.Combo && Player.IsWindingUp)
                return;
            if (Config.Item("autoE", true).GetValue<bool>() )
            {
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget() )
                {
                    var positionT = Player.ServerPosition - (t.Position - Player.ServerPosition);

                    if (Player.Position.Extend(positionT, 400).CountEnemiesInRange(700) < 2)
                    {
                        var eDmg = E.GetDamage(t);
                        var qDmg = Q.GetDamage(t);
                        if (Config.Item("EQks", true).GetValue<bool>() && qDmg + eDmg + Player.GetAutoAttackDamage(t) > t.Health && Player.Mana > EMANA + QMANA  )
                        {
                            Program.CastSpell(E, t);
                        }
                        else if ((Program.Farm || Program.Combo) && Config.Item("harrasEQ", true).GetValue<bool>() && Player.Mana > EMANA + QMANA + RMANA)
                        {
                            Program.CastSpell(E, t);
                        }
                    }

                    if (Player.Mana > RMANA + EMANA)
                    {
                        if (Config.Item("Ehitchance", true).GetValue<bool>())
                        {
                            E.CastIfHitchanceEquals(t, HitChance.Dashing);
                        }
                        if (Player.Health < Player.MaxHealth * 0.3)
                        {
                            if (GetRealDistance(t) < 500)
                                E.Cast(t, true);
                            if (Player.CountEnemiesInRange(250) > 0)
                                E.Cast(t, true);
                        }
                    }
                        
                }
            }
            if (Config.Item("useE", true).GetValue<KeyBind>().Active)
            {
                var position = Player.ServerPosition - (Game.CursorPos - Player.ServerPosition);
                E.Cast(position, true);
            }
        }

        private float GetRealRange(GameObject target)
        {
            return 680f + Player.BoundingRadius + target.BoundingRadius;
        }

        private float GetRealDistance(GameObject target)
        {
            return Player.ServerPosition.Distance(target.Position) + ObjectManager.Player.BoundingRadius + target.BoundingRadius;
        }
        public float bonusRange()
        {
            return 720f + Player.BoundingRadius;
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
