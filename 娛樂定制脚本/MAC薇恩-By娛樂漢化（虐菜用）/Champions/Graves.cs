﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using MAC_Reborn.Handlers;
using MAC_Reborn.Extras;

namespace MAC_Reborn.Champions
{
    internal class Graves : BaseChampion
    {

        public Spell Q;
        public Spell W;
        public Spell E;
        public Spell R;

        public Graves()
        {
            Q = new Spell(SpellSlot.Q, 950f);
            Q.SetSkillshot(0.25f, 10f * 1.9f * (float)Math.PI / 180, 1950, false, SkillshotType.SkillshotCone);

            W = new Spell(SpellSlot.W, 950);
            W.SetSkillshot(0.25f, 250f, 1650f, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 425f);

            R = new Spell(SpellSlot.R, 1150f);
            R.SetSkillshot(0.25f, 150f, 2100, false, SkillshotType.SkillshotLine);

            Game.OnUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        internal static float RCastTime = 0;

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("drawQ");
            var drawW = GetBool("drawW");
            var drawE = GetBool("drawE");
            var drawR = GetBool("drawR");

            var comboTypeIndex = Menu.Item("comboType").GetValue<StringList>().SelectedIndex;

            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);

            var wts = Drawing.WorldToScreen(Player.Position);

            var p = Player.Position;

