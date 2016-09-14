using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Geometry = LeagueSharp.Common.Geometry;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using Item = LeagueSharp.Common.Items.Item;

namespace HikiCarry
{
    class lost
    {
        public const string ChampionName = "Vayne";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        static List<Spells> SpellListt = new List<Spells>();
        static int Delay = 0;

        public static Menu Config;
        public static int getSliderValue(String item)
        {
            return lost.Config.Item(item) != null ? lost.Config.Item(item).GetValue<Slider>().Value : -1;
        }
        private static float Hp百分比(Obj_AI_Hero Player)
        {
            return Player.Health / Player.MaxHealth * 100;
        }

    /*    public static readonly Item 弯刀 = ItemData.Bilgewater_Cutlass.GetItem();
        public static readonly Item 破败 = ItemData.Blade_of_the_Ruined_King.GetItem();
        public static readonly Item 幽梦 = ItemData.Youmuus_Ghostblade.GetItem();*/
        public static Items.Item 红药 = new Items.Item(2003, 0);
        public static Items.Item 蓝药 = new Items.Item(2004, 0);

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static readonly Vector2 midPos = new Vector2(6707.485f, 8802.744f);
        private static readonly Vector2 dragPos = new Vector2(11514, 4462);
        public static float LastMoveC;

        public T GetValue<T>(string name)
        {
            return Config.Item(name).GetValue<T>();
        }
        public bool GetBool(string name)
        {
            return GetValue<bool>(name);
        }

        private static Obj_AI_Hero Player;
        public struct Spells
        {
            public string ChampionName;
            public string SpellName;
            public SpellSlot slot;
        }

        static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;


            Q = new Spell(SpellSlot.Q, 300f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550f);
            E.SetTargetted(0.25f, 1600f);
            R = new Spell(SpellSlot.R);


            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellListt.Add(new Spells { ChampionName = "akali", SpellName = "akalismokebomb", slot = SpellSlot.W });   //Akali W
            SpellListt.Add(new Spells { ChampionName = "shaco", SpellName = "deceive", slot = SpellSlot.Q }); //Shaco Q
            SpellListt.Add(new Spells { ChampionName = "khazix", SpellName = "khazixr", slot = SpellSlot.R }); //Khazix R
            SpellListt.Add(new Spells { ChampionName = "khazix", SpellName = "khazixrlong", slot = SpellSlot.R }); //Khazix R Evolved
            SpellListt.Add(new Spells { ChampionName = "talon", SpellName = "talonshadowassault", slot = SpellSlot.R }); //Talon R
            SpellListt.Add(new Spells { ChampionName = "monkeyking", SpellName = "monkeykingdecoy", slot = SpellSlot.W }); //Wukong W

            //MENU
            Config = new Menu("王者VN-真正神话", "HikiCarry - Vayne", true);

            //TARGET SELECTOR
            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //ORBWALKER
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //INFO
            Config.AddSubMenu(new Menu("信息", "Info"));
            Config.SubMenu("Info").AddItem(new MenuItem("作者", "@Hikigaya @huabian @Feeez"));

            //COMBO
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushQCombo", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushECombo", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushRCombo", "使用 R").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("usermin", "多少个敌人自动开R").SetValue(new Slider(2, 1, 5)));

            Config.SubMenu("Combo").AddItem(new MenuItem("combotype", "花式连招模式").SetValue(new StringList(new[] { "Solo模式", "团战模式" })));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "连招按键!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Item
            Config.AddSubMenu(new Menu("物品", "Item"));
            Config.SubMenu("Item").AddItem(new MenuItem("YoumuuC", "幽梦").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("BotrkC", "破败").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("pots", "使用药水").SetValue(true));


            //INVISIBLE KICKER
            Config.AddSubMenu(new Menu("隐身的敌人", "Invisiblez"));
            Config.SubMenu("Invisiblez").AddItem(new MenuItem("Use", "使用真眼连招").SetValue(new KeyBind(32, KeyBindType.Press)));

            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                {
                    foreach (var spell in SpellListt.Where(x => x.ChampionName.ToLower() == hero.ChampionName.ToLower()))
                    {
                        Config.SubMenu("Invisiblez").AddItem(new MenuItem(hero.ChampionName.ToLower() + spell.slot.ToString(), hero.ChampionName + " - " + spell.slot.ToString()).SetValue(true));
                    }
                }

