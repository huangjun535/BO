namespace YuLeMorgana
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using YuLeLibrary;

    class Morgana
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 1150);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 1200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.50f, 200f, 2200f, false, SkillshotType.SkillshotCircle);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("ts", "智能 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQ", "清野 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQMana", "清野 Q | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("qCC", "自动 Q | 无法移动的敌人", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("grabqCC", "使用Q对象", true));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu("Q 设置").AddItem(new MenuItem("grab" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "自动 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("autoWcc", "自动 W | 无法移动的敌人", true).SetValue(false));
            Config.SubMenu("W 设置").AddItem(new MenuItem("farmW", "清线 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("Mana", "清线 W | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("LCminions", "清线 W | 最低命中数", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("jungleW", "清野 W", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("jungleWMana", "清野 W | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));

            foreach (var enemy in HeroManager.Enemies)
            {
                for (int i = 0; i < 4; i++)
                {
                    var spell2 = enemy.Spellbook.Spells[i];
                    if (spell2 != null && spell2.SData != null && spell2.SData.TargettingType != SpellDataTargetType.Self && spell2.SData.TargettingType != SpellDataTargetType.SelfAndUnit)
                    {
                        var spell = Damage.Spells[enemy.ChampionName].FirstOrDefault(s => s.Slot == enemy.Spellbook.Spells[i].Slot);
                        if (spell != null)
                        {
                            if (spell.DamageType == Damage.DamageType.Physical || spell.DamageType == Damage.DamageType.True)
                                Config.SubMenu("E 设置").SubMenu("技能设置").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell2.SData.Name, spell2.Name, true).SetValue(false));
                            else
                                Config.SubMenu("E 设置").SubMenu("技能设置").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell2.SData.Name, spell2.Name, true).SetValue(true));
                        }
                        else
                            Config.SubMenu("E 设置").SubMenu("技能设置").SubMenu(enemy.ChampionName).AddItem(new MenuItem("spell" + spell2.SData.Name, spell2.Name, true).SetValue(true));
                    }
                }
            }

            Config.SubMenu("E 设置").SubMenu("友军设置").AddItem(new MenuItem("skillshotssss", "使用对象", true));

            foreach (var ally in HeroManager.Allies)
            {
                Config.SubMenu("E 设置").SubMenu("友军设置").AddItem(new MenuItem("skillshot" + ally.ChampionName, ally.ChampionName, true).SetValue(true));
            }

            Config.SubMenu("R 设置").AddItem(new MenuItem("rCount", "自动 R | 附近敌人数", true).SetValue(new Slider(3, 0, 5)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("rKs", "自动 R | 击杀", true).SetValue(false));
            Config.SubMenu("R 设置").AddItem(new MenuItem("inter", "自动 R | 打断技能", true)).SetValue(true);
            Config.SubMenu("R 设置").AddItem(new MenuItem("Gap", "自动 R | 反突进", true)).SetValue(true);

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "杀戮天使", "地狱厨房", "刀锋女王", "黑色荆棘", "鬼魂新娘", "胜利女神", "紫金罗刹" })));
            }

            Config.SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu("Draw").AddItem(new MenuItem("wRange", "W range", true).SetValue(false));
            Config.SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));

            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && Config.Item("inter", true).GetValue<bool>() && sender.IsValidTarget(R.Range))
                R.Cast();
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!E.IsReady() || !sender.IsEnemy || sender.IsMinion || args.SData.IsAutoAttack() || !sender.IsValid<Obj_AI_Hero>() || Player.Distance(sender.ServerPosition) > 2000)
                return;

            if (Config.Item("spell" + args.SData.Name, true) != null && !Config.Item("spell" + args.SData.Name, true).GetValue<bool>())
                return;

            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && Player.Distance(ally.ServerPosition) < E.Range && Config.Item("skillshot" + ally.ChampionName, true).GetValue<bool>()))
            {
                //double dmg = 0;

                if (args.Target != null && args.Target.NetworkId == ally.NetworkId)
                {
                    E.CastOnUnit(ally);
                    return;
                    //dmg = dmg + sender.GetSpellDamage(ally, args.SData.Name);
                }
                else
                {
                    if (!Common.CanHitSkillShot(ally, args))
                        continue;
                    //dmg = dmg + sender.GetSpellDamage(ally, args.SData.Name);
                    E.CastOnUnit(ally);
                    return;
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (R.IsReady() && Config.Item("Gap", true).GetValue<bool>() && gapcloser.Sender.IsValidTarget(R.Range))
                R.Cast();
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

        private void Game_OnGameUpdate(EventArgs args)
        {
            setbool();
            Skin();

            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }
            if (Program.LagFree(1) && Q.IsReady())
                LogicQ();
            if (Program.LagFree(2) && R.IsReady())
                LogicR();
            if (Program.LagFree(3) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
            if (Program.LagFree(4) && E.IsReady())
                LogicE();
        }

        private void LogicE()
        {
            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValid && ally.Distance(Player.Position) < E.Range))
            {
                if (Config.Item("skillshot" + ally.ChampionName, true).GetValue<bool>() && ally.HasBuffOfType(BuffType.Poison))
                {
                    E.CastOnUnit(ally);
                }
            }
        }

        private void LogicQ()
        {
            if (Program.Combo && Config.Item("ts", true).GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget(Q.Range) && !t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield) && Config.Item("grab" + t.ChampionName).GetValue<bool>())
                    Program.CastSpell(Q, t);
            }
            foreach (var t in HeroManager.Enemies.Where(t => t.IsValidTarget(Q.Range) && Config.Item("grab" + t.ChampionName).GetValue<bool>()))
            {
                if (!t.HasBuffOfType(BuffType.SpellImmunity) && !t.HasBuffOfType(BuffType.SpellShield))
                {
                    if (Program.Combo && !Config.Item("ts", true).GetValue<bool>())
                        Program.CastSpell(Q, t);

                    if (Config.Item("qCC", true).GetValue<bool>())
                    {
                        if (!Common.CanMove(t))
                            Q.Cast(t, true);
                        Q.CastIfHitchanceEquals(t, HitChance.Dashing);
                        Q.CastIfHitchanceEquals(t, HitChance.Immobile);
                    }
                }
            }
        }

        private void LogicR()
        {
            bool rKs = Config.Item("rKs", true).GetValue<bool>();
            foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) && target.HasBuff("rocketgrab2")))
            {
                if (rKs && R.GetDamage(target) > target.Health)
                    R.Cast();
            }
            if (Player.CountEnemiesInRange(R.Range) >= Config.Item("rCount", true).GetValue<Slider>().Value && Config.Item("rCount", true).GetValue<Slider>().Value > 0)
                R.Cast();
        }
        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (!Config.Item("autoWcc", true).GetValue<bool>() && !Q.IsReady())
                {
                    if (W.GetDamage(t) > t.Health)
                        Program.CastSpell(W, t);
                    else if (Program.Combo && Player.Mana > RMANA + WMANA + EMANA + QMANA)
                        Program.CastSpell(W, t);
                }

                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !Common.CanMove(enemy)))
                    W.Cast(enemy, true);
            }
            else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmW", true).GetValue<bool>() && Player.Mana > RMANA + WMANA)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, W.Range);
                var farmPosition = W.GetCircularFarmLocation(minionList, W.Width);

                if (farmPosition.MinionsHit > Config.Item("LCminions", true).GetValue<Slider>().Value)
                    W.Cast(farmPosition.Position);
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + RMANA + WMANA)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && Config.Item("jungleW", true).GetValue<bool>() && Player.ManaPercent > Config.Item("jungleWMana", true).GetValue<Slider>().Value)
                    {
                        W.Cast(mob.ServerPosition);
                        return;
                    }
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>() && Player.ManaPercent > Config.Item("jungleQMana", true).GetValue<Slider>().Value)
                    {
                        Q.Cast(mob.ServerPosition);
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