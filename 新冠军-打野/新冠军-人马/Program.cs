namespace YuLeHecarim
{
    using LeagueSharp;
    using LeagueSharp.Data.Enumerations;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using SharpDX;
    using System;
    using System.Linq;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;
    internal class Program
    {
        private static Obj_AI_Hero Me;
        private static Menu Menu;
        private static Spell Q, W, E, R;
        private static SpellSlot Ignite = SpellSlot.Unknown;
        private static HpBarDraw DrawHpBar = new HpBarDraw();
        private static Spell Smite;
        private static string[] AutoEnableList =
        {
             "Annie", "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana", "Evelynn", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus",
             "Kassadin", "Katarina", "Kayle", "Kennen", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna",
             "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz", "Azir", "Ekko",
             "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jayce", "Jinx", "KogMaw", "Lucian", "MasterYi", "MissFortune", "Quinn", "Shaco", "Sivir",
             "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Yasuo", "Zed", "Kindred", "AurelionSol"
        };

        public static int GetChallengingSmiteDamage => 54 + 6 * GameObjects.Player.Level;

        static void Main(string[] Args)
        {
            Bootstrap.Init(Args);
            Events.OnLoad += Events_OnLoad;
        }

        private static void Events_OnLoad()
        {
            if (GameObjects.Player.ChampionName.ToLower() != "hecarim")
            {
                return;
            }

            Me = GameObjects.Player;

            Variables.Orbwalker.Enabled = true;
            LoadSpell();
            LoadMenu();
            LoadEvent();

            DelayAction.Add(10000, () => Variables.Orbwalker.Enabled = true);
        }

        private static void LoadSpell()
        {
            Q = new Spell(SpellSlot.Q, 350f);
            W = new Spell(SpellSlot.W, 525f);
            E = new Spell(SpellSlot.E, 1000f);//Cast E First Search Range
            R = new Spell(SpellSlot.R, 1230f);

            R.SetSkillshot(0.25f, 300f, 1200f, false, SkillshotType.SkillshotCircle);

            Ignite = Me.GetSpellSlot("SummonerDot");
            Smite = new Spell(GetSmiteSlot(), 500f + GameObjects.Player.BoundingRadius);
        }

        private static void LoadMenu()
        {
            Menu = new Menu("VIP人马Q群438230879", "VIP人马Q群438230879", true).Attach();

            var ComboMenu = Menu.Add(new Menu("Combo", "连招设置"));
            {
                ComboMenu.Add(new MenuBool("Q", "Use Q", true));
                ComboMenu.Add(new MenuBool("W", "Use W", true));
                ComboMenu.Add(new MenuBool("E", "Use E", true));
                ComboMenu.Add(new MenuBool("R", "Use R", true));
                ComboMenu.Add(new MenuBool("RSolo", "单挑R（对面半血以下直接R）", true));
                ComboMenu.Add(new MenuSlider("RCount", "连招R敌人数量 >=", 2, 1, 5));
                ComboMenu.Add(new MenuBool("Ignite", "Use Ignite", true));
                ComboMenu.Add(new MenuBool("Item", "Use Item", true));
                ComboMenu.Add(new MenuBool("Smite", "使用惩戒", true));
            }

            var HarassMenu = Menu.Add(new Menu("Harass", "骚扰设置"));
            {
                HarassMenu.Add(new MenuBool("Q", "Use Q", true));
                HarassMenu.Add(new MenuSlider("Mana", "骚扰蓝量＞", 60));
                HarassMenu.Add(new MenuKeyBind("Auto", "自动Q骚扰", System.Windows.Forms.Keys.G, KeyBindType.Toggle));
                HarassMenu.Add(new MenuSlider("AutoMana", "自动骚扰 >= %", 60));
            }

            var LaneClearMenu = Menu.Add(new Menu("LaneClear", "清线设置"));
            {
                LaneClearMenu.Add(new MenuSliderButton("Q", "使用Q最小击中数 >= ", 3, 1, 5, true));
                LaneClearMenu.Add(new MenuSliderButton("W", "使用W最小击中数 >= ", 3, 1, 5, true));
                LaneClearMenu.Add(new MenuSlider("Mana", "清线最小蓝量 >= %", 60));
            }

            var JungleClearMenu = Menu.Add(new Menu("JungleClear", "打野设置"));
            {
                JungleClearMenu.Add(new MenuBool("Q", "Use Q", true));
                JungleClearMenu.Add(new MenuBool("W", "Use W", true));
                JungleClearMenu.Add(new MenuBool("Item", "Use Item", true));
                JungleClearMenu.Add(new MenuSlider("Mana", "打野最小蓝量 >= %", 20));
            }

            var KillStealMenu = Menu.Add(new Menu("KillSteal", "击杀设置"));
            {
                KillStealMenu.Add(new MenuBool("Q", "Use Q", true));
                KillStealMenu.Add(new MenuBool("W", "Use W", true));
                KillStealMenu.Add(new MenuSliderButton("R", "使用R击杀如果目标距离 >=",600, 100, (int)R.Range, true));
                KillStealMenu.Add(new MenuSeparator("RList", "R 击杀名单"));
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => KillStealMenu.Add(new MenuBool(i.ChampionName.ToLower(), i.ChampionName, AutoEnableList.Contains(i.ChampionName))));
                }
            }

            var FleeMenu = Menu.Add(new Menu("Flee", "逃跑设置"));
            {
                FleeMenu.Add(new MenuBool("W", "Use W", true));
                FleeMenu.Add(new MenuBool("E", "Use E", true));
                FleeMenu.Add(new MenuKeyBind("Key", "按键", System.Windows.Forms.Keys.Z, KeyBindType.Press));
            }

            var ItemMenu = Menu.Add(new Menu("Items", "使用道具"));
            {
                ItemMenu.Add(new MenuBool("Youmuus", "使用幽梦", true));
                ItemMenu.Add(new MenuBool("Cutlass", "使用弯刀", true));
                ItemMenu.Add(new MenuBool("Botrk", "使用海克斯", true));
                ItemMenu.Add(new MenuBool("Hydra", "使用九头蛇", true));
                ItemMenu.Add(new MenuBool("Tiamat", "使用提亚马特", true));
            }

            var SkinMenu = Menu.Add(new Menu("Skin", "更换皮肤"));
            {
                SkinMenu.Add(new MenuBool("Enable", "Enabled", false));
                SkinMenu.Add(new MenuList<string>("SkinName", "选择皮肤", new[] { "经典", "血色骑士", "幽魂骑士", "无头骑士", "电玩战魂", "长者之森" }));
            }

            new AutoWard(Menu);

            var MiscMenu = Menu.Add(new Menu("Misc", "其他设置"));
            {
                MiscMenu.Add(new MenuBool("EGap", "使用E反突进", true));
                MiscMenu.Add(new MenuBool("EInt", "使用E打断技能", true));
                MiscMenu.Add(new MenuBool("RInt", "使用R打断技能", true));
            }

            var DrawMenu = Menu.Add(new Menu("Draw", "显示范围"));
            {
                DrawMenu.Add(new MenuBool("Q", "Q Range"));
                DrawMenu.Add(new MenuBool("W", "W Range"));
                DrawMenu.Add(new MenuBool("E", "E Range"));
                DrawMenu.Add(new MenuBool("R", "R Range"));
                DrawMenu.Add(new MenuBool("DrawDamage", "显示连招伤害", true));
                DrawMenu.Add(new MenuBool("Auto", "显示自动堆Q状态", true));
            }
        }

        private static void LoadEvent()
        {
            Variables.Orbwalker.OnAction += OnAction;
            Events.OnInterruptableTarget += OnInterruptableTarget;
            Events.OnGapCloser += OnGapCloser;
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdate;
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

        private static SpellSlot GetSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(s => s.Name.ToLower().Contains("smite")))
            {
                return spell.Slot;
            }

            return SpellSlot.Unknown;
        }

        private static void OnAction(object obj, OrbwalkingActionArgs Args)
        {
            if (Args.Type == OrbwalkingType.AfterAttack)
            {
                if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo)
                {
                    var target = Variables.TargetSelector.GetTarget(R.Range, DamageType.Physical);

                    if (target != null && target.IsHPBarRendered)
                    {
                        if (Menu["Combo"]["W"] && Q.IsReady() && target.IsValidTarget(W.Range) && AutoAttack.IsAutoAttack(Me.ChampionName))
                        {
                            W.Cast();
                        }

                        Item(target);
                    }
                }

                if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.LaneClear)
                {
                    var Mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(W.Range) && !GameObjects.JungleSmall.Contains(x)).ToList();

                    if (Mobs.Count() > 0)
                    {
                        var mob = Mobs.FirstOrDefault();

                        if (Me.ManaPercent >= Menu["JungleClear"]["Mana"].GetValue<MenuSlider>().Value)
                        {
                            if (Menu["JungleClear"]["W"] && W.IsReady() && mob.IsValidTarget(W.Range) && AutoAttack.IsAutoAttack(Me.ChampionName))
                            {
                                W.Cast();
                            }
                        }

                        if (Menu["JungleClear"]["Item"])
                        {
                            if (Menu["Items"]["Hydra"] && Items.HasItem(3074) && Mobs.FirstOrDefault().IsValidTarget(AttackRange()))
                            {
                                Items.UseItem(3074, mob);
                            }

                            if (Menu["Items"]["Tiamat"] && Items.HasItem(3077) && Mobs.FirstOrDefault().IsValidTarget(AttackRange()))
                            {
                                Items.UseItem(3077, mob);
                            }
                        }
                    }
                }
            }
        }

        private static void OnInterruptableTarget(object obj, Events.InterruptableTargetEventArgs Args)
        {
            if (Args.Sender.IsEnemy)
            {
                var sender = Args.Sender as Obj_AI_Hero;

                if (Menu["Misc"]["EInt"] && Args.DangerLevel >= DangerLevel.Medium && sender.IsValidTarget(E.Range) && E.IsReady())
                {
                    E.Cast();
                    Variables.Orbwalker.ForceTarget = sender;
                    return;
                }

                if (Menu["Misc"]["RInt"] && Args.DangerLevel >= DangerLevel.High && sender.IsValidTarget(R.Range) && R.IsReady())
                {
                    R.Cast(sender);
                    return;
                }
            }
        }

        private static void OnGapCloser(object obj, Events.GapCloserEventArgs Args)
        {
            if (Menu["Misc"]["EGap"] && Args.IsDirectedToPlayer)
            {
                var sender = Args.Sender as Obj_AI_Hero;

                if (sender.IsEnemy && (Args.End.DistanceToPlayer() <= 200 || sender.DistanceToPlayer() <= 250) && E.IsReady())
                {
                    E.Cast();
                    Variables.Orbwalker.ForceTarget = sender;
                    return;
                }
            }
        }

        private static float GetRemainingTime()
        {
            var buff = Me.GetBuff("hecarimrampspeed");

            if (E.IsReady() && buff == null)
                return 4.0f;

            if (buff == null)
                return 0;

            return buff.EndTime - Game.Time;
        }

        private static void OnDraw(EventArgs Args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Menu["Draw"]["Q"] && Q.IsReady())
                Render.Circle.DrawCircle(Me.Position, Q.Range, System.Drawing.Color.AliceBlue, 2);

            if (Menu["Draw"]["W"] && (W.IsReady() || Me.HasBuff("HecarimW")))
                Render.Circle.DrawCircle(Me.Position, W.Range, System.Drawing.Color.LightSeaGreen, 2);

            if (Menu["Draw"]["E"])
            {
                var stealthTime = GetRemainingTime();

                if (stealthTime > 0)
                {
                    Render.Circle.DrawCircle(Me.Position, stealthTime * Me.MoveSpeed, System.Drawing.Color.GreenYellow);
                }
            }

            if (Menu["Draw"]["R"] && R.IsReady())
                Render.Circle.DrawCircle(Me.Position, R.Range, System.Drawing.Color.OrangeRed, 2);

            if (Menu["Draw"]["DrawDamage"])
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget() && e.IsValid && !e.IsDead && !e.IsZombie))
                {
                    HpBarDraw.Unit = target;
                    HpBarDraw.DrawDmg(GetDamage(target), new ColorBGRA(255, 204, 0, 170));
                }
            }

            if (Menu["Draw"]["Auto"].GetValue<MenuBool>())
            {
                var text = "";

                if (Menu["Harass"]["Auto"].GetValue<MenuKeyBind>().Active)
                {
                    text = "On";
                }
                else
                {
                    text = "Off";
                }

                Drawing.DrawText(Me.HPBarPosition.X + 30, Me.HPBarPosition.Y - 40, System.Drawing.Color.Red, "Auto Q (" + Menu["Harass"]["Auto"].GetValue<MenuKeyBind>().Key + "): ");
                Drawing.DrawText(Me.HPBarPosition.X + 115, Me.HPBarPosition.Y - 40, System.Drawing.Color.Yellow, text);
            }
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo)
            {
                Combo();
            }

            if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.Hybrid)
            {
                Harass();
            }

            if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.LaneClear)
            {
                Lane();
                Jungle();
            }

            if (Menu["Flee"]["Key"].GetValue<MenuKeyBind>().Active)
            {
                Flee();
            }

            if (Menu["Harass"]["Auto"].GetValue<MenuKeyBind>().Active)
            {
                AutoHarass();
            }

            KillSteal();
            SmiteLogic();
            Skin();
        }

        private static void SmiteLogic()
        {
            if (Smite.IsReady() && Smite.Slot != SpellSlot.Unknown)
            {
                if (Menu["Combo"]["Smite"] && Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo)
                {
                    if (Variables.Orbwalker.GetTarget() as Obj_AI_Hero != null)
                    {
                        Smite.CastOnUnit(Variables.Orbwalker.GetTarget() as Obj_AI_Hero);
                    }
                }

                if (GameObjects.Player.HasBuff("smitedamagetrackerstalker") || GameObjects.Player.HasBuff("smitedamagetrackerskirmisher"))
                {
                    if (GameObjects.Player.Spellbook.GetSpell(Smite.Slot).Ammo == 1)
                    {
                        return;
                    }

                    foreach (var target in GameObjects.EnemyHeroes.Where(t => t.IsValidTarget(Smite.Range)))
                    {
                        if (GetChallengingSmiteDamage > target.Health && GameObjects.Player.HasBuff("smitedamagetrackerstalker"))
                        {
                            Smite.CastOnUnit(target);
                        }
                        else if (GetChallengingSmiteDamage > target.Health && GameObjects.Player.HasBuff("smitedamagetrackerskirmisher"))
                        {
                            Smite.CastOnUnit(target);
                        }
                    }
                }
            }
        }

        private static void Flee()
        {
            Variables.Orbwalker.Move(Game.CursorPos);

            if (Menu["Flee"]["E"] && E.IsReady())
            {
                E.Cast();
            }

            if (Menu["Flee"]["W"] && W.IsReady())
            {
                var target = Variables.TargetSelector.GetTarget(W.Range, DamageType.Physical);

                if (target != null && !target.IsDead && !target.IsZombie && target.IsHPBarRendered && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        }

        private static void Combo()
        {
            var target = Variables.TargetSelector.GetTarget(R.Range, DamageType.Physical);

            if (target != null && target.IsHPBarRendered)
            {
                if (Menu["Combo"]["Q"] && Q.IsReady() && target.IsValidTarget(Q.Range) && Q.CanCast(target))
                {
                    Q.Cast(target);
                }

                if (Menu["Combo"]["W"] && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }

                if (Menu["Combo"]["E"] && E.IsReady() && E.CanCast(target) && !InAutoAttackRange(target) && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }

                if (Menu["Combo"]["R"] && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (Menu["Combo"]["RSolo"] && Me.CountEnemyHeroesInRange(R.Range) == 1 && target.Health <= GetDamage(target) + Me.GetAutoAttackDamage(target) * 3) 
                    {
                        R.Cast(target);
                    }

                    if (R.GetPrediction(target).CastPosition.CountEnemyHeroesInRange(250) >= Menu["Combo"]["RCount"].GetValue<MenuSlider>().Value)
                    {
                        R.Cast(R.GetPrediction(target).CastPosition);
                    }
                }

                Item(target);

                if (Menu["Combo"]["Ignite"] && Ignite != SpellSlot.Unknown && Ignite.IsReady() && target.IsValidTarget(600) && target.HealthPercent < 20)
                {
                    Me.Spellbook.CastSpell(Ignite, target);
                }
            }
        }

        private static void Harass()
        {
            if (Me.ManaPercent >= Menu["Harass"]["Mana"].GetValue<MenuSlider>().Value)
            {
                var target = Variables.TargetSelector.GetTarget(Q.Range, DamageType.Physical);

                if (target != null && !target.IsDead && !target.IsZombie && target.IsHPBarRendered)
                {
                    if (Menu["Harass"]["Q"] && Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        Q.Cast();
                    }
                }
            }
        }

        private static void AutoHarass()
        {
            if (!Me.IsRecalling() && !Me.IsUnderEnemyTurret() && Variables.Orbwalker.ActiveMode != OrbwalkingMode.Combo && Variables.Orbwalker.ActiveMode != OrbwalkingMode.Hybrid &&Me.ManaPercent >= Menu["Harass"]["AutoMana"].GetValue<MenuSlider>().Value)
            {
                var target = Variables.TargetSelector.GetTarget(Q.Range, DamageType.Physical);

                if (target != null && !target.IsDead && !target.IsZombie && target.IsHPBarRendered)
                {
                    if (Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        Q.Cast();
                    }
                }
            }
        }

        private static void Lane()
        {
            var Minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(W.Range)).ToList();

            if (Minions.Count() > 0)
            {
                if (Me.ManaPercent >= Menu["LaneClear"]["Mana"].GetValue<MenuSlider>().Value)
                {
                    if (Menu["LaneClear"]["Q"].GetValue<MenuSliderButton>().BValue && Q.IsReady())
                    {
                        if (Minions.Where(x => x.IsValidTarget(Q.Range)).Count() >= Menu["LaneClear"]["Q"].GetValue<MenuSliderButton>().SValue)
                        {
                            Q.Cast();
                        }
                    }

                    if (Menu["LaneClear"]["W"].GetValue<MenuSliderButton>().BValue && W.IsReady())
                    {
                        if (Minions.Where(x => x.IsValidTarget(W.Range)).Count() >= Menu["LaneClear"]["W"].GetValue<MenuSliderButton>().SValue)
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        private static void Jungle()
        {
            var Mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(W.Range) && !GameObjects.JungleSmall.Contains(x)).ToList();

            if (Mobs.Count() > 0)
            {
                var mob = Mobs.FirstOrDefault();

                if (Me.ManaPercent >= Menu["JungleClear"]["Mana"].GetValue<MenuSlider>().Value)
                {
                    if (Menu["JungleClear"]["Q"] && Q.IsReady() && mob.IsValidTarget(Q.Range))
                    {
                        Q.Cast();
                    }

                    if (Menu["JungleClear"]["W"] && W.IsReady() && mob.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }
                }

                if (Menu["JungleClear"]["Item"])
                {
                    if (Menu["Items"]["Hydra"] && Items.HasItem(3074) && Mobs.FirstOrDefault().IsValidTarget(AttackRange()))
                    {
                        Items.UseItem(3074, mob);
                    }

                    if (Menu["Items"]["Tiamat"] && Items.HasItem(3077) && Mobs.FirstOrDefault().IsValidTarget(AttackRange()))
                    {
                        Items.UseItem(3077, mob);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            if (Menu["KillSteal"]["Q"] && Q.IsReady())
            {
                var qt = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && x.Health < Q.GetDamage(x)).FirstOrDefault();

                if (qt != null && qt.IsHPBarRendered)
                {
                    Q.Cast();
                    return;
                }
            }

            if (Menu["KillSteal"]["W"] && W.IsReady())
            {
                var wt = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range) && x.Health < W.GetDamage(x)).FirstOrDefault();

                if (wt != null && wt.IsHPBarRendered)
                {
                    W.Cast();
                    return;
                }
            }

            if (Menu["KillSteal"]["R"].GetValue<MenuSliderButton>().BValue && R.IsReady())
            {
                var rt = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range) && x.Health < R.GetDamage(x) && Menu["KillSteal"][x.ChampionName.ToLower()] && x.DistanceToPlayer() >= Menu["KillSteal"]["R"].GetValue<MenuSliderButton>().SValue).FirstOrDefault();

                if (rt != null && rt.IsHPBarRendered)
                {
                    R.Cast(rt);
                    return;
                }
            }
        }

        private static void Skin()
        {
            if (Menu["Skin"]["Enable"])
            {
                Me.SetSkin(Me.CharData.BaseSkinName, Menu["Skin"]["SkinName"].GetValue<MenuList>().Index);
            }
            else if (!Menu["Skin"]["Enable"])
            {
                Me.SetSkin(Me.CharData.BaseSkinName, 0);
            }
        }

        private static void Item(Obj_AI_Hero target)
        {
            if (target != null && target.IsHPBarRendered && Menu["Combo"]["Item"] && Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo)
            {
                if (Menu["Items"]["Youmuus"] && Items.HasItem(3142) && target.IsValidTarget(AttackRange() + 150))
                {
                    Items.UseItem(3142);
                }

                if (Menu["Items"]["Cutlass"] && Items.HasItem(3144) && target.IsValidTarget(AttackRange()))
                {
                    Items.UseItem(3144, target);
                }

                if (Menu["Items"]["Botrk"] && Items.HasItem(3153) && target.IsValidTarget(AttackRange()))
                {
                    Items.UseItem(3153, target);
                }

                if (Menu["Items"]["Hydra"] && Items.HasItem(3074) && target.IsValidTarget(AttackRange()))
                {
                    Items.UseItem(3074, target);
                }

                if (Menu["Items"]["Tiamat"] && Items.HasItem(3077) && target.IsValidTarget(AttackRange()))
                {
                    Items.UseItem(3077, target);
                }
            }
        }

        private static float GetDamage(Obj_AI_Hero target)
        {
            float Damage = 0f;

            if (Q.IsReady())
            {
                Damage += Q.GetDamage(target);
            }

            if (W.IsReady())
            {
                Damage += W.GetDamage(target);
            }

            if (E.IsReady())
            {
                Damage += E.GetDamage(target);
            }

            if (R.IsReady())
            {
                Damage += R.GetDamage(target);
            }

            return Damage;
        }

        private static float AttackRange()
        {
            return Me.GetRealAutoAttackRange();
        }

        private static bool InAutoAttackRange(AttackableUnit target)
        {
            var baseTarget = (Obj_AI_Base)target;
            var myRange = AttackRange();

            if (baseTarget != null)
            {
                return baseTarget.IsHPBarRendered && Vector2.DistanceSquared(baseTarget.ServerPosition.ToVector2(), Me.ServerPosition.ToVector2()) <= myRange * myRange;
            }

            return target.IsValidTarget() && Vector2.DistanceSquared(target.Position.ToVector2(), Me.ServerPosition.ToVector2()) <= myRange * myRange;
        }
    }
}
