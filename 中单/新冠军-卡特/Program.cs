using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Microsoft.Win32;
using SharpDX;
using Color = System.Drawing.Color;


namespace YuleKatarina{
    class Program {

        #region Declaration

        private static bool ShallJumpNow;
        private static Vector3 JumpPosition;
        private static Spell Q, W, E, R;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Menu _menu;
        private static int whenToCancelR;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Obj_AI_Hero qTarget;
        private static Obj_AI_Base qMinion;
        private static readonly Obj_AI_Hero[] AllEnemy = HeroManager.Enemies.ToArray();
        private static bool WardJumpReady;
        private static SpellSlot IgniteSpellSlot = SpellSlot.Unknown;
        private static readonly List<int> AllEnemyTurret = new List<int>();
        private static readonly List<int> AllAllyTurret = new List<int>();
        private static Dictionary<int,bool> TurretHasAggro = new Dictionary<int, bool>();
        private static int lastLeeQTick;
        private static int tickValue;

        #endregion

        static bool IsTurretPosition(Vector3 pos)
        {
            float mindistance = 2000;
            foreach (int NetID in AllEnemyTurret)
            {
                Obj_AI_Turret turret = ObjectManager.GetUnitByNetworkId<Obj_AI_Turret>(NetID);
                if (turret != null && !turret.IsDead && !TurretHasAggro[NetID])
                {
                    float distance = pos.Distance(turret.Position);
                    if (mindistance >= distance)
                    {
                        mindistance = distance;
                        
                    }

                }
            }
            return mindistance <= 950;
        }

        static void Game_OnGameLoad(EventArgs args) {
            //Wird aufgerufen, wenn LeagueSharp Injected
            if (Player.ChampionName != "Katarina")
            {
                return;
            }
            #region Spells
            Q = new Spell(SpellSlot.Q, 675, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 375, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 700, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 550, TargetSelector.DamageType.Magical);
            //Get Ignite
            if (Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name.Contains("summonerdot"))
            {
                IgniteSpellSlot = SpellSlot.Summoner1;
            }
            if (Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains("summonerdot"))
            {
                IgniteSpellSlot = SpellSlot.Summoner2;
            }
            #endregion

            foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                if (turret.IsEnemy)
                {
                    AllEnemyTurret.Add(turret.NetworkId);
                    TurretHasAggro[turret.NetworkId] = false;
                }
                if (turret.IsAlly)
                {
                    AllAllyTurret.Add(turret.NetworkId);
                    TurretHasAggro[turret.NetworkId] = false;
                }
            }

            Utility.HpBarDamageIndicator.Enabled = true;
            Utility.HpBarDamageIndicator.DamageToUnit = CalculateDamage;

            
            #region Menu
            _menu = new Menu("QQ群：438230879", "QQ群：438230879", true);