            if (GetBool("disableAll"))
            {
                return;
            }

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? System.Drawing.Color.Aqua : System.Drawing.Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(p, W.Range, W.IsReady() ? System.Drawing.Color.Aqua : System.Drawing.Color.Red);

            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? System.Drawing.Color.Aqua : System.Drawing.Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? System.Drawing.Color.Aqua : System.Drawing.Color.Red);
            }

            if (GetBool("drawComboType"))
            {
                switch (comboTypeIndex)
                {
                    case 0:
                        Drawing.DrawText(wts[0] - 35, wts[1] + 10, System.Drawing.Color.White, "Manual Combo Selected");
                        break;
                    case 1:
                        Drawing.DrawText(wts[0] - 35, wts[1] + 10, System.Drawing.Color.Red, "Advanced Combo Selected");
                        break;
                    case 2:
                        Drawing.DrawText(wts[0] - 35, wts[1] + 10, System.Drawing.Color.Gold, "Automatic Combo");
                        break;
                }
            }
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
            switch (OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;

                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }
        }

        private void Combo()
        {
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);

            var comboTypeIndex = Menu.Item("comboType").GetValue<StringList>().SelectedIndex;

            /*
                Automatic mode ignore all your combo configs 
             */

            if (comboTypeIndex == 2)
            {

                var damageR = Player.GetSpellDamage(target, SpellSlot.R);

                if (R.IsReady() && target.Health < damageR + 40 && R.IsInRange(target) && Q.GetDamage(target) + 40 < target.Health && Q.IsReady() && Game.Time > RCastTime || R.IsReady() && target.Health < damageR + 40 && R.IsInRange(target) && !Q.IsReady() && Game.Time > RCastTime)
                {
                    R.Cast(target);
                }

                if (E.IsReady() && Player.Distance(target.Position) > Player.AttackRange && Player.Distance(target.Position) < Player.AttackRange + E.Range)
                {
                    if (isUnderEnemyTurret(target.Position) && !GetBool("dontETurr"))
                    {
                        E.Cast(Game.CursorPos);
                    }
                    else if (isUnderEnemyTurret(target.Position) && GetBool("dontETurr") && !isUnderEnemyTurret(Game.CursorPos))
                    {
                        E.Cast(Game.CursorPos);
                    }
                    else if (isUnderEnemyTurret(target.Position) && GetBool("dontETurr"))
                    {

                    }
                    else
                    {
                        E.Cast(Game.CursorPos);
                    }
                }

                if (CanUseItem(3142) && Player.Distance(target.Position) < Player.AttackRange)
                    UseItem(3142);

                if ((Player.Health / Player.MaxHealth) * 100 < (target.Health / target.MaxHealth) * 100 && (CanUseItem(3153) || CanUseItem(3144)))
                {
                    UseItem(3144, target);

                    UseItem(3153, target);
                }

                if (W.IsReady() && W.IsInRange(target) && (Player.Health / Player.MaxHealth) * 100 < (target.Health / target.MaxHealth) * 100)
                {
                    W.CastIfHitchanceEquals(target, HitChance.High, false);
                }

            }

            /* End of Gosu Mode */

            /**
             * Advanced Combo Mode 
             */

            if (comboTypeIndex == 1)
            {

                var damageR = Player.GetSpellDamage(target, SpellSlot.R);

                    if (R.IsReady() && target.Health < damageR + 40 && R.IsInRange(target) && Q.GetDamage(target) + 40 < target.Health && Q.IsReady() && Game.Time > RCastTime || R.IsReady() && target.Health < damageR + 40 && R.IsInRange(target) && !Q.IsReady() && Game.Time > RCastTime)
                    {
                        if (GetBool("comboR"))
                            R.Cast(target);
                    }

                if (E.IsReady() && Player.Distance(target.Position) > Player.AttackRange && Player.Distance(target.Position) < Player.AttackRange + E.Range && GetBool("comboE"))
                {
                    if (isUnderEnemyTurret(target.Position) && !GetBool("dontETurr"))
                    {
                        E.Cast(Game.CursorPos);
                    }
                    else if (isUnderEnemyTurret(target.Position) && GetBool("dontETurr") && !isUnderEnemyTurret(Game.CursorPos))
                    {
                        E.Cast(Game.CursorPos);
                    }
                    else if (isUnderEnemyTurret(target.Position) && GetBool("dontETurr"))
                    {

                    }
                    else
                    {
                        E.Cast(Game.CursorPos);
                    }
                }

                if (CanUseItem(3142) && Player.Distance(target.Position) < Player.AttackRange && GetBool("YoumuuC"))
                    UseItem(3142);

                if ((Player.Health / Player.MaxHealth) * 100 < (target.Health / target.MaxHealth) * 100 && (CanUseItem(3153) || CanUseItem(3144)) && GetBool("BotrkC"))
                {
                    UseItem(3144, target);

                    UseItem(3153, target);
                }

                if (W.IsReady() && W.IsInRange(target) && (Player.Health / Player.MaxHealth) * 100 < (target.Health / target.MaxHealth) * 100 && GetBool("comboW"))
                {
                    W.CastIfHitchanceEquals(target, HitChance.High, false);
                }

            }

            /* End of Advanced Combo Mode */

            /**
             * Manual Combo Mode 
             */

            if (comboTypeIndex == 0)
            {
                if (E.IsReady() && Player.Distance(target.Position) > Player.AttackRange && Player.Distance(target.Position) < Player.AttackRange + E.Range && GetBool("comboE"))
                {
                    E.Cast(Game.CursorPos);
                }
            }

            /* End of Manual Combo Mode */

        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);

            if (E.IsReady() && GetBool("harassE") && manaManager() && Player.Distance(target.Position) > Player.AttackRange)
            {
                if (isUnderEnemyTurret(target.Position) && !GetBool("dontETurr"))
                {
                    E.Cast(Game.CursorPos);
                }
                else if (isUnderEnemyTurret(target.Position) && GetBool("dontETurr") && !isUnderEnemyTurret(Game.CursorPos))
                {
                    E.Cast(Game.CursorPos);
                }
                else if (isUnderEnemyTurret(target.Position) && GetBool("dontETurr"))
                {

                }
                else
                {
                    E.Cast(Game.CursorPos);
                }
            }

        }

        private void LaneClear()
        {

            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);

            if (E.IsReady() && GetBool("laneClearE") && manaManager())
            {
                E.Cast(Game.CursorPos);
            }

        }

        public override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var comboTypeIndex = Menu.Item("comboType").GetValue<StringList>().SelectedIndex;

            if (OrbwalkerMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (comboTypeIndex == 2 && Q.IsReady() && Q.IsInRange(target))
                {
                    var pred = Q.GetPrediction((Obj_AI_Hero)target);
                    if (pred.Hitchance == HitChance.High || pred.Hitchance == HitChance.VeryHigh)
                    {
                        Q.Cast((Obj_AI_Hero)target);
                        RCastTime = Game.Time + Q.Delay;
                    }
                }
                else if (comboTypeIndex < 2 && GetBool("comboQ") && Q.IsReady() && target.Position.Distance(Player.Position) < (float)GetValue<Slider>("qRange").Value)
                {
                    var pred = Q.GetPrediction((Obj_AI_Hero)target);
                    if (pred.Hitchance == HitChance.High || pred.Hitchance == HitChance.VeryHigh)
                    {
                        Q.Cast((Obj_AI_Hero)target);
                        RCastTime = Game.Time + Q.Delay;
                    }
                }
            }
            else if (OrbwalkerMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (GetBool("harassQ") && Q.IsReady() && manaManager() && target.Position.Distance(Player.Position) < (float)GetValue<Slider>("qRange").Value)
                {
                    Q.CastIfHitchanceEquals((Obj_AI_Hero)target, HitChance.High, false);
                }
            }
            else if (OrbwalkerMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (GetBool("laneClearQ") && Q.IsReady() && manaManager())
                {
                    var allMinionE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All,
                        MinionTeam.NotAlly);
                    var Q_Pred = Q.GetLineFarmLocation(allMinionE);
                    if (Q_Pred.MinionsHit > 0)
                        Q.Cast(Q_Pred.Position);
                }
            }
        }

        public bool manaManager()
        {
            var mana = GetValue<Slider>("saveMana").Value;

            if (Player.Mana >= Player.MaxMana * (mana / 100))
            {
                return true;
            }

            return false;
        }

        public int enemiesInRange(Obj_AI_Hero obj, float range)
        {
            return obj.CountEnemiesInRange(range);
        }

        public override float GetComboDamage(Obj_AI_Hero enemy)
        {
            var damage = 0d;

            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) * 1.3;
            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            damage += Player.GetAutoAttackDamage(enemy, true) * 2;

            return (float)damage;
        }

        bool isUnderEnemyTurret(Vector3 Position)
        {
            return Position.UnderTurret(true);
        }


        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("comboQ", "使用 Q").SetValue(true));
            config.AddItem(new MenuItem("comboW", "使用 W").SetValue(true));
            config.AddItem(new MenuItem("comboE", "使用 E").SetValue(true));
            config.AddItem(new MenuItem("comboR", "使用 R").SetValue(true));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("harassQ", "使用 Q").SetValue(true));
            config.AddItem(new MenuItem("harassE", "使用 E").SetValue(false));
        }

        public override void Laneclear(Menu config)
        {
            config.AddItem(new MenuItem("laneClearQ", "使用 Q").SetValue(true));
            config.AddItem(new MenuItem("laneClearE", "使用 E").SetValue(true));
        }

        public override void Misc(Menu config)
        {
            config.AddItem(new MenuItem("comboType", "连招类型").SetValue(new StringList(new[] { "正常", "先进", "高手" }, 2)));
            config.AddItem(new MenuItem("dontETurr", "禁止E到敌人塔下").SetValue(true));
            config.AddItem(new MenuItem("qRange", "定制 Q 范围").SetValue(new Slider((int)Q.Range, 50, (int)Q.Range)));
        }

        public override void Extra(Menu config)
        {
            var MiscMSubMenu = new Menu("蓝量管理", "MiscM");
            {
                MiscMSubMenu.AddItem(new MenuItem("saveMana", "连招最低蓝量").SetValue(new Slider(50, 0, 100)));
            }

            config.AddSubMenu(MiscMSubMenu);

        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("disableAll", "禁止所有范围").SetValue(false));
            config.AddItem(new MenuItem("drawQ", "显示 Q").SetValue(true));
            config.AddItem(new MenuItem("drawW", "显示 W").SetValue(true));
            config.AddItem(new MenuItem("drawE", "显示 E").SetValue(true));
            config.AddItem(new MenuItem("drawR", "显示 R").SetValue(true));
            config.AddItem(new MenuItem("drawComboType", "显示连招类型").SetValue(true));
        }
    }
}
