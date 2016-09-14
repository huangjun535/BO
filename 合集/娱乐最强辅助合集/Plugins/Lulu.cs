﻿namespace Support.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using Support.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Lulu : PluginBase
    {
        public Lulu()
        {
            this.Q = new Spell(SpellSlot.Q, 925);
            this.W = new Spell(SpellSlot.W, 650);
            this.E = new Spell(SpellSlot.E, 650); //shield
            this.R = new Spell(SpellSlot.R, 900);

            this.Q.SetSkillshot(0.25f, 60, 1450, false, SkillshotType.SkillshotLine);
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "Use Q", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserW", "Use W to Interrupt Gapcloser", true);

            config.AddBool("InterruptW", "Use W to Interrupt Spells", true);
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (this.W.CastCheck(gapcloser.Sender, "GapcloserW"))
            {
                this.W.CastOnUnit(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (this.W.CastCheck(target, "InterruptW"))
            {
                this.W.CastOnUnit(target);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (this.ComboMode)
            {
                if (this.Q.CastCheck(this.Target, "ComboQ"))
                {
                    this.Q.Cast(this.Target);
                }

                if (this.W.CastCheck(this.Target, "ComboW"))
                {
                    this.W.CastOnUnit(this.Target);
                }
            }

            if (this.HarassMode)
            {
                if (this.Q.CastCheck(this.Target, "HarassQ"))
                {
                    this.Q.Cast(this.Target);
                }
            }
        }
    }
}