            //Orbwalker-Menü
            Menu orbwalkerMenu = new Menu("走砍设置", "YuleKatarina.orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //Combo-Menü
            Menu comboMenu = new Menu("连招设置", "Yule.katarina.combo");
            comboMenu.AddItem(new MenuItem("Yule.katarina.combo.useq", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("Yule.katarina.combo.usew", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("Yule.katarina.combo.usee", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("Yule.katarina.combo.user", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("Yule.katarina.combo.mode", "连招模式").SetValue(new StringList(new[] { "智能连招", "快速连招" })));
            comboMenu.AddItem(new MenuItem("Yule.katarina.combo.order", "技能顺序").SetValue(new StringList(new []{"Q -> E -> W -> R", "E -> Q -> W -> R","随机" })));
            _menu.AddSubMenu(comboMenu);

            //Harrass-Menü
            Menu harassMenu = new Menu("骚扰设置", "Yule.katarina.harrass");
            harassMenu.AddItem(new MenuItem("Yule.katarina.harass.useq", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("Yule.katarina.harass.usew", "Use W").SetValue(true));
            Menu autoHarassMenu = new Menu("Autoharass","Yule.katarina.autoharass");
            autoHarassMenu.AddItem(new MenuItem("Yule.katarina.harass.autoharass.toggle", "自动骚扰").SetValue(true));
            autoHarassMenu.AddItem(new MenuItem("Yule.katarina.harass.autoharass.key","骚扰切换键").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
            autoHarassMenu.AddItem(new MenuItem("Yule.katarina.harass.autoharass.useq", "Use Q").SetValue(false));
            autoHarassMenu.AddItem(new MenuItem("Yule.katarina.harass.autoharass.usew", "Use W").SetValue(true));
            harassMenu.AddSubMenu(autoHarassMenu);
            _menu.AddSubMenu(harassMenu);
            
            //Laneclear-Menü
            Menu laneclear = new Menu("清线设置", "Yule.katarina.laneclear");
            laneclear.AddItem(new MenuItem("Yule.katarina.laneclear.useq", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("Yule.katarina.laneclear.usew", "Use W").SetValue(true));
            laneclear.AddItem(new MenuItem("Yule.katarina.laneclear.minw", "清线W小兵数量").SetValue(new Slider(3,1,6)));
            laneclear.AddItem(new MenuItem("Yule.katarina.laneclear.minwlasthit", "补刀W小兵数量").SetValue(new Slider(2, 0, 6)));
            _menu.AddSubMenu(laneclear);

            //Jungleclear-Menü
            Menu jungleclear = new Menu("打野设置", "Yule.katarina.jungleclear");
            jungleclear.AddItem(new MenuItem("Yule.katarina.jungleclear.useq", "Use Q").SetValue(true));
            jungleclear.AddItem(new MenuItem("Yule.katarina.jungleclear.usew", "Use W").SetValue(true));
            jungleclear.AddItem(new MenuItem("Yule.katarina.jungleclear.usee", "Use E").SetValue(true));
            _menu.AddSubMenu(jungleclear);

            //Lasthit-Menü
            Menu lasthit = new Menu("补刀设置", "Yule.katarina.lasthit");
            lasthit.AddItem(new MenuItem("Yule.katarina.lasthit.useq", "Use Q").SetValue(true));
            lasthit.AddItem(new MenuItem("Yule.katarina.lasthit.usew", "Use W").SetValue(true).SetTooltip("默认即可"));
            lasthit.AddItem(new MenuItem("Yule.katarina.lasthit.usee", "Use E").SetValue(false).SetTooltip("默认即可"));
            lasthit.AddItem(new MenuItem("Yule.katarina.lasthit.noenemiese", "只有当周围没有敌人的时候才使用E补刀").SetValue(true));
            _menu.AddSubMenu(lasthit);

            //KS-Menü
            Menu ksMenu = new Menu("击杀设置", "Yule.katarina.killsteal");
            ksMenu.AddItem(new MenuItem("Yule.katarina.killsteal.useq", "Use Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("Yule.katarina.killsteal.usew", "Use W").SetValue(true));
            ksMenu.AddItem(new MenuItem("Yule.katarina.killsteal.usee", "Use E").SetValue(true));
            ksMenu.AddItem(new MenuItem("Yule.katarina.killsteal.usef", "使用点燃").SetValue(true));
            ksMenu.AddItem(new MenuItem("Yule.katarina.killsteal.wardjump", "自动跳眼抢人头").SetValue(true));
            _menu.AddSubMenu(ksMenu);

            //Drawings-Menü
            Menu drawingsMenu = new Menu("显示设置","Yule.katarina.drawings");
            drawingsMenu.AddItem(new MenuItem("Yule.katarina.drawings.drawq", "Draw Q").SetValue(false));
            drawingsMenu.AddItem(new MenuItem("Yule.katarina.drawings.draww", "Draw W").SetValue(false));
            drawingsMenu.AddItem(new MenuItem("Yule.katarina.drawings.drawe", "Draw E").SetValue(false));
            drawingsMenu.AddItem(new MenuItem("Yule.katarina.drawings.drawr", "Draw R").SetValue(false));
            drawingsMenu.AddItem(new MenuItem("Yule.katarina.drawings.dmg", "显示对目标造成伤害").SetValue(true));
            drawingsMenu.AddItem(new MenuItem("Yule.katarina.drawings.drawalways", "显示全部").SetValue(false).SetTooltip("这个开关打开可以显示你的技能冷却时间"));
            _menu.AddSubMenu(drawingsMenu);

            //Misc-Menü
            Menu miscMenu = new Menu("杂项设置", "Yule.katarina.misc");
            miscMenu.AddItem(new MenuItem("Yule.katarina.misc.wardjump", "一键摸眼").SetValue(true));
            miscMenu.AddItem(new MenuItem("Yule.katarina.misc.wardjumpkey", "摸眼快捷键").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            miscMenu.AddItem(new MenuItem("Yule.katarina.misc.noRCancel", "防止R取消").SetValue(true).SetTooltip("这是防止你开R的0.4秒被中断选项"));
            miscMenu.AddItem(new MenuItem("Yule.katarina.misc.cancelR", "当周围没有敌人的时候取消R").SetValue(false));
            miscMenu.AddItem(new MenuItem("Yule.katarina.misc.kswhileult", "击杀提醒").SetValue(true));
            miscMenu.AddItem(new MenuItem("Yule.katarina.misc.allyTurret", "跳到队友或者防御塔如果想突进").SetTooltip("试着E到队友身上或者自己塔下当敌人想突进你").SetValue(true));
            
            Menu performanceMenu = new Menu("主要性能", "Yule.katarina.performance");
            performanceMenu.AddItem(new MenuItem("Yule.katarina.performance.track","补刀性能").SetTooltip("高=准确的补尾刀，低=最优化FPS").SetValue(new Slider(3,1,10)));
            performanceMenu.AddItem(new MenuItem("Yule.katarina.performance.tickmanager", "开关性能").SetValue(false));
            performanceMenu.AddItem(new MenuItem("Yule.katarina.performance.ticks", "性能更新频率").SetTooltip("当追踪小兵在毫秒的时间").SetValue(new Slider(8,2,50)));

            lasthit.AddSubMenu(performanceMenu);
            _menu.AddSubMenu(miscMenu);

            //Dev-Menü
            Menu devMenu = new Menu("特殊工具", "Yule.katarina.dev");
            devMenu.AddItem(new MenuItem("Yule.katarina.dev.enable", "开关工具").SetValue(false));
            devMenu.AddItem(new MenuItem("Yule.katarina.dev.targetdistance", "目标距离").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press)));
            _menu.AddSubMenu(devMenu);

            //alles zum Hauptmenü hinzufügen
            _menu.AddToMainMenu();

            #endregion
            Game.PrintChat("<font color='#FFFF00'><b>QQQUN：438230879</b></font><font color='#CCFF66'><b>-QQQUN：438230879</b></font><font color='#FF9900'><b>-QQQUN：438230879</b></font>");
            #region Subscriptions
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Obj_AI_Base.OnTarget += Turret_OnTarget;
            Obj_AI_Base.OnBuffRemove += BuffRemove;
            

            #endregion
        }

       

        private static void OnDraw(EventArgs args)
        {
            if(_menu.Item("Yule.katarina.drawings.drawq").GetValue<bool>() && (Q.IsReady() || _menu.Item("Yule.katarina.drawings.drawalways").GetValue<bool>()))
                Render.Circle.DrawCircle(Player.Position,Q.Range,Color.IndianRed);
            if (_menu.Item("Yule.katarina.drawings.draww").GetValue<bool>() && (W.IsReady() || _menu.Item("Yule.katarina.drawings.drawalways").GetValue<bool>()))
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.IndianRed);
            if (_menu.Item("Yule.katarina.drawings.drawe").GetValue<bool>() && (E.IsReady() || _menu.Item("Yule.katarina.drawings.drawalways").GetValue<bool>()))
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.IndianRed);
            if (_menu.Item("Yule.katarina.drawings.drawr").GetValue<bool>() && (R.IsReady() || _menu.Item("Yule.katarina.drawings.drawalways").GetValue<bool>()))
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.IndianRed);
        }


        private static void BuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsMe && args.Buff.Name == "BlindMonkQOne")
            {

                //Game.PrintChat("Player lost Lee Sin Q Buff");
                lastLeeQTick = Utils.TickCount;
            }
        }


        static void Game_OnUpdate(EventArgs args) {
            Demark();
            Utility.HpBarDamageIndicator.Enabled = _menu.Item("Yule.katarina.drawings.dmg").GetValue<bool>();
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }
            if (HasRBuff())
            {
                _orbwalker.SetAttack(false);
                _orbwalker.SetMovement(false);
                if (_menu.Item("Yule.katarina.misc.cancelR").GetValue<bool>() && Player.GetEnemiesInRange(R.Range + 50).Count == 0)
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                if (_menu.Item("Yule.katarina.misc.kswhileult").GetValue<bool>())
                    Killsteal();
                return;
            }
            if (ShallJumpNow)
            {
                WardJump(JumpPosition,false,false);
                if (!E.IsReady())
                {
                    ShallJumpNow = false;
                }
            }
            _orbwalker.SetAttack(true);
            _orbwalker.SetMovement(true);
            //Dev();
            Killsteal();
            //Combo
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Combo(Q.IsReady() && _menu.Item("Yule.katarina.combo.useq").GetValue<bool>(), W.IsReady() && _menu.Item("Yule.katarina.combo.usew").GetValue<bool>(), E.IsReady() && _menu.Item("Yule.katarina.combo.usee").GetValue<bool>(), R.IsReady() && _menu.Item("Yule.katarina.combo.user").GetValue<bool>());
            //Harass
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                Combo(Q.IsReady() && _menu.Item("Yule.katarina.harass.useq").GetValue<bool>(), W.IsReady() && _menu.Item("Yule.katarina.harass.usew").GetValue<bool>(), false, false, true);
            //Autoharass
            if (_menu.Item("Yule.katarina.harass.autoharass.toggle").GetValue<bool>() && _menu.Item("Yule.katarina.harass.autoharass.key").GetValue<KeyBind>().Active)
                Combo(Q.IsReady() && _menu.Item("Yule.katarina.harass.autoharass.useq").GetValue<bool>(), W.IsReady() && _menu.Item("Yule.katarina.harass.autoharass.usew").GetValue<bool>(), false, false, true);
            Lasthit();
            LaneClear();
            JungleClear();
            if (_menu.Item("Yule.katarina.misc.wardjumpkey").GetValue<KeyBind>().Active && _menu.Item("Yule.katarina.misc.wardjump").GetValue<bool>())
            {
                WardJump(Game.CursorPos);
            }
        }

        private static void Dev()
        {
            if(_menu.Item("Yule.katarina.dev.enable").GetValue<bool>() && _menu.Item("Yule.katarina.dev.targetdistance").GetValue<KeyBind>().Active)
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
                if (target != null)
                {
                    Game.PrintChat("Distance to Target:" + Player.Distance(target));
                }
            }
        }


        static bool HasRBuff()
        {
            return Player.HasBuff("KatarinaR") || Player.IsChannelingImportantSpell() || Player.HasBuff("katarinarsound");

        }



        static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        


        static void Combo(bool useq, bool usew, bool usee, bool user, bool anyTarget = false)
        {
            bool startWithQ = _menu.Item("Yule.katarina.combo.order").GetValue<StringList>().SelectedIndex == 0 && useq;
            bool dynamic = _menu.Item("Yule.katarina.combo.order").GetValue<StringList>().SelectedIndex == 2;
            bool smartcombo = _menu.Item("Yule.katarina.combo.mode").GetValue<StringList>().SelectedIndex == 0;
            Obj_AI_Hero target = TargetSelector.GetTarget(!startWithQ || dynamic ? E.Range : Q.Range, TargetSelector.DamageType.Magical);
            if(target != null && !target.IsZombie)
            {
                if (useq && (startWithQ || !usee || dynamic) && target.Distance(Player)<Q.Range)
                {
                    Q.Cast(target);
                    qTarget = target;
                    return;
                }
                if (usee && (usew || user || qTarget != target || !smartcombo))
                {
                    E.Cast(target);
                    return;
                }
                if (anyTarget)
                {
                    List<Obj_AI_Hero> enemies = Player.Position.GetEnemiesInRange(390);
                    if (enemies.Count >= 2)
                    {
                        W.Cast();
                        return;
                    }
                    if (enemies.Count == 1)
                    {
                        target = enemies.ElementAt(0);
                    }
                }
                if (target.Distance(Player) < 390 && usew && (user || qTarget != target || !smartcombo))
                {
                    W.Cast();
                    return;
                }
                if (target.Distance(Player) < R.Range - 200 && user)
                {
                    R.Cast();
                }
            }
        }

        private static void Turret_OnTarget(Obj_AI_Base sender, Obj_AI_BaseTargetEventArgs args)
        {
            if (sender.GetType() == typeof (Obj_AI_Turret))
            {
                TurretHasAggro[sender.NetworkId] = !(args.Target == null || args.Target is Obj_AI_Minion);
                //Game.PrintChat("Turret with Index[" + sender.Index + "] has Aggro: " + (TurretHasAggro[sender.Index]? "yes" : "no"));
            }
        }

        public static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name == "KatarinaQ" && args.Target.GetType() == typeof(Obj_AI_Hero))
            {
                qTarget = (Obj_AI_Hero) args.Target;
            }
            if (args.SData.Name == "katarinaE")
            {
                WardJumpReady = false;
            }
            if (sender.IsMe && WardJumpReady)
            {
                E.Cast((Obj_AI_Base)args.Target);
                WardJumpReady = false;
            }
            //Todo Check for Lee Q
            if (args.SData.Name == "blindmonkqtwo")
            {

                if (lastLeeQTick - Utils.TickCount <= 10)
                {
                    //Game.PrintChat("Trying to Jump undeder Ally Turret - OnProcessSpellCast");
                    JumpUnderTurret(-100,sender.Position);
                }
                lastLeeQTick = Utils.TickCount;
            }
            // Todo Test
            if (args.Target != null && args.Target.IsMe && _menu.Item("Yule.katarina.misc.allyTurret").GetValue<bool>())
            {
                switch (args.SData.Name)
                {
                    case "ZedR":
                        JumpUnderTurret(-100,sender.Position);
                        break;
                    case "ViR":
                        JumpUnderTurret(100,sender.Position);
                        break;
                    case "NocturneParanoia":
                        JumpUnderTurret(100,sender.Position);
                        break;
                    case "MaokaiUnstableGrowth":
                        JumpUnderTurret(0,sender.Position);
                        break;
                }

            }

        }

        

