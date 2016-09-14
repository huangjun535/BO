using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using CM = YuLeNidalee.CastManager;
using Color = System.Drawing.Color;
using KL = YuLeNidalee.YuLeLib;
using YuLeLibrary;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;

namespace YuLeNidalee
{
    internal class YuLeNidalee
    {
        internal static Menu Root;
        internal static Obj_AI_Hero Target;
        internal static Orbwalking.Orbwalker Orbwalker;
        internal static Obj_AI_Hero Player => ObjectManager.Player;

        internal YuLeNidalee()
        {                                                             
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        internal static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "Nidalee")
            {
                return;
            }
			

            Root = new Menu("新冠军豹女QQ群438230879", "nidalee", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Yellow);

            var orbm = new Menu("走砍设置", "orbm");
            Orbwalker = new Orbwalking.Orbwalker(orbm);
            Root.AddSubMenu(orbm);

            var KeyMenu = new Menu("按键设置", "KeyMenuMenu");
            {
                KeyMenu.AddItem(new MenuItem("usecombo", "连招")).SetValue(new KeyBind(32, KeyBindType.Press));
                KeyMenu.AddItem(new MenuItem("usecombo2", "爆发")).SetValue(new KeyBind('Z', KeyBindType.Press));
                KeyMenu.AddItem(new MenuItem("useharass", "骚扰")).SetValue(new KeyBind('C', KeyBindType.Press));
                KeyMenu.AddItem(new MenuItem("usefarm", "清线清野")).SetValue(new KeyBind('V', KeyBindType.Press));
                KeyMenu.AddItem(new MenuItem("jgaacount", "快速清野模式(测试)")).SetValue(new KeyBind('H', KeyBindType.Toggle)).Permashow();
                KeyMenu.AddItem(new MenuItem("flee", "逃跑")).SetValue(new KeyBind('A', KeyBindType.Press));
            }
            Root.AddSubMenu(KeyMenu);

            var QMenu = new Menu("Q设置", "QMenuMenu");
            {
                // human
                QMenu.AddItem(new MenuItem("ndhqcheck", "Check Hitchance")).SetValue(true);
                QMenu.AddItem(new MenuItem("qsmcol", "-> Smite Collision")).SetValue(false);
                QMenu.AddItem(new MenuItem("ndhqco", "连招中使用")).SetValue(true);
                QMenu.AddItem(new MenuItem("ndhqha", "骚扰中使用")).SetValue(true);
                QMenu.AddItem(new MenuItem("ndhqwc", "清线中使用")).SetValue(false);
                QMenu.AddItem(new MenuItem("ndhqjg", "清野中使用")).SetValue(true);
                // cougar
                QMenu.AddItem(new MenuItem("ndcqco", "连招中使用")).SetValue(true);
                QMenu.AddItem(new MenuItem("ndcqha", "骚扰中使用")).SetValue(true);
                QMenu.AddItem(new MenuItem("ndcqwc", "清线中使用")).SetValue(true);
                QMenu.AddItem(new MenuItem("ndcqjg", "清野中使用")).SetValue(true);
            }
            Root.AddSubMenu(QMenu);

