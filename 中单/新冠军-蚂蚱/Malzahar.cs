using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using YuLeLibrary;

namespace YuLeMalzahar
{
    class Malzahar
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell Q, Qr, W, E, R;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private float Rtime = 0;
        public Obj_AI_Hero Player = ObjectManager.Player;

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 900);
            Qr = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 700);

            Qr.SetSkillshot(0.25f, 100, float.MaxValue, false, SkillshotType.SkillshotCircle);
            Q.SetSkillshot(0.75f, 80, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(1.2f, 230, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "自动 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harrasQ", "骚扰 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("farmQ", "清线 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("ManaQ", "清线 Q|最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("LCminionsQ", "清线 Q|最低命中数", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQ", "清野 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("ManaQQ", "清野 Q|最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("intQ", "打断技能 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("gapQ", "反突进 Q", true).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "自动 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("harrasW", "骚扰 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("farmW", "清线 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("ManaW", "清线 W|最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("LCminionsW", "清线 W|最低命中数", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("jungleW", "清野 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("ManaWW", "清野 W|最低蓝量比", true).SetValue(new Slider(80, 100, 0)));

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("harrasE", "骚扰 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("harrasEminion", "尝试通过E死小兵骚扰英雄", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("farmE", "清线 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("ManaE", "清线 E|最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("LCminionsE", "清线 E|最低命中数", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("jungleE", "清野 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("ManaEE", "清野 E|最低蓝量比", true).SetValue(new Slider(80, 100, 0)));

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "自动 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rturrent", "禁止塔下R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("gapcloserlist", "反突进R释放对象", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("R 设置").AddItem(new MenuItem("gapcloser" + enemy.ChampionName, enemy.ChampionName).SetValue(false));
            Config.SubMenu("R 设置").AddItem(new MenuItem("useR", "手动R按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("useRlist", "手动R释放对象", true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                Config.SubMenu("R 设置").AddItem(new MenuItem("Ron" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "元老会议", "暗影王子", "魔灵", "至高君王", "冰雪节" })));
            }

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private void setbool()
        {
            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
        }

        private void Skin()
        {
            if (Config.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, Config.Item("SkinSelect").GetValue<StringList>().SelectedIndex);
            }
            else if (!Config.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, 0);
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R )
            {
                var t = TargetSelector.GetTarget(R.Range - 20, TargetSelector.DamageType.Magical);

                if (E.IsReady() && Player.Mana > RMANA + EMANA)
                {
                    E.CastOnUnit(t);
                    args.Process = false;
                    return;
                }

                if (W.IsReady() && Player.Mana > RMANA + WMANA)
                {
                    W.Cast(t.Position);
                    args.Process = false;
                    return;
                }

                if (Q.IsReady() && t.IsValidTarget(Q.Range) && Player.Mana > RMANA + QMANA)
                {
                    Qr.Cast(t);
                    args.Process = false;
                    return;
                }

                if(R.IsReady() && t.IsValidTarget())
                     Rtime = Game.Time;
                
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {

            var t = gapcloser.Sender;

            if (Q.IsReady() && Config.Item("gapQ", true).GetValue<bool>() && t.IsValidTarget(Q.Range))
            {
                Q.Cast(gapcloser.End);
            }
            else if (R.IsReady() && Config.Item("gapcloser" + gapcloser.Sender.ChampionName).GetValue<bool>() && t.IsValidTarget(R.Range))
            {
                R.CastOnUnit(t);
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero t, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Config.Item("intQ", true).GetValue<bool>() || !Q.IsReady())
                return;

            if (t.IsValidTarget(Q.Range))
            {
                 Q.Cast(t);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || Game.Time - Rtime < 2.5 || Player.HasBuff("malzaharrsound"))
            {
                Common.blockMove = true;
                Common.blockAttack = true;
                Common.blockSpells = true;
                Orbwalking.Attack = false;
                Orbwalking.Move = false;
                return;
            }
            else
            {
                Common.blockSpells = false;
                Common.blockMove = false;
                Common.blockAttack = false;
                Orbwalking.Attack = true;
                Orbwalking.Move = true;
            }

            if (R.IsReady() && Config.Item("useR", true).GetValue<KeyBind>().Active)
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(R.Range) && Config.Item("Ron" + t.ChampionName).GetValue<bool>())
                {
                    R.CastOnUnit(t);
                    return;
                }
            }

            setbool();
            Skin();

            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }

            if (Program.LagFree(1) && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();
            if (Program.LagFree(2) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();
            if (Program.LagFree(3) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
            if (Program.LagFree(4) && R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();
        }

        private void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var qDmg = Common.GetKsDamage(t, Q) + BonusDmg(t);

                if (qDmg > t.Health)
                    Program.CastSpell(Q, t);

                if (R.IsReady() && t.IsValidTarget(R.Range))
                {
                    return;
                }
                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if (Program.Farm && Config.Item("harrasQ", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    Program.CastSpell(Q, t);

                if (Player.Mana > RMANA + QMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !Common.CanMove(enemy)))
                        Q.Cast(enemy);
                }
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("ManaQ", true).GetValue<Slider>().Value && Config.Item("farmQ", true).GetValue<bool>() )
            {
                var allMinions = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var farmPos = Q.GetCircularFarmLocation(allMinions, 150);
                if (farmPos.MinionsHit > Config.Item("LCminionsQ", true).GetValue<Slider>().Value)
                    Q.Cast(farmPos.Position);
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var qDmg = Q.GetDamage(t);
                var wDmg = Common.GetKsDamage(t, W) + BonusDmg(t) ;
                if (wDmg > t.Health)
                {
                    W.Cast(Player.Position.Extend(t.Position,450));
                }
                else if (wDmg + qDmg > t.Health && Player.Mana > QMANA + EMANA)
                    W.Cast(Player.Position.Extend(t.Position, 450));
                else if (Program.Combo && Player.Mana > RMANA + WMANA)
                    W.Cast(Player.Position.Extend(t.Position, 450));
                else if (Program.Farm && Config.Item("harrasW", true).GetValue<bool>() && !Player.UnderTurret(true) && Player.Mana > RMANA + WMANA + EMANA + QMANA + WMANA && Common.CanHarras())
                    W.Cast(Player.Position.Extend(t.Position, 450));
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("ManaW", true).GetValue<Slider>().Value && Config.Item("farmW", true).GetValue<bool>() )
            {
                var allMinions = Cache.GetMinions(Player.ServerPosition, W.Range);
                var farmPos = W.GetCircularFarmLocation(allMinions, W.Width);
                if (farmPos.MinionsHit >= Config.Item("LCminionsW", true).GetValue<Slider>().Value)
                    W.Cast(farmPos.Position);
            }
        }

        private void LogicE()
        {
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                var eDmg = Common.GetKsDamage(t, E) + BonusDmg(t);
                var wDmg = W.GetDamage(t);

                if (eDmg > t.Health)
                    E.CastOnUnit(t);
                else if (W.IsReady() && wDmg + eDmg > t.Health && Player.Mana > WMANA + EMANA)
                    E.CastOnUnit(t);
                else if (R.IsReady() && W.IsReady() && wDmg + eDmg + R.GetDamage(t) > t.Health && Player.Mana > WMANA + EMANA + RMANA)
                    E.CastOnUnit(t);
                if (Program.Combo && Player.Mana > RMANA + EMANA)
                    E.CastOnUnit(t);
                else if (Program.Farm && Config.Item("harrasE", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    E.CastOnUnit(t);
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("ManaE", true).GetValue<Slider>().Value && Config.Item("farmE", true).GetValue<bool>())
            {
                var allMinions = Cache.GetMinions(Player.ServerPosition, E.Range);
                if (allMinions.Count >= Config.Item("LCminionsE", true).GetValue<Slider>().Value)
                {
                    foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(E.Range) && minion.Health < E.GetDamage(minion) && !minion.HasBuff("AlZaharMaleficVisions")))
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }
            else if (Program.Farm && Player.Mana > RMANA + EMANA + WMANA + EMANA && Config.Item("harrasEminion", true).GetValue<bool>())
            {
                var te = TargetSelector.GetTarget(E.Range + 400, TargetSelector.DamageType.Magical);
                if (te.IsValidTarget())
                {
                    var allMinions = Cache.GetMinions(Player.ServerPosition, E.Range);
                    foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(E.Range) && minion.Health < E.GetDamage(minion) && te.Distance(minion.Position) < 500 && !minion.HasBuff("AlZaharMaleficVisions")))
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }
        }

        private void LogicR()
        {
            if (Player.UnderTurret(true) && Config.Item("Rturrent", true).GetValue<bool>())
                return;
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (Player.CountEnemiesInRange(900) < 3 && t.IsValidTarget() )
            {
                var totalComboDamage = Common.GetKsDamage(t, R);

                totalComboDamage += E.GetDamage(t);

                if (W.IsReady() && Player.Mana > RMANA + WMANA)
                {
                    totalComboDamage += W.GetDamage(t) * 5;
                }

                if ( Player.Mana > RMANA + QMANA)
                    totalComboDamage += Q.GetDamage(t);

                if (totalComboDamage > t.Health - Common.GetIncomingDamage(t) && Common.ValidUlt(t))
                {
                    R.CastOnUnit(t);
                }
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + EMANA)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>() && Player.ManaPercent > Config.Item("ManaWW", true).GetValue<Slider>().Value)
                    {
                        W.Cast(mob.ServerPosition);
                        return;
                    }

                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>() && Player.ManaPercent > Config.Item("ManaQQ", true).GetValue<Slider>().Value)
                    {
                        Q.Cast(mob.ServerPosition);
                        return;
                    }

                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>() && mob.HasBuff("brandablaze") && Player.ManaPercent > Config.Item("ManaEE", true).GetValue<Slider>().Value)
                    {
                        E.Cast(mob);
                        return;
                    }
                }
            }
        }

        private int CountMinionsInRange(float range, Vector3 pos)
        {
            var minions = MinionManager.GetMinions(pos, range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            int count = 0;
            foreach (var minion in minions)
            {
                count++;
            }
            return count;
        }

        private float BonusDmg(Obj_AI_Hero target)
        {
            return (float)Player.CalcDamage(target, Damage.DamageType.Magical, (target.MaxHealth * 0.08) - (target.HPRegenRate * 5));
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

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1);
            }

            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Orange, 1);
            }

            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1);
            }

            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1);
            }
        }
    }
}