        private static void JumpUnderTurret(float extrarange, Vector3 objectPosition)
        {
            float mindistance = 100000;
            //Getting next Turret
            Obj_AI_Turret turretToJump = null;

            foreach (int NetID in AllAllyTurret)
            {
                Obj_AI_Turret turret = ObjectManager.GetUnitByNetworkId<Obj_AI_Turret>(NetID);
                if (turret != null && !turret.IsDead )
                {
                    float distance = Player.Position.Distance(turret.Position);
                    if (mindistance >= distance)
                    {
                        mindistance = distance;
                        turretToJump = turret;
                    }
                    
                }
            }
            if (turretToJump != null && !TurretHasAggro[turretToJump.NetworkId] && Player.Position.Distance(turretToJump.Position) < 1500)
            {
                int i = 0;
                
                do
                {
                    Vector3 extPos = Player.Position.Extend(turretToJump.Position, 685 - i);
                    float dist = objectPosition.Distance(extPos + extrarange);
                    Vector3 predictedPosition = objectPosition.Extend(extPos, dist);
                    if (predictedPosition.Distance(turretToJump.Position) <= 890 && !predictedPosition.IsWall())
                    {
                        WardJump(Player.Position.Extend(turretToJump.Position, 650 - i), false);
                        JumpPosition = Player.Position.Extend(turretToJump.Position, 650 - i);
                        ShallJumpNow = true;
                        break;
                    }

                    i += 50;
                } while (i <= 300 || !Player.Position.Extend(turretToJump.Position, 650 - i).IsWall());
            }
            
        }


