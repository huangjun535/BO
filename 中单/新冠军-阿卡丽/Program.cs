namespace YuLeAkali
{
    using LeagueSharp;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Color = System.Drawing.Color;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class Program
    {
        private static Menu Menu;
        private static Obj_AI_Hero Me;
        private static Spell Q, W, E, R;

        static void Main(string[] args)
        {
            Bootstrap.Init();
            Events.OnLoad += Events_OnLoad;
        }

        private static void Events_OnLoad()
        {
            if (GameObjects.Player.ChampionName != "Akali")
            {
                return;
            }

            Me = GameObjects.Player;

            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W, 700f);
            E = new Spell(SpellSlot.E, 325f);
            R = new Spell(SpellSlot.R, 700f);
            W.SetSkillshot(0.25f, 400f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Menu = new Menu("QQ群438230879", "QQ群438230879", true).Attach();

            var QMenu = new Menu("q", "Q设置");
            {
                QMenu.Add(new MenuBool("combo", "连招", true));
                QMenu.Add(new MenuSliderButton("harass", "骚扰|假如自己的Mp>= %", 50, 0, 100, true));
                QMenu.Add(new MenuSliderButton("jungleclear", "清野|假如自己的Mp>= %", 50, 0, 100, true));
                QMenu.Add(new MenuBool("killsteal", "击杀", true));
                QMenu.Add(new MenuBool("lasthit", "补刀", true));
            }
            Menu.Add(QMenu);

            var WMenu = new Menu("w", "W设置");
            {
                WMenu.Add(new MenuBool("logical", "自动", true));
                WMenu.Add(new MenuBool("flee", "逃跑", true));
            }
            Menu.Add(WMenu);

            var EMenu = new Menu("e", "E设置");
            {
                EMenu.Add(new MenuBool("combo", "连招", true));
                EMenu.Add(new MenuBool("killsteal", "击杀", true));
                EMenu.Add(new MenuSliderButton("clear", "清线清野|假如自己的Mp>= %", 50, 0, 100, true));
            }
            Menu.Add(EMenu);

            var RMenu = new Menu("r", "R设置");
            {
                RMenu.Add(new MenuBool("safe", "禁止R到塔底", true));
                RMenu.Add(new MenuBool("combo", "连招", true));
                RMenu.Add(new MenuSeparator("使用R对象", "使用R对象"));
                foreach (var target in GameObjects.EnemyHeroes)
                {
                    RMenu.Add(new MenuBool(target.ChampionName.ToLower(), target.ChampionName, true));
                }
                RMenu.Add(new MenuBool("killsteal", "击杀", true));
                RMenu.Add(new MenuBool("flee", "击杀", true));
            }
            Menu.Add(RMenu);

            new AutoWard(Menu);

            var DrawingsMenu = new Menu("drawings", "技能范围");
            {
                DrawingsMenu.Add(new MenuBool("q", "Q 范围"));
                DrawingsMenu.Add(new MenuBool("w", "W 范围"));
                DrawingsMenu.Add(new MenuBool("e", "E 范围"));
                DrawingsMenu.Add(new MenuBool("r", "R 范围"));
            }
            Menu.Add(DrawingsMenu);

            Menu.Add(new MenuKeyBind("Flee", "逃跑!", System.Windows.Forms.Keys.Z, KeyBindType.Press));

            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnDoCast += OnDoCast;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Me.IsDead)
            {
                return;
            }

            Automatic(args);
            Killsteal(args);

            if (GameObjects.Player.IsWindingUp)
            {
                return;
            }
            switch (Variables.Orbwalker.ActiveMode)
            {
                case OrbwalkingMode.Combo:
                    Combo(args);
                    break;
                case OrbwalkingMode.Hybrid:
                    Harass(args);
                    break;
                case OrbwalkingMode.LaneClear:
                    Clear(args);
                    break;
                case OrbwalkingMode.LastHit:
                    {
                        if (Q.IsReady() && Me.ManaPercent > Menu["q"]["jungleclear"].GetValue<MenuSliderButton>().SValue && Menu["q"]["lasthit"].GetValue<MenuBool>().Value)
                        {
                            if (Minions(Q.Range).Count() >= 1)
                            {
                                foreach (var min in Minions(Q.Range).Where(x => x.IsValidTarget(Q.Range) && x.Health < Q.GetDamage(x)))
                                {
                                    if (min != null)
                                    {
                                        Q.CastOnUnit(min);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            if (Menu["Flee"].GetValue<MenuKeyBind>().Active)
            {
                Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                if (W.IsReady() && Menu["w"]["flee"].GetValue<MenuBool>().Value)
                {
                    W.Cast(Me.ServerPosition);
                }

                if (R.IsReady() && Menu["r"]["flee"].GetValue<MenuBool>().Value)
                {
                    if (Minions(R.Range).Count() >= 1)
                    {
                        R.CastOnUnit(Minions(R.Range)[0]);
                    }

                    if (JungleMinions(R.Range).Count() >= 1)
                    {
                        R.CastOnUnit(JungleMinions(R.Range)[0]);
                    }
                }
            }
        }

        private static void Events_OnLoad(object sender, EventArgs e)
        {
            Task.Factory.StartNew(
                () =>
                {Events_OnLoad();
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

        private static void Automatic(EventArgs args)
        {
            if (Me.IsRecalling())
            {
                return;
            }

            if (W.IsReady() && Menu["w"]["logical"].GetValue<MenuBool>().Value)
            {
                if (HasDeadlyMark() || Health.GetPrediction(Me, (int)(750 + Game.Ping / 2f), 70) <= Me.MaxHealth / 4)
                {
                    W.Cast(Me.ServerPosition);
                }
            }
        }

        private static void Killsteal(EventArgs args)
        {
            if (R.IsReady() && Menu["r"]["killsteal"].GetValue<MenuBool>().Value)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(t => t.IsValidTarget(R.Range) && !Invulnerable.Check(t, DamageType.Magical) && GetRealHealth(t) < (float)Me.GetSpellDamage(t, SpellSlot.R)))
                {
                    R.CastOnUnit(target);
                    return;
                }
            }

            if (E.IsReady() && Menu["e"]["killsteal"].GetValue<MenuBool>().Value)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(t => t.IsValidTarget(AARange) && !Invulnerable.Check(t, DamageType.Physical) && GetRealHealth(t) < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.E)))
                {
                    E.Cast();
                    return;
                }
            }

            if (Q.IsReady() && Menu["q"]["killsteal"].GetValue<MenuBool>().Value)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(t => t.IsValidTarget(Q.Range) && !Invulnerable.Check(t, DamageType.Magical) && GetRealHealth(t) < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q)))
                {
                    Q.CastOnUnit(target);
                }
            }
        }

        private static void Combo(EventArgs args)
        {
            if (HasSheenBuff() || !Target.IsValidTarget() || Invulnerable.Check(Target))
            {
                return;
            }

            if (Q.IsReady() && Target.IsValidTarget(Q.Range) && Menu["q"]["combo"].GetValue<MenuBool>().Value)
            {
                Q.CastOnUnit(Target);
            }

            if (R.IsReady() && !Q.IsReady() && Target.IsValidTarget(R.Range) && !Target.IsValidTarget(AARange) && Menu["r"]["combo"].GetValue<MenuBool>().Value && Menu["r"][Target.ChampionName.ToLower()].GetValue<MenuBool>().Value)
            {
                if (!Target.IsUnderEnemyTurret() || !Menu["r"]["safe"].GetValue<MenuBool>().Value)
                {
                    R.CastOnUnit(Target);
                }
            }
        }

        private static void Harass(EventArgs args)
        {
            if (!Target.IsValidTarget() || Invulnerable.Check(Target))
            {
                return;
            }

            if (Q.IsReady() && !Me.IsUnderEnemyTurret() && Target.IsValidTarget(Q.Range) && Me.ManaPercent > Menu["q"]["harass"].GetValue<MenuSliderButton>().SValue && Menu["q"]["harass"].GetValue<MenuSliderButton>().BValue)
            {
                Q.CastOnUnit(Target);
            }
        }

        private static void Clear(EventArgs args)
        {
            if (HasSheenBuff())
            {
                return;
            }

            if (Q.IsReady() && Me.ManaPercent > Menu["q"]["jungleclear"].GetValue<MenuSliderButton>().SValue && Menu["q"]["jungleclear"].GetValue<MenuSliderButton>().BValue)
            {
                Q.CastOnUnit(JungleMinions(E.Range)[0]);
            }

            if (E.IsReady() && Minions(E.Range).Count() >= 3 && Me.ManaPercent > Menu["e"]["clear"].GetValue<MenuSliderButton>().SValue && Menu["e"]["clear"].GetValue<MenuSliderButton>().BValue)
            {
                E.Cast();
            }

            if (Q.IsReady() && Me.ManaPercent > Menu["q"]["jungleclear"].GetValue<MenuSliderButton>().SValue && Menu["q"]["lasthit"].GetValue<MenuBool>().Value)
            {
                if (Minions(Q.Range).Count() >= 1)
                {
                    foreach (var min in Minions(Q.Range).Where(x => x.IsValidTarget(Q.Range) && x.Health < Q.GetDamage(x)))
                    {
                        if (min != null)
                        {
                            Q.CastOnUnit(min);
                        }
                    }
                }
            }
        }

        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && AutoAttack.IsAutoAttack(args.SData.Name))
            {
                switch (Variables.Orbwalker.ActiveMode)
                {
                    case OrbwalkingMode.Combo:
                        Combo(sender, args);
                        break;
                    case OrbwalkingMode.LaneClear:
                        JungleClear(sender, args);
                        break;
                }
            }
        }

        private static void Combo(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(args.Target is Obj_AI_Hero) || Invulnerable.Check(args.Target as Obj_AI_Hero, DamageType.Physical))
            {
                return;
            }

            if (E.IsReady() && Menu["e"]["combo"].GetValue<MenuBool>().Value)
            {
                E.Cast();
            }
        }

        private static void JungleClear(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Variables.Orbwalker.GetTarget() as Obj_AI_Minion == null || !JungleMinions(E.Range).Contains(Variables.Orbwalker.GetTarget() as Obj_AI_Minion))
            {
                return;
            }

            if (E.IsReady() && Me.ManaPercent > Menu["e"]["clear"].GetValue<MenuSliderButton>().SValue && Menu["e"]["clear"].GetValue<MenuSliderButton>().BValue)
            {
                E.Cast();
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Q.IsReady() && Menu["drawings"]["q"].GetValue<MenuBool>().Value)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, Q.Range, Color.Green, 1);
            }

            if (W.IsReady() && Menu["drawings"]["w"].GetValue<MenuBool>().Value)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, W.Range, Color.Purple, 1);
            }

            if (E.IsReady() && Menu["drawings"]["e"].GetValue<MenuBool>().Value)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, E.Range, Color.Cyan, 1);
            }

            if (R.IsReady() && Menu["drawings"]["r"].GetValue<MenuBool>().Value)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, R.Range, Color.Red, 1);
            }
        }

        public static Obj_AI_Hero Target
        {
            get
            {
                return Variables.TargetSelector.GetTarget(R.Range, DamageType.Magical);
            }
        }

        public static List<Obj_AI_Minion> Minions(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsMinion() && m.IsValidTarget(range)).ToList();
        }

        public static List<Obj_AI_Minion> JungleMinions(float range)
        {
            return GameObjects.Jungle.Where(m => m.IsValidTarget(range) && !GameObjects.JungleSmall.Contains(m)).ToList();
        }

        public static bool HasDeadlyMark()
        {
            return !Invulnerable.Check(GameObjects.Player, DamageType.True, false) && GameObjects.Player.HasBuff("zedrtargetmark") || GameObjects.Player.HasBuff("summonerexhaust") || GameObjects.Player.HasBuff("fizzmarinerdoombomb") || GameObjects.Player.HasBuff("vladimirhemoplague") || GameObjects.Player.HasBuff("mordekaiserchildrenofthegrave");
        }

        public static bool HasSheenBuff()
        {
            return GameObjects.Player.HasBuff("Sheen") || GameObjects.Player.HasBuff("LichBane") || GameObjects.Player.HasBuff("ItemFrozenFist");
        }

        public static float GetRealHealth(Obj_AI_Base target)
        {
            var debuffer = 0f;

            if (target is Obj_AI_Hero)
            {
                if ((target as Obj_AI_Hero).ChampionName.Equals("Blitzcrank") && !(target as Obj_AI_Hero).HasBuff("BlitzcrankManaBarrierCD"))
                {
                    debuffer += target.Mana / 2;
                }
            }

            return target.Health + target.PhysicalShield + target.HPRegenRate + debuffer;
        }

        public static float AARange
        {
            get
            {
                return Me.GetRealAutoAttackRange() + (Items.HasItem(3094) && Me.GetBuffCount("itemstatikshankcharge") == 100 ? Me.GetRealAutoAttackRange() / 100 * 30 : 0f);
            }
        }
    }
}