                if (HeroManager.Enemies.Any(x => x.ChampionName.ToLower() == "rengar"))
                {
                    Config.SubMenu("Invisiblez").AddItem(new MenuItem("RengarR", "狮子狗 R").SetValue(true));
                }


            }


            //HARASS
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("RushEHarass", "使用 E", true).SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassmana", "蓝量控制").SetValue(new Slider(30, 0, 100)));
            //MISC
            Config.AddSubMenu(new Menu("额外选项", "Misc"));
            Config.SubMenu("Misc").AddSubMenu(new Menu("占卜宝珠", "orbset"));
            Config.SubMenu("Misc").SubMenu("orbset").AddItem(new MenuItem("bT", "自动购买宝珠!").SetValue(false));
            Config.SubMenu("Misc").SubMenu("orbset").AddItem(new MenuItem("bluetrinketlevel", "多少级才购买宝珠!").SetValue(new Slider(6, 0, 18)));
            Config.SubMenu("Misc").AddItem(new MenuItem("agapcloser", "启动反突进!", true).SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("ainterrupt", "启动打断!", true).SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("ksR", "击杀 E!").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("onevoneactive", "SOLO模式个人秀").SetValue(new KeyBind('K', KeyBindType.Toggle, false)));
            Config.SubMenu("Misc").AddItem(new MenuItem("walltumble", "翻墙")).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press));

            //DRAWINGS
            Config.AddSubMenu(new Menu("显示", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushQRange", "Q 范围").SetValue(new Circle(false, System.Drawing.Color.FromArgb(135, 206, 235))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushERange", "E 范围").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 0))));
           





            Config.AddToMainMenu();
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;




        }

    /*    public static bool UseBotrk(Obj_AI_Hero target)
        {
            var 使用弯刀 = Config.Item("UseBC").GetValue<bool>();
            var 使用破败 = Config.Item("BotrkC").GetValue<bool>();

            if (使用破败 && 破败.IsReady() && target.IsValidTarget(破败.Range) &&
                Player.Health + Player.GetItemDamage(target, Damage.DamageItems.Botrk) < Player.MaxHealth)
            {
                return 破败.Cast(target);
            }
            else if (使用弯刀 && 弯刀.IsReady() && target.IsValidTarget(弯刀.Range))
            {
                return 弯刀.Cast(target);
            }
            return false;
        }

        public static bool UseYoumuu(Obj_AI_Base target)
        {
            var 使用幽梦 = Config.Item("YoumuuC").GetValue<bool>();

            if (使用幽梦 && 幽梦.IsReady() && Player.Distance(target.Position) > Player.AttackRange)
            {
                return 幽梦.Cast();
            }
            return false;
        }*/
            
        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Config.Item("ainterrupt").GetValue<bool>() || Player.IsDead)
                return;

            if (sender.IsValidTarget(1000))
            {
                Render.Circle.DrawCircle(sender.Position, sender.BoundingRadius, Color.Gold, 5);
                var targetpos = Drawing.WorldToScreen(sender.Position);
                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Interrupt");
            }

            if (E.CanCast(sender))
                E.Cast(sender);
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("agapcloser").GetValue<bool>() || Player.IsDead)
                return;

            if (gapcloser.Sender.IsValidTarget(1000))
            {
                Render.Circle.DrawCircle(gapcloser.Sender.Position, gapcloser.Sender.BoundingRadius, Color.Gold, 5);
                var targetpos = Drawing.WorldToScreen(gapcloser.Sender.Position);
                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Gapcloser");
            }

            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.Sender);
        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!Config.Item("Use").GetValue<KeyBind>().Active)
            return;

            var Rengar = HeroManager.Enemies.Find(x => x.ChampionName.ToLower() == "rengar");

            if (Rengar == null)
                return;

            if (!Config.Item("RengarR").GetValue<bool>())
                return;

            if (ObjectManager.Player.Distance(sender.Position) < 1500)
            {
                Console.WriteLine("Sender : " + sender.Name);
            }

            if (sender.IsEnemy && sender.Name.Contains("Rengar_Base_R_Alert"))
            {
                if (ObjectManager.Player.HasBuff("rengarralertsound") &&
                !CheckWard() &&
                !Rengar.IsVisible &&
                !Rengar.IsDead &&
                    CheckSlot() != SpellSlot.Unknown)
                {
                    ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), ObjectManager.Player.Position);
                }
            }
        }
        static SpellSlot CheckSlot()
        {
            SpellSlot slot = SpellSlot.Unknown;

            if (Items.CanUseItem(3362) && Items.HasItem(3362, ObjectManager.Player))
            {
                slot = SpellSlot.Trinket;
            }
            else if (Items.CanUseItem(2043) && Items.HasItem(2043, ObjectManager.Player))
            {
                slot = ObjectManager.Player.GetSpellSlot("VisionWard");
            }
            return slot;
        }

        static bool CheckWard()
        {
            var status = false;

            foreach (var a in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name == "VisionWard"))
            {
                if (ObjectManager.Player.Distance(a.Position) < 450)
                {
                    status = true;
                }
            }

            return status;
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            #region 真眼
            if (!Config.Item("Use").GetValue<KeyBind>().Active)
            {
            if (!sender.IsEnemy || sender.IsDead || !(sender is Obj_AI_Hero))
                return;

            if (SpellListt.Exists(x => x.SpellName.Contains(args.SData.Name.ToLower())))
            {
                var _sender = sender as Obj_AI_Hero;

                if (!Config.Item(_sender.ChampionName.ToLower() + _sender.GetSpellSlot(args.SData.Name).ToString()).GetValue<bool>())
                    return;

                if (CheckSlot() == SpellSlot.Unknown)
                    return;

                if (CheckWard())
                    return;

                if (ObjectManager.Player.Distance(sender.Position) > 700)
                    return;

                if (Environment.TickCount - Delay > 1500 || Delay == 0)
                {
                    var pos = ObjectManager.Player.Distance(args.End) > 600 ? ObjectManager.Player.Position : args.End;
                    ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), pos);
                    Delay = Environment.TickCount;
                }
            }
            }
            #endregion
            if (ObjectManager.Player.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(Player)) >= lost.getSliderValue("usermin"))
            {
                if (!R.IsReady())
                {
                    R.Cast();
                }
            }
        }



        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttack(true);
            #region 走砍按键触发
            //COMBO
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Clear();
            }
            if (Config.Item("ksR").GetValue<bool>())
            {
                Killsteal();
            }

            if (Config.Item("bT").GetValue<bool>() && Player.Level >= Config.Item("bluetrinketlevel").GetValue<Slider>().Value && Player.InShop() && !(Items.HasItem(3342) || Items.HasItem(3363)))
            {
                Player.BuyItem(ItemId.Scrying_Orb_Trinket);
            }

            if (Config.Item("walltumble").GetValue<KeyBind>().Active)
            {
                TumbleHandler();
            }
            #endregion
            PotionMenager();

        }
        public static void PotionMenager()
        {

            if (Config.Item("pots").GetValue<bool>() && !ObjectManager.Player.InFountain() && !ObjectManager.Player.HasBuff("Recall"))
            {
                if (红药.IsReady() && !ObjectManager.Player.HasBuff("RegenerationPotion", true))
                {
                    if (ObjectManager.Player.CountEnemiesInRange(700) > 0 && ObjectManager.Player.Health + 200 < ObjectManager.Player.MaxHealth)
                        红药.Cast();
                    else if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.6)
                        红药.Cast();
                }
                if (蓝药.IsReady() && !ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true))
                {
                    if (ObjectManager.Player.CountEnemiesInRange(1200) > 0 && ObjectManager.Player.Mana < 45)
                        蓝药.Cast();
                }
            }
        }

        private static IEnumerable<Vector3> GetPossibleQPositions()
        {
            var pointList = new List<Vector3>();

            var j = 300;

            var offset = (int)(2 * Math.PI * j / 100);

            for (var i = 0; i <= offset; i++)
            {
                var angle = i * Math.PI * 2 / offset;
                var point = new Vector3((float)(ObjectManager.Player.Position.X + j * Math.Cos(angle)),
                    (float)(ObjectManager.Player.Position.Y - j * Math.Sin(angle)),
                    ObjectManager.Player.Position.Z);

                if (!NavMesh.GetCollisionFlags(point).HasFlag(CollisionFlags.Wall))
                    pointList.Add(point);
            }


            return pointList;
        }
        static bool isAllyFountain(Vector3 Position)
        {
            float fountainRange = 750;
            var map = Utility.Map.GetMap();
            if (map != null && map.Type == Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return
                ObjectManager.Get<GameObject>().Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly).Any(spawnPoint => Vector2.Distance(Position.To2D(), spawnPoint.Position.To2D()) < fountainRange);
        }

        private static void Combo()
        {
            switch (Config.Item("combotype").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    var target0 = TargetSelector.GetTarget(550, TargetSelector.DamageType.Physical);
                    float rangez0 = ObjectManager.Player.CountEnemiesInRange(1500);
                    //start hikigaya
                   
                     /*   if (E.IsReady() && rangez0 == 1)
                        {
                            foreach (
                          var qPosition in
                            GetPossibleQPositions()
                            .OrderBy(qPosition => qPosition.Distance(target0.ServerPosition)))
                            {
                                if (qPosition.Distance(target0.Position) < E.Range)
                                    E.UpdateSourcePosition(qPosition, qPosition);
                                var targetPosition = E.GetPrediction(target0).CastPosition;
                                var finalPosition = targetPosition.Extend(qPosition, 400);
                                if (finalPosition.IsWall())
                                {
                                    Q.Cast(qPosition);
                                }
                                else
                                {
                                    foreach (var targetz in HeroManager.Enemies.Where(h => h.IsValidTarget(E.Range) && h.Path.Count() < 2))
                                    {
                                        E.UpdateSourcePosition(Player.Position, Player.Position);
                                        var targetPositions = E.GetPrediction(targetz).CastPosition;
                                        var finalPositions = targetPositions.Extend(Player.ServerPosition, 400);

                                        if (finalPositions.IsWall())
                                        {
                                            Q.Cast(Game.CursorPos);
                                            E.Cast(targetz);
                                        }
                                    }
                                }

                            }
                        }
                    */
                 /*   if (Items.CanUseItem(3142))//破败
                    {
                        if(Player.Distance(target0.Position) < Player.AttackRange)
                        {
                            if (Hp百分比(Player) <= 65)
                            {
                            Items.UseItem(3142);//血量低于65%
                            }
                            else if (!Player.IsDashing())
                            {
                            Items.UseItem(3142);//被突进
                            }
                            
                            Items.UseItem(3142);//都不符合 还是会用
                        }
                    }

                    if (Items.CanUseItem(3144))//幽梦
                    {
                        if (Player.Distance(target0.Position) > Player.AttackRange)
                        {
                            Items.UseItem(3144);//追人幽梦
                        }
                    }*/

                        if (Q.IsReady() && Config.Item("RushQCombo").GetValue<bool>())
                        {
                            if (target0.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                            {
                                Q.Cast(Game.CursorPos);
                            }
                        }
                        if (E.IsReady() && Config.Item("RushECombo").GetValue<bool>())
                        {
                            

                            foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)))
                            {


                                var EPred = E.GetPrediction(En);
                                int pushDist = 425;
                                var FinalPosition = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -pushDist).To3D();

                                for (int i = 1; i < pushDist; i += (int)En.BoundingRadius)
                                {
                                    Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -i).To3D();

                                    if (loc3.IsWall() || isAllyFountain(FinalPosition))
                                        E.Cast(En);
                                }
                            }
                        }
                    //finish hikigaya
                        if (ObjectManager.Player.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(Player)) >= lost.getSliderValue("usermin"))
                        {
                            if (R.IsReady() && Config.Item("RushRCombo").GetValue<bool>())
                            {
                                R.Cast();
                            }
                        }

                        if (Items.CanUseItem(3142) && Player.Distance(target0.Position) < Player.AttackRange && Config.Item("YoumuuC").GetValue<bool>())
                            Items.UseItem(3142);

                        if ((Player.Health / Player.MaxHealth) * 100 < (target0.Health / target0.MaxHealth) * 100 && (Items.CanUseItem(3153) || Items.CanUseItem(3144)) && Config.Item("BotrkC").GetValue<bool>())
                        {
                            Items.UseItem(3144, target0);

                            Items.UseItem(3153, target0);
                        }
                    break;
                    case 1:
                    var target1 = TargetSelector.GetTarget(550, TargetSelector.DamageType.Physical);
                    float rangez1 = ObjectManager.Player.CountEnemiesInRange(1500);
                   
                   
                        if (Q.IsReady() && Config.Item("RushQCombo").GetValue<bool>())
                        {
                             foreach (
                        var en in
                            HeroManager.Enemies.Where(
                                hero =>
                                    hero.IsValidTarget(550)))
                             {
                                 Q.Cast(Game.CursorPos);
                             }
                            
                               
                            
                        }
                        if (E.IsReady() && Config.Item("RushECombo").GetValue<bool>())
                        {


                            foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)))
                            {


                                var EPred = E.GetPrediction(En);
                                int pushDist = 425;
                                var FinalPosition = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -pushDist).To3D();

                                for (int i = 1; i < pushDist; i += (int)En.BoundingRadius)
                                {
                                    Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -i).To3D();

                                    if (loc3.IsWall() || isAllyFountain(FinalPosition))
                                        E.Cast(En);
                                }
                            }
                        }
                        if (ObjectManager.Player.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(Player)) >= lost.getSliderValue("usermin"))
                        {
                            if (R.IsReady() && Config.Item("RushRCombo").GetValue<bool>())
                            {
                                R.Cast();
                            }
                        }
                        if (Items.CanUseItem(3142) && Player.Distance(target1.Position) < Player.AttackRange && Config.Item("YoumuuC").GetValue<bool>())
                            Items.UseItem(3142);

                        if ((Player.Health / Player.MaxHealth) * 100 < (target1.Health / target1.MaxHealth) * 100 && (Items.CanUseItem(3153) || Items.CanUseItem(3144)) && Config.Item("BotrkC").GetValue<bool>())
                        {
                            Items.UseItem(3144, target1);

                            Items.UseItem(3153, target1);
                        }
                break;
            } 
        }
        private static void Harass()
        {
            if (E.IsReady() && Config.Item("RushEHarass").GetValue<bool>() && Player.ManaPercent >= Config.Item("harassmana").GetValue<Slider>().Value)
            {
                var target = TargetSelector.GetTarget(550, TargetSelector.DamageType.Physical);
                if (target.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count >= 2))
                {
                    E.Cast((Obj_AI_Base)target);
                }

            }
        }
        private static void Clear()
        {
           

        }

        private static void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - LastMoveC < 80)
            {
                return;
            }
            LastMoveC = Environment.TickCount;
            Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }
        private static void TumbleHandler()
        {
            if (Player.Distance(midPos) >= Player.Distance(dragPos))
            {
                if (Player.Position.X < 12000 || Player.Position.X > 12070 || Player.Position.Y < 4800 ||
                Player.Position.Y > 4872)
                {
                    MoveToLimited(new Vector2(12050, 4827).To3D());
                }
                else
                {
                    MoveToLimited(new Vector2(12050, 4827).To3D());
                    Q.Cast(dragPos, true);
                }
            }
            else
            {
                if (Player.Position.X < 6908 || Player.Position.X > 6978 || Player.Position.Y < 8917 ||
                Player.Position.Y > 8989)
                {
                    MoveToLimited(new Vector2(6958, 8944).To3D());
                }
                else
                {
                    MoveToLimited(new Vector2(6958, 8944).To3D());
                    Q.Cast(midPos, true);
                }
            }
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem2 = Config.Item("RushQRange").GetValue<Circle>();
            var menuItem3 = Config.Item("RushERange").GetValue<Circle>();




            if (Config.Item("RushQCombo").GetValue<bool>() && Q.IsReady())
            {
                if (menuItem2.Active) Render.Circle.DrawCircle(Player.Position, Q.Range, Color.SkyBlue);
            }

            if (Config.Item("RushECombo").GetValue<bool>() && E.IsReady())
            {
                if (menuItem3.Active) Render.Circle.DrawCircle(Player.Position, E.Range, Color.Yellow);
            }
            

        }

        private static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && E.IsKillable(target))
                    E.Cast(target);
            }
        }



    }
}