            var WMenu = new Menu("W设置", "WMenuMenu");
            {
                // human
                WMenu.AddItem(new MenuItem("ndhwco", "连招中使用")).SetValue(false);
                WMenu.AddItem(new MenuItem("ndhwsp", "-> 快速W使用")).SetValue(false);
                WMenu.AddItem(new MenuItem("ndhwwc", "清线中使用")).SetValue(false);
                WMenu.AddItem(new MenuItem("ndhwjg", "清野中使用")).SetValue(true);
                WMenu.AddItem(new MenuItem("ndhwforce", "Location")).SetValue(new StringList(new[] { "Prediction", "Behind Target" }));
                // cougar
                WMenu.AddItem(new MenuItem("ndcwcheck", "Check Hitchance")).SetValue(false);
                WMenu.AddItem(new MenuItem("ndcwch", "-> Min Hitchance")).SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2));
                WMenu.AddItem(new MenuItem("ndcwco", "连招中使用")).SetValue(true);
                WMenu.AddItem(new MenuItem("ndcwhunt", "-> Ignore Checks if Hunted")).SetValue(false);
                WMenu.AddItem(new MenuItem("ndcwdistco", "-> Pounce Only if > AARange")).SetValue(true);
                WMenu.AddItem(new MenuItem("ndcwwc", "清线中使用")).SetValue(true);
                WMenu.AddItem(new MenuItem("ndcwdistwc", "-> Pounce Only if > AARange")).SetValue(false);
                WMenu.AddItem(new MenuItem("ndcwene", "-> Dont Pounce into Enemies")).SetValue(true);
                WMenu.AddItem(new MenuItem("ndcwtow", "-> Dont Pounce into Turret")).SetValue(true);
                WMenu.AddItem(new MenuItem("ndcwjg", "清野中使用")).SetValue(true);
            }
            Root.AddSubMenu(WMenu);

            var EMenu = new Menu("E设置", "EMenuMenu");
            {
                // human
                EMenu.AddItem(new MenuItem("ndheon", "Enable Healing")).SetValue(true);
                EMenu.AddItem(new MenuItem("ndhemana", "-> Minumum Mana")).SetValue(new Slider(55, 1));
                EMenu.AddItem(new MenuItem("ndhesw", "Switch Forms")).SetValue(false);
                foreach (var hero in HeroManager.Allies)
                {
                    EMenu.AddItem(new MenuItem("xx" + hero.ChampionName, hero.ChampionName)).SetValue(true);
                    EMenu.AddItem(new MenuItem("zz" + hero.ChampionName, "  自动治疗生命百分比 ")).SetValue(new Slider(88, 1, 99));
                }
                EMenu.AddItem(new MenuItem("ndheord", "Ally Priority:")).SetValue(new StringList(new[] { "Low HP", "Most AD/AP", "Max HP" }, 1));
                // cougar
                EMenu.AddItem(new MenuItem("ndcecheck", "Check Hitchance")).SetValue(false);
                EMenu.AddItem(new MenuItem("ndcech", "-> Min Hitchance")).SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2));
                EMenu.AddItem(new MenuItem("ndceco", "连招中使用")).SetValue(true);
                EMenu.AddItem(new MenuItem("ndceha", "骚扰中使用")).SetValue(true);
                EMenu.AddItem(new MenuItem("ndcewc", "清线中使用")).SetValue(true);
                EMenu.AddItem(new MenuItem("ndcenum", "-> Minimum Minions Hit")).SetValue(new Slider(3, 1, 5));
                EMenu.AddItem(new MenuItem("ndcejg", "清野中使用")).SetValue(true);
            }
            Root.AddSubMenu(EMenu);

            var RMenu = new Menu("R设置", "RMenuMenu");
            {
                // human
                RMenu.AddItem(new MenuItem("ndhrco", "连招中使用")).SetValue(true);
                RMenu.AddItem(new MenuItem("ndhrcreq", "-> 仅其他技能CD好了")).SetValue(true);
                RMenu.AddItem(new MenuItem("ndhrha", "骚扰中使用")).SetValue(true);
                RMenu.AddItem(new MenuItem("ndhrjg", "清野中使用")).SetValue(true);
                RMenu.AddItem(new MenuItem("ndhrjreq", "-> 仅其他技能CD好了")).SetValue(true);
                RMenu.AddItem(new MenuItem("ndhrwc", "清线中使用")).SetValue(false);
                // cougar
                RMenu.AddItem(new MenuItem("ndcrco", "连招中使用")).SetValue(true);
                RMenu.AddItem(new MenuItem("ndcrha", "骚扰中使用")).SetValue(true);
                RMenu.AddItem(new MenuItem("ndcrwc", "清线中使用")).SetValue(false);
                RMenu.AddItem(new MenuItem("ndcrjg", "清野中使用")).SetValue(true);
            }
            Root.AddSubMenu(RMenu);

            var AutoMenu = new Menu("自动技能", "AutoMenuMenu");
            {
                AutoMenu.AddItem(new MenuItem("alvl6", "自动升级R")).SetValue(false);
                AutoMenu.AddItem(new MenuItem("ndhqimm", "敌人无法移动自动人形态Q")).SetValue(false);
                AutoMenu.AddItem(new MenuItem("ndhqgap", "敌人突进自动人形态Q")).SetValue(true);
                AutoMenu.AddItem(new MenuItem("ndcqgap", "敌人突进自动豹形态Q")).SetValue(true);
                AutoMenu.AddItem(new MenuItem("ndhwimm", "敌人无法移动人形态W(夹子)")).SetValue(false);
                AutoMenu.AddItem(new MenuItem("ndcegap", "敌人突进自动豹形态E")).SetValue(true);
                AutoMenu.AddItem(new MenuItem("ndhrgap", "敌人突进自动切换形态")).SetValue(true);
            }
            Root.AddSubMenu(AutoMenu);

            var MiscMenu = new Menu("增强设置", "MiscMenuMenu");
            {
                MiscMenu.AddItem(new MenuItem("ASDSFA", "目标选择增强"));
                MiscMenu.AddItem(new MenuItem("pstyle", "目标择取")).SetValue(new StringList(new[] { "单一目标", "多重目标" }, 1));
                MiscMenu.AddItem(new MenuItem("ASDWFA", "清野增强选项"));
                MiscMenu.AddItem(new MenuItem("spcol", "  快速RQ清野(假如有碰撞)")).SetValue(false);
                MiscMenu.AddItem(new MenuItem("aareq", "-> 最大攻击数能击杀")).SetValue(new Slider(2, 1, 5));
                MiscMenu.AddItem(new MenuItem("kitejg", "  快速清野")).SetValue(false);
                MiscMenu.AddItem(new MenuItem("CJSFWA", "惩戒增强选项"));
                MiscMenu.AddItem(new MenuItem("jgsmite", "启动惩戒")).SetValue(false);
                MiscMenu.AddItem(new MenuItem("jgsmitetd", "自动Q+惩戒")).SetValue(true);
                MiscMenu.AddItem(new MenuItem("jgsmiteep", "-> Smite Epic")).SetValue(true);
                MiscMenu.AddItem(new MenuItem("jgsmitebg", "-> Smite Large")).SetValue(true);
                MiscMenu.AddItem(new MenuItem("jgsmitesm", "-> Smite Small")).SetValue(true);
                MiscMenu.AddItem(new MenuItem("jgsmitehe", "-> Smite On Hero")).SetValue(true);

                var zzz = new MenuItem("ppred", ":: Prediction");
                MiscMenu.AddItem(zzz).SetValue(new StringList(new[] { "Common", "OKTW", "SPrediction" }, 1));
                MiscMenu.AddItem(new MenuItem("ndhqch", "-> Min Hitchance")).SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3));
                MiscMenu.AddItem(new MenuItem("bbb", ":: SPrediction not Loaded Please F5!")).Show(false).SetFontStyle(FontStyle.Bold, SharpDX.Color.DeepPink);

                zzz.ValueChanged += (sender, eventArgs) =>
                {
                    Root.Item("bbb")
                        .Show(eventArgs.GetNewValue<StringList>().SelectedIndex == 2 &&
                              Root.Children.All(x => x.Name != "SPRED"));

                    if (eventArgs.GetNewValue<StringList>().SelectedIndex == 2)
                    {
                        Root.Item("ndhqch").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2));
                    }

                    if (eventArgs.GetNewValue<StringList>().SelectedIndex == 0 || eventArgs.GetNewValue<StringList>().SelectedIndex == 1)
                    {
                        Root.Item("ndhqch").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2));
                    }
                };

            }
            Root.AddSubMenu(MiscMenu);

            var DrawMenu = new Menu("显示设置", "DrawMenuMenu");
            {
                DrawMenu.AddItem(new MenuItem("dp", "显示Q范围")).SetValue(false);
                DrawMenu.AddItem(new MenuItem("dti", "显示技能时间")).SetValue(false);
                DrawMenu.AddItem(new MenuItem("dz", "触发被动W范围")).SetValue(false);
                DrawMenu.AddItem(new MenuItem("dt", "显示目标")).SetValue(false);
            }
            Root.AddSubMenu(DrawMenu);

            Root.AddToMainMenu();

            Utility.DelayAction.Add(100, () =>
            {
                if (Root.Item("ppred").GetValue<StringList>().SelectedValue == "SPrediction")
                {
                    SPrediction.Prediction.Initialize(Root);

                    if (Root.SubMenu("SPRED") != null)
                    {
                        Root.SubMenu("SPRED").DisplayName = ":: SPrediction";
                    }
                }
            });

            Game.OnUpdate += Game_OnUpdate;

            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
            Obj_AI_Base.OnDoCast += Obj_AI_Base_OnDoCast;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy || sender.Type != Player.Type || !args.SData.IsAutoAttack())
            {
                return;
            }

            foreach (var ally in Allies().Where(hero => !hero.IsMelee))
            {
                if (ally.NetworkId != sender.NetworkId || !Root.Item("xx" + ally.ChampionName).GetValue<bool>())
                {
                    return;
                }

                if (args.Target.Type == GameObjectType.obj_AI_Hero || args.Target.Type == GameObjectType.obj_AI_Turret)
                {
                    // auto heal on ally hero attacking
                    if (KL.CanUse(KL.Spells["Primalsurge"], true, "on"))
                    {
                        if (ally.IsValidTarget(KL.Spells["Primalsurge"].Range, false) &&
                            ally.Health / ally.MaxHealth * 100 <= 90)
                        {
                            if (!Player.Spellbook.IsChanneling && !Player.IsRecalling())
                            {
                                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None ||
                                    ally.Health / ally.MaxHealth * 100 <= 20 || !KL.CatForm())
                                {
                                    if (Player.Mana / Player.MaxMana * 100 <
                                        Root.Item("ndhemana").GetValue<Slider>().Value &&
                                        !(ally.Health / ally.MaxHealth * 100 <= 20))
                                        return;

                                    if (KL.CatForm() == false)
                                        KL.Spells["Primalsurge"].CastOnUnit(ally);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.IsAutoAttack())
            {
                if (Root.Item("usecombo2").GetValue<KeyBind>().Active)
                {
                    if (KL.CatForm() && KL.Spells["Aspect"].IsReady() && KL.SpellTimer["Javelin"].IsReady())
                    {
                        KL.Spells["Takedown"].Cast();

                        if (Player.HasBuff("Takedown"))
                        {
                            KL.Spells["Aspect"].Cast();
                        }
                    }

                    if (!KL.CatForm() && KL.SpellTimer["Javelin"].IsReady())
                    {
                        if (Utils.GameTimeTickCount - KL.LastBite <= 1200 || KL.SpellTimer["Javelin"].IsReady())
                        {
                            var targ = args.Target as Obj_AI_Base;
                            if (targ == null)
                            {
                                return;
                            }

                            if (targ.Path.Length < 1)
                                KL.Spells["Javelin"].Cast(targ.ServerPosition);

                            if (targ.Path.Length > 0)
                                KL.Spells["Javelin"].Cast(targ);                           
                        }
                    }
                }
            }
        }

        #region OnBuffAdd
        internal static void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            var hero = sender as Obj_AI_Hero;
            if (hero != null && hero.IsEnemy && KL.SpellTimer["Javelin"].IsReady() && Root.Item("ndhqimm").GetValue<bool>())
            {
                if (hero.IsValidTarget(KL.Spells["Javelin"].Range))
                {
                    if (args.Buff.Type == BuffType.Stun || args.Buff.Type == BuffType.Snare ||
                        args.Buff.Type == BuffType.Taunt || args.Buff.Type == BuffType.Knockback)
                    {
                        if (!KL.CatForm())
                        {
                            KL.Spells["Javelin"].Cast(hero);
                            KL.Spells["Javelin"].CastIfHitchanceEquals(hero, HitChance.Immobile);
                        }
                        else
                        {
                            if (KL.Spells["Aspect"].IsReady() &&
                                KL.Spells["Javelin"].Cast(hero) == Spell.CastStates.Collision)
                                KL.Spells["Aspect"].Cast();
                        }
                    }
                }
            }

            if (hero != null && hero.IsEnemy && KL.SpellTimer["Bushwhack"].IsReady() && Root.Item("ndhwimm").GetValue<bool>())
            {
                if (hero.IsValidTarget(KL.Spells["Bushwhack"].Range))
                {
                    if (args.Buff.Type == BuffType.Stun || args.Buff.Type == BuffType.Snare ||
                        args.Buff.Type == BuffType.Taunt || args.Buff.Type == BuffType.Knockback)
                    {
                        KL.Spells["Bushwhack"].Cast(hero);
                        KL.Spells["Bushwhack"].CastIfHitchanceEquals(hero, HitChance.Immobile);
                    }
                }
            }
        }

        #endregion
        
        #region OnDraw
        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || !Player.IsValid)
            {
                return;
            }

            foreach (
                var unit in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(900) && x.PassiveRooted()))
            {
                var b = unit.GetBuff("NidaleePassiveMonsterRoot");
                if (b.Caster.IsMe && b.EndTime - Game.Time > 0)
                {
                    var tpos = Drawing.WorldToScreen(unit.Position);
                    Drawing.DrawText(tpos[0], tpos[1], Color.DeepPink,
                        "ROOTED " + (b.EndTime - Game.Time).ToString("F"));
                }
            }

            if (Root.Item("dti").GetValue<bool>())
            {
                var pos = Drawing.WorldToScreen(Player.Position);

                Drawing.DrawText(pos[0] + 100, pos[1] - 135, Color.White,
                    "Q: " + KL.SpellTimer["Javelin"].ToString("F"));             
            }

            if (Root.Item("dt").GetValue<bool>() && Target != null)
            {
                if (Root.Item("pstyle").GetValue<StringList>().SelectedIndex == 0)
                {
                    Render.Circle.DrawCircle(Target.Position, Target.BoundingRadius, Color.DeepPink, 6);
                }
            }

            if (Root.Item("dp").GetValue<bool>() && !KL.CatForm())
            {
                Render.Circle.DrawCircle(KL.Player.Position, KL.Spells["Javelin"].Range, Color.FromArgb(155, Color.DeepPink), 4);
            }

            if (Root.Item("dz").GetValue<bool>() && KL.CatForm())
            {
                Render.Circle.DrawCircle(KL.Player.Position, KL.Spells["ExPounce"].Range, Color.FromArgb(155, Color.DeepPink), 4);
            }
        }

        #endregion

        #region Ally Heroes
        internal static IEnumerable<Obj_AI_Hero> Allies()
        {
            switch (Root.Item("ndheord").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HeroManager.Allies.OrderBy(h => h.Health / h.MaxHealth * 100);
                case 1:
                    return
                        HeroManager.Allies.OrderByDescending(h => h.BaseAttackDamage + h.FlatPhysicalDamageMod)
                            .ThenByDescending(h => h.FlatMagicDamageMod);
                case 2:
                    return HeroManager.Allies.OrderByDescending(h => h.MaxHealth);
            }

            return null;
        }

        #endregion

        private static void GameOnOnGameLoad(EventArgs args)
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

        internal static void Game_OnUpdate(EventArgs args)
        {
            Target = TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical);

            #region Active Modes

            if (Root.Item("usecombo2").GetValue<KeyBind>().Active)
            {
                Combo2();
            }

            if (Root.Item("usecombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Root.Item("useharass").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            if (Root.Item("usefarm").GetValue<KeyBind>().Active)
            {
                Clear();
            }

            if (Root.Item("flee").GetValue<KeyBind>().Active)
            {
                Flee();
            }

            #endregion

            #region Auto Heal

            // auto heal on ally hero
            if (KL.CanUse(KL.Spells["Primalsurge"], true, "on"))
            {
                if (!Player.Spellbook.IsChanneling && !Player.IsRecalling())
                {
                    if (Root.Item("flee").GetValue<KeyBind>().Active && KL.CatForm())
                        return;

                    foreach (
                        var hero in
                            Allies().Where(
                                h => Root.Item("xx" + h.ChampionName).GetValue<bool>() &&
                                        h.IsValidTarget(KL.Spells["Primalsurge"].Range, false) &&
                                        h.Health / h.MaxHealth * 100 <
                                        Root.Item("zz" + h.ChampionName).GetValue<Slider>().Value))
                    {
                        if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None ||
                            hero.Health / hero.MaxHealth * 100 <= 20 || !KL.CatForm())
                        {
                            if (Player.Mana / Player.MaxMana * 100 < Root.Item("ndhemana").GetValue<Slider>().Value &&
                                !(hero.Health / hero.MaxHealth * 100 <= 20))
                                return;

                            if (KL.CatForm() == false)
                                KL.Spells["Primalsurge"].CastOnUnit(hero);

                            if (KL.CatForm() && Root.Item("ndhesw").GetValue<bool>() &&
                                KL.SpellTimer["Primalsurge"].IsReady() &&
                                KL.Spells["Aspect"].IsReady())
                                KL.Spells["Aspect"].Cast();
                        }
                    }             
                }            
            }

            #endregion
        }

        internal static void Orb(Obj_AI_Base target)
        {
            if (target != null && target.IsHPBarRendered && target.IsEnemy)
            {
                Orbwalking.Orbwalk(target, Game.CursorPos);
            }
        }

        internal static void Combo()
        {
            var solo = Root.Item("pstyle").GetValue<StringList>().SelectedIndex == 0;

            if (!Player.IsWindingUp)
            {
                CM.CastJavelin(solo ? Target : TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical), "co");
                CM.SwitchForm(solo ? Target : TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical), "co");
            }

            if (!Root.Item("ndhwsp").GetValue<bool>())
            {
                CM.CastBushwhack(solo ? Target : TargetSelector.GetTarget(KL.Spells["Bushwhack"].Range, TargetSelector.DamageType.Magical), "co");
            }

            CM.CastTakedown(solo ? Target : TargetSelector.GetTarget(KL.Spells["Takedown"].Range, TargetSelector.DamageType.Magical), "co");
            CM.CastPounce(solo ? Target : TargetSelector.GetTarget(KL.Spells["ExPounce"].Range, TargetSelector.DamageType.Magical), "co");
            CM.CastSwipe(solo ? Target : TargetSelector.GetTarget(KL.Spells["Swipe"].Range, TargetSelector.DamageType.Magical), "co");
        }

        internal static void Combo2()
        {
            var target = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Distance(Player.ServerPosition) <= 600 
                        && x.IsEnemy && x.IsHPBarRendered
                        && !MinionManager.IsWard(x)).OrderByDescending(x => x.MaxHealth).FirstOrDefault();

            Orb(target);

            if (target == null)
            {
                return;
            }

            if (Utils.GameTimeTickCount - KL.LastR >= 500 - Game.Ping)
            {
                if (!KL.CanUse(KL.Spells["Javelin"], true, "jg") && KL.CanUse(KL.Spells["Swipe"], false, "jg"))
                {
                    if (KL.CatForm() && target.IsValidTarget(KL.Spells["Swipe"].Range))
                    {
                        KL.Spells["Swipe"].Cast(target.ServerPosition);
                    }
                }

                if (!KL.CanUse(KL.Spells["Javelin"], true, "jg") &&
                    KL.CanUse(KL.Spells["Bushwhack"], false, "jg"))
                {
                    if (!KL.CatForm() && target.IsValidTarget(KL.Spells["Bushwhack"].Range) && KL.Player.ManaPercent > 40)
                    {
                        KL.Spells["Bushwhack"].Cast(target.ServerPosition);
                    }
                }

                if (!KL.CanUse(KL.Spells["Javelin"], true, "jg") && KL.CanUse(KL.Spells["Pounce"], false, "jg"))
                {
                    var r = target.IsHunted() ? KL.Spells["ExPounce"].Range : KL.Spells["Pounce"].Range;
                    if (KL.CatForm() && target.IsValidTarget(r))
                    {
                        KL.Spells["Pounce"].Cast(target.ServerPosition);
                    }
                }
            }

            if (KL.Spells["Takedown"].Level > 0 && KL.SpellTimer["Takedown"].IsReady() && !KL.CatForm())
            {
                if (KL.Spells["Aspect"].IsReady())
                {
                    KL.Spells["Aspect"].Cast();
                }
            }

            if (KL.Spells["Javelin"].Level > 0 && !KL.SpellTimer["Javelin"].IsReady() && !KL.CatForm())
            {
                if (KL.Spells["Aspect"].IsReady())
                {
                    KL.Spells["Aspect"].Cast();
                }
            }
        }

        internal static void Harass()
        {
            CM.CastJavelin(TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical), "ha");
            CM.CastTakedown(TargetSelector.GetTarget(KL.Spells["Takedown"].Range, TargetSelector.DamageType.Magical), "ha");
            CM.CastSwipe(TargetSelector.GetTarget(KL.Spells["Swipe"].Range, TargetSelector.DamageType.Magical), "ha");
            CM.SwitchForm(TargetSelector.GetTarget(KL.Spells["Javelin"].Range, TargetSelector.DamageType.Magical), "ha");
        }

        internal static bool m;
        internal static void Clear()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, 
                750f, MinionTypes.All, MinionTeam.All, MinionOrderTypes.MaxHealth);

            m = minions.Any(KL.IsJungleMinion);

            foreach (var unit in minions.OrderByDescending(KL.IsJungleMinion))
            {
                switch (unit.Team)
                {
                    case GameObjectTeam.Neutral:
                        if (!unit.Name.Contains("Mini"))
                        {
                            CM.CastJavelin(unit, "jg");
                            CM.CastBushwhack(unit, "jg");
                        }

                        CM.CastPounce(unit, "jg");
                        CM.CastTakedown(unit, "jg");
                        CM.CastSwipe(unit, "jg");

                        if (unit.PassiveRooted() && Root.Item("jgaacount").GetValue<KeyBind>().Active &&
                            Player.Distance(unit.ServerPosition) > 450)
                        {
                            return;
                        }

                        CM.SwitchForm(unit, "jg");
                        break;
                    default:
                        if (unit.Team != Player.Team && unit.Team != GameObjectTeam.Neutral)
                        {
                            CM.CastJavelin(unit, "wc");
                            CM.CastPounce(unit, "wc");
                            CM.CastBushwhack(unit, "wc");
                            CM.CastTakedown(unit, "wc");
                            CM.CastSwipe(unit, "wc");
                            CM.SwitchForm(unit, "wc");
                        }
                        break;
                }
            }

        }

        #region Walljumper @Hellsing
        internal static void Flee()
        {
            if (!KL.CatForm() && KL.Spells["Aspect"].IsReady())
            {
                if (KL.SpellTimer["Pounce"].IsReady())
                    KL.Spells["Aspect"].Cast();
            }

            var wallCheck = KL.GetFirstWallPoint(KL.Player.Position, Game.CursorPos);

            if (wallCheck != null)
                wallCheck = KL.GetFirstWallPoint((Vector3) wallCheck, Game.CursorPos, 5);

            var movePosition = wallCheck != null ? (Vector3) wallCheck : Game.CursorPos;

            var tempGrid = NavMesh.WorldToGrid(movePosition.X, movePosition.Y);
            var fleeTargetPosition = NavMesh.GridToWorld((short) tempGrid.X, (short)tempGrid.Y);

            Obj_AI_Base target = null;

            var wallJumpPossible = false;

            if (KL.CatForm() && KL.SpellTimer["Pounce"].IsReady() && wallCheck != null)
            {
                var wallPosition = movePosition;

                var direction = (Game.CursorPos.To2D() - wallPosition.To2D()).Normalized();
                float maxAngle = 80f;
                float step = maxAngle / 20;
                float currentAngle = 0;
                float currentStep = 0;
                bool jumpTriggered = false;

                while (true)
                {
                    if (currentStep > maxAngle && currentAngle < 0)
                        break;

                    if ((currentAngle == 0 || currentAngle < 0) && currentStep != 0)
                    {
                        currentAngle = (currentStep) * (float)Math.PI / 180;
                        currentStep += step;
                    }

                    else if (currentAngle > 0)
                        currentAngle = -currentAngle;

                    Vector3 checkPoint;

                    if (currentStep == 0)
                    {
                        currentStep = step;
                        checkPoint = wallPosition + KL.Spells["Pounce"].Range * direction.To3D();
                    }

                    else
                        checkPoint = wallPosition + KL.Spells["Pounce"].Range * direction.Rotated(currentAngle).To3D();

                    if (checkPoint.IsWall()) 
                        continue;

                    wallCheck = KL.GetFirstWallPoint(checkPoint, wallPosition);

                    if (wallCheck == null) 
                        continue;

                    var wallPositionOpposite =  (Vector3) KL.GetFirstWallPoint((Vector3)wallCheck, wallPosition, 5);

                    if (KL.Player.GetPath(wallPositionOpposite).ToList().To2D().PathLength() -
                        KL.Player.Distance(wallPositionOpposite) > 200)
                    {
                        if (KL.Player.Distance(wallPositionOpposite) < KL.Spells["Pounce"].Range - KL.Player.BoundingRadius / 2)
                        {
                            KL.Spells["Pounce"].Cast(wallPositionOpposite);
                            jumpTriggered = true;
                            break;
                        }

                        else
                            wallJumpPossible = true;
                    }

                    else
                    {
                        Render.Circle.DrawCircle(Game.CursorPos, 35, Color.Red, 2);
                    }
                }

                if (!jumpTriggered)
                {
                    Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 35f);
                }
            }

            else
            {
                Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 35f);
                if (KL.CatForm() && KL.SpellTimer["Pounce"].IsReady())
                    KL.Spells["Pounce"].Cast(Game.CursorPos);
            }
        }

        #endregion
    }
}
