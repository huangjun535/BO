using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using YuLeLibrary;

namespace YuLeTwitch
{
    class Twitch
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        public float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private int count = 0, countE = 0;
        private float grabTime = Game.Time;

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R, 975);

            W.SetSkillshot(0.25f, 100f, 1410f, false, SkillshotType.SkillshotCircle);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("countQ", "自动Q|附近敌人数", true).SetValue(new Slider(3, 5, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "连招自动Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("recallSafe", "回城自动Q", true).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "自动 W", true).SetValue(true));

            Config.SubMenu("E 设置").AddItem(new MenuItem("Eks", "E 击杀", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("Edead", "临死前自动E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("countE", "自动E|敌人身上层数并且离开AA范围", true).SetValue(new Slider(6, 6, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("5e", "敌人身上满层数自动E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("jungleE", "E 清野", true).SetValue(true));

            Config.SubMenu("R 设置").AddItem(new MenuItem("Rks", "敌人离开AA范围并且能击杀", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("countR", "连招自动R敌人数", true).SetValue(new Slider(3, 5, 0)));

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            Config.SubMenu("Draw").AddItem(new MenuItem("notif", "技能倒计时", true).SetValue(true));
            Config.SubMenu("Draw").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("Draw").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("Draw").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Recall && Q.IsReady() && Config.Item("recallSafe", true).GetValue<bool>())
            {
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Q);
                Utility.DelayAction.Add(200, () => ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall));
                args.Process = false;
                return;
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Program.LagFree(0))
            {
                SetMana();
            }
            if (Program.LagFree(1) && E.IsReady())
                LogicE();
            if (Program.LagFree(2) && Q.IsReady() && !Player.IsWindingUp)
                LogicQ();
            if (Program.LagFree(3) && W.IsReady() && !Player.IsWindingUp)
                LogicW();
            if (Program.LagFree(4) && R.IsReady() && Program.Combo)
                LogicR();

            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Program.Combo;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Config.Item("Edead", true).GetValue<bool>() && E.IsReady() && sender.IsEnemy && sender.IsValidTarget(1500))
            {
                double dmg = 0;

                if (args.Target != null && args.Target.IsMe)
                {
                    dmg = dmg + sender.GetSpellDamage(Player, args.SData.Name);
                }
                else
                {
                    var castArea = Player.Distance(args.End) * (args.End - Player.ServerPosition).Normalized() + Player.ServerPosition;
                    if (castArea.Distance(Player.ServerPosition) < Player.BoundingRadius / 2)
                    {
                        dmg = dmg + sender.GetSpellDamage(Player, args.SData.Name);
                    }
                }

                if (Player.Health - dmg < (Player.CountEnemiesInRange(600) * Player.Level * 10))
                {
                    E.Cast();
                }
            }
        }

        private void LogicR()
        {
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                if (!Orbwalking.InAutoAttackRange(t) && Config.Item("Rks", true).GetValue<bool>() && Player.GetAutoAttackDamage(t) * 4 > t.Health)
                    R.Cast();

                if (t.CountEnemiesInRange(450) >= Config.Item("countR", true).GetValue<Slider>().Value && 0 != Config.Item("countR", true).GetValue<Slider>().Value)
                    R.Cast();
            }
        }

        private void LogicW()
        {
            var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {

                if (Program.Combo && Player.Mana > WMANA + RMANA + EMANA && (Player.GetAutoAttackDamage(t) * 2 < t.Health || !Orbwalking.InAutoAttackRange(t)))
                    W.Cast(t, true);
                else if ((Program.Combo || Program.Farm) && Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !Common.CanMove(enemy)))
                        W.Cast(enemy, true);
                }
            }
        }

        private void LogicQ()
        {

            if (Config.Item("autoQ", true).GetValue<bool>() && Program.Combo && Orbwalker.GetTarget().IsValid<Obj_AI_Hero>() && Player.Mana > RMANA + QMANA)
                Q.Cast();

            if (Config.Item("countQ", true).GetValue<Slider>().Value == 0 || Player.Mana < RMANA + QMANA)
                return;

            var count = 0;
            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(3000)))
            {
                List<Vector2> waypoints = enemy.GetWaypoints();

                if (Player.Distance(waypoints.Last().To3D()) < 600)
                    count++;
            }

            if (count >= Config.Item("countQ", true).GetValue<Slider>().Value)
                Q.Cast();
        }

        private void LogicE()
        {
            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && enemy.HasBuff("TwitchDeadlyVenom")))
            {
                if (Config.Item("Eks", true).GetValue<bool>() && E.GetDamage(enemy) > enemy.Health)
                {
                    E.Cast();
                }
                if (Player.Mana > RMANA + EMANA)
                {
                    int buffsNum = Common.GetBuffCount(enemy, "TwitchDeadlyVenom");
                    if (Config.Item("5e", true).GetValue<bool>() && buffsNum == 6)
                    {
                        E.Cast();
                    }
                    float buffTime = Common.GetPassiveTime(enemy, "TwitchDeadlyVenom");
                
                    if (!Orbwalking.InAutoAttackRange(enemy) && (Player.ServerPosition.Distance(enemy.ServerPosition) > 950 || buffTime < 1) && 0 < Config.Item("countE", true).GetValue<Slider>().Value && buffsNum >= Config.Item("countE", true).GetValue<Slider>().Value)
                    {
                        E.Cast();
                    }
                }
            }
            JungleE();
        }

        private void JungleE()
        {
            if (!Config.Item("jungleE", true).GetValue<bool>() || Player.Mana < RMANA + EMANA || Player.Level == 1)
                return;

            var mobs = Cache.GetMinions(Player.ServerPosition, E.Range, MinionTeam.Neutral);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (E.IsKillable(mob))
                {
                    E.Cast();
                }
            }
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = EMANA - Player.PARRegenRate * E.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        private float GetRemainingTime()
        {
            var buff = ObjectManager.Player.GetBuff("TwitchHideInShadows");

            if (buff == null && Q.IsReady()) return Q.Level + 3 + 1.5f + 1f;
            if (buff == null) return 0;
            return buff.EndTime - Game.Time;
        }

        public static void drawText2(string msg, Vector3 Hero, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - 200, color, msg);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("notif", true).GetValue<bool>())
            {
                if (Player.HasBuff("TwitchHideInShadows"))
                    drawText2("Q:  " + String.Format("{0:0.0}", Common.GetPassiveTime(Player, "TwitchHideInShadows")), Player.Position, System.Drawing.Color.Yellow);
                if (Player.HasBuff("twitchhideinshadowsbuff"))
                    drawText2("Q AS buff:  " + String.Format("{0:0.0}", Common.GetPassiveTime(Player, "twitchhideinshadowsbuff")), Player.Position, System.Drawing.Color.YellowGreen);
                if (Player.HasBuff("TwitchFullAutomatic"))
                    drawText2("R ACTIVE:  " + String.Format("{0:0.0}", Common.GetPassiveTime(Player, "TwitchFullAutomatic")), Player.Position, System.Drawing.Color.OrangeRed);

            }

            if(Config.GetBool("qRange"))
            {
                var stealthTime = GetRemainingTime();
                if (stealthTime > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, stealthTime * ObjectManager.Player.MoveSpeed, System.Drawing.Color.Red);
                }
            }

            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }

            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }
    }
}