        static void Demark()
        {
            if ((qTarget!=null && qTarget.HasBuff("katarinaqmark")) || Q.Cooldown < 3)
            {
                qTarget = null;
            }
        }


        #region WardJumping
        private static void WardJump(Vector3 where,bool move = true,bool placeward = true)
        {
            if (move)
                Orbwalking.MoveTo(Game.CursorPos);
            if (!E.IsReady())
            {
                return;
            }
            Vector3 wardJumpPosition = Player.Position.Distance(where) < 600 ? where : Player.Position.Extend(where, 600);
            var lstGameObjects = ObjectManager.Get<Obj_AI_Base>().ToArray();
            Obj_AI_Base entityToWardJump = lstGameObjects.FirstOrDefault(obj =>
                obj.Position.Distance(wardJumpPosition) < 150
                && (obj is Obj_AI_Minion || obj is Obj_AI_Hero)
                && !obj.IsMe && !obj.IsDead
                && obj.Position.Distance(Player.Position) < E.Range);

            if (entityToWardJump != null)
            {
                E.Cast(entityToWardJump);
            }
            else if(placeward)
            {
                int wardId = GetWardItem();
                if (wardId != -1 && !wardJumpPosition.IsWall())
                {
                    WardJumpReady = true;
                    PutWard(wardJumpPosition.To2D(), (ItemId)wardId);
                }
            }

        }

