namespace YuLeGragas
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using Color = System.Drawing.Color;
    using YuLeLibrary;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class Program
    {
        public const string ChampName = "Gragas";
        public static HpBarIndicator Hpi = new HpBarIndicator();
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        private static SpellSlot Flash = SpellSlot.Unknown;

        public static Vector3 insecpos;
        public static Vector3 eqpos;
        public static Vector3 movingawaypos;
        public static GameObject Barrel;
        public static SpellSlot Ignite;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;
        private static Spell Smite;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            if (player.ChampionName != ChampName)
                return;

            Q = new Spell(SpellSlot.Q, 775);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 675);
            R = new Spell(SpellSlot.R, 1100);

            Q.SetSkillshot(0.3f, 110f, 1000f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.0f, 50, 1000, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.3f, 700, 1000, false, SkillshotType.SkillshotCircle);

            Flash = player.GetSpellSlot("SummonerFlash");

            Smite = new Spell(GetSmiteSlot(), 500f + ObjectManager.Player.BoundingRadius);


            Config = new Menu("新冠军酒桶Q群438230879", "新冠军酒桶Q群438230879", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Chartreuse);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走砍设置", "Orbwalker")));

            Config.SubMenu("连招设置").AddItem(new MenuItem("UseSmite", "Use Smite").SetValue(true));
            Config.SubMenu("连招设置").AddItem(new MenuItem("UseQ", "使用Q").SetValue(true));
            Config.SubMenu("连招设置").AddItem(new MenuItem("autoQ", "Auto Detonate Q").SetValue(true));
            Config.SubMenu("连招设置").AddItem(new MenuItem("UseW", "Use W - Drunken Rage ").SetValue(true));
            Config.SubMenu("连招设置").AddItem(new MenuItem("UseE", "Use E - Body Slam").SetValue(true));
            Config.SubMenu("连招设置").AddItem(new MenuItem("UseR", "使用R抢人头").SetValue(true));
            Config.SubMenu("连招设置").AddItem(new MenuItem("UseRprotector", "使用R保护自己人").SetValue(false));
            Config.SubMenu("连招设置").AddItem(new MenuItem("Rhp", "自己人的生命值").SetValue(new Slider(35, 100, 0)));
            Config.SubMenu("连招设置").AddItem(new MenuItem("UseRdmg", "使用R炸多人").SetValue(false));
            Config.SubMenu("连招设置").AddItem(new MenuItem("rdmgslider", "Enemy Count").SetValue(new Slider(3, 5, 1)));
            Config.SubMenu("连招设置").AddItem(new MenuItem("InsecMode", "回旋踢（把敌人R回来）").SetValue(new KeyBind('T', KeyBindType.Press)));
            Config.SubMenu("连招设置").AddItem(new MenuItem("OnlySelect", "仅回旋踢已选择目标").SetValue(false));

            Config.SubMenu("骚扰设置").AddItem(new MenuItem("harassQ", "使用Q").SetValue(true));
            Config.SubMenu("骚扰设置").AddItem(new MenuItem("harassE", "使用E").SetValue(true));
            Config.SubMenu("骚扰设置").AddItem(new MenuItem("harassmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            Config.SubMenu("清线设置").AddItem(new MenuItem("laneQ", "使用Q").SetValue(true));
            Config.SubMenu("清线设置").AddItem(new MenuItem("jungleW", "Use W - Drunken Rage").SetValue(true));
            Config.SubMenu("清线设置").AddItem(new MenuItem("laneE", "使用E").SetValue(true));
            Config.SubMenu("清线设置").AddItem(new MenuItem("laneclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            Config.SubMenu("清野设置").AddItem(new MenuItem("jungleQ", "使用Q").SetValue(true));
            Config.SubMenu("清野设置").AddItem(new MenuItem("jungleW", "Use W - Drunken Rage").SetValue(true));
            Config.SubMenu("清野设置").AddItem(new MenuItem("jungleE", "使用E").SetValue(true));
            Config.SubMenu("清野设置").AddItem(new MenuItem("jungleclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            Config.SubMenu("击杀设置").AddItem(new MenuItem("SmartKS", "Use SmartKS").SetValue(true));
            Config.SubMenu("击杀设置").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));
            Config.SubMenu("击杀设置").AddItem(new MenuItem("KSQ", "Use Q").SetValue(true));
            Config.SubMenu("击杀设置").AddItem(new MenuItem("KSE", "Use E").SetValue(true));
            Config.SubMenu("击杀设置").AddItem(new MenuItem("RKS", "Use R").SetValue(true));

            Config.SubMenu("逃跑设置").AddItem(new MenuItem("FleeKey", "逃跑按键").SetValue(new KeyBind('Z', KeyBindType.Press)));
            Config.SubMenu("逃跑设置").AddItem(new MenuItem("FleeQ", "Use Q").SetValue(true));
            Config.SubMenu("逃跑设置").AddItem(new MenuItem("FleeE", "Use E").SetValue(true));

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" })));
            }

            Config.SubMenu("杂项设置").AddItem(new MenuItem("AntiGapE", "Use E on Gapclosers").SetValue(true));
            Config.SubMenu("杂项设置").AddItem(new MenuItem("AntiGapR", "Use R on Gapclosers").SetValue(false));
            Config.SubMenu("杂项设置").AddItem(new MenuItem("EInterrupt", "Use E to Interrupt Spells").SetValue(true));
            Config.SubMenu("杂项设置").AddItem(new MenuItem("RInterrupt", "Use R to Interrupt Spells").SetValue(false));
            Config.SubMenu("杂项设置").AddItem(new MenuItem("FlashE", "E闪按键").SetValue(new KeyBind('A', KeyBindType.Press)));

            Config.SubMenu("显示设置").AddItem(new MenuItem("Draw Insec Position", "Draw Insec Position").SetValue(true));
            Config.SubMenu("显示设置").AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, Color.Orange)));
            Config.SubMenu("显示设置").AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(new Circle(true, Color.DarkOrange)));
            Config.SubMenu("显示设置").AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, Color.AntiqueWhite)));
            Config.SubMenu("显示设置").AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(new Circle(true, Color.LawnGreen)));
            Config.SubMenu("显示设置").AddItem(new MenuItem("Rrdy", "Draw R - Status").SetValue(true));
            Config.SubMenu("显示设置").AddItem(new MenuItem("DrawD", "Damage Indicator").SetValue(true));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
            GameObject.OnCreate += GragasObject;
            GameObject.OnDelete += GragasBarrelNull;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapCloser_OnEnemyGapcloser;
            Drawing.OnDraw += etcdraw;
        }

        private static SpellSlot GetSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(s => s.Name.ToLower().Contains("smite")))
            {
                return spell.Slot;
            }

            return SpellSlot.Unknown;
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;

            return (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }


        private static void etcdraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var starget = TargetSelector.GetSelectedTarget();
            var epos = Drawing.WorldToScreen(starget.Position);

            if (Config.Item("Draw Insec Position").GetValue<bool>() && R.IsReady() && starget.IsValidTarget(R.Range) && R.Level > 0)
                Drawing.DrawText(epos.X, epos.Y, Color.DarkSeaGreen, "Insec Target");

            if (Config.Item("Draw Insec Position").GetValue<bool>() && R.IsReady() && starget.IsValidTarget(R.Range) && R.Level > 0)
                Render.Circle.DrawCircle(target.Position, 150, Color.LightSeaGreen);

            if (Config.Item("Draw Insec Position").GetValue<bool>() && R.IsReady() && starget.IsValidTarget(R.Range) && R.Level > 0)
            {
                insecpos = player.Position.Extend(target.Position, player.Distance(target) + 150);
                Render.Circle.DrawCircle(insecpos, 100, Color.GreenYellow);
            }
        }

        private static void AntiGapCloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && Config.Item("AntiGapE").GetValue<bool>() && E.GetPrediction(gapcloser.Sender).Hitchance >= HitChance.High)
                E.Cast(gapcloser.Sender);

            if (R.IsReady() && gapcloser.Sender.IsValidTarget(R.Range) && Config.Item("AntiGapR").GetValue<bool>())
                R.Cast(gapcloser.Sender);
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (R.IsReady() && sender.IsValidTarget(R.Range) && Config.Item("interrupt").GetValue<bool>())
                R.CastIfHitchanceEquals(sender, HitChance.High);

            if (E.IsReady() && sender.IsValidTarget(E.Range) && Config.Item("interrupt").GetValue<bool>())
                E.CastIfHitchanceEquals(sender, HitChance.High);       
        }

        private static void GragasBarrelNull(GameObject sender, EventArgs args) //BARREL LOCATION - GONE
        {
            if (sender.Name == "Gragas_Base_Q_Ally.troy")
            {
                Barrel = null;
            }
        }

        private static void Killsteal()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range)).Where(x => !x.IsZombie).Where(x => !x.IsDead))
            {
                Ignite = player.GetSpellSlot("summonerdot");
                var edmg = E.GetDamage(enemy);
                var qdmg = Q.GetDamage(enemy);
                var rdmg = R.GetDamage(enemy);
                var rpred = R.GetPrediction(enemy);
                var qpred = Q.GetPrediction(enemy);
                var epred = E.GetPrediction(enemy);

                if (enemy.Health < edmg && E.IsReady() && epred.Hitchance >= HitChance.VeryHigh)
                    E.Cast(epred.CastPosition, Config.Item("packetcast").GetValue<bool>());

                if (enemy.Health < qdmg && qpred.Hitchance >= HitChance.VeryHigh &&
                    Q.IsReady() &&
                    Config.Item("KSQ").GetValue<bool>())

                    Q.Cast(enemy, Config.Item("packetcast").GetValue<bool>());

                if (enemy.Health < rdmg && rpred.Hitchance >= HitChance.VeryHigh &&
                    !Q.IsReady() &&
                    Config.Item("KSR").GetValue<bool>())

                    R.Cast(enemy, Config.Item("packetcast").GetValue<bool>());

                if (player.Distance(enemy.Position) <= 600 && IgniteDamage(enemy) >= enemy.Health &&
                    Config.Item("UseIgnite").GetValue<bool>() && R.IsReady() && Ignite.IsReady())
                    player.Spellbook.CastSpell(Ignite, enemy);
            }
        }

        public static bool Exploded { get; set; }

        public static Obj_AI_Hero GetTarget
        {
            get
            {
                Obj_AI_Hero target = null;
                var select = TargetSelector.GetSelectedTarget();

                if (Config.Item("OnlySelect").GetValue<bool>()  && select.IsValidTarget())
                {
                    target = select;
                }
                else
                {
                    target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
                }

                return target;
            }
        }

        public static void InsecCombo()
        {
            var target = GetTarget;
            Orbwalking.Orbwalk(null, Game.CursorPos);

            if (target.IsValidTarget())
            {
                eqpos = player.Position.Extend(target.Position, player.Distance(target));
                insecpos = player.Position.Extend(target.Position, player.Distance(target) + 200);
                movingawaypos = player.Position.Extend(target.Position, player.Distance(target) + 300);
                eqpos = player.Position.Extend(target.Position, player.Distance(target) + 100);

                if (target.IsFacing(player) == false && target.IsMoving & (R.IsInRange(insecpos) && target.Distance(insecpos) < 300))
                    R.Cast(movingawaypos);

                if (R.IsInRange(insecpos) && target.Distance(insecpos) < 300 && target.IsFacing(player) && target.IsMoving)
                    R.Cast(eqpos);
                else if (R.IsInRange(insecpos) && target.Distance(insecpos) < 300)
                    R.Cast(insecpos);

                if (!Exploded) return;

                var prediction = E.GetPrediction(target);

                if (prediction.Hitchance >= HitChance.High)
                {
                    E.Cast(target.ServerPosition);
                    Q.Cast(target.ServerPosition);
                }
            }
        }

        private static void GragasObject(GameObject sender, EventArgs args) //BARREL LOCATION
        {
            if (sender.Name == "Gragas_Base_R_End.troy")
            {
                Exploded = true;
                Utility.DelayAction.Add(3000, () => { Exploded = false; });
            }
            if (sender.Name == "Gragas_Base_Q_Ally.troy")
            {
                Barrel = sender;



            }
        }
    

        private static bool IsWall(Vector3 pos)
        {
            CollisionFlags cFlags = NavMesh.GetCollisionFlags(pos);
            return (cFlags == CollisionFlags.Wall);
        }

        private static void setbool()
        {
            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
        }

        private static void Skin()
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

        private static void Game_OnGameUpdate(EventArgs args)
        {
            setbool();
            Skin();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                LaneClear();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                Harass();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Combo();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                eLogic();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Overkill();

            if (Config.Item("UseSmite").GetValue<bool>())
                SmiteLogic();

            if (Config.Item("SmartKS").GetValue<bool>())
            {
                Killsteal();
            }

            if (Config.Item("InsecMode").GetValue<KeyBind>().Active)
                InsecCombo();

            if (Config.Item("FlashE").GetValue<KeyBind>().Active)
                EFlash();

            if (Config.Item("FleeKey").GetValue<KeyBind>().Active)
                Flee();

            if (Barrel.Position.CountEnemiesInRange(275) >= 1)
                Q.Cast();

        }

        private static void SmiteLogic()
        {
            if (Smite.IsReady() && Smite.Slot != SpellSlot.Unknown)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    if (Orbwalker.GetTarget() as Obj_AI_Hero != null)
                    {
                        Smite.CastOnUnit(Orbwalker.GetTarget() as Obj_AI_Hero);
                    }
                }

                if (ObjectManager.Player.HasBuff("smitedamagetrackerstalker") || ObjectManager.Player.HasBuff("smitedamagetrackerskirmisher"))
                {
                    if (ObjectManager.Player.Spellbook.GetSpell(Smite.Slot).Ammo == 1)
                    {
                        return;
                    }

                    foreach (var target in HeroManager.Enemies.Where(t => t.IsValidTarget(Smite.Range)))
                    {
                        if (GetChallengingSmiteDamage > target.Health && ObjectManager.Player.HasBuff("smitedamagetrackerstalker"))
                        {
                            Smite.CastOnUnit(target);
                        }
                        else if (GetChallengingSmiteDamage > target.Health && ObjectManager.Player.HasBuff("smitedamagetrackerskirmisher"))
                        {
                            Smite.CastOnUnit(target);
                        }
                    }
                }
            }
        }

        public static int GetChallengingSmiteDamage => 54 + 6 * ObjectManager.Player.Level;


        private static void Flee()
        {
            Orbwalking.Orbwalk(null, Game.CursorPos);

            if (E.IsReady() && Config.Item("FleeE").GetValue<bool>())
                E.Cast(Game.CursorPos);

            if (Q.IsReady() && Config.Item("FleeQ").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

                if(target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);

                    Utility.DelayAction.Add(500, () => Q.Cast());
                }
            }
        }

        private static void EFlash()
        {
            Orbwalking.Orbwalk(null, Game.CursorPos);

            var target = GetTarget;

            if (target.IsValidTarget())
            {
                if (target.IsValidTarget(E.Range + 475))
                {
                    if (E.IsReady() && Flash.IsReady())
                    {
                        E.Cast(target);
                    }
                    else if (!E.IsReady() && Flash.IsReady())
                    {
                        player.Spellbook.CastSpell(Flash, target.ServerPosition);
                    }
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            if (R.IsReady() && Config.Item("Rrdy").GetValue<bool>())
            {
                Drawing.DrawText(pos.X, pos.Y, Color.Gold, "R is Ready!");
            }

            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Config.Item("Qdraw").GetValue<Circle>().Active)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Config.Item("Qdraw").GetValue<Circle>().Color : Color.Red);


            if (Config.Item("Wdraw").GetValue<Circle>().Active)
                if (W.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Config.Item("Wdraw").GetValue<Circle>().Color : Color.Red);

            if (Config.Item("Edraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Config.Item("Edraw").GetValue<Circle>().Color : Color.Red);

            if (Config.Item("Rdraw").GetValue<Circle>().Active)
                if (R.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range - 2,
                        R.IsReady() ? Config.Item("Rdraw").GetValue<Circle>().Color : Color.Red);
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Config.SubMenu("杂项设置").Item("DrawD").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), Color.Gold);
                }
            }
        }

        private static int CalcDamage(Obj_AI_Base target)
        {
            var aa = player.GetAutoAttackDamage(target, true);
            var damage = aa;

            if (Config.Item("UseE").GetValue<bool>())
            {
                if (E.IsReady())
                {
                    damage += E.GetDamage(target);
                }

                if (W.IsReady() && Config.Item("UseW").GetValue<bool>())
                {
                    damage += W.GetDamage(target);
                }
            }

            if (R.IsReady() && Config.Item("UseR").GetValue<bool>())
            {
                damage += R.GetDamage(target);
            }

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>())
            {
                damage += Q.GetDamage(target);
            }
            return (int) damage;
        }

        public static void GameOnOnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        using (var c = new WebClient())
                        {
                            var rawVersion = c.DownloadString("https://raw.githubusercontent.com/YuLeDingZhi/YuLeQQ365827287/master/" +
                                Assembly.GetExecutingAssembly().GetName().Name
                                + ".txt");

                            Library.Check(rawVersion);

                            switch (rawVersion.Length)
                            {
                                case 51:
                                    GameOnOnGameLoad();
                                    Game.PrintChat("<font color='#FF9900'><b>濞涙▊鍗曠嫭鍒朵綔</b></font><font color='#0099FF'><b>-鏂扮増鍐犲啗绯诲垪鑴氭湰</b></font><font color='#CCFF66'><b>-鍒囧嬁澶栦紶</b></font>");
                                    break;
                                default:
                                    Game.PrintChat("<font color='#FF0033'><b>鎮ㄧ殑鑴氭湰宸茬粡澶辨晥</b></font><font color='#990066'><b>-璇锋壘濞涙▊棰嗗彇鏈€寮哄崌绾х増</b></font><font color='#F00000'><b>-濞涙▊QQ&#51;&#54;&#53;&#56;&#50;&#55;&#50;&#56;&#55;</b></font>");
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            );
        }

        private static int overkill(Obj_AI_Base target)
        {
            var aa = player.GetAutoAttackDamage(target, true);
            var damage = aa;

            if (Ignite != SpellSlot.Unknown && player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Config.Item("UseE").GetValue<bool>())
            {
                if (E.IsReady() && target.IsValidTarget(E.Range))
                {
                    damage += E.GetDamage(target);
                }
                if (W.IsReady() && target.IsValidTarget(E.Range))
                {
                    damage += W.GetDamage(target) + aa;
                }

                if (Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(E.Range + 100) && Q.IsReady())
                {
                    damage += Q.GetDamage(target)*1;
                }
                return (int) damage;
            }
            return 0;
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var prediction = Q.GetPrediction(target);

            if (prediction.Hitchance >= HitChance.High  && Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range) && Barrel == null)
                Q.Cast(target);

            if (R.IsReady() && Config.Item("UseRprotector").GetValue<bool>() && player.HealthPercent <= Config.Item("Rhp").GetValue<Slider>().Value)
                R.Cast(player.Position);

            if (R.IsReady() && Config.Item("UseRdmg").GetValue<bool>() && target.Position.CountEnemiesInRange(250) >= Config.Item("rdmgslider").GetValue<Slider>().Value)
                R.Cast(target);
        }

        private static void eLogic()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            var prediction = E.GetPrediction(target);
            if (E.IsReady() && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(player)))
                E.Cast(target);
            else if (W.IsReady() && target.IsValidTarget(E.Range) && Config.Item("UseW").GetValue<bool>())
                W.Cast();
            if (W.IsReady() && Config.Item("UseW").GetValue<bool>())
                return;
            if (player.HasBuff("GragasWAttackBuff"))
                E.Cast(target);
            else if (E.IsReady() && target.IsValidTarget(E.Range))
                E.Cast(target);


            if (player.HasBuff("GragasWAttackBuff") && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(player)))
                player.IssueOrder(GameObjectOrder.AutoAttack, target);
        }

        private static void Overkill()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
  
            if (R.IsReady() && Config.Item("UseR").GetValue<bool>() && target.IsValidTarget(R.Range) &&
                     R.GetDamage(target) >= target.Health && overkill(target) <= target.Health) 

                R.Cast(target.ServerPosition);
        }

        private static void Harass()
        {

            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var qpred = Q.GetPrediction(t);
            var epred = E.GetPrediction(t);
            if (E.IsReady() && Config.Item("harassE").GetValue<bool>() && t.IsValidTarget(E.Range) &&
                player.ManaPercent >= harassmana && epred.Hitchance >= HitChance.High)
                E.Cast(t);
            {
                if (Q.IsReady() && Config.Item("harassQ").GetValue<bool>() && t.IsValidTarget(Q.Range) &&
                    player.ManaPercent >= harassmana && qpred.Hitchance >= HitChance.High)
                    Q.Cast(t);
            }
        }

        private static void LaneClear()
        {
            var lanemana = Config.Item("laneclearmana").GetValue<Slider>().Value;
            var junglemana = Config.Item("jungleclearmana").GetValue<Slider>().Value;
            var jungleQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var jungleE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var laneE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width);
            var laneQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width);

            var Qjunglepos = Q.GetCircularFarmLocation(jungleQ, Q.Width);
            var Ejunglepos = E.GetLineFarmLocation(jungleE, E.Width);

            var Qfarmpos = Q.GetCircularFarmLocation(laneQ, Q.Width);
            var Efarmpos = E.GetLineFarmLocation(laneE, E.Width);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Qjunglepos.MinionsHit >= 1 &&
                Config.Item("jungleQ").GetValue<bool>()
                && player.ManaPercent >= junglemana)
            {
                Q.Cast(Qjunglepos.Position);
                Utility.DelayAction.Add(500, () => Q.Cast(Qjunglepos.Position));
            }
            foreach (var minion in jungleE)
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && minion.IsValidTarget(E.Range) &&
                    Ejunglepos.MinionsHit >= 1 && jungleE.Count >= 1 && Config.Item("jungleE").GetValue<bool>()
                    && player.ManaPercent >= junglemana)
                {
                    E.Cast(minion.Position);
                }
                foreach (var minion in jungleE)
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && minion.IsValidTarget(E.Range) &&
                    Ejunglepos.MinionsHit >= 1 && jungleE.Count >= 1 && Config.Item("jungleE").GetValue<bool>()
                    && player.ManaPercent >= junglemana)
                {
                    W.Cast(player);
                }
            {
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Qfarmpos.MinionsHit >= 3 &&
                Config.Item("laneQ").GetValue<bool>()
                && player.ManaPercent >= lanemana)
            {
                Q.Cast(Qfarmpos.Position);
                Utility.DelayAction.Add(500, () => Q.Cast(Qfarmpos.Position));
            }

            foreach (var minion in laneE)
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && minion.IsValidTarget(E.Range) &&
                    Efarmpos.MinionsHit >= 2 && laneE.Count >= 2 && Config.Item("laneE").GetValue<bool>()
                    && player.ManaPercent >= lanemana)
                {
                    E.Cast(minion.Position);
                }
            foreach (var minion in laneE)
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && minion.IsValidTarget(E.Range) &&
                    Efarmpos.MinionsHit >= 1 && laneE.Count >= 1 && Config.Item("laneE").GetValue<bool>()
                    && player.ManaPercent >= lanemana)
                {
                    W.Cast(player);
                }
        }
    }
}