namespace YuLeAhri
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using YuLeLibrary;
    using System.Threading.Tasks;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Reflection;

    internal class Program
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static int tickNum = 4, tickIndex = 0;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "Ahri")
                return;

            Config = new Menu("QQ群：438230879", "QQ群：438230879", true);
            Config.SetMenuColor(Color.Yellow);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            new YuLeAhri().Load();

            Config.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            tickIndex++;

            if (tickIndex > 4)
                tickIndex = 0;
        }

        public static bool LagFree(int offset)
        {
            if (tickIndex == offset)
                return true;
            else
                return false;
        }

        public static bool Farm { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed; } }

        public static bool None { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None); } }

        public static bool Combo { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo); } }

        public static bool LaneClear { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear); } }

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

        public static void CastSpell(Spell QWER, Obj_AI_Base target)
        {
            YuLeLibarary.Prediction.SkillshotType CoreType2 = YuLeLibarary.Prediction.SkillshotType.SkillshotLine;
            bool aoe2 = false;

            if (QWER.Type == SkillshotType.SkillshotCircle)
            {
                CoreType2 = YuLeLibarary.Prediction.SkillshotType.SkillshotCircle;
                aoe2 = true;
            }

            if (QWER.Width > 80 && !QWER.Collision)
                aoe2 = true;

            var predInput2 = new YuLeLibarary.Prediction.PredictionInput
            {
                Aoe = aoe2,
                Collision = QWER.Collision,
                Speed = QWER.Speed,
                Delay = QWER.Delay,
                Range = QWER.Range,
                From = ObjectManager.Player.ServerPosition,
                Radius = QWER.Width,
                Unit = target,
                Type = CoreType2
            };
            var poutput2 = YuLeLibarary.Prediction.Prediction.GetPrediction(predInput2);

            if (QWER.Speed != float.MaxValue && Common.CollisionYasuo(ObjectManager.Player.ServerPosition, poutput2.CastPosition))
                return;

            if (poutput2.Hitchance >= YuLeLibarary.Prediction.HitChance.VeryHigh)
                QWER.Cast(poutput2.CastPosition);
            else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= YuLeLibarary.Prediction.HitChance.High)
            {
                QWER.Cast(poutput2.CastPosition);
            }
        }
    }

    class MissileReturn
    {
        public Obj_AI_Hero Target;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Menu Config = Program.Config;
        private static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private string MissileName, MissileReturnName;
        private Spell QWER;
        public MissileClient Missile;
        private Vector3 MissileEndPos;

        public MissileReturn(string missile, string missileReturnName, Spell qwer)
        {
            MissileName = missile;
            MissileReturnName = missileReturnName;
            QWER = qwer;

            GameObject.OnCreate += SpellMissile_OnCreateOld;
            GameObject.OnDelete += Obj_SpellMissile_OnDelete;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Missile != null && Missile.IsValid && Config.Item("drawHelper", true).GetValue<bool>())
                Common.DrawLineRectangle(Missile.Position, Player.Position, (int)QWER.Width, 1, System.Drawing.Color.White);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("aim", true).GetValue<bool>())
            {
                var posPred = CalculateReturnPos();
                if (posPred != Vector3.Zero)
                    Orbwalker.SetOrbwalkingPoint(posPred);
                else
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
            }
            else
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == QWER.Slot)
            {
                MissileEndPos = args.End;
            }
        }

        private void SpellMissile_OnCreateOld(GameObject sender, EventArgs args)
        {
            if (sender.IsEnemy || sender.Type != GameObjectType.MissileClient || !sender.IsValid<MissileClient>())
                return;

            MissileClient missile = (MissileClient)sender;

            if (missile.SData.Name != null)
            {
                if (missile.SData.Name.ToLower() == MissileName.ToLower() || missile.SData.Name.ToLower() == MissileReturnName.ToLower())
                {
                    Missile = missile;
                }
            }
        }

        private void Obj_SpellMissile_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.IsEnemy || sender.Type != GameObjectType.MissileClient || !sender.IsValid<MissileClient>())
                return;

            MissileClient missile = (MissileClient)sender;

            if (missile.SData.Name != null)
            {
                if (missile.SData.Name.ToLower() == MissileReturnName.ToLower())
                {
                    Missile = null;
                }
            }
        }

        public Vector3 CalculateReturnPos()
        {
            if (Missile != null && Missile.IsValid && Target.IsValidTarget())
            {
                var finishPosition = Missile.Position;
                if (Missile.SData.Name.ToLower() == MissileName.ToLower())
                {
                    finishPosition = MissileEndPos;
                }

                var misToPlayer = Player.Distance(finishPosition);
                var tarToPlayer = Player.Distance(Target);

                if (misToPlayer > tarToPlayer)
                {
                    var misToTarget = Target.Distance(finishPosition);

                    if (misToTarget < QWER.Range && misToTarget > 50)
                    {
                        var cursorToTarget = Target.Distance(Player.Position.Extend(Game.CursorPos, 100));
                        var ext = finishPosition.Extend(Target.ServerPosition, cursorToTarget + misToTarget);

                        if (ext.Distance(Player.Position) < 800 && ext.CountEnemiesInRange(400) < 2)
                        {
                            if (Config.Item("drawHelper", true).GetValue<bool>())
                                Render.Circle.DrawCircle(ext, 100, System.Drawing.Color.White, 1);
                            return ext;
                        }
                    }
                }
            }
            return Vector3.Zero;
        }
    }
}