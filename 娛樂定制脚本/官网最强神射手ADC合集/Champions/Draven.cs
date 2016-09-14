using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Marksman.Common;
using Marksman.Orb;
using SharpDX;
using Color = System.Drawing.Color;
using Orbwalking = Marksman.Orb.Orbwalking;


namespace Marksman.Champions
{
    internal class Reticles
    {
        public GameObject Object { get; set; }
        public float NetworkId { get; set; }
        public Vector3 ReticlePos { get; set; }
        public double ExpireTime { get; set; }
    }

    internal class Draven : Champion
    {
        private static readonly List<Reticles> ExistingReticles = new List<Reticles>();
        public static Spell Q, W, E, R;
        public int QStacks = 0;

        private static string Tab
        {
            get { return "    "; }
        }

        public Draven()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 20000);
            E.SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Utils.Utils.PrintMessage("Draven loaded.");
        }

        public override void DrawingOnEndScene(EventArgs args)
        {

            var xComboString = "Combo Mode: ";
            System.Drawing.Color xComboColor = System.Drawing.Color.FromArgb(100, 255, 200, 37);

            string[] vComboString = new[]
            {
                "Offensive", "Deffensive"
            };

            System.Drawing.Color[] vComboColor = new[]
            {
                System.Drawing.Color.FromArgb(255, 4, 0, 255),
                System.Drawing.Color.Red,
                System.Drawing.Color.FromArgb(255, 46, 47, 46),
            };

            var nComboMode =GetValue<StringList>("Combo.Mode").SelectedIndex;
            xComboString = xComboString + vComboString[nComboMode];
            xComboColor = vComboColor[nComboMode];

            Common.CommonGeometry.DrawBox(new Vector2(Drawing.Width * 0.45f, Drawing.Height * 0.80f), 125, 18, xComboColor, 1, System.Drawing.Color.Black);
            Common.CommonGeometry.DrawText(CommonGeometry.Text, xComboString, Drawing.Width * 0.455f, Drawing.Height * 0.803f, SharpDX.Color.Wheat);

            var rCircle = Config.Item("DrawRMini").GetValue<bool>();
            if (rCircle)
            {
                var maxRRange = Config.Item("UseRCMaxR").GetValue<Slider>().Value;
                var rMax = Config.Item("DrawRMax").GetValue<Circle>();
#pragma warning disable 618
                Utility.DrawCircle(ObjectManager.Player.Position, maxRRange, rMax.Color, 1, 23, true);
#pragma warning restore 618
            }

        }
        public void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && Config.Item("EGapCloser").GetValue<bool>() && gapcloser.Sender.IsValidTarget(E.Range))
            {
                E.Cast(gapcloser.Sender);
            }
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && Config.Item("EInterruptable").GetValue<bool>() && unit.IsValidTarget(E.Range))
            {
                E.Cast(unit);
            }
        }

        public override void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if ((sender.Name.Contains("Q_reticle_self")))
            {
                for (var i = 0; i < ExistingReticles.Count; i++)
                {
                    if (Math.Abs(ExistingReticles[i].NetworkId - sender.NetworkId) < 0.00001)
                    {
                        ExistingReticles.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        public override void OnCreateObject(GameObject sender, EventArgs args)
        {
            if ((sender.Name.Contains("Q_reticle_self")))
            {
                ExistingReticles.Add(
                    new Reticles
                    {
                        Object = sender,
                        NetworkId = sender.NetworkId,
                        ReticlePos = sender.Position,
                        ExpireTime = Game.Time + 1.20
                    });
            }
        }

        
        public override void Drawing_OnDraw(EventArgs args)
        {
            var drawOrbwalk = Config.Item("DrawOrbwalk").GetValue<Circle>();
            var drawReticles = Config.Item("DrawReticles").GetValue<Circle>();
            var drawCatchRadius = Config.Item("DrawCatchRadius").GetValue<Circle>();

            if (drawOrbwalk.Active)
            {
                Render.Circle.DrawCircle(GetOrbwalkPos(), 100, drawOrbwalk.Color);
            }

            if (drawReticles.Active)
            {
                foreach (var existingReticle in ExistingReticles)
                {
                    Render.Circle.DrawCircle(existingReticle.ReticlePos, 100, drawReticles.Color);
                }
            }

            if (drawCatchRadius.Active)
            {
                if (GetOrbwalkPos() != Game.CursorPos &&
                    (ComboActive || LaneClearActive || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit))
                {
                    Render.Circle.DrawCircle(Game.CursorPos, Config.Item("CatchRadius").GetValue<Slider>().Value,Color.Red);
                }
                else
                {
                    Render.Circle.DrawCircle(
                        Game.CursorPos, Config.Item("CatchRadius").GetValue<Slider>().Value, Color.CornflowerBlue);
                }
            }

            var drawE = Config.Item("DrawE").GetValue<Circle>();
            if (drawE.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, drawE.Color);
            }

            var drawRMin = Config.Item("DrawRMin").GetValue<Circle>();
            if (drawRMin.Active)
            {
                var minRRange = Config.Item("UseRCMinR").GetValue<Slider>().Value;
                Render.Circle.DrawCircle(ObjectManager.Player.Position, minRRange, drawRMin.Color, 2);
            }

            var drawRMax = Config.Item("DrawRMax").GetValue<Circle>();
            if (drawRMax.Active)
            {
                var maxRRange = Config.Item("UseRCMaxR").GetValue<Slider>().Value;
                Render.Circle.DrawCircle(ObjectManager.Player.Position, maxRRange, drawRMax.Color, 2);
            }
        }

        public override void Game_OnUpdate(EventArgs args)
        {
            var orbwalkPos = GetOrbwalkPos();
            var cursor = Game.CursorPos;
            if (orbwalkPos != cursor &&
                (ComboActive || LaneClearActive || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit))
            {
                Orbwalker.SetMarksmanOrbwalkingPoint(orbwalkPos);
            }
            else
            {
                Orbwalker.SetMarksmanOrbwalkingPoint(cursor);
            }

            Obj_AI_Hero t;
            //Combo
            if (ComboActive)
            {
                var minRRange = Config.Item("UseRCMinR").GetValue<Slider>().Value;
                var maxRRange = Config.Item("UseRCMaxR").GetValue<Slider>().Value;

                t = TargetSelector.GetTarget(maxRRange, TargetSelector.DamageType.Physical);
                if (!t.IsValidTarget())
                {
                    return;
                }

                if (W.IsReady() && Config.Item("UseWC").GetValue<bool>() && t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65) &&
                    ObjectManager.Player.Buffs.FirstOrDefault(
                        buff => buff.Name == "dravenfurybuff" || buff.Name == "DravenFury") == null)
                {
                    W.Cast();
                }
                if (IsFleeing(t) && Config.Item("UseEC").GetValue<bool>() && t.IsValidTarget(E.Range))
                {
                    E.Cast(t);
                }

                if (Config.Item("UseRC").GetValue<bool>() && R.IsReady())
                {
                    t = TargetSelector.GetTarget(maxRRange, TargetSelector.DamageType.Physical);
                    if (t.Distance(ObjectManager.Player) >= minRRange && t.Distance(ObjectManager.Player) <= maxRRange &&
                        t.Health < ObjectManager.Player.GetSpellDamage(t, SpellSlot.R) * 2)
                    //R.GetHealthPrediction(target) <= 0)
                    {
                        R.Cast(t);
                    }
                }
            }

            //Peel from melees
            if (Config.Item("EPeel").GetValue<bool>()) 
            {
                foreach (var pos in from enemy in ObjectManager.Get<Obj_AI_Hero>()
                                    where
                                        enemy.IsValidTarget() &&
                                        enemy.Distance(ObjectManager.Player) <=
                                        enemy.BoundingRadius + enemy.AttackRange + ObjectManager.Player.BoundingRadius &&
                                        LeagueSharp.Common.Orbwalking.IsMelee(enemy)
                                    let direction =
                                        (enemy.ServerPosition.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized()
                                    let pos = ObjectManager.Player.ServerPosition.To2D()
                                    select pos + Math.Min(200, Math.Max(50, enemy.Distance(ObjectManager.Player) / 2)) * direction)
                {
                    E.Cast(pos.To3D());
                }
            }
        }

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }
            Console.WriteLine("Hai");
            Console.WriteLine(Config.Item("maxqamount").GetValue<Slider>().Value);
            var qOnHero = QBuffCount();
            if (unit.IsMe &&
                ((ComboActive && Config.Item("UseQC").GetValue<bool>()) ||
                 (HarassActive && Config.Item("UseQC").GetValue<bool>())) && qOnHero < 2 &&
                qOnHero + ExistingReticles.Count < Config.Item("maxqamount").GetValue<Slider>().Value)
            {
                Q.Cast();
                Console.WriteLine("Casted Q");
            }
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("Combo.Mode" + Id, "Combo Mode:").SetValue(new StringList(new[] { "Q:R", "W:R" }, 1)).SetFontStyle(FontStyle.Regular, SharpDX.Color.GreenYellow));

            config.AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEC", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRC", "Use R").SetValue(true));
            config.AddItem(new MenuItem("UseRCMinR", Tab + "Min. R Range").SetValue(new Slider(350, 200, 750)));
            config.AddItem(new MenuItem("UseRCMaxR", Tab + "Max. R Range").SetValue(new Slider(1000, 750, 3000)));
            config.AddItem(new MenuItem("DrawRMin", Tab + "Draw Min. R Range").SetValue(new Circle(true, Color.DarkRed)));
            config.AddItem(new MenuItem("DrawRMax", Tab + "Draw Max. R Range").SetValue(new Circle(true, Color.DarkMagenta)));
            config.AddItem(new MenuItem("DrawRMini", Tab + "Draw R on Mini Map").SetValue(true));

            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("DrawE", "E range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawOrbwalk", "Draw orbwalk position").SetValue(new Circle(true, Color.Yellow)));
            config.AddItem(new MenuItem("DrawReticles", "Draw on reticles").SetValue(new Circle(true, Color.Green)));
            config.AddItem(new MenuItem("DrawCatchRadius", "Draw Catch Radius").SetValue(new Circle(true, Color.Green)));
            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(new MenuItem("maxqamount", "Max Qs to use simultaneous").SetValue(new Slider(2, 4, 1)));
            config.AddItem(new MenuItem("EGapCloser", "Auto E Gap closers").SetValue(true));
            config.AddItem(new MenuItem("EInterruptable", "Auto E interruptable spells").SetValue(true));
            //config.AddItem(new MenuItem("RManualCast", "Cast R Manually(2000 range)")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
            config.AddItem(new MenuItem("Epeel", "Peel self with E").SetValue(true));
            config.AddItem(new MenuItem("CatchRadius", "Axe catch radius").SetValue(new Slider(600, 200, 1000)));
            return true;
        }

        public static int QBuffCount()
        {
            var buff =
                ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("dravenspinningattack"));
            return ExistingReticles.Count + (buff != null ? buff.Count : 0);
        }

        public Vector3 GetOrbwalkPos()
        {
            if (ExistingReticles.Count <= 0)
            {
                return Game.CursorPos;
            }
            var myHero = ObjectManager.Player;
            var cursor = Game.CursorPos;
            var reticles =
                ExistingReticles.OrderBy(reticle => reticle.ExpireTime)
                    .FirstOrDefault(
                        reticle =>
                            reticle.ReticlePos.Distance(cursor) <= Config.Item("CatchRadius").GetValue<Slider>().Value &&
                            reticle.Object.IsValid &&
                            myHero.GetPath(reticle.ReticlePos).ToList().To2D().PathLength() / myHero.MoveSpeed + Game.Time <
                            reticle.ExpireTime);

            return reticles != null && myHero.Distance(reticles.ReticlePos) >= 100 ? reticles.ReticlePos : cursor;
        }

        public static bool IsFleeing(Obj_AI_Hero hero)
        {
            var position = E.GetPrediction(hero);
            return position != null &&
                   Vector3.DistanceSquared(ObjectManager.Player.Position, position.CastPosition) >
                   Vector3.DistanceSquared(hero.Position, position.CastPosition);
        }

        public override bool LaneClearMenu(Menu config)
        {
            return true;
        }
        public override bool JungleClearMenu(Menu config)
        {
            return true;
        }
    }
}
