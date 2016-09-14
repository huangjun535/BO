using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using Color = System.Drawing.Color;
using LeagueSharp.Data.Enumerations;
using YuLeLibrary;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;

namespace YuLeKarma
{
    class Program
    {
        private static Spell Q, W, E, R;
        private static Menu Menu;
        static void Main(string[] args)
        {
            Bootstrap.Init();
            Events.OnLoad += Events_OnLoad;
        }

        private static void Events_OnLoad()
        {
            if (GameObjects.Player.ChampionName != "Karma")
                return;

            Init();
        }

        private static void Init()
        {
            Q = new Spell(SpellSlot.Q, 950f);
            W = new Spell(SpellSlot.W, 675f);
            E = new Spell(SpellSlot.E, 800f);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 60f, 1700f, true, SkillshotType.SkillshotLine);

            Menu = new Menu("QQ群438230879", "QQ群438230879", true).Attach();

            var QMenu = new Menu("q", "Q 设置");
            {
                QMenu.Add(new MenuBool("combo", "连招中使用", true));
                QMenu.Add(new MenuSliderButton("harass", "骚扰中使用-> 最低蓝量比", 50, 0, 99, true));
                QMenu.Add(new MenuSliderButton("clear", "清线清野中使用-> 最低蓝量比", 50, 0, 99, true));
                QMenu.Add(new MenuBool("killsteal", "击杀中使用", true));
                QMenu.Add(new MenuBool("flee", "逃跑中使用"));
            }
            Menu.Add(QMenu);

            var WMenu = new Menu("w", "W 设置");
            {
                WMenu.Add(new MenuBool("combo", "连招中使用", true));
                WMenu.Add(new MenuSliderButton("jungleclear", "清野中使用-> 最低蓝量比", 50, 0, 99, true));
                WMenu.Add(new MenuSliderButton("lifesaver", "自保中使用-> 最低蓝量比", 20, 10, 100, true));
            }
            Menu.Add(WMenu);

            var EMenu = new Menu("e", "E 设置");
            {
                EMenu.Add(new MenuBool("engager", "给自己放E"));
                EMenu.Add(new MenuBool("logical", "给友军放E", true));
                EMenu.Add(new MenuBool("gapcloser", "反突进", true));
                EMenu.Add(new MenuBool("flee", "逃跑中使用", true));
                EMenu.Add(new MenuSliderButton("aoe", "团队中使用-> 最低释放友军数", 3, 2, 6, true));
                {
                    EMenu.Add(new MenuSeparator("whitelist", "使用对象"));
                    {
                        foreach (var ally in GameObjects.AllyHeroes)
                        {
                            EMenu.Add(new MenuBool(ally.ChampionName.ToLower(), ally.ChampionName, true));
                        }
                    }
                }
            }
            Menu.Add(EMenu);

            var RMenu = new Menu("r", "R 设置");
            {
                RMenu.Add(new MenuBool("empq", "充能Q", true));
                RMenu.Add(new MenuBool("empe", "充能E", true));
            }
            Menu.Add(RMenu);

            new AutoWard(Menu);

            var DrawingsMenu = new Menu("drawings", "显示 设置");
            {
                DrawingsMenu.Add(new MenuBool("q", "Q 范围"));
                DrawingsMenu.Add(new MenuBool("w", "W 范围"));
                DrawingsMenu.Add(new MenuBool("e", "E 范围"));
            }
            Menu.Add(DrawingsMenu);

            Menu.Add(new MenuBool("support", "辅助模式", true));
            Menu.Add(new MenuKeyBind("Flee", "逃跑按键!", System.Windows.Forms.Keys.Z, KeyBindType.Press));

            Game.OnUpdate += OnUpdate;
            Events.OnGapCloser += OnGapCloser;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (GameObjects.Player.IsDead)
            {
                return;
            }

            Automatic(args);
            Killsteal(args);

            if (!GameObjects.Player.IsWindingUp)
            {
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
                    default:
                        break;
                }
            }

