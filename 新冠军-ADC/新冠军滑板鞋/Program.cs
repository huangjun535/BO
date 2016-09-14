using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Collision = LeagueSharp.Common.Collision;
using Color = System.Drawing.Color;
using YuLeLibrary;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using HealthPrediction = LeagueSharp.Common.HealthPrediction;



namespace SharpShooter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "Kalista")
                return;

            MenuProvider.Initialize();

            new Kalista();
            new WardUsing().LoadWard();
            new Tracker().LoadTrack();
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

    }

    class Tracker
    {
        public static List<ChampionInfo> ChampionInfoList = new List<ChampionInfo>();
        private Vector3 EnemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position;

        public void LoadTrack()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    ChampionInfoList.Add(new ChampionInfo() { NetworkId = hero.NetworkId, LastVisablePos = hero.Position });
                }
            }

            Game.OnUpdate += OnUpdate;
        }

        private void OnUpdate(EventArgs args)
        {
            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValid))
            {
                var ChampionInfoOne = ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);
                if (enemy.IsDead)
                {
                    if (ChampionInfoOne != null)
                    {
                        ChampionInfoOne.NetworkId = enemy.NetworkId;
                        ChampionInfoOne.LastVisablePos = EnemySpawn;
                        ChampionInfoOne.LastVisableTime = Game.Time;
                        ChampionInfoOne.PredictedPos = EnemySpawn;
                    }
                }
                else if (enemy.IsVisible)
                {
                    Vector3 prepos = enemy.Position;

                    if (enemy.IsMoving)
                        prepos = prepos.Extend(enemy.GetWaypoints().Last().To3D(), 125);

                    if (ChampionInfoOne == null)
                    {
                        ChampionInfoList.Add(new ChampionInfo() { NetworkId = enemy.NetworkId, LastVisablePos = enemy.Position, LastVisableTime = Game.Time, PredictedPos = prepos });
                    }
                    else
                    {
                        ChampionInfoOne.NetworkId = enemy.NetworkId;
                        ChampionInfoOne.LastVisablePos = enemy.Position;
                        ChampionInfoOne.LastVisableTime = Game.Time;
                        ChampionInfoOne.PredictedPos = prepos;
                    }
                }
            }
        }
    }

    class HiddenObj
    {
        public int type;
        public float endTime { get; set; }
        public Vector3 pos { get; set; }
    }

    class WardUsing
    {
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private Menu Config = MenuProvider.MenuInstance;
        private bool rengar = false;
        Obj_AI_Hero Vayne = null;
        private static Spell Q, W, E, R;

        public static List<HiddenObj> HiddenObjList = new List<HiddenObj>();

        private Items.Item
            VisionWard = new Items.Item(2043, 550f),
            OracleLens = new Items.Item(3364, 550f),
            WardN = new Items.Item(2044, 600f),
            TrinketN = new Items.Item(3340, 600f),
            SightStone = new Items.Item(2049, 600f),
            EOTOasis = new Items.Item(2302, 600f),
            EOTEquinox = new Items.Item(2303, 600f),
            EOTWatchers = new Items.Item(2301, 600f),
            FarsightOrb = new Items.Item(3342, 4000f),
            ScryingOrb = new Items.Item(3363, 3500f);

        public void LoadWard()
        {
            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R);

            Config.SubMenu("Misc").SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWard", "Auto Ward").SetValue(true));
            Config.SubMenu("Misc").SubMenu("AutoWard OKTW©").AddItem(new MenuItem("autoBuy", "Auto buy blue trinket after lvl 9").SetValue(false));
            Config.SubMenu("Misc").SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWardBlue", "Auto Blue Trinket").SetValue(true));
            Config.SubMenu("Misc").SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWardCombo", "Only combo mode").SetValue(true));
            Config.SubMenu("Misc").SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWardPink", "Auto VisionWard, OracleLens").SetValue(true));

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    if (hero.ChampionName == "Rengar")
                        rengar = true;
                    if (hero.ChampionName == "Vayne")
                        Vayne = hero;
                }
            }

            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsRecalling() || Player.IsDead)
                return;

            foreach (var obj in HiddenObjList)
            {
                if (obj.endTime < Game.Time)
                {
                    HiddenObjList.Remove(obj);
                    return;
                }
            }

            if (Config.Item("autoBuy").GetValue<bool>() && Player.InFountain() && !ScryingOrb.IsOwned() && Player.Level >= 9)
                Player.BuyItem(ItemId.Farsight_Orb_Trinket);

            if (rengar && Player.HasBuff("rengarralertsound"))
                CastVisionWards(Player.ServerPosition);

            if (Vayne != null && Vayne.IsValidTarget(1000) && Vayne.HasBuff("vaynetumblefade"))
                CastVisionWards(Vayne.ServerPosition);

            AutoWardLogic();
        }

        private void AutoWardLogic()
        {
            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValid && !enemy.IsVisible && !enemy.IsDead))
            {
                var need = Tracker.ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);

                if (need == null || need.PredictedPos == null)
                    continue;

                var PPDistance = need.PredictedPos.Distance(Player.Position);

                if (PPDistance > 1400)
                    continue;

                var timer = Game.Time - need.LastVisableTime;

                if (timer < 4)
                {
                    if (Config.Item("AutoWardCombo").GetValue<bool>() && !(MenuProvider.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
                        return;

                    if (NavMesh.IsWallOfGrass(need.PredictedPos, 0))
                    {
                        if (PPDistance < 600 && Config.Item("AutoWard").GetValue<bool>())
                        {
                            if (TrinketN.IsReady())
                            {
                                TrinketN.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                            else if (SightStone.IsReady())
                            {
                                SightStone.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                            else if (WardN.IsReady())
                            {
                                WardN.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                            else if (EOTOasis.IsReady())
                            {
                                EOTOasis.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                            else if (EOTEquinox.IsReady())
                            {
                                EOTEquinox.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                            else if (EOTWatchers.IsReady())
                            {
                                EOTWatchers.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                        }

                        if (Config.Item("AutoWardBlue").GetValue<bool>())
                        {
                            if (FarsightOrb.IsReady())
                            {
                                FarsightOrb.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                            else if (ScryingOrb.IsReady())
                            {
                                ScryingOrb.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                        }
                    }
                }
            }
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsEnemy || sender.IsAlly)
                return;

            if (sender is MissileClient)
            {
                var missile = (MissileClient)sender;

                if (!missile.SpellCaster.IsVisible)
                {

                    if ((missile.SData.Name == "BantamTrapShort" || missile.SData.Name == "BantamTrapBounceSpell") && !HiddenObjList.Exists(x => missile.EndPosition == x.pos))
                        AddWard("teemorcast", missile.EndPosition);
                }
            }
            else if (sender.Type == GameObjectType.obj_AI_Minion)
            {
                if ((sender.Name.ToLower() == "visionward" || sender.Name.ToLower() == "sightward") && !HiddenObjList.Exists(x => x.pos.Distance(sender.Position) < 100))
                {
                    foreach (var obj in HiddenObjList)
                    {
                        if (obj.pos.Distance(sender.Position) < 400)
                        {
                            if (obj.type == 0)
                            {
                                HiddenObjList.Remove(obj);
                                return;
                            }
                        }
                    }

                    var dupa = (Obj_AI_Minion)sender;
                    if (dupa.Mana == 0)
                        HiddenObjList.Add(new HiddenObj() { type = 2, pos = sender.Position, endTime = float.MaxValue });
                    else
                        HiddenObjList.Add(new HiddenObj() { type = 1, pos = sender.Position, endTime = Game.Time + dupa.Mana });
                }
            }
            else if (rengar && sender.Position.Distance(Player.Position) < 800)
            {
                switch (sender.Name)
                {
                    case "Rengar_LeapSound.troy":
                        CastVisionWards(sender.Position);
                        break;
                    case "Rengar_Base_R_Alert":
                        CastVisionWards(sender.Position);
                        break;
                }
            }
        }

        private void GameObject_OnDelete(GameObject sender, EventArgs args)
        {

            if (!sender.IsEnemy || sender.IsAlly || sender.Type != GameObjectType.obj_AI_Minion)
                return;

            foreach (var obj in HiddenObjList)
            {
                if (obj.pos == sender.Position)
                {
                    HiddenObjList.Remove(obj);
                    return;
                }
                else if (obj.type == 3 && obj.pos.Distance(sender.Position) < 100)
                {
                    HiddenObjList.Remove(obj);
                    return;
                }
                else if (obj.pos.Distance(sender.Position) < 400)
                {
                    if (obj.type == 2 && sender.Name.ToLower() == "visionward")
                    {
                        HiddenObjList.Remove(obj);
                        return;
                    }
                    else if ((obj.type == 0 || obj.type == 1) && sender.Name.ToLower() == "sightward")
                    {
                        HiddenObjList.Remove(obj);
                        return;
                    }
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && !sender.IsMinion && sender.Type == GameObjectType.obj_AI_Hero)
            {
                if (args.Target == null)
                    AddWard(args.SData.Name.ToLower(), args.End);

                if ((OracleLens.IsReady() || VisionWard.IsReady()) && sender.Distance(Player.Position) < 800)
                {
                    switch (args.SData.Name.ToLower())
                    {
                        case "akalismokebomb":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "deceive":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "khazixr":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "khazixrlong":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "talonshadowassault":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "monkeykingdecoy":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "RengarR":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "TwitchHideInShadows":
                            CastVisionWards(sender.ServerPosition);
                            break;
                    }
                }
            }
        }

        private void AddWard(string name, Vector3 posCast)
        {
            switch (name)
            {
                //PINKS
                case "visionward":
                    HiddenObjList.Add(new HiddenObj() { type = 2, pos = posCast, endTime = float.MaxValue });
                    break;
                case "trinkettotemlvl3B":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                //SIGH WARD
                case "itemghostward":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                case "wrigglelantern":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                case "sightward":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                case "itemferalflare":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                //TRINKET
                case "trinkettotemlvl1":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 60 });
                    break;
                case "trinkettotemlvl2":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 120 });
                    break;
                case "trinkettotemlvl3":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                //others
                case "teemorcast":
                    HiddenObjList.Add(new HiddenObj() { type = 3, pos = posCast, endTime = Game.Time + 300 });
                    break;
                case "noxious trap":
                    HiddenObjList.Add(new HiddenObj() { type = 3, pos = posCast, endTime = Game.Time + 300 });
                    break;
                case "JackInTheBox":
                    HiddenObjList.Add(new HiddenObj() { type = 3, pos = posCast, endTime = Game.Time + 100 });
                    break;
                case "Jack In The Box":
                    HiddenObjList.Add(new HiddenObj() { type = 3, pos = posCast, endTime = Game.Time + 100 });
                    break;
            }
        }

        private void CastVisionWards(Vector3 position)
        {
            if (Config.Item("AutoWardPink").GetValue<bool>())
            {
                if (OracleLens.IsReady())
                    OracleLens.Cast(Player.Position.Extend(position, OracleLens.Range));
                else if (VisionWard.IsReady())
                    VisionWard.Cast(Player.Position.Extend(position, VisionWard.Range));
            }
        }
    }

    public class ChampionInfo
    {
        public int NetworkId { get; set; }
        public Vector3 LastVisablePos { get; set; }
        public float LastVisableTime { get; set; }
        public Vector3 PredictedPos { get; set; }
        public float StartRecallTime { get; set; }
        public float AbortRecallTime { get; set; }
        public float FinishRecallTime { get; set; }

        public ChampionInfo()
        {
            LastVisableTime = Game.Time;
            StartRecallTime = 0;
            AbortRecallTime = 0;
            FinishRecallTime = 0;
        }
    }

    public class Kalista
    {
        private readonly int[] _qManaCost = { 0, 50, 55, 60, 65, 70 };
        private readonly Vector3 _baronLocation;
        private readonly Vector3 _dragonLocation;
        private int _eLastCastTime;
        private readonly Spell _q;
        private readonly Spell _w;
        private readonly Spell _e;
        private readonly Spell _r;
        public Items.Item
            VisionWard = new Items.Item(2043, 550f),
            OracleLens = new Items.Item(3364, 550f),
            WardN = new Items.Item(2044, 600f),
            TrinketN = new Items.Item(3340, 600f),
            SightStone = new Items.Item(2049, 600f),
            EOTOasis = new Items.Item(2302, 600f),
            EOTEquinox = new Items.Item(2303, 600f),
            EOTWatchers = new Items.Item(2301, 600f),
            ScryingOrb = new Items.Item(3363, 3500f);
        public List<ChampionInfo> ChampionInfoList = new List<ChampionInfo>();
        private List<Obj_AI_Hero> Enemies = new List<Obj_AI_Hero>();

        public Kalista()
        {
            _q = new Spell(SpellSlot.Q, 1150f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };
            _w = new Spell(SpellSlot.W, 5000f);
            _e = new Spell(SpellSlot.E, 950f);
            _r = new Spell(SpellSlot.R, 1200f);

            _q.SetSkillshot(0.35f, 40f, 2400f, true, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.AddUseQ();
            MenuProvider.Champion.Combo.AddUseE();
            MenuProvider.Champion.Combo.AddItem("连招利用小兵走砍", false);

            MenuProvider.Champion.Harass.AddUseQ();
            MenuProvider.Champion.Harass.AddIfMana();

            MenuProvider.Champion.Laneclear.AddUseQ(false);
            MenuProvider.Champion.Laneclear.AddItem("如果小兵个数 >=", new Slider(3, 1, 7));
            MenuProvider.Champion.Laneclear.AddUseE();
            MenuProvider.Champion.Laneclear.AddItem("如果小兵的个数 >=", new Slider(2, 1, 5));
            MenuProvider.Champion.Laneclear.AddIfMana(20);

            MenuProvider.Champion.Jungleclear.AddUseQ();
            MenuProvider.Champion.Jungleclear.AddUseE();
            MenuProvider.Champion.Jungleclear.AddIfMana(20);

            MenuProvider.Champion.Misc.AddItem("Use Killsteal (With E)", true);
            MenuProvider.Champion.Misc.AddItem("Use Mobsteal (With E)", true);
            MenuProvider.Champion.Misc.AddItem("Use Lasthit Assist (With E)", true);
            MenuProvider.Champion.Misc.AddItem("Use Soulbound Saver (With R)", true);
            MenuProvider.Champion.Misc.AddItem("Auto Balista Combo (With R)", true);
            MenuProvider.Champion.Misc.AddItem("Auto Steal Siege minion & Super minion (With E)", true);
            MenuProvider.Champion.Misc.AddItem("Auto E Harass (With E)", true);
            MenuProvider.Champion.Misc.AddItem("^ Don't do this in ComboMode", false);
            MenuProvider.Champion.Misc.AddItem("Auto E Before Die", true);
            MenuProvider.Champion.Misc.AddItem("Auto W on Dragon or Baron (With W)", true);
            MenuProvider.Champion.Misc.AddItem("Cast W on Dragon", new KeyBind('J', KeyBindType.Press));
            MenuProvider.Champion.Misc.AddItem("Cast W on Baron", new KeyBind('K', KeyBindType.Press));

            MenuProvider.Champion.Drawings.AddDrawQrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawWrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawErange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddDrawRrange(Color.FromArgb(100, Color.DeepSkyBlue), false);
            MenuProvider.Champion.Drawings.AddItem("显示E技能百分比伤害", true);
            MenuProvider.Champion.Drawings.AddDamageIndicator(GetComboDamage);
            MenuProvider.Champion.Drawings.AddDamageIndicatorForJungle(GetJungleDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.OnNonKillableMinion += Orbwalking_OnNonKillableMinion;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            _baronLocation = new Vector3(5064f, 10568f, -71f);
            _dragonLocation = new Vector3(9796f, 4432f, -71f);
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (Orbwalking.CanMove(100))
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            {
                                ComboLogic();
                                break;
                            }
                        case Orbwalking.OrbwalkingMode.Mixed:
                            {
                                HarassLogic();
                                break;
                            }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                            {
                                Clear();
                                break;
                            }
                    }
                }

                if (MenuProvider.Champion.Misc.GetBoolValue("Use Killsteal (With E)"))
                    if (_e.IsReady())
                        if (
                            HeroManager.Enemies.Any(
                                x =>
                                    HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                    x.IsKillableAndValidTarget(_e.GetDamage(x) - 5, TargetSelector.DamageType.Physical,
                                        _e.Range)))
                            _e.Cast();

                if (MenuProvider.Champion.Misc.GetBoolValue("Use Mobsteal (With E)"))
                {
                    if (_e.IsReady())
                        if (
                            MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.Neutral,
                                MinionOrderTypes.MaxHealth)
                                .Any(
                                    x =>
                                        HealthPrediction.GetHealthPrediction(x, 500) > 0 &&
                                        x.IsKillableAndValidTarget(_e.GetDamage(x) - 5, TargetSelector.DamageType.Physical)))
                            _e.Cast();
                }

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto Steal Siege minion & Super minion (With E)"))
                {
                    if (_e.IsReady())
                        if (
                            MinionManager.GetMinions(_e.Range)
                                .Any(
                                    x =>
                                        HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                        x.IsKillableAndValidTarget(_e.GetDamage(x) - 5, TargetSelector.DamageType.Physical) &&
                                        (x.CharData.BaseSkinName.ToLower().Contains("siege") ||
                                         x.CharData.BaseSkinName.ToLower().Contains("super"))))
                            _e.Cast();
                }

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto Balista Combo (With R)"))
                    if (_r.IsReady())
                    {
                        var myBlitzcrank =
                            HeroManager.Allies.FirstOrDefault(
                                x => !x.IsDead && x.HasBuff("kalistacoopstrikeally") && x.ChampionName == "Blitzcrank");
                        if (myBlitzcrank != null)
                        {
                            var grabTarget =
                                HeroManager.Enemies.FirstOrDefault(x => !x.IsDead && x.HasBuff("rocketgrab2"));
                            if (grabTarget != null)
                                if (ObjectManager.Player.Distance(grabTarget) > myBlitzcrank.Distance(grabTarget))
                                    _r.Cast();
                        }
                    }

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto E Harass (With E)"))
                    if (_e.IsReady())
                        if (
                            !(MenuProvider.Champion.Misc.GetBoolValue("^ Don't do this in ComboMode") &&
                              MenuProvider.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
                            if (ObjectManager.Player.Mana - _e.ManaCost >= _e.ManaCost)
                                if (HeroManager.Enemies.Any(x => x.IsValidTarget(_e.Range) && _e.GetDamage(x) > 10))
                                    if (
                                        MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.NotAlly)
                                            .Any(
                                                x =>
                                                    HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                                    x.IsKillableAndValidTarget(_e.GetDamage(x) - 5,
                                                        TargetSelector.DamageType.Physical, _e.Range)))
                                        _e.Cast();

                if (MenuProvider.Champion.Misc.GetBoolValue("Auto W on Dragon or Baron (With W)"))
                    if (ObjectManager.Player.IsManaPercentOkay(50))
                        if (!ObjectManager.Player.IsRecalling())
                            if (ObjectManager.Player.Position.CountEnemiesInRange(1500f) <= 0)
                                if (MenuProvider.Orbwalker.GetTarget() == null)
                                {
                                    if (_w.IsReady())
                                        if (ObjectManager.Player.Distance(_baronLocation) <= _w.Range)
                                            _w.Cast(_baronLocation);

                                    if (_w.IsReady())
                                        if (ObjectManager.Player.Distance(_dragonLocation) <= _w.Range)
                                            _w.Cast(_dragonLocation);
                                }

                if (MenuProvider.Champion.Misc.GetKeyBindValue("Cast W on Dragon").Active)
                    if (_w.IsReady())
                        if (ObjectManager.Player.Distance(_dragonLocation) <= _w.Range)
                            _w.Cast(_dragonLocation);

                if (MenuProvider.Champion.Misc.GetKeyBindValue("Cast W on Baron").Active)
                    if (_w.IsReady())
                        if (ObjectManager.Player.Distance(_baronLocation) <= _w.Range)
                            _w.Cast(_baronLocation);
            }
        }

        private List<Obj_AI_Base> Q_GetCollisionMinions(Obj_AI_Hero source, Vector3 targetposition)
        {
            var input = new PredictionInput
            {
                Unit = source,
                Radius = _q.Width,
                Delay = _q.Delay,
                Speed = _q.Speed,
            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return Collision.GetCollision(new List<Vector3> { targetposition }, input).OrderBy(obj => obj.Distance(source, false)).ToList();
        }

        private void Clear()
        {
            if (MenuProvider.Champion.Laneclear.UseQ && _q.IsReady())
                if (
                    ObjectManager.Player.IsManaPercentOkay(
                        MenuProvider.Champion.Laneclear.IfMana))
                {
                    var Minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range, MinionTypes.All, MinionTeam.Enemy);

                    if (Minions.Count <= 0)
                        return;

                    foreach (var minion in Minions.Where(x => x.Health <= _q.GetDamage(x)))
                    {
                        var killcount = 0;

                        foreach (var colminion in Q_GetCollisionMinions(ObjectManager.Player, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q.Range)))
                        {
                            if (colminion.Health <= _q.GetDamage(colminion))
                                killcount++;
                            else
                                break;
                        }

                        if (killcount >=  MenuProvider.Champion.Laneclear.GetSliderValue("如果小兵个数 >=").Value)
                        {
                            if (!ObjectManager.Player.IsWindingUp && !ObjectManager.Player.IsDashing())
                            {
                                _q.Cast(minion.ServerPosition);
                                break;
                            }
                        }
                    }
                }

            if (MenuProvider.Champion.Laneclear.UseE)
                if (_e.IsReady())
                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                    {
                        var minionkillcount = 0;
                        var Minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range, MinionTypes.All, MinionTeam.Enemy);
                        foreach (var Minion in Minions.Where(x => _e.CanCast(x) && x.Health <= _e.GetDamage(x) - 5)) { minionkillcount++; }

                        if (minionkillcount >= MenuProvider.Champion.Laneclear.GetSliderValue("如果小兵的个数 >=").Value)
                            _e.Cast();
                    }

            if (MenuProvider.Champion.Jungleclear.UseQ)
                if (_q.IsReady())
                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                    {
                        var qTarget =
                            MinionManager.GetMinions(_q.Range, MinionTypes.All, MinionTeam.Neutral,
                                MinionOrderTypes.MaxHealth)
                                .FirstOrDefault(
                                    x =>
                                        x.IsValidTarget(_q.Range) &&
                                        _q.GetPrediction(x).Hitchance >= HitChance.High);

                        if (qTarget != null)
                            _q.Cast(qTarget);
                    }

            if (MenuProvider.Champion.Jungleclear.UseE)
                if (_e.IsReady())
                    if (ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                        if (MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.Neutral,
                                MinionOrderTypes.MaxHealth)
                                .Any(x => HealthPrediction.GetHealthPrediction(x, 250) > 0 && _e.GetDamage(x) - 5 > x.Health))
                            _e.Cast();
        }

        private void ComboLogic()
        {
            if (MenuProvider.Champion.Combo.GetBoolValue("连招利用小兵走砍"))
                if (MenuProvider.Orbwalker.GetTarget() == null)
                    if (
                        !HeroManager.Enemies.Any(
                            x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x)))
                    {
                        var minion =
                            MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(null) + 65,
                                MinionTypes.All, MinionTeam.NotAlly)
                                .Where(x => x.IsValidTarget())
                                .OrderBy(x => x.Distance(ObjectManager.Player))
                                .FirstOrDefault();
                        if (minion != null)
                            Orbwalking.Orbwalk(minion, Game.CursorPos, 0f);
                    }

            if (MenuProvider.Champion.Combo.UseQ && _q.IsReady())
            {
                var target = TargetSelector.GetTargetNoCollision(_q);

                if (target != null)
                {
                    if (ObjectManager.Player.Mana - _qManaCost[_q.Level] >= 40)
                    {
                        _q.CastIfHitchanceEquals(target, _q.MinHitChance);
                    }
                    else
                    {
                        var killableTarget = HeroManager.Enemies.FirstOrDefault(x => !Orbwalking.InAutoAttackRange(x) && x.IsKillableAndValidTarget(_q.GetDamage(x), TargetSelector.DamageType.Physical, _q.Range) && _q.GetPrediction(x).Hitchance >= _q.MinHitChance);

                        if (killableTarget != null)
                        {
                            _q.Cast(killableTarget);
                        }
                    }
                }
            }

            if (MenuProvider.Champion.Combo.UseE && _e.IsReady())
            {
                var emeny = HeroManager.Enemies.Find(x => HealthPrediction.GetHealthPrediction(x, 250) > 0 && x.IsKillableAndValidTarget(_e.GetDamage(x) - 5, TargetSelector.DamageType.Physical, _e.Range));

                if(emeny != null)
                    _e.Cast();
            }
                        
        }

        private void HarassLogic()
        {
            if (MenuProvider.Champion.Harass.UseQ && ObjectManager.Player.IsManaPercentOkay(MenuProvider.Champion.Harass.IfMana) && _q.IsReady())
            {
                var target = TargetSelector.GetTargetNoCollision(_q);

                if (target != null)
                    _q.Cast(target);
            }
        }

        private void Orbwalking_OnNonKillableMinion(AttackableUnit minion)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var Minion = minion as Obj_AI_Minion;
                if (MenuProvider.Champion.Misc.GetBoolValue("Use Lasthit Assist (With E)"))
                    if (_e.IsReady())
                        if (Minion.IsKillableAndValidTarget(_e.GetDamage(Minion), TargetSelector.DamageType.Physical))
                            if (HealthPrediction.GetHealthPrediction(Minion, 250) > 0)
                                if (!HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                                    _e.Cast();
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
                if (sender.Owner.IsMe)
                    if (args.Slot == SpellSlot.E)
                        if (_eLastCastTime > Utils.TickCount - 700)
                            args.Process = false;
                        else
                            _eLastCastTime = Utils.TickCount;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && args.Target != null)
                if (sender.IsEnemy)
                    if (sender.Type == GameObjectType.obj_AI_Hero)
                    {
                        if (MenuProvider.Champion.Misc.GetBoolValue("Use Soulbound Saver (With R)"))
                            if (_r.IsReady())
                            {
                                var soulbound =
                                    HeroManager.Allies.FirstOrDefault(
                                        x => !x.IsDead && x.HasBuff("kalistacoopstrikeally"));
                                if (soulbound != null)
                                    if (args.Target.NetworkId == soulbound.NetworkId ||
                                        args.End.Distance(soulbound.Position) <= 200)
                                        if (soulbound.HealthPercent < 20)
                                            _r.Cast();
                            }

                        if (MenuProvider.Champion.Misc.GetBoolValue("Auto E Before Die"))
                            if (args.Target.IsMe)
                                if (ObjectManager.Player.HealthPercent <= 10)
                                    if (_e.IsReady())
                                        if (
                                            HeroManager.Enemies.Any(
                                                x => x.IsValidTarget(_e.Range) && _e.GetDamage(x) - 5 > 0))
                                            _e.Cast();
                    }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.Champion.Drawings.DrawQrange.Active && _q.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range,
                        MenuProvider.Champion.Drawings.DrawQrange.Color);

                if (MenuProvider.Champion.Drawings.DrawWrange.Active && _w.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range,
                        MenuProvider.Champion.Drawings.DrawWrange.Color);

                if (MenuProvider.Champion.Drawings.DrawErange.Active && _e.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range,
                        MenuProvider.Champion.Drawings.DrawErange.Color);

                if (MenuProvider.Champion.Drawings.DrawRrange.Active && _r.IsReady())
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range,
                        MenuProvider.Champion.Drawings.DrawRrange.Color);

                if (MenuProvider.Champion.Drawings.GetBoolValue("显示E技能百分比伤害"))
                {
                    foreach (var target in HeroManager.Enemies.Where(x => !x.IsDead && x.IsVisible))
                    {
                        if (_e.GetDamage(target) > 2)
                        {
                            var targetPos = Drawing.WorldToScreen(target.Position);
                            var damagePercent = (_e.GetDamage(target) / target.Health) * 100;

                            if (damagePercent > 0)
                                Drawing.DrawText(targetPos.X, targetPos.Y - 100,
                                    damagePercent >= 100 ? Color.Red : Color.GreenYellow, damagePercent.ToString("0.0"));
                        }
                    }

                    foreach (
                        var target in
                            MinionManager.GetMinions(float.MaxValue, MinionTypes.All, MinionTeam.Neutral)
                                .Where(x => !x.IsDead && x.IsVisible))
                    {
                        if (_e.GetDamage(target) > 2)
                        {
                            var targetPos = Drawing.WorldToScreen(target.Position);
                            var damagePercent = (_e.GetDamage(target) / target.Health) * 100;

                            if (damagePercent > 0)
                                Drawing.DrawText(targetPos.X, targetPos.Y - 100,
                                    damagePercent >= 100 ? Color.Red : Color.GreenYellow, damagePercent.ToString("0.0"));
                        }
                    }
                }
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = _e.GetDamage(enemy);

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                damage *= 0.6f;

            if (enemy.HasBuff("FerociousHowl"))
                damage *= 0.3f;

            return _e.IsReady() ? damage : 0;
        }

        private float GetJungleDamage(Obj_AI_Minion enemy)
        {
            var damage = _e.GetDamage(enemy);
            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                damage *= 0.6f;

            var dragonSlayerBuff = ObjectManager.Player.GetBuff("s5test_dragonslayerbuff");
            if (dragonSlayerBuff != null)
            {
                if (dragonSlayerBuff.Count >= 4)
                    damage += dragonSlayerBuff.Count == 5 ? damage * 0.30f : damage * 0.15f;

                if (enemy.CharData.BaseSkinName.ToLowerInvariant().Contains("dragon"))
                    damage *= 1 - dragonSlayerBuff.Count * 0.07f;
            }

            if (enemy.CharData.BaseSkinName.ToLowerInvariant().Contains("baron") &&
                ObjectManager.Player.HasBuff("barontarget"))
                damage *= 0.5f;

            return _e.IsReady() ? damage : 0;
        }
    }

    internal class DamageIndicator
    {
        public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);

        private const int XOffset = 10;
        private const int YOffset = 20;
        private const int Width = 103;
        private const int Height = 8;

        public static Color Color = Color.Lime;
        public static Color FillColor = Color.Goldenrod;
        public static bool Fill = true;

        public static bool Enabled = true;
        private static DamageToUnitDelegate _damageToUnit;

        private static readonly Render.Text Text = new Render.Text(0, 0, "", 14, SharpDX.Color.Red, "monospace");

        public static DamageToUnitDelegate DamageToUnit
        {
            get { return _damageToUnit; }

            set
            {
                if (_damageToUnit == null)
                {
                    Drawing.OnDraw += Drawing_OnDraw;
                }
                _damageToUnit = value;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Enabled || _damageToUnit == null)
            {
                return;
            }

            foreach (var unit in HeroManager.Enemies.Where(h => h.IsValid && h.IsHPBarRendered))
            {
                var damage = _damageToUnit(unit);

                if (damage > 2)
                {
                    var barPos = unit.HPBarPosition;

                    var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                    var yPos = barPos.Y + YOffset;
                    var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                    var xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                    if (damage > unit.Health)
                    {
                        Text.X = (int)barPos.X + XOffset;
                        Text.Y = (int)barPos.Y + YOffset - 13;
                        Text.text = "KILLABLE: " + (unit.Health - damage);
                        Text.OnEndScene();
                    }

                    Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 1, Color);

                    if (Fill)
                    {
                        var differenceInHp = xPosCurrentHp - xPosDamage;
                        var pos1 = barPos.X + 9 + 107 * percentHealthAfterDamage;

                        for (var i = 0; i < differenceInHp; i++)
                        {
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, FillColor);
                        }
                    }
                }
            }
        }
    }

    internal class DamageIndicatorForJungle
    {
        public delegate float DamageToUnitDelegate(Obj_AI_Minion hero);

        public static Color Color = Color.Lime;
        public static Color FillColor = Color.Goldenrod;
        public static bool Fill = true;

        public static bool Enabled = true;
        private static DamageToUnitDelegate _damageToUnit;

        public static List<JungleMobOffsets> JungleMobOffsetsList = new List<JungleMobOffsets>
        {
            new JungleMobOffsets {BaseSkinName = "SRU_Red", Width = 139, Height = 4, XOffset = 6, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "SRU_RedMini", Width = 49, Height = 2, XOffset = 36, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "SRU_Blue", Width = 139, Height = 4, XOffset = 6, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "SRU_BlueMini", Width = 49, Height = 2, XOffset = 36, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "SRU_BlueMini2", Width = 49, Height = 2, XOffset = 36, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "SRU_Gromp", Width = 86, Height = 2, XOffset = 62, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "Sru_Crab", Width = 60, Height = 2, XOffset = 45, YOffset = 36},
            new JungleMobOffsets {BaseSkinName = "SRU_Dragon", Width = 140, Height = 4, XOffset = 5, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "SRU_Baron", Width = 190, Height = 4, XOffset = -20, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "SRU_Krug", Width = 80, Height = 2, XOffset = 58, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "SRU_KrugMini", Width = 55, Height = 2, XOffset = 40, YOffset = 20},
            new JungleMobOffsets {BaseSkinName = "SRU_Razorbeak", Width = 74, Height = 2, XOffset = 53, YOffset = 22},
            new JungleMobOffsets
            {
                BaseSkinName = "SRU_RazorbeakMini",
                Width = 49,
                Height = 2,
                XOffset = 36,
                YOffset = 20
            },
            new JungleMobOffsets {BaseSkinName = "SRU_Murkwolf", Width = 74, Height = 2, XOffset = 53, YOffset = 22},
            new JungleMobOffsets {BaseSkinName = "SRU_MurkwolfMini", Width = 55, Height = 2, XOffset = 40, YOffset = 20}
            //new JungleMobOffsets { BaseSkinName = "SRU_ChaosMinionMelee", Width = 62, Height = 2, XOffset = 44, YOffset= 21 },
            //new JungleMobOffsets { BaseSkinName = "SRU_ChaosMinionSiege", Width = 60, Height = 2, XOffset = 44, YOffset= 21 },
            //new JungleMobOffsets { BaseSkinName = "SRU_ChaosMinionSuper", Width = 55, Height = 2, XOffset = 44, YOffset= 21 },
            //new JungleMobOffsets { BaseSkinName = "SRU_ChaosMinionRanged", Width = 62, Height = 2, XOffset = 44, YOffset= 21 },
            //new JungleMobOffsets { BaseSkinName = "SRU_OrderMinionMelee", Width = 62, Height = 2, XOffset = 44, YOffset= 21 },
            //new JungleMobOffsets { BaseSkinName = "SRU_OrderMinionSiege", Width = 60, Height = 2, XOffset = 44, YOffset= 21 },
            //new JungleMobOffsets { BaseSkinName = "SRU_OrderMinionSuper", Width = 55, Height = 2, XOffset = 44, YOffset= 21 },
            //new JungleMobOffsets { BaseSkinName = "SRU_OrderMinionRanged", Width = 62, Height = 2, XOffset = 44, YOffset= 21 }
        };

        public static DamageToUnitDelegate DamageToUnit
        {
            get { return _damageToUnit; }

            set
            {
                if (_damageToUnit == null)
                {
                    Drawing.OnDraw += Drawing_OnDraw;
                }
                _damageToUnit = value;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Enabled || _damageToUnit == null)
            {
                return;
            }

            foreach (
                var unit in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(h => h.IsHPBarRendered && h.IsValid && h.Team == GameObjectTeam.Neutral))
            {
                if (_damageToUnit(unit) > 2)
                {
                    var mobOffset = JungleMobOffsetsList.Find(x => x.BaseSkinName == unit.CharData.BaseSkinName);
                    if (mobOffset != null)
                    {
                        var barPos = unit.HPBarPosition;
                        barPos.X += mobOffset.XOffset;
                        barPos.Y += mobOffset.YOffset;

                        var damage = _damageToUnit(unit);

                        if (damage > 0)
                        {
                            var hpPercent = unit.Health / unit.MaxHealth * 100;
                            var hpPrecentAfterDamage = (unit.Health - damage) / unit.MaxHealth * 100;
                            var drawStartXPos = barPos.X + mobOffset.Width * (hpPrecentAfterDamage / 100);
                            var drawEndXPos = barPos.X + mobOffset.Width * (hpPercent / 100);

                            if (unit.Health < damage)
                            {
                                drawStartXPos = barPos.X;
                            }

                            Drawing.DrawLine(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, mobOffset.Height, FillColor);
                        }
                    }
                }
            }
        }
    }

    internal class JungleMobOffsets
    {
        public string BaseSkinName;
        public int Height;
        public int Width;
        public int XOffset;
        public int YOffset;
    }

    internal class DynamicInitializer
    {
        internal static TV NewInstance<TV>() where TV : class
        {
            return ObjectGenerator(typeof(TV)) as TV;
        }

        internal static object NewInstance(Type type)
        {
            return ObjectGenerator(type);
        }

        private static object ObjectGenerator(Type type)
        {
            var target = type.GetConstructor(Type.EmptyTypes);
            var dynamic = new DynamicMethod(string.Empty, type, new Type[0], target.DeclaringType);
            var il = dynamic.GetILGenerator();
            il.DeclareLocal(target.DeclaringType);
            il.Emit(OpCodes.Newobj, target);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));
            return method();
        }
    }

    internal class MenuProvider
    {
        internal static Menu MenuInstance;
        internal static Orbwalking.Orbwalker Orbwalker;

        internal static void Initialize()
        {
            if (PluginLoader.CanLoadPlugin(ObjectManager.Player.ChampionName))
            {
                MenuInstance = new Menu("VIP滑板鞋Q群438230879", "kasilita", true);

                Champion.AddOrbwalker();
            }

            MenuInstance.AddToMainMenu();
        }

        internal static void AddSubMenu(string name, string displayName)
        {
            MenuInstance.AddSubMenu(new Menu(displayName, name));
        }

        internal class Champion
        {
            internal static void AddOrbwalker()
            {
                Orbwalker =
                    new Orbwalking.Orbwalker(
                        MenuInstance.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            }

            internal class Combo
            {
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                internal static bool UseE
                {
                    get { return GetBoolValue("Use E"); }
                }

                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance
                            .SubMenu("Combo")
                            .AddItem(new MenuItem("Combo." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance
                        .SubMenu("Combo")
                        .AddItem(new MenuItem("Combo." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Combo." + displayName, champUniq).GetValue<bool>();
                }

                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Combo." + displayName, champUniq).GetValue<Slider>();
                }

                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                internal static void AddUseE(bool enabled = true)
                {
                    AddItem("Use E", enabled);
                }
            }

            internal class Harass
            {
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                internal static int IfMana
                {
                    get { return GetSliderValue("If Mana >").Value; }
                }

                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance
                            .SubMenu("Harass")
                            .AddItem(new MenuItem("Harass." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance
                        .SubMenu("Harass")
                        .AddItem(new MenuItem("Harass." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Harass." + displayName, champUniq).GetValue<bool>();
                }

                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Harass." + displayName, champUniq).GetValue<Slider>();
                }

                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                internal static void AddIfMana(int defaultValue = 60)
                {
                    AddItem("If Mana >", new Slider(defaultValue));
                }
            }

            internal class Laneclear
            {
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                internal static bool UseE
                {
                    get { return GetBoolValue("Use E"); }
                }

                internal static int IfMana
                {
                    get { return GetSliderValue("If Mana >").Value; }
                }

                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance
                            .SubMenu("Laneclear")
                            .AddItem(new MenuItem("Laneclear." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance
                        .SubMenu("Laneclear")
                        .AddItem(new MenuItem("Laneclear." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Laneclear." + displayName, champUniq).GetValue<bool>();
                }

                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Laneclear." + displayName, champUniq).GetValue<Slider>();
                }

                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                internal static void AddUseE(bool enabled = true)
                {
                    AddItem("Use E", enabled);
                }

                internal static void AddIfMana(int defaultValue = 0)
                {
                    AddItem("If Mana >", new Slider(defaultValue));
                }
            }

            internal class Jungleclear
            {
                internal static bool UseQ
                {
                    get { return GetBoolValue("Use Q"); }
                }

                internal static bool UseE
                {
                    get { return GetBoolValue("Use E"); }
                }

                internal static int IfMana
                {
                    get { return GetSliderValue("If Mana >").Value; }
                }

                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance
                            .SubMenu("Jungleclear")
                            .AddItem(new MenuItem("Jungleclear." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance
                        .SubMenu("Jungleclear")
                        .AddItem(new MenuItem("Jungleclear." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Jungleclear." + displayName, champUniq).GetValue<bool>();
                }

                internal static Slider GetSliderValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Jungleclear." + displayName, champUniq).GetValue<Slider>();
                }

                internal static void AddUseQ(bool enabled = true)
                {
                    AddItem("Use Q", enabled);
                }

                internal static void AddUseE(bool enabled = true)
                {
                    AddItem("Use E", enabled);
                }

                internal static void AddIfMana(int defaultValue = 0)
                {
                    AddItem("If Mana >", new Slider(defaultValue));
                }
            }

            internal class Misc
            {
                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance
                            .SubMenu("Misc")
                            .AddItem(new MenuItem("Misc." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance
                        .SubMenu("Misc")
                        .AddItem(new MenuItem("Misc." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Misc." + displayName, champUniq).GetValue<bool>();
                }

                internal static KeyBind GetKeyBindValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Misc." + displayName, champUniq).GetValue<KeyBind>();
                }
            }

            internal class Drawings
            {
                internal static Circle DrawQrange
                {
                    get { return GetCircleValue("Draw Q Range"); }
                }

                internal static Circle DrawWrange
                {
                    get { return GetCircleValue("Draw W Range"); }
                }

                internal static Circle DrawErange
                {
                    get { return GetCircleValue("Draw E Range"); }
                }

                internal static Circle DrawRrange
                {
                    get { return GetCircleValue("Draw R Range"); }
                }

                internal static void AddItem(string displayName, object value = null, bool champUniq = true)
                {
                    if (value == null)
                    {
                        MenuInstance
                            .SubMenu("Drawings")
                            .AddItem(new MenuItem("Drawings." + displayName, displayName, champUniq));
                        return;
                    }

                    MenuInstance
                        .SubMenu("Drawings")
                        .AddItem(new MenuItem("Drawings." + displayName, displayName, champUniq))
                        .SetValue(value);
                }

                internal static bool GetBoolValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Drawings." + displayName, champUniq).GetValue<bool>();
                }

                internal static Circle GetCircleValue(string displayName, bool champUniq = true)
                {
                    return MenuInstance.Item("Drawings." + displayName, champUniq).GetValue<Circle>();
                }

                internal static void AddDrawQrange(Color color, bool enabled = true)
                {
                    AddItem("Draw Q Range", new Circle(enabled, color));
                }

                internal static void AddDrawWrange(Color color, bool enabled = true)
                {
                    AddItem("Draw W Range", new Circle(enabled, color));
                }

                internal static void AddDrawErange(Color color, bool enabled = true)
                {
                    AddItem("Draw E Range", new Circle(enabled, color));
                }

                internal static void AddDrawRrange(Color color, bool enabled = true)
                {
                    AddItem("Draw R Range", new Circle(enabled, color));
                }

                internal static void AddDamageIndicator(DamageIndicator.DamageToUnitDelegate damage)
                {
                    var drawDamageMenu = new MenuItem("Draw_DamageIndicator", "DamageIndicator", true).SetValue(true);
                    var drawDamageFill =
                        new MenuItem("DamageIndicator_FillColor", "DamageIndicator FillColor", true).SetValue(
                            new Circle(true, Color.Goldenrod));

                    MenuInstance.SubMenu("Drawings").AddItem(drawDamageMenu);
                    MenuInstance.SubMenu("Drawings").AddItem(drawDamageFill);

                    DamageIndicator.DamageToUnit = damage;
                    DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
                    DamageIndicator.Fill = drawDamageFill.GetValue<Circle>().Active;
                    DamageIndicator.FillColor = drawDamageFill.GetValue<Circle>().Color;

                    drawDamageMenu.ValueChanged +=
                        delegate (object sender, OnValueChangeEventArgs eventArgs)
                        {
                            DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                        };

                    drawDamageFill.ValueChanged +=
                        delegate (object sender, OnValueChangeEventArgs eventArgs)
                        {
                            DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                            DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                        };
                }

                internal static void AddDamageIndicatorForJungle(DamageIndicatorForJungle.DamageToUnitDelegate damage)
                {
                    var drawDamageMenu =
                        new MenuItem("Draw_DamageIndicatorForJungle", "DamageIndicator For Jungle", true).SetValue(true);
                    var drawDamageFill =
                        new MenuItem("DamageIndicatorForJungle_FillColor", "DamageIndicator For Jungle FillColor", true)
                            .SetValue(new Circle(true, Color.GreenYellow));

                    MenuInstance.SubMenu("Drawings").AddItem(drawDamageMenu);
                    MenuInstance.SubMenu("Drawings").AddItem(drawDamageFill);

                    DamageIndicatorForJungle.DamageToUnit = damage;
                    DamageIndicatorForJungle.Enabled = drawDamageMenu.GetValue<bool>();
                    DamageIndicatorForJungle.Fill = drawDamageFill.GetValue<Circle>().Active;
                    DamageIndicatorForJungle.FillColor = drawDamageFill.GetValue<Circle>().Color;

                    drawDamageMenu.ValueChanged +=
                        delegate (object sender, OnValueChangeEventArgs eventArgs)
                        {
                            DamageIndicatorForJungle.Enabled = eventArgs.GetNewValue<bool>();
                        };

                    drawDamageFill.ValueChanged +=
                        delegate (object sender, OnValueChangeEventArgs eventArgs)
                        {
                            DamageIndicatorForJungle.Fill = eventArgs.GetNewValue<Circle>().Active;
                            DamageIndicatorForJungle.FillColor = eventArgs.GetNewValue<Circle>().Color;
                        };
                }
            }
        }
    }

    internal class PluginLoader
    {
        internal static bool LoadPlugin(string pluginName)
        {
            if (CanLoadPlugin(pluginName))
            {
                DynamicInitializer.NewInstance(Type.GetType("SharpShooter." + ObjectManager.Player.ChampionName));
                return true;
            }

            return false;
        }

        internal static bool CanLoadPlugin(string pluginName)
        {
            return Type.GetType("SharpShooter." + ObjectManager.Player.ChampionName) != null;
        }
    }

    internal static class ExtraExtensions
    {
        internal static bool IsKillableAndValidTarget(this Obj_AI_Hero target, double calculatedDamage,
            TargetSelector.DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.CharData.BaseSkinName == "gangplankbarrel")
                return false;

            if (target.HasBuff("kindredrnodeathbuff"))
            {
                return false;
            }

            if (target.HasBuff("Undying Rage"))
            {
                return false;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            if (target.HasBuff("DiplomaticImmunity") && !ObjectManager.Player.HasBuff("poppyulttargetmark"))
            {
                return false;
            }

            if (target.HasBuff("BansheesVeil"))
            {
                return false;
            }

            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            if (target.ChampionName == "Blitzcrank")
                if (!target.HasBuff("manabarriercooldown"))
                    if (target.Health + target.HPRegenRate + target.Mana * 0.6 + target.PARRegenRate < calculatedDamage)
                        return true;

            if (target.ChampionName == "Garen")
                if (target.HasBuff("GarenW"))
                    calculatedDamage *= 0.7;

            if (target.HasBuff("FerociousHowl"))
                calculatedDamage *= 0.3;

            return target.Health + target.HPRegenRate < calculatedDamage - 2;
        }

        internal static bool IsKillableAndValidTarget(this Obj_AI_Minion target, double calculatedDamage,
            TargetSelector.DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.Health <= 0 ||
                target.HasBuffOfType(BuffType.SpellImmunity) || target.HasBuffOfType(BuffType.SpellShield) ||
                target.CharData.BaseSkinName == "gangplankbarrel")
                return false;

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            var dragonSlayerBuff = ObjectManager.Player.GetBuff("s5test_dragonslayerbuff");
            if (dragonSlayerBuff != null)
            {
                if (dragonSlayerBuff.Count >= 4)
                    calculatedDamage += dragonSlayerBuff.Count == 5 ? calculatedDamage * 0.30 : calculatedDamage * 0.15;

                if (target.CharData.BaseSkinName.ToLowerInvariant().Contains("dragon"))
                    calculatedDamage *= 1 - dragonSlayerBuff.Count * 0.07;
            }

            if (target.CharData.BaseSkinName.ToLowerInvariant().Contains("baron") &&
                ObjectManager.Player.HasBuff("barontarget"))
                calculatedDamage *= 0.5;

            return target.Health + target.HPRegenRate  < calculatedDamage - 2;
        }

        internal static bool IsKillableAndValidTarget(this Obj_AI_Base target, double calculatedDamage,
            TargetSelector.DamageType damageType, float distance = float.MaxValue)
        {
            if (target == null || !target.IsValidTarget(distance) || target.CharData.BaseSkinName == "gangplankbarrel")
                return false;

            if (target.HasBuff("kindredrnodeathbuff"))
            {
                return false;
            }

            if (target.HasBuff("Undying Rage"))
            {
                return false;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            if (target.HasBuff("DiplomaticImmunity") && !ObjectManager.Player.HasBuff("poppyulttargetmark"))
            {
                return false;
            }

            if (target.HasBuff("BansheesVeil"))
            {
                return false;
            }

            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                calculatedDamage *= 0.6;

            if (target.CharData.BaseSkinName == "Blitzcrank")
                if (!target.HasBuff("manabarriercooldown"))
                    if (target.Health + target.HPRegenRate + target.Mana * 0.6 + target.PARRegenRate < calculatedDamage)
                        return true;

            if (target.CharData.BaseSkinName == "Garen")
                if (target.HasBuff("GarenW"))
                    calculatedDamage *= 0.7;


            if (target.HasBuff("FerociousHowl"))
                calculatedDamage *= 0.3;

            var dragonSlayerBuff = ObjectManager.Player.GetBuff("s5test_dragonslayerbuff");
            if (dragonSlayerBuff != null)
                if (target.IsMinion)
                {
                    if (dragonSlayerBuff.Count >= 4)
                        calculatedDamage += dragonSlayerBuff.Count == 5 ? calculatedDamage * 0.30 : calculatedDamage * 0.15;

                    if (target.CharData.BaseSkinName.ToLowerInvariant().Contains("dragon"))
                        calculatedDamage *= 1 - dragonSlayerBuff.Count * 0.07;
                }

            if (target.CharData.BaseSkinName.ToLowerInvariant().Contains("baron") &&
                ObjectManager.Player.HasBuff("barontarget"))
                calculatedDamage *= 0.5;

            return target.Health + target.HPRegenRate < calculatedDamage - 2;
        }

        internal static bool IsManaPercentOkay(this Obj_AI_Hero hero, int manaPercent)
        {
            return hero.ManaPercent > manaPercent;
        }
    }
}