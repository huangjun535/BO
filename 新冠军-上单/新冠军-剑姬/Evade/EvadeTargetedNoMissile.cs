﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;
using YuLeFiora.Evade;

namespace YuLeFiora
{
    #region Targeted NoMissile
    internal class TargetedNoMissile
    {
        private static readonly List<SpellData> Spells = new List<SpellData>();
        private static Menu Menu = Program.Menu;
        private static Obj_AI_Hero Player = Program.Player;
        private static Spell W = Program.W;
        private static readonly List<DashTarget> DetectedDashes = new List<DashTarget>();
        internal static void Init()
        {
            LoadSpellData();
            Spells.RemoveAll(i => !HeroManager.Enemies.Any(
            a =>
            string.Equals(
                a.ChampionName,
                i.ChampionName,
                StringComparison.InvariantCultureIgnoreCase)));
            var evadeMenu = Program.Menu.SubMenu("Evade");
            {
                foreach (var spell in
                    Spells.Where(
                        i =>
                        HeroManager.Enemies.Any(
                            a =>
                            string.Equals(
                                a.ChampionName,
                                i.ChampionName,
                                StringComparison.InvariantCultureIgnoreCase))))
                {
                    evadeMenu.SubMenu(spell.ChampionName.ToLowerInvariant()).Bool(
                        spell.ChampionName + spell.Slot,
                        spell.ChampionName + " (" + spell.Slot + ")",
                        true);
                }
            }

            Game.OnUpdate += OnUpdateDashes;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var caster = sender as Obj_AI_Hero;
            if (caster == null || !caster.IsValid || caster.Team == Player.Team || !(args.Target != null && args.Target.IsMe))
            {
                return;
            }
            var spellData =
               Spells.FirstOrDefault(
                   i =>
                   caster.ChampionName.ToLowerInvariant() == i.ChampionName.ToLowerInvariant()
                   && (i.UseSpellSlot ? args.Slot == i.Slot :
                   i.SpellNames.Any(x => x.ToLowerInvariant() == args.SData.Name.ToLowerInvariant()))
                   && Menu.SubMenu("Evade").SubMenu(i.ChampionName.ToLowerInvariant())
                   .Item(i.ChampionName + i.Slot).GetValue<bool>());
            if (spellData == null)
            {
                return;
            }
            if (spellData.IsDash)
            {
                DetectedDashes.Add(new DashTarget { Hero = caster, DistanceDash = spellData.DistanceDash, TickCount = Utils.GameTimeTickCount });
            }
            else
            {
                if (Player.IsDead)
                {
                    return;
                }
                if (Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity))
                {
                    return;
                }
                if (!Menu.SubMenu("Evade").Item("W").GetValue<KeyBind>().Active || !W.IsReady())
                {
                    return;
                }
                var tar = Program.GetTarget(W.Range);
                if (tar.IsValidTarget(W.Range))
                    Player.Spellbook.CastSpell(SpellSlot.W, tar.Position);
                else
                {
                    var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                    if (hero != null)
                        Player.Spellbook.CastSpell(SpellSlot.W, hero.Position);
                    else
                        Player.Spellbook.CastSpell(SpellSlot.W, Player.ServerPosition.Extend(caster.Position, 100));
                }
            }
        }

