namespace YuLeAzir
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Linq;
    using Color = System.Drawing.Color;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    class Program
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker _orbwalker;
        public static Spell _q, _w, _e, _r , _q2, _r2;
        public static Menu _menu;
        public static int qcount, ecount;

        public static bool Eisready { get { return Player.Mana >= _e.Instance.ManaCost && Utils.GameTimeTickCount - ecount >= _e.Instance.Cooldown * 1000f; } }

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            if (Player.ChampionName != "Azir")
                return;

            InitSpell();
            InitMenu();
            GameObjects.Initialize();
            Soldiers.AzirSoldier();
            OrbwalkCommands.Initialize();
            Combo.Initialize();
            Harass.Initialize();
            Farm.Initialize();
            LaneClear.Initialize();
            JumpToMouse.Initialize();
            Insec.Initialize();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnWndProc += Game_OnWndProc;
        }

        private static void InitSpell()
        {
            _q = new Spell(SpellSlot.Q, 1175);
            _q2 = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W, 450);
            _e = new Spell(SpellSlot.E, 1100);
            _r = new Spell(SpellSlot.R, 250);
            _r2 = new Spell(SpellSlot.R);
            _q.SetSkillshot(0.0f, 65, 1500, false, SkillshotType.SkillshotLine);
            _q.MinHitChance = HitChance.Medium;
        }

        private static void InitMenu()
        {
            _menu = new Menu("QQ群：438230879", "YuLeAzir", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Chartreuse);

            var orbwalkerMenu = new Menu("Orbwalker", "OOOORB");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            orbwalkerMenu.AddItem(new MenuItem("EQmouse", "EQ到鼠标位置").SetValue(new KeyBind('A', KeyBindType.Press)));
            _menu.AddSubMenu(orbwalkerMenu);

            var QMenu = new Menu("Q设置", "QQQQQ");
            QMenu.AddItem(new MenuItem("QC", "连招中使用").SetValue(true));
            QMenu.AddItem(new MenuItem("donotqC", "如果目标在士兵的范围内则保留Q").SetValue(false));
            QMenu.AddItem(new MenuItem("QH", "骚扰中使用").SetValue(true));
            QMenu.AddItem(new MenuItem("donotqH", "如果目标在士兵的范围内则保留Q").SetValue(false));
            QMenu.AddItem(new MenuItem("QLC", "清线中使用").SetValue(true));
            _menu.AddSubMenu(QMenu);

            var WMenu = new Menu("W设置", "WWWW");
            WMenu.AddItem(new MenuItem("WC", "连招中使用").SetValue(true));
            WMenu.AddItem(new MenuItem("WH", "骚扰中使用").SetValue(true));
            WMenu.AddItem(new MenuItem("WLC", "清线中使用").SetValue(true));
            _menu.AddSubMenu(WMenu);

            var RMenu = new Menu("R设置", "RMenu");
            RMenu.AddItem(new MenuItem("RKS", "击杀!").SetValue(true));
            RMenu.AddItem(new MenuItem("RTOWER", "R回塔下").SetValue(true));
            RMenu.AddItem(new MenuItem("RGAP", "反突进R").SetValue(false));
            _menu.AddSubMenu(RMenu);

            var InsecMenu = new Menu("回旋踢", "InsecMenu");
            InsecMenu.AddItem(new MenuItem("insec", "回旋踢按键").SetValue(new KeyBind('T', KeyBindType.Press)));
            InsecMenu.AddItem(new MenuItem("insecmode", "回旋踢模式: ").SetValue(new StringList(new[] { "友军位置", "防御塔", "鼠标" }, 0)));
            _menu.AddSubMenu(InsecMenu);

            var Drawing = new Menu("显示设置", "Drawing");
            Drawing.AddItem(new MenuItem("Draw Q", "显示 Q").SetValue(true));
            Drawing.AddItem(new MenuItem("Draw W", "显示 W").SetValue(true));
            Drawing.AddItem(new MenuItem("Draw Insec", "显示 回旋踢").SetValue(true));
            Drawing.AddItem(new MenuItem("drawSoldierAA", "显示 沙兵普攻范围").SetValue(true));
            Drawing.AddItem(new MenuItem("drawFly", "显示回旋踢范围").SetValue(true));
            _menu.AddSubMenu(Drawing);

            _menu.AddItem(new MenuItem("EnabledFarm", "技能清线开关(鼠标滑动)").SetValue(true)).Permashow();

            _menu.AddToMainMenu();
        }

        public static bool qclear { get { return _menu.Item("QLC").GetValue<bool>(); } }
        public static bool wclear { get { return _menu.Item("WLC").GetValue<bool>(); } }
        public static bool drawinsecLine { get { return _menu.Item("Draw Insec").GetValue<bool>(); } }
        public static bool eqmouse { get { return _menu.Item("EQmouse").GetValue<KeyBind>().Active; } }
        public static bool RTOWER { get { return _menu.Item("RTOWER").GetValue<bool>(); } }
        public static bool RKS { get { return _menu.Item("RKS").GetValue<bool>(); } }
        public static bool RGAP { get { return _menu.Item("RGAP").GetValue<bool>(); } }
        public static bool qcombo { get { return _menu.Item("QC").GetValue<bool>(); } }
        public static bool wcombo { get { return _menu.Item("WC").GetValue<bool>(); } }
        public static bool donotqcombo { get { return _menu.Item("donotqC").GetValue<bool>(); } }
        public static bool qharass { get { return _menu.Item("QH").GetValue<bool>(); } }
        public static bool wharass { get { return _menu.Item("WH").GetValue<bool>(); } }
        public static bool donotqharass { get { return _menu.Item("donotqH").GetValue<bool>(); } }
        public static bool insec { get { return _menu.Item("insec").GetValue<KeyBind>().Active; } }
        public static int insecmode { get { return _menu.Item("insecmode").GetValue<StringList>().SelectedIndex; } }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != 0x20a)
                return;

            _menu.Item("EnabledFarm").SetValue(!_menu.Item("EnabledFarm").GetValue<bool>());
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var target = gapcloser.Sender;
            if (target.IsEnemy && _r.IsReady() && target.IsValidTarget() && !target.IsZombie && RGAP)
            {
                if (target.IsValidTarget(250)) _r.Cast(target.Position);
            }
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.Name.ToLower().Contains("azirq"))
            {
                Qtick = Utils.GameTimeTickCount;
                qcount = Utils.GameTimeTickCount;
 
            }
            if (args.SData.Name.ToLower().Contains("azirw"))
            {

            }
            if (args.SData.Name.ToLower().Contains("azire"))
            {
                ecount = Utils.GameTimeTickCount;

            }
            if (args.SData.Name.ToLower().Contains("azirr"))
            {

            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Auto();
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (_menu.Item("Draw Q").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, _q.Range, Color.Yellow);
            if (_menu.Item("Draw W").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, _w.Range, Color.Yellow);
            if (_menu.Item("drawSoldierAA").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, 925, Color.Red);
                foreach (var sold in Soldiers.soldier.Where(x => x.Position.Distance(Player.Position) <= 925))
                {
                    Render.Circle.DrawCircle(sold.Position, 300, Color.Red);
                }
            }
            if (_menu.Item("drawFly").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, 875 + 300 - 100, Color.Pink);
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

        private static void Auto()
        {
            if (RKS)
            {
                if (_r.IsReady())
                {
                    foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget(250) && !x.IsZombie && x.Health < _r.GetDamage(x)))
                    {
                        _r.Cast(hero.Position);
                    }
                }
            }
            if(RTOWER)
            {
                if (_r.IsReady())
                {
                    var turret = ObjectManager.Get<Obj_AI_Turret>().Where(x => x.IsAlly && !x.IsDead).OrderByDescending(x => x.Distance(Player.Position)).LastOrDefault();
                    foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget(250) && !x.IsZombie))
                    {
                        if (Player.ServerPosition.Distance(turret.Position)+100 >= hero.Distance(turret.Position) && hero.Distance(turret.Position) <= 775 + 250)
                        {
                            var pos = Player.Position.Extend(turret.Position, 250);
                            _r.Cast(pos);
                        }
                    }
                }
            }
        }



        public static bool  Qisready()
        {
            if (Utils.GameTimeTickCount - Qtick >= _q.Instance.Cooldown * 1000)
            {
                return true;
            }
            else
                return false;
        }
        public static int Qtick;

        public static double Wdamage(Obj_AI_Base target)
        {
            return Player.CalcDamage(target, Damage.DamageType.Magical,
                        new double[]
                        {
                            50 , 52 , 54 , 56 , 58 , 60 , 63 , 66 , 69 , 72 , 75 , 85 , 95 , 110 , 125 , 140 , 155 , 170
                        }[Player.Level - 1] + 0.6 * Player.FlatMagicDamageMod);
        }
        
    }
}