            Flee(args);
        }

        public static void Events_OnLoad(object sender, EventArgs e)
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

        private static void Flee(EventArgs args)
        {
            if (Menu["Flee"].GetValue<MenuKeyBind>().Active && Variables.Orbwalker.ActiveMode == OrbwalkingMode.None)
            {
                Variables.Orbwalker.Move(Game.CursorPos);

                if (E.IsReady() && Menu["e"]["flee"].GetValue<MenuBool>() && GameObjects.Player.Mana > E.Instance.ManaCost)
                {
                    E.CastOnUnit(GameObjects.Player);
                }

                if (Q.IsReady() && Menu["q"]["flee"].GetValue<MenuBool>() && GameObjects.Player.Mana > Q.Instance.ManaCost)
                {
                    if (Target.IsValidTarget(Q.Range - 100f) && !Invulnerable.Check(Target, DamageType.Magical))
                    {
                        if (!Q.GetPrediction(Target).CollisionObjects.Any(c => Minions.Contains(c)))
                        {
                            if (R.IsReady() && Menu["r"]["empq"].GetValue<MenuBool>().Value)
                            {
                                R.Cast();
                            }

                            Q.Cast(Q.GetPrediction(Target).UnitPosition);
                        }
                    }
                }
            }
        }

        private static void Automatic(EventArgs args)
        {
            if (Menu["support"].GetValue<MenuBool>().Value)
            {
                Variables.Orbwalker.AttackState = (Variables.Orbwalker.ActiveMode != OrbwalkingMode.Hybrid && Variables.Orbwalker.ActiveMode != OrbwalkingMode.LaneClear);
            }

            if (E.IsReady() && R.IsReady() && Menu["r"]["empe"].GetValue<MenuBool>().Value && GameObjects.Player.CountEnemyHeroesInRange(2000f) >= 2 && GameObjects.Player.CountAllyHeroesInRange(600f) >= Menu["e"]["aoe"].GetValue<MenuSliderButton>().SValue + 1 && Menu["e"]["aoe"].GetValue<MenuSliderButton>().BValue)
            {
                R.Cast();
                E.CastOnUnit(GameObjects.Player);
            }
        }

        private static void Killsteal(EventArgs args)
        {
            if (Q.IsReady() && Menu["q"]["killsteal"].GetValue<MenuBool>().Value)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(t => t.IsValidTarget(Q.Range - 100f) && !Invulnerable.Check(t, DamageType.Magical) && GetRealHealth(t) < (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q) + (R.IsReady() ? (float)GameObjects.Player.GetSpellDamage(t, SpellSlot.Q, DamageStage.Empowered) : 0)))
                {
                    if (!Q.GetPrediction(target).CollisionObjects.Any(c => Minions.Contains(c)))
                    {
                        if (R.IsReady() && Menu["r"]["empq"].GetValue<MenuBool>().Value)
                        {
                            R.Cast();
                        }

                        Q.Cast(Q.GetPrediction(target).UnitPosition);
                    }
                }
            }
        }

        private static void Combo(EventArgs args)
        {
            if (HasSheenBuff() || !Target.IsValidTarget())
            {
                return;
            }

            if (E.IsReady() && !Target.IsValidTarget(W.Range) && Target.IsValidTarget(W.Range + 200f) && !Invulnerable.Check(Target, DamageType.Magical, false) && Menu["e"]["engager"].GetValue<MenuBool>().Value)
            {
                E.CastOnUnit(GameObjects.Player);
            }

            if (W.IsReady() && Target.IsValidTarget(W.Range) && !Invulnerable.Check(Target, DamageType.Magical, false) && Menu["w"]["combo"].GetValue<MenuBool>().Value)
            {
                if (R.IsReady() && Menu["w"]["lifesaver"].GetValue<MenuSliderButton>().BValue && Menu["w"]["lifesaver"].GetValue<MenuSliderButton>().SValue > GameObjects.Player.HealthPercent)
                {
                    R.Cast();
                }

                W.CastOnUnit(Target);
            }

            if (Q.IsReady() && Target.IsValidTarget(Q.Range - 100f) && !Invulnerable.Check(Target, DamageType.Magical) && Menu["q"]["combo"].GetValue<MenuBool>().Value)
            {
                if (!Q.GetPrediction(Target).CollisionObjects.Any(c => Minions.Contains(c)))
                {
                    if (R.IsReady() && Menu["r"]["empq"].GetValue<MenuBool>().Value)
                    {
                        R.Cast();
                    }

                    Q.Cast(Q.GetPrediction(Target).UnitPosition);
                }
            }
        }

        private static void Harass(EventArgs args)
        {
            if (!Target.IsValidTarget() || Invulnerable.Check(Target))
            {
                return;
            }

            if (Q.IsReady() && Target.IsValidTarget(Q.Range - 100f) && GameObjects.Player.ManaPercent > GetNeededMana(Q.Slot, Menu["q"]["harass"]) && Menu["q"]["harass"].GetValue<MenuSliderButton>().BValue)
            {
                if (!Q.GetPrediction(Target).CollisionObjects.Any())
                {
                    Q.Cast(Q.GetPrediction(Target).UnitPosition);
                }
            }
        }

        private static void Clear(EventArgs args)
        {
            if (HasSheenBuff())
            {
                return;
            }

            if (Q.IsReady() && GameObjects.Player.ManaPercent > GetNeededMana(Q.Slot, Menu["q"]["clear"]) && Menu["q"]["clear"].GetValue<MenuSliderButton>().BValue)
            {
                if (JungleMinions.Any())
                {
                    if (R.IsReady() && Menu["r"]["empq"].GetValue<MenuBool>().Value)
                    {
                        R.Cast();
                    }

                    Q.Cast(JungleMinions[0].ServerPosition);
                }
                else if (Q.GetCircularFarmLocation(Minions, 125f).MinionsHit >= 3)
                {
                    if (R.IsReady() && Menu["r"]["empq"].GetValue<MenuBool>().Value)
                    {
                        R.Cast();
                    }

                    Q.Cast(Q.GetCircularFarmLocation(Minions, 125f).Position);
                }
            }

            if (W.IsReady() && JungleMinions.Any() && GameObjects.Player.ManaPercent > GetNeededMana(W.Slot, Menu["w"]["jungleclear"]) && Menu["w"]["jungleclear"].GetValue<MenuSliderButton>().BValue)
            {
                if (R.IsReady() && Menu["w"]["lifesaver"].GetValue<MenuSliderButton>().BValue && Menu["w"]["lifesaver"].GetValue<MenuSliderButton>().SValue > GameObjects.Player.HealthPercent)
                {
                    R.Cast();
                }

                W.CastOnUnit(JungleMinions[0]);
            }
        }

        private static void OnGapCloser(object sender, Events.GapCloserEventArgs args)
        {
            if (E.IsReady() && GameObjects.Player.Distance(args.End) < 750 && Menu["e"]["gapcloser"].GetValue<MenuBool>().Value)
            {
                if (R.IsReady() && Menu["r"]["empe"].GetValue<MenuBool>().Value && GameObjects.AllyHeroes.Count(a => a.IsValidTarget(600f, false)) >= 2)
                {
                    R.Cast();
                }

                E.Cast();
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender as Obj_AI_Hero == null && sender as Obj_AI_Turret == null && !JungleMinions.Contains(sender as Obj_AI_Minion))
            {
                return;
            }

            if (sender.IsAlly || args.Target as Obj_AI_Hero == null || !(args.Target as Obj_AI_Hero).IsAlly)
            {
                return;
            }

            if (E.IsReady() && (args.Target as Obj_AI_Hero).IsValidTarget(E.Range, false) && Menu["e"]["logical"].GetValue<MenuBool>().Value && Menu["e"][(args.Target as Obj_AI_Hero).ChampionName.ToLower()].GetValue<MenuBool>().Value)
            {
                E.CastOnUnit(args.Target as Obj_AI_Hero);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Q != null && Q.IsReady())
            {
                if (Menu["drawings"]["q"] != null && Menu["drawings"]["q"].GetValue<MenuBool>().Value)
                {
                    Render.Circle.DrawCircle(GameObjects.Player.Position, Q.Range, Color.Green, 1);
                }
            }

            if (W != null && W.IsReady() && Menu["drawings"]["w"] != null && Menu["drawings"]["w"].GetValue<MenuBool>().Value)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, W.Range, Color.Purple, 1);
            }

            if (E != null && E.IsReady() && Menu["drawings"]["e"] != null && Menu["drawings"]["e"].GetValue<MenuBool>().Value)
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, E.Range, Color.Cyan, 1);
            }
        }

        private static float GetRealHealth(Obj_AI_Base target)
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

        private static Obj_AI_Hero Target => Variables.TargetSelector.GetTarget(Q.Range + 200f, DamageType.Magical);

        private static List<Obj_AI_Minion> Minions => GameObjects.EnemyMinions.Where(m => m.IsMinion() && m.IsValidTarget(Q.Range)).ToList();

        private static List<Obj_AI_Minion> JungleMinions => GameObjects.Jungle.Where(m => m.IsValidTarget(Q.Range) && !GameObjects.JungleSmall.Contains(m)).ToList();

        private static int GetNeededMana(SpellSlot slot, AMenuComponent value) => value.GetValue<MenuSliderButton>().SValue + (int)(GameObjects.Player.Spellbook.GetSpell(slot).ManaCost / GameObjects.Player.MaxMana * 100);

        private static bool HasSheenBuff() => GameObjects.Player.HasBuff("sheen") || GameObjects.Player.HasBuff("LichBane") || GameObjects.Player.HasBuff("ItemFrozenFist");
    }
}