        private static void OnUpdateDashes(EventArgs args)
        {
            DetectedDashes.RemoveAll(
                x =>
                x.Hero == null || !x.Hero.IsValid
                || (!x.Hero.IsDashing() && Utils.GameTimeTickCount > x.TickCount + 500));

            if (Player.IsDead)
            {
                return;
            }
            if (Player.HasBuffOfType(BuffType.SpellShield) || Player.HasBuffOfType(BuffType.SpellImmunity))
            {
                return;
            }
            if (!Menu.SubMenu("Evade").Item("W").GetValue<KeyBind>().Active || !W.IsReady())
            {
                return;
            }
            foreach (var target in
                 DetectedDashes.OrderBy(i => i.Hero.Position.Distance(Player.Position)))
            {
                var dashdata = target.Hero.GetDashInfo();
                if (dashdata != null && target.Hero.Position.To2D().Distance(Player.Position.To2D())
                    < target.DistanceDash + Game.Ping * dashdata.Speed / 1000)
                {
                    var tar = Program.GetTarget(W.Range);
                    if (tar.IsValidTarget(W.Range))
                        Player.Spellbook.CastSpell(SpellSlot.W, tar.Position);
                    else
                    {
                        var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(W.Range));
                        if (hero != null)
                            Player.Spellbook.CastSpell(SpellSlot.W, hero.Position);
                        else
                            Player.Spellbook.CastSpell(SpellSlot.W, Player.ServerPosition.Extend(target.Hero.Position, 100));
                    }
                }
            }
        }

        #region SpellData
        private static void LoadSpellData()
        {
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Akali",
                    UseSpellSlot = true,
                    Slot = SpellSlot.R,
                    IsDash = true
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Alistar",
                    UseSpellSlot = true,
                    Slot = SpellSlot.W
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Alistar",
                    UseSpellSlot = true,
                    Slot = SpellSlot.W
                });
            //blitz
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Blitzcrank",
                    Slot = SpellSlot.E,
                    SpellNames = new[] { "PowerFistAttack" }
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Brand",
                    UseSpellSlot = true,
                    Slot = SpellSlot.E
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Chogath",
                    UseSpellSlot = true,
                    Slot = SpellSlot.R
                });
            //darius W confirmed
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Darius",
                    Slot = SpellSlot.W,
                    SpellNames = new[] { "DariusNoxianTacticsONHAttack" }
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Darius",
                    UseSpellSlot = true,
                    Slot = SpellSlot.R
                });
            //ekkoE confirmed
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ekko",
                    Slot = SpellSlot.E,
                    SpellNames = new[] { "EkkoEAttack" }
                });
            //eliseQ confirm
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Elise",
                    Slot = SpellSlot.Q,
                    SpellNames = new[] { "EliseSpiderQCast" }
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Evelynn",
                    UseSpellSlot = true,
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Fiddlesticks",
                    UseSpellSlot = true,
                    Slot = SpellSlot.Q,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Fizz",
                    UseSpellSlot = true,
                    Slot = SpellSlot.Q,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Garen",
                    Slot = SpellSlot.Q,
                    SpellNames = new[] { "GarenQAttack" }
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Garen",
                    UseSpellSlot = true,
                    Slot = SpellSlot.R,
                });
            // hercarim E confirmed
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Hecarim",
                    Slot = SpellSlot.E,
                    SpellNames = new[] { "HecarimRampAttack" }
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Irelia",
                    UseSpellSlot = true,
                    Slot = SpellSlot.Q,
                    IsDash = true
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Irelia",
                    UseSpellSlot = true,
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jarvan",
                    UseSpellSlot = true,
                    Slot = SpellSlot.R,
                });
            ////jax W later
            //Spells.Add(
            //    new SpellData
            //    {
            //        ChampionName = "Jax",
            //        Slot = SpellSlot.W,
            //        SpellNames = new[] { "JaxEmpowerAttack", "JaxEmpowerTwo" }
            //    });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jax",
                    UseSpellSlot = true,
                    Slot = SpellSlot.Q,
                    IsDash = true
                });
            //jax R confirmed
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jax",
                    Slot = SpellSlot.R,
                    SpellNames = new[] { "JaxRelentlessAttack" }
                });
            //jayce Q confirm
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jayce",
                    Slot = SpellSlot.Q,
                    SpellNames = new[] { "JayceToTheSkies" },
                    IsDash = true,
                    DistanceDash = 400
                });
            //jayce E confirm
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jayce",
                    Slot = SpellSlot.E,
                    SpellNames = new[] { "JayceThunderingBlow" }
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Khazix",
                    UseSpellSlot = true,
                    Slot = SpellSlot.Q,
                });
            //leesin Q2 later
            //Spells.Add(
            //    new SpellData
            //    {
            //        ChampionName = "Leesin",
            //        Slot = SpellSlot.Q,
            //        SpellNames = new[] { "BlindMonkQTwo" },
            //        IsDash = true
            //    });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Leesin",
                    UseSpellSlot = true,
                    Slot = SpellSlot.R,
                });
            //leona Q confirmed
            Spells.Add(
               new SpellData
               {
                   ChampionName = "Leona",
                   Slot = SpellSlot.Q,
                   SpellNames = new[] { "LeonaShieldOfDaybreakAttack" }
               });
            // lissandra R
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lissandra",
                    UseSpellSlot = true,
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lucian",
                    UseSpellSlot = true,
                    Slot = SpellSlot.Q,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Malzahar",
                    UseSpellSlot = true,
                    Slot = SpellSlot.E,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Malzahar",
                    UseSpellSlot = true,
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Maokai",
                    UseSpellSlot = true,
                    Slot = SpellSlot.W,
                    IsDash = true
                });
            //mordekaiserQ confirmed
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Mordekaiser",
                    Slot = SpellSlot.Q,
                    SpellNames = new[] { "MordekaiserQAttack", "MordekaiserQAttack1", "MordekaiserQAttack2" }
                });
            // mordekaiser R confirmed
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Mordekaiser",
                    UseSpellSlot = true,
                    Slot = SpellSlot.R,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nasus",
                    Slot = SpellSlot.Q,
                    SpellNames = new[] { "NasusQAttack" }
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nasus",
                    UseSpellSlot = true,
                    Slot = SpellSlot.W,
                });
            Spells.Add(
                new SpellData
                {
                    ChampionName = "MonkeyKing",
                    Slot = SpellSlot.Q,
                    SpellNames = new[] { "MonkeyKingQAttack" }
                });
            //nidalee Q confirmed
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Nidalee",
                     Slot = SpellSlot.Q,
                     SpellNames = new[] { "NidaleeTakedownAttack", "Nidalee_CougarTakedownAttack" }
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Olaf",
                     UseSpellSlot = true,
                     Slot = SpellSlot.E,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Pantheon",
                     UseSpellSlot = true,
                     Slot = SpellSlot.W,
                 });
            //poppy Q later
            //Spells.Add(
            //     new SpellData
            //     {
            //         ChampionName = "Poppy",
            //         Slot = SpellSlot.Q,
            //         SpellNames = new[] { "PoppyDevastatingBlow" }
            //     });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Poppy",
                     UseSpellSlot = true,
                     Slot = SpellSlot.E,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Poppy",
                     UseSpellSlot = true,
                     Slot = SpellSlot.R,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Quinn",
                     UseSpellSlot = true,
                     Slot = SpellSlot.E,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Rammus",
                     UseSpellSlot = true,
                     Slot = SpellSlot.E,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "RekSai",
                     UseSpellSlot = true,
                     Slot = SpellSlot.E,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Renekton",
                     Slot = SpellSlot.W,
                     SpellNames = new[] { "RenektonExecute", "RenektonSuperExecute" }
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Ryze",
                     UseSpellSlot = true,
                     Slot = SpellSlot.W,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Singed",
                     UseSpellSlot = true,
                     Slot = SpellSlot.E,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Skarner",
                     UseSpellSlot = true,
                     Slot = SpellSlot.R,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "TahmKench",
                     UseSpellSlot = true,
                     Slot = SpellSlot.W,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Talon",
                     UseSpellSlot = true,
                     Slot = SpellSlot.E,
                 });
            //talonQ confirmed
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Talon",
                     Slot = SpellSlot.Q,
                     SpellNames = new[] { "TalonNoxianDiplomacyAttack" }
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Trundle",
                     UseSpellSlot = true,
                     Slot = SpellSlot.R,
                 });
            //udyr E : todo : check for stun buff
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Udyr",
                     Slot = SpellSlot.E,
                     SpellNames = new[] { "UdyrBearAttack", "UdyrBearAttackUlt" }
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Vi",
                     UseSpellSlot = true,
                     Slot = SpellSlot.R,
                     IsDash = true,
                 });
            //viktor Q confirmed
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Viktor",
                     Slot = SpellSlot.Q,
                     SpellNames = new[] { "ViktorQBuff" }
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Vladimir",
                     UseSpellSlot = true,
                     Slot = SpellSlot.Q,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Volibear",
                     UseSpellSlot = true,
                     Slot = SpellSlot.W,
                 });
            //volibear Q confirmed
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Volibear",
                     Slot = SpellSlot.Q,
                     SpellNames = new[] { "VolibearQAttack" }
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Warwick",
                     UseSpellSlot = true,
                     Slot = SpellSlot.Q,
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Warwick",
                     UseSpellSlot = true,
                     Slot = SpellSlot.R,
                 });
            //xinzhaoQ3 confirmed
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "XinZhao",
                     Slot = SpellSlot.Q,
                     SpellNames = new[] { "XenZhaoThrust3" }
                 });
            Spells.Add(
                 new SpellData
                 {
                     ChampionName = "Yorick",
                     UseSpellSlot = true,
                     Slot = SpellSlot.E,
                 });
            //yorick Q
            //Spells.Add(
            //     new SpellData
            //     {
            //         ChampionName = "Yorick",
            //         Slot = SpellSlot.Q,
            //         SpellNames = new[] {"" }
            //     });
            Spells.Add(
             new SpellData
             {
                 ChampionName = "Zilean",
                 UseSpellSlot = true,
                 Slot = SpellSlot.E,
             });
        }
        #endregion SpellData

        private class SpellData
        {
            #region Fields

            public string ChampionName;

            public bool UseSpellSlot = false;

            public SpellSlot Slot;

            public string[] SpellNames = { };

            public bool IsDash = false;

            public float DistanceDash = 200;

            #endregion

            #region Public Properties

            public string MissileName
            {
                get
                {
                    return this.SpellNames.First();
                }
            }

            #endregion
        }
        private class DashTarget
        {
            #region Fields

            public Obj_AI_Hero Hero;

            public float DistanceDash = 200;

            public int TickCount;

            #endregion
        }
    }
    #endregion Targeted NoMissile

}