        public static int GetWardItem()
        {
            int[] wardItems = { 3340, 3350, 3205, 3207, 2049, 2045, 2044, 3361, 3154, 3362, 3160, 2043 };
            foreach (var id in wardItems.Where(id => Items.HasItem(id) && Items.CanUseItem(id)))
                return id;
            return -1;
        }

        public static void PutWard(Vector2 pos, ItemId warditem)
        {

            foreach (var slot in Player.InventoryItems.Where(slot => slot.Id == warditem))
            {
                ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, pos.To3D());
                return;
            }
        }
        #endregion
        //Calculating Damage
        static float CalculateDamage(Obj_AI_Hero target)
        {
            double damage = 0d;
            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1);
            }
            if (target.HasBuff("katarinaqmark") || target == qTarget)
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q, 1);
            }
            if (W.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.W);
            }
            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (R.IsReady() || (Player.GetSpell(R.Slot).State == SpellState.Surpressed && R.Level > 0))
            {
                damage += Player.GetSpellDamage(target, SpellSlot.R);
            }
            if (Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > 0 && IgniteSpellSlot != SpellSlot.Unknown && IgniteSpellSlot.IsReady())
            {
                damage += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                damage -= target.HPRegenRate*2.5;
            }
            return (float)damage;
        }

        #region Killsteal
        static int CanKill(Obj_AI_Hero target, bool useq, bool usew, bool usee, bool usef)
        {
            double damage = 0;
            if (!useq && !usew && !usee &&!usef)
                return 0;
            if (Q.IsReady() && useq)
            {
                damage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
                if ((W.IsReady() && usew) || (E.IsReady() && usee))
                {
                    damage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q, 1);
                }
            }
            if (target.HasBuff("katarinaqmark") || target == qTarget)
            {
                damage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q, 1);
            }
            if (W.IsReady() && usew)
            {
                damage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
            }
            if (E.IsReady() && usee)
            {
                damage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (damage >= target.Health)
            {
                return 1;
            }
            if (Player.GetSummonerSpellDamage(target,Damage.SummonerSpell.Ignite) > 0 && !target.HasBuff("summonerdot") && !HasRBuff() && IgniteSpellSlot != SpellSlot.Unknown && IgniteSpellSlot.IsReady())
            {
                damage += Player.GetSummonerSpellDamage(target,Damage.SummonerSpell.Ignite);
                damage -= target.HPRegenRate*2.5;
            }
            return damage >= target.Health? 2 : 0;

        }

        private static void Killsteal()
        {
            foreach (Obj_AI_Hero enemy in AllEnemy)
            {
                if (enemy == null || enemy.HasBuffOfType(BuffType.Invulnerability))
                    return;
                
                if (CanKill(enemy, false, _menu.Item("Yule.katarina.killsteal.usew").GetValue<bool>(), false, false)==1 && enemy.IsValidTarget(390))
                {
                    W.Cast(enemy);
                    return;
                }
                if (CanKill(enemy, false, false, _menu.Item("Yule.katarina.killsteal.usee").GetValue<bool>(), false)==1 && enemy.IsValidTarget(700))
                {
                    E.Cast(enemy);
                    return;
                }
                if (CanKill(enemy, _menu.Item("Yule.katarina.killsteal.useq").GetValue<bool>(), false, false, false)==1 && enemy.IsValidTarget(675))
                {
                    Q.Cast(enemy);
                    qTarget = enemy;
                    return;
                }
                int cankill = CanKill(enemy, _menu.Item("Yule.katarina.killsteal.useq").GetValue<bool>(),_menu.Item("Yule.katarina.killsteal.usew").GetValue<bool>(),_menu.Item("Yule.katarina.killsteal.usee").GetValue<bool>(),_menu.Item("Yule.katarina.killsteal.usef").GetValue<bool>());
                if (( cankill==1 || cankill == 2) && enemy.IsValidTarget(Q.Range))
                {
                    if (cankill == 2 && enemy.IsValidTarget(600))
                        Player.Spellbook.CastSpell(IgniteSpellSlot,enemy);
                    if (Q.IsReady())
                        Q.Cast(enemy);
                    if (E.IsReady() && (W.IsReady() || qTarget != enemy))
                        E.Cast(enemy);
                    if (W.IsReady() && enemy.IsValidTarget(390) && qTarget != enemy)
                        W.Cast();
                    return;
                }
                //KS with Wardjump
                cankill = CanKill(enemy, true, false, false,_menu.Item("Yule.katarina.killsteal.usef").GetValue<bool>());
                if (_menu.Item("Yule.katarina.killsteal.wardjump").GetValue<bool>() && (cankill ==1 || cankill ==2) && enemy.IsValidTarget(1300) && Q.IsReady() && E.IsReady())
                {
                    WardJump(enemy.Position, false);
                    if (cankill == 2 && enemy.IsValidTarget(600))
                        Player.Spellbook.CastSpell(IgniteSpellSlot, enemy);
                    if (enemy.IsValidTarget(675))
                        Q.Cast(enemy);
                    return;
                }
            }
        }
        #endregion

        

        #region Lasthit

        private static void Lasthit()
        {
            
            if (_orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit || (_menu.Item("Yule.katarina.performance.tickmanager").GetValue<bool>() && Utils.TickCount < tickValue))
                return;
            Obj_AI_Base[] sourroundingMinions;
            int tickCount = _menu.Item("Yule.katarina.performance.ticks").GetValue<Slider>().Value;
            tickValue = Utils.TickCount + tickCount;
            if (_menu.Item("Yule.katarina.lasthit.usew").GetValue<bool>() && W.IsReady())
            {
                sourroundingMinions = MinionManager.GetMinions(Player.Position, 390).Take(3).ToArray();
                {
                    //Only Cast W when minion is not killable with Autoattacks
                    if (
                        sourroundingMinions.Any(
                            minion =>
                                !minion.IsDead && _orbwalker.GetTarget() != minion && (qMinion == null || minion != qMinion) &&
                                W.GetDamage(minion) > minion.Health &&
                                HealthPrediction.GetHealthPrediction(minion,
                                    (Player.CanAttack
                                        ? Game.Ping/2
                                        : Orbwalking.LastAATick - Utils.GameTimeTickCount +
                                          (int) Player.AttackDelay*1000) + 200 + (_menu.Item("Yule.katarina.performance.tickmanager").GetValue<bool>()?tickCount-1:0)+(int) Player.AttackCastDelay*1000) <= 0))
                    {
                        W.Cast();
                    }
                }

            }
            if (_menu.Item("Yule.katarina.lasthit.useq").GetValue<bool>() && Q.IsReady())
            {
                sourroundingMinions = MinionManager.GetMinions(Player.Position, Q.Range).ToArray();
                foreach (var minion in sourroundingMinions.Where(minion => !minion.IsDead && Q.GetDamage(minion) > minion.Health))
                {
                    Q.Cast(minion);
                    qMinion = minion;
                    break;
                }
            }
            if (_menu.Item("Yule.katarina.lasthit.usee").GetValue<bool>() && E.IsReady() && (!_menu.Item("Yule.katarina.lasthit.noenemiese").GetValue<bool>() || Player.GetEnemiesInRange(1000).Count == 0))
            {
                //Same Logic with W + not killable with W
                sourroundingMinions = MinionManager.GetMinions(Player.Position, E.Range).Take(_menu.Item("Yule.katarina.performance.track").GetValue<Slider>().Value).ToArray();
                {
                    foreach (var minions in sourroundingMinions.Where(
                        minion =>
                            !minion.IsDead && _orbwalker.GetTarget() != minion && (qMinion == null || minion != qMinion) &&
                            E.GetDamage(minion) >= minion.Health &&
                            (!W.IsReady() || !_menu.Item("Yule.katarina.lasthit.usew").GetValue<bool>() || Player.Position.Distance(minion.Position) > 390)
                            &&
                            HealthPrediction.GetHealthPrediction(minion,
                                (Player.CanAttack
                                    ? Game.Ping/2
                                    : Orbwalking.LastAATick - Utils.GameTimeTickCount + (int) Player.AttackDelay*1000) +
                                200 + (_menu.Item("Yule.katarina.performance.tickmanager").GetValue<bool>() ? tickCount - 1 : 0) + (int) Player.AttackCastDelay*1000) <= 0
                            &&
                            !IsTurretPosition(Player.Position.Extend(minion.Position,
                                Player.Position.Distance(minion.Position) + 35))))
                    {
                        E.Cast(minions);
                        break;
                    }
                }
            }
        }
        #endregion

        #region LaneClear
        private static void LaneClear()
        {
            if (_orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
                return;
            Obj_AI_Base[] sourroundingMinions;
            if (_menu.Item("Yule.katarina.laneclear.usew").GetValue<bool>() && W.IsReady())
            {
                sourroundingMinions = MinionManager.GetMinions(Player.Position, W.Range - 5).ToArray();
                if (sourroundingMinions.GetLength(0) >= _menu.Item("Yule.katarina.laneclear.minw").GetValue<Slider>().Value)
                {
                    int lasthittable = sourroundingMinions.Count(minion => W.GetDamage(minion) + (minion.HasBuff("katarinaqmark")? Q.GetDamage(minion,1) : 0) > minion.Health);
                    if (lasthittable >= _menu.Item("Yule.katarina.laneclear.minwlasthit").GetValue<Slider>().Value)
                    {
                        W.Cast();
                    }
                }
            }
            if (_menu.Item("Yule.katarina.laneclear.useq").GetValue<bool>() && Q.IsReady())
            {
                sourroundingMinions = MinionManager.GetMinions(Player.Position, Q.Range - 5).ToArray();
                foreach (var minion in sourroundingMinions.Where(minion => !minion.IsDead))
                {
                    Q.Cast(minion);
                    break;
                }
            }
        }
        #endregion

        #region Jungleclear

        private static void JungleClear()
        {
            Obj_AI_Base[] sourroundingMinions;
            if (_orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
                return;
            if (_menu.Item("Yule.katarina.jungleclear.useq").GetValue<bool>() && Q.IsReady())
            {
                sourroundingMinions = MinionManager.GetMinions(Player.Position, Q.Range, MinionTypes.All, MinionTeam.Neutral).ToArray();
                float maxhealth = 0;
                int chosenminion = 0;
                if (sourroundingMinions.GetLength(0) >= 1)
                {
                    for(int i = 0;i < sourroundingMinions.Length; i++)
                    {
                        if (maxhealth < sourroundingMinions[i].MaxHealth)
                        {
                            maxhealth = sourroundingMinions[i].MaxHealth;
                            chosenminion = i;
                        }
                    }
                    Q.Cast(sourroundingMinions[chosenminion]);
                }
            }
            if (_menu.Item("Yule.katarina.jungleclear.usew").GetValue<bool>() && W.IsReady())
            {
                sourroundingMinions = MinionManager.GetMinions(Player.Position, W.Range - 5, MinionTypes.All,MinionTeam.Neutral).ToArray();
                if (sourroundingMinions.GetLength(0) >= 1)
                {
                    W.Cast();
                }
            }
            if (_menu.Item("Yule.katarina.jungleclear.usee").GetValue<bool>() && E.IsReady())
            {
                sourroundingMinions = MinionManager.GetMinions(Player.Position, E.Range, MinionTypes.All, MinionTeam.Neutral).ToArray();
                float maxhealth = 0;
                int chosenminion = 0;
                if (sourroundingMinions.GetLength(0) >= 1)
                {
                    for (int i = 0; i < sourroundingMinions.Length; i++)
                    {
                        if (maxhealth < sourroundingMinions[i].MaxHealth)
                        {
                            maxhealth = sourroundingMinions[i].MaxHealth;
                            chosenminion = i;
                        }
                    }
                    E.Cast(sourroundingMinions[chosenminion]);
                }
            }
        }
        #endregion
        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe && HasRBuff() && Utils.GameTimeTickCount <= whenToCancelR && _menu.Item("Yule.katarina.misc.noRCancel").GetValue<bool>())
                args.Process = false;
        }

    }
}