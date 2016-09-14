namespace YuLeZilean
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using YuLeLibrary;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class Zilean
    {
        static Zilean()
        {
            Spells = new List<InitiatorSpell>
                         {
                             new InitiatorSpell { ChampionName = "Monkeyking", SDataName = "monkeykingnimbus" },
                             new InitiatorSpell { ChampionName = "Monkeyking", SDataName = "monkeykingdecoy" },
                             new InitiatorSpell { ChampionName = "Monkeyking", SDataName = "monkeykingspintowin" },
                             new InitiatorSpell { ChampionName = "Olaf", SDataName = "olafragnarok" },
                             new InitiatorSpell { ChampionName = "Gragas", SDataName = "gragase" },
                             new InitiatorSpell { ChampionName = "Hecarim", SDataName = "hecarimult" },
                             new InitiatorSpell { ChampionName = "Hecarim", SDataName = "HecarimRamp" },
                             new InitiatorSpell { ChampionName = "Ekko", SDataName = "ekkoe" },
                             new InitiatorSpell { ChampionName = "Malphite", SDataName = "ufslash " },
                             new InitiatorSpell { ChampionName = "Vi", SDataName = "viq" },
                             new InitiatorSpell { ChampionName = "Vi", SDataName = "vir" },
                             new InitiatorSpell { ChampionName = "Volibear", SDataName = "volibearq" },
                             new InitiatorSpell { ChampionName = "Lissandra", SDataName = "lissandrae" },
                             new InitiatorSpell { ChampionName = "Gnar", SDataName = "gnare" },
                             new InitiatorSpell { ChampionName = "Fiora", SDataName = "fioraq" },
                             new InitiatorSpell { ChampionName = "Sion", SDataName = "sionr" },
                             new InitiatorSpell { ChampionName = "Zac", SDataName = "zace" },
                             new InitiatorSpell { ChampionName = "KhaZix", SDataName = "khazixe" },
                             new InitiatorSpell { ChampionName = "KhaZix", SDataName = "khazixelong" },
                             new InitiatorSpell { ChampionName = "Kennen", SDataName = "kennenlightningrush" },
                             new InitiatorSpell { ChampionName = "Jax", SDataName = "jaxleapstrike" },
                             new InitiatorSpell { ChampionName = "Leona", SDataName = "leonazenithblademissle" },
                             new InitiatorSpell { ChampionName = "Shen", SDataName = "shene" },
                             new InitiatorSpell { ChampionName = "Ryze", SDataName = "ryzer" },
                             new InitiatorSpell { ChampionName = "Lucian", SDataName = "luciane" },
                             new InitiatorSpell { ChampionName = "Elise", SDataName = "elisespidereinitial" },
                             new InitiatorSpell { ChampionName = "Diana", SDataName = "dianateleport" },
                             new InitiatorSpell { ChampionName = "Akali", SDataName = "akalishadowdance" },
                             new InitiatorSpell { ChampionName = "Renekton", SDataName = "renektonsliceanddice" },
                             new InitiatorSpell { ChampionName = "Thresh", SDataName = "threshqleap" },
                             new InitiatorSpell { ChampionName = "Rengar", SDataName = "rengarr" },
                             new InitiatorSpell { ChampionName = "Shyvana", SDataName = "shyvanatransformcast" },
                             new InitiatorSpell { ChampionName = "Shyvana", SDataName = "shyvanatransformleap" },
                             new InitiatorSpell { ChampionName = "Shyvana", SDataName = "ShyvanaImmolationAura" },
                             new InitiatorSpell { ChampionName = "Udyr", SDataName = "udyrbearstance" },
                             new InitiatorSpell { ChampionName = "Kassadin", SDataName = "riftwalk" },
                             new InitiatorSpell { ChampionName = "JarvanIV", SDataName = "jarvanivdragonstrike" },
                             new InitiatorSpell { ChampionName = "Irelia", SDataName = "ireliagatotsu" },
                             new InitiatorSpell { ChampionName = "DrMundo", SDataName = "Sadism" },
                             new InitiatorSpell { ChampionName = "MasterYi", SDataName = "Highlander" },
                             new InitiatorSpell { ChampionName = "Shaco", SDataName = "Deceive" },
                             new InitiatorSpell { ChampionName = "Ahri", SDataName = "AhriTumble" },
                             new InitiatorSpell { ChampionName = "LeeSin", SDataName = "blindmonkqtwo" },
                             new InitiatorSpell { ChampionName = "Yasuo", SDataName = "yasuorknockupcombow" },
                             new InitiatorSpell { ChampionName = "Evelynn", SDataName = "evelynnw" },
                             new InitiatorSpell { ChampionName = "FiddleSticks", SDataName = "Crowstorm" },
                             new InitiatorSpell { ChampionName = "Sivir", SDataName = "SivirR" }
                         };
        }

        public static List<InitiatorSpell> Spells { get; set; }

        private static Spell E { get; set; }

        private static bool HasSpeedBuff => Player.Buffs.Any(x => x.Name.ToLower().Contains("timewarp"));

        private static Spell IgniteSpell { get; set; }

        private static Menu Menu { get; set; }

        private static Orbwalking.Orbwalker Orbwalker { get; set; }

        private static Obj_AI_Hero Player => ObjectManager.Player;

        private static Spell Q { get; set; }

        private static Spell R { get; set; }

        private static Spell W { get; set; }

        public static void GameOnOnGameLoad()
        {
            try
            {
                if (Player.ChampionName != "Zilean")
                {
                    return;
                }

                var igniteSlot = Player.GetSpellSlot("summonerdot");
                if (igniteSlot != SpellSlot.Unknown)
                {
                    IgniteSpell = new Spell(igniteSlot, 600f);
                }

                foreach (var ally in HeroManager.Allies)
                {
                    IncomingDamageManager.AddChampion(ally);
                }

                IncomingDamageManager.Skillshots = true;

                Q = new Spell(SpellSlot.Q, 1150f - 100f);
                W = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(Player));
                E = new Spell(SpellSlot.E, 700f);
                R = new Spell(SpellSlot.R, 900f);

                Q.SetSkillshot(0.7f, 140f - 25f, int.MaxValue, false, SkillshotType.SkillshotCircle);

                GenerateMenu();

                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
                Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
                Orbwalking.BeforeAttack += BeforeAttack;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static void GenerateMenu()
        {
            try
            {
                Menu = new Menu("QQ群438230879", "QQ群438230879", true).SetFontStyle(FontStyle.Regular, SharpDX.Color.Chartreuse);

                var orbwalkMenu = new Menu("Orbwalker", "Orbwalker");
                Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
                Menu.AddSubMenu(orbwalkMenu);

                var comboMenu = new Menu("连招设置", "Combo");
                comboMenu.AddItem(new MenuItem("Combo.Q", "使用 Q").SetValue(true));
				comboMenu.AddItem(new MenuItem("DoubleBombMouse", "扔2个炸弹到鼠标位置上").SetValue(new KeyBind("Q".ToCharArray()[0], KeyBindType.Press)));
                comboMenu.AddItem(new MenuItem("DoubleBombTarget", "扔2个炸弹到鼠标附近的敌人").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
                comboMenu.AddItem(new MenuItem("Combo.Focus.Bomb", "使用 Q|集中释放给一个目标").SetValue(true));
                comboMenu.AddItem(new MenuItem("Prediction", "使用 Q|预判模式: ", true).SetValue(new StringList(new[] { "新预判", "Common" })));
                comboMenu.AddItem(new MenuItem("Combo.W", "使用 W").SetValue(true));
                comboMenu.AddItem(new MenuItem("Combo.W2", "使用 W|当Q在CD的时候就用").SetValue(false));
                comboMenu.AddItem(new MenuItem("Combo.E", "使用 E").SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboEMana", "使用 E|自身最低蓝量比").SetValue(new Slider(50)));
                comboMenu.AddItem(new MenuItem("Ignite", "使用 点燃").SetValue(true));
                Menu.AddSubMenu(comboMenu);

                var harassMenu = new Menu("骚扰设置", "Harass");
                harassMenu.AddItem(new MenuItem("Harass.Q", "使用 Q").SetValue(true));
                harassMenu.AddItem(new MenuItem("Harass.W", "使用 W").SetValue(true));
                Menu.AddSubMenu(harassMenu);

                var laneclearMenu = new Menu("清线设置", "Laneclear");
                laneclearMenu.AddItem(new MenuItem("laneclear.Q", "使用 Q").SetValue(true));
                laneclearMenu.AddItem(new MenuItem("laneclear.QMouse", "使用 Q|扔到鼠标位置").SetValue(false));
                laneclearMenu.AddItem(new MenuItem("laneclear.W", "使用 W").SetValue(false));
                laneclearMenu.AddItem(new MenuItem("laneclear.Mana", "自身最低蓝量比").SetValue(new Slider(20)));
                Menu.AddSubMenu(laneclearMenu);

                var jungleclearMenu = new Menu("清野设置", "Jungleclear");
                jungleclearMenu.AddItem(new MenuItem("jungleclear.Q", "使用 Q").SetValue(true));
                jungleclearMenu.AddItem(new MenuItem("jungleclear.W", "使用 W").SetValue(true));
                jungleclearMenu.AddItem(new MenuItem("jungleclear.Mana", "自身最低蓝量比").SetValue(new Slider(20)));
                Menu.AddSubMenu(jungleclearMenu);

                var ultimateMenu = new Menu("大招设置", "Ultimate");
                ultimateMenu.AddItem(new MenuItem("Ultimate.R", "使用 R").SetValue(true));
                ultimateMenu.AddItem(new MenuItem("min-health", "生命百分比").SetValue(new Slider(20, 1)));
                ultimateMenu.AddItem(new MenuItem("min-damage", "受到的伤害占据 x%生命百分比").SetValue(new Slider(20, 1)));
                ultimateMenu.AddItem(new MenuItem("blank-line", "---使用R对象"));
                foreach (var x in HeroManager.Allies)
                    ultimateMenu.AddItem(new MenuItem($"R{x.ChampionName}", x.ChampionName)).SetValue(true);
                Menu.AddSubMenu(ultimateMenu);

                var initiatorMenu = new Menu("加速设置", "Initiators");
                initiatorMenu.AddItem(new MenuItem("useetarget", "---使用E对象"));
                foreach (var ally in HeroManager.Allies)
                {
                    initiatorMenu.AddItem(new MenuItem($"Initiator{ally.CharData.BaseSkinName}", ally.ChampionName)).SetValue(true);
                }
                Menu.AddSubMenu(initiatorMenu);

                var fleeMenu = new Menu("逃跑设置", "Flee");
				fleeMenu.AddItem(new MenuItem("Flee.Key", "逃跑按键").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                fleeMenu.AddItem(new MenuItem("Flee.Mana", "自身最低蓝量比").SetValue(new Slider(20)));
                Menu.AddSubMenu(fleeMenu);

                var miscMenu = new Menu("杂项设置", "Misc");
                {
                    miscMenu.AddItem(new MenuItem("Combo.AA", "禁止在Q释放前攻击敌人").SetValue(true));
                    miscMenu.AddItem(new MenuItem("Q.Stun", "自动使用Q眩晕目标").SetValue(false));
                    miscMenu.AddItem(new MenuItem("Q.Interrupt", "智能Q打断技能").SetValue(true));
                    miscMenu.AddItem(new MenuItem("E.Slow", "友军被减速自动E").SetValue(true));
                }
                Menu.AddSubMenu(miscMenu);

                Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动自动插眼", true).SetValue(true));
                Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
                Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
                Menu.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
                new YuLeLibrary.AutoWard().Load();
                new YuLeLibrary.Tracker().Load();

                var SkinMenu = Menu.AddSubMenu(new Menu("换肤设置", "Skin"));
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "原版", "圣诞狂欢", "嘻哈精神", "遗迹守护者", "时间机器", "腥红之月" })));

                var drawingsMenu = new Menu("显示设置", "Drawings");
                drawingsMenu.AddItem(new MenuItem("Draw.Q", "显示 Q").SetValue(new Circle()));
                Menu.AddSubMenu(drawingsMenu);

                Menu.AddToMainMenu();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public static void CastQ(Obj_AI_Base target)
        {
            if (Menu.Item("Prediction", true).GetValue<StringList>().SelectedIndex == 0)
            {
                Prediction.SkillshotType CoreType2 = Prediction.SkillshotType.SkillshotLine;
                bool aoe2 = false;

                if (Q.Width > 80 && !Q.Collision)
                    aoe2 = true;

                var predInput2 = new Prediction.PredictionInput
                {
                    Aoe = aoe2,
                    Collision = Q.Collision,
                    Speed = Q.Speed,
                    Delay = Q.Delay,
                    Range = Q.Range,
                    From = Player.ServerPosition,
                    Radius = Q.Width,
                    Unit = target,
                    Type = CoreType2
                };
                var poutput2 = Prediction.Prediction.GetPrediction(predInput2);

                if (Q.Speed != float.MaxValue && YuLeLibrary.Common.CollisionYasuo(Player.ServerPosition, poutput2.CastPosition))
                    return;

                if (poutput2.Hitchance >= Prediction.HitChance.VeryHigh)
                    Q.Cast(poutput2.CastPosition);
                else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= Prediction.HitChance.High)
                {
                    Q.Cast(poutput2.CastPosition);
                }
            }
            else
            {
                var Pred = Q.GetPrediction(target);

                if (Pred.Hitchance >= HitChance.VeryHigh)
                {
                    Q.Cast(Pred.CastPosition);
                }
            }
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (IsActive("Combo.AA"))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    if (Q.IsReady())
                    {
                        args.Process = false;
                    }
                }
            }
        }

        private static void HandleIgnite()
        {
            if (Player.GetSpellSlot("summonerdot") == SpellSlot.Unknown)
            {
                return;
            }

            var kSableEnemy =
                HeroManager.Enemies.FirstOrDefault(
                    hero =>
                    hero.IsValidTarget(550f) && !hero.HasBuff("summonerdot") && !hero.IsZombie
                    && Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite) >= hero.Health);

            if (kSableEnemy != null)
            {
                Player.Spellbook.CastSpell(IgniteSpell.Slot, kSableEnemy);
            }
        }

        private static bool IsActive(string menuName)
        {
            return Menu.Item(menuName).IsActive();
        }

        private static void MouseCombo()
        {
            if (IsActive("Combo.Q") && Q.IsReady())
            {
                Q.Cast(Game.CursorPos);
                Utility.DelayAction.Add(100, () => W.Cast());
            }
        }

        private static void TargetCombo()
        {
            Orbwalking.MoveTo(Game.CursorPos);

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target.IsValidTarget(Q.Range))
            {
                if (Q.IsReady())
                {
                    CastQ(target);
                }

                if (!Q.IsReady() && W.IsReady())
                {
                    Utility.DelayAction.Add(100, () => W.Cast());
                }
            }
        }

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (IsActive("Combo.Q") && Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                CastQ(target);
            }

            // Check if target has a bomb
            var isBombed = HeroManager.Enemies.Find(x => x.HasBuff("ZileanQEnemyBomb") && x.IsValidTarget(Q.Range));
            if (!isBombed.IsValidTarget())
            {
                return;
            }

            if (isBombed != null && isBombed.IsValidTarget(Q.Range))
            {
                if (Q.Instance.CooldownExpires - Game.Time < 3)
                {
                    return;
                }

                if (IsActive("Combo.W"))
                {
                    W.Cast();
                }
            }

            if (IsActive("Combo.W") && IsActive("Combo.W2") && W.IsReady() && !Q.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.Health > Q.GetDamage(x) && x.IsValidTarget(Q.Range)))
                {
                    return;
                }

                W.Cast();
            }

            if (IsActive("Combo.E") && E.IsReady() && ObjectManager.Player.ManaPercent >= Menu.Item("ComboEMana").GetValue<Slider>().Value)
            {
                if (Player.GetEnemiesInRange(E.Range).Any())
                {
                    var closestEnemy =
                        Player.GetEnemiesInRange(E.Range)
                            .OrderByDescending(h => (h.PhysicalDamageDealtPlayer + h.MagicDamageDealtPlayer))
                            .FirstOrDefault();

                    if (closestEnemy == null || closestEnemy.HasBuffOfType(BuffType.Stun))
                    {
                        return;
                    }

                    E.Cast(closestEnemy);
                }
            }

            if (IsActive("Ignite") && isBombed != null)
            {
                if (Player.GetSpellSlot("summonerdot") == SpellSlot.Unknown)
                {
                    return;
                }

                if (Q.GetDamage(isBombed) + IgniteSpell.GetDamage(isBombed) > isBombed.Health)
                {
                    if (isBombed.IsValidTarget(Q.Range))
                    {
                        Player.Spellbook.CastSpell(IgniteSpell.Slot, isBombed);
                    }
                }
            }
        }

        public static void GameOnOnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {GameOnOnGameLoad();
                    try
                    {
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            );
        }

        private static void OnDraw(EventArgs args)
        {
            if (Menu.Item("Draw.Q").GetValue<Circle>().Active)
            {
                if (Q.Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.DodgerBlue);
                }
            }
        }

        private static void OnFlee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (E.IsReady() && Player.Mana > Menu.Item("Flee.Mana").GetValue<Slider>().Value)
            {
                E.Cast();
            }

            if (!E.IsReady() && W.IsReady())
            {
                if (HasSpeedBuff)
                {
                    return;
                }

                W.Cast();
            }
        }

        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (IsActive("Harass.Q") && Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                CastQ(target);
            }

            if (IsActive("Harass.W") && W.IsReady() && !Q.IsReady())
            {
                W.Cast();
            }

            var isBombed = HeroManager.Enemies.FirstOrDefault(x => x.HasBuff("ZileanQEnemyBomb") && x.IsValidTarget(Q.Range));
            if (!isBombed.IsValidTarget())
            {
                return;
            }

            if (IsActive("Harass.W"))
            {
                Utility.DelayAction.Add(100, () => W.Cast());
            }
        }

        private static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender == null || !sender.IsValidTarget(Q.Range) || !sender.IsEnemy)
            {
                return;
            }

            if (sender.IsValid && args.DangerLevel == Interrupter2.DangerLevel.High && IsActive("Q.Interrupt"))
            {
                if (Q.IsReady() && sender.IsValidTarget(Q.Range))
                {
                    CastQ(sender);
                }
                Utility.DelayAction.Add(100, () => W.Cast());
            }
        }

        private static void OnLaneclear()
        {
            var minion = MinionManager.GetMinions(Player.Position, Q.Range + Q.Width);
            if (minion == null)
            {
                return;
            }

            if (Player.ManaPercent < Menu.Item("laneclear.Mana").GetValue<Slider>().Value)
            {
                return;
            }

            var farmLocation =
                MinionManager.GetBestCircularFarmLocation(
                    MinionManager.GetMinions(Q.Range).Select(x => x.ServerPosition.To2D()).ToList(),
                    Q.Width,
                    Q.Range);

            if (farmLocation.MinionsHit == 0)
            {
                return;
            }

            if (IsActive("laneclear.Q") && IsActive("laneclear.QMouse") && Q.IsReady())
            {
                Q.Cast(Game.CursorPos);
            }

            if (IsActive("laneclear.Q") && Q.IsReady() && !IsActive("laneclear.QMouse")
                && farmLocation.MinionsHit >= 3)
            {
                Q.Cast(farmLocation.Position.To3D());
            }

            if (IsActive("laneclear.W") && W.IsReady())
            {
                W.Cast();
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var hero = sender;
            if (hero == null || !sender.IsAlly || !(sender is Obj_AI_Hero))
            {
                return;
            }

            if (!Menu.Item($"Initiator{sender.CharData.BaseSkinName}").IsActive())
            {
                return;
            }

            var initiatorChampionSpell =
                Spells.FirstOrDefault(x => x.SDataName.Equals(args.SData.Name, StringComparison.OrdinalIgnoreCase));

            if (initiatorChampionSpell != null)
            {
                if (args.Start.Distance(Player.Position) <= E.Range && args.End.Distance(Player.Position) <= E.Range
                    && HeroManager.Enemies.Any(
                        e =>
                        e.IsValidTarget(E.Range, false) && !e.IsDead
                        && (e.Position.Distance(args.End) < 600f || e.Position.Distance(args.Start) < 800f)))
                {
                    if (E.IsReady() && E.IsInRange(hero))
                    {
                        E.CastOnUnit(hero);
                    }
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                if (Player.IsDead)
                {
                    return;
                }

                if (IsActive("Combo.Focus.Bomb"))
                {
                    var passiveTarget = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget() && x.HasBuff("ZileanQEnemyBomb") && x.IsValidTarget(Q.Range + 100));

                    if (passiveTarget != null)
                    {
                        Orbwalker.ForceTarget(passiveTarget);
                    }
                }


                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        OnCombo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        OnHarass();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        OnLaneclear();
                        OnJungleClear();
                        break;
                }

                if (IsActive("Ignite"))
                {
                    HandleIgnite();
                }

                if (Menu.Item("DoubleBombMouse").GetValue<KeyBind>().Active)
                {
                    MouseCombo();
                }

                if (Menu.Item("DoubleBombTarget").GetValue<KeyBind>().Active)
                {
                    TargetCombo();
                }

                if (Menu.Item("Flee.Key").GetValue<KeyBind>().Active)
                {
                    OnFlee();
                }

                if (IsActive("E.Slow"))
                {
                    foreach (var slowedAlly in
                        HeroManager.Allies.Where(x => x.HasBuffOfType(BuffType.Slow) && x.IsValidTarget(Q.Range, false))
                        )
                    {
                        if (E.IsReady() && E.IsInRange(slowedAlly))
                        {
                            E.CastOnUnit(slowedAlly);
                        }
                    }
                }

                if (IsActive("Q.Stun"))
                {
                    var target =
                        HeroManager.Enemies.FirstOrDefault(
                            h =>
                            h.IsValidTarget(Q.Range) && h.HasBuffOfType(BuffType.Slow)
                            || h.HasBuffOfType(BuffType.Knockup) || h.HasBuffOfType(BuffType.Charm)
                            || h.HasBuffOfType(BuffType.Stun));

                    if (target != null)
                    {
                        if (Q.IsReady() && target.IsValidTarget(Q.Range))
                        {
                            CastQ(target);
                            Utility.DelayAction.Add(100, () => W.Cast());
                        }
                    }
                }

                foreach (var ally in HeroManager.Allies.Where(a => a.IsValidTarget(R.Range, false)))
                {
                    if (!Menu.Item($"R{ally.ChampionName}").IsActive() || ally.IsRecalling() || ally.IsInvulnerable
                        || !ally.IsValidTarget(R.Range, false))
                    {
                        return;
                    }

                    var enemies = ally.CountEnemiesInRange(750f);
                    var totalDamage = IncomingDamageManager.GetDamage(ally) * 1.1f;
                    if (ally.HealthPercent <= Menu.Item("min-health").GetValue<Slider>().Value && !ally.IsDead
                        && enemies >= 1 && ally.IsValidTarget(R.Range, false))
                    {
                        if ((int)(totalDamage / ally.Health) > Menu.Item("min-damage").GetValue<Slider>().Value
                            || ally.HealthPercent < Menu.Item("min-health").GetValue<Slider>().Value)
                        {
                            if (ally.Buffs.Any( b => b.DisplayName == "judicatorintervention" || b.DisplayName == "undyingrage" || b.DisplayName == "kindredrnodeathbuff" || b.DisplayName == "zhonyasringshield" || b.DisplayName == "willrevive"))
                            {
                                return;
                            }

                            R.Cast(ally);
                        }
                    }
                }

                if (Menu.SubMenu("Skin").Item("EnableSkin").GetValue<bool>())
                {
                    ObjectManager.Player.SetSkin(ObjectManager.Player.ChampionName, Menu.SubMenu("Skin").Item("SkinSelect").GetValue<StringList>().SelectedIndex);
                }
                else if (Menu.SubMenu("Skin").Item("EnableSkin").GetValue<bool>())
                {
                    ObjectManager.Player.SetSkin(ObjectManager.Player.ChampionName, 0);
                }

                YuLeLibrary.AutoWard.Enable = Menu.GetBool("AutoWard");
                YuLeLibrary.AutoWard.AutoBuy = Menu.GetBool("AutoBuy");
                YuLeLibrary.AutoWard.AutoPink = Menu.GetBool("AutoPink");
                YuLeLibrary.AutoWard.OnlyCombo = Menu.GetBool("AutoWardCombo");
                YuLeLibrary.AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static void OnJungleClear()
        {
            if (ObjectManager.Player.ManaPercent >= Menu.Item("jungleclear.Mana").GetValue<Slider>().Value)
            {
                var useQ = Menu.Item("jungleclear.Q").GetValue<bool>();
                var useW = Menu.Item("jungleclear.W").GetValue<bool>();

                var mobs = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

                if (mobs.Count() > 0)
                {
                    if (useQ && Q.IsReady())
                    {
                        Q.Cast(mobs.FirstOrDefault().Position);
                    }

                    if (useW && !Q.IsReady() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }

        public class InitiatorSpell
        {
            public string ChampionName { get; set; }

            public string SDataName { get; set; }
        }
    }
}