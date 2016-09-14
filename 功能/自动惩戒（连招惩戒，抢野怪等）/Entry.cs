namespace ElSmite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Entry
    {
        #region Static Fields

        private static readonly string[] SmiteObjects =
            {
                "SRU_Red", "SRU_Blue", "SRU_Dragon_Water",  "SRU_Dragon_Fire", "SRU_Dragon_Earth", "SRU_Dragon_Air", "SRU_Dragon_Elder",
                "SRU_Baron", "SRU_Gromp", "SRU_Murkwolf",
                "SRU_Razorbeak", "SRU_RiftHerald",
                "SRU_Krug", "Sru_Crab", "TT_Spiderboss",
                "TT_NGolem", "TT_NWolf", "TT_NWraith"
            };

        private static Obj_AI_Minion Minion;

        #endregion

        #region Fields

        /// <value>
        ///     The spell type.
        /// </value>
        private SpellDataTargetType TargetType;

        #endregion

        #region Constructors and Destructors

        static Entry()
        {
            Spells = new List<Entry>
                         {
                             new Entry
                                 {
                                     ChampionName = "ChoGath", Range = 325f, Slot = SpellSlot.R, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Elise", Range = 475f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Fizz", Range = 550f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "LeeSin", Range = 1100f, Slot = SpellSlot.Q, Stage = 1,
                                     TargetType = SpellDataTargetType.Self
                                 },
                             new Entry
                                 {
                                     ChampionName = "MonkeyKing", Range = 375f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Nunu", Range = 300f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Olaf", Range = 325f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Pantheon", Range = 600f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "MasterYi", Range = 600f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Volibear", Range = 400f, Slot = SpellSlot.W, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "XinZhao", Range = 600f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Mundo", Range = 1050f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "KhaZix", Range = 325f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Evelynn", Range = 225f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Shen", Range = 520f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Diana", Range = 895f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Alistar", Range = 365f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Nocturne", Range = 1200f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Maokai", Range = 600f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Twitch", Range = 950f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Self
                                 },
                             new Entry
                                 {
                                     ChampionName = "JarvanIV", Range = 770f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Rengar", Range = 325f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Aatrox", Range = 650f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Amumu", Range = 1100f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Gragas", Range = 600f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Trundle", Range = 325f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Fiddlesticks", Range = 750f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Jax", Range = 700f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Ekko", Range = 1075f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Vi", Range = 325f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Self
                                 },
                             new Entry
                                 {
                                     ChampionName = "Shaco", Range = 625f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Warwick", Range = 400f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Sejuani", Range = 625f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Tryndamere", Range = 660f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Zac", Range = 550f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "TahmKench", Range = 880f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Quinn", Range = 1025f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Location
                                 },
                             new Entry
                                 {
                                     ChampionName = "Poppy", Range = 525f, Slot = SpellSlot.E, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Kayle", Range = 650f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Hecarim", Range = 350f, Slot = SpellSlot.Q, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 },
                             new Entry
                                 {
                                     ChampionName = "Renekton", Range = 350f, Slot = SpellSlot.W, Stage = 0,
                                     TargetType = SpellDataTargetType.Unit
                                 }
                         };
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets Summoners Rift map
        /// </summary>
        /// <value>
        ///     The SR map
        /// </value>
        public static bool IsSummonersRift => Game.MapId == GameMapId.SummonersRift;

        /// <summary>
        ///     Gets Twisted Treeline map
        /// </summary>
        /// <value>
        ///     The TT map
        /// </value>
        public static bool IsTwistedTreeline => Game.MapId == GameMapId.TwistedTreeline;

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        public static Obj_AI_Hero Player => ObjectManager.Player;

        /// <summary>
        ///     Gets the script Version
        /// </summary>
        /// <value>
        ///     The Script Version
        /// </value>
        public static string ScriptVersion => typeof(Entry).Assembly.GetName().Version.ToString();

        /// <summary>
        ///     Gets or sets the slot.
        /// </summary>
        /// <value>
        ///     The Smitespell
        /// </value>
        public static Spell SmiteSpell { get; set; }

        /// <summary>
        ///     Gets or sets the spells.
        /// </summary>
        /// <value>
        ///     The spells.
        /// </value>
        public static List<Entry> Spells { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string ChampionName { get; set; }

        /// <summary>
        ///     Gets or sets the range.
        /// </summary>
        /// <value>
        ///     The range.
        /// </value>
        public float Range { get; set; }

        /// <summary>
        ///     Gets or sets the slot.
        /// </summary>
        /// <value>
        ///     The slot.
        /// </value>
        public SpellSlot Slot { get; set; }

        /// <summary>
        ///     Gets or sets the slot.
        /// </summary>
        /// <value>
        ///     The stage.
        /// </value>
        public int Stage { get; set; }

        #endregion

        #region Public Methods and Operators

        public static void OnLoad(EventArgs args)
        {
            try
            {
                var smiteSlot =
                   Player.Spellbook.Spells.FirstOrDefault(
                       x => x.Name.ToLower().Contains("smite"));

                if (smiteSlot != null)
                {
                    Game.PrintChat("[00:00] <font color='#f9eb0b'>ElSmite:</font> Use ElUtilitySuite for a better and faster smite!");
                    SmiteSpell = new Spell(smiteSlot.Slot, 570f, TargetSelector.DamageType.True);
                    Drawing.OnDraw += OnDraw;
                    Game.OnUpdate += OnUpdate;
                }

                InitializeMenu.Load();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                if (Player.IsDead)
                {
                    return;
                }

                try
                {
                    if (!InitializeMenu.Menu.Item("ElSmite.Activated").GetValue<KeyBind>().Active)
                    {
                        return;
                    }

                    Minion =
                   (Obj_AI_Minion)
                   MinionManager.GetMinions(Player.ServerPosition, 570f, MinionTypes.All, MinionTeam.Neutral)
                       .FirstOrDefault(
                           buff => buff.Name.StartsWith(buff.CharData.BaseSkinName)
                           && SmiteObjects.Contains(buff.CharData.BaseSkinName)
                           && !buff.Name.Contains("Mini") && !buff.Name.Contains("Spawn"));

                    if (Minion == null)
                    {
                        return;
                    }

                    if (InitializeMenu.Menu.Item(Minion.CharData.BaseSkinName).IsActive())
                    {
                        if (SmiteSpell.IsReady())
                        {
                            if (Vector3.Distance(ObjectManager.Player.ServerPosition, Minion.ServerPosition)
                                <= SmiteSpell.Range)
                            {
                                if (Player.GetSummonerSpellDamage(Minion, Damage.SummonerSpell.Smite) >= Minion.Health
                                    && SmiteSpell.CanCast(Minion))
                                {
                                    Player.Spellbook.CastSpell(SmiteSpell.Slot, Minion);
                                }
                            }

                            if (InitializeMenu.Menu.Item("Smite.Spell").IsActive())
                            {
                                ChampionSpellSmite(
                                    (float)Player.GetSummonerSpellDamage(Minion, Damage.SummonerSpell.Smite),
                                    Minion);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: '{0}'", e);
                }
                SmiteKill();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }


        #endregion

        #region Methods

        private static void ChampionSpellSmite(double damage, Obj_AI_Base mob)
        {
            try
            {
                foreach (var spell in
                    Spells.Where(
                        x => x.Slot.IsReady() && x.ChampionName.Equals(Player.ChampionName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (Player.GetSpellDamage(mob, spell.Slot, spell.Stage) + damage >= mob.Health)
                    {
                        if (mob.IsValidTarget(SmiteSpell.Range))
                        {
                            if (spell.TargetType == SpellDataTargetType.Unit)
                            {
                                Player.Spellbook.CastSpell(spell.Slot, mob);
                            }
                            else if (spell.TargetType == SpellDataTargetType.Self)
                            {
                                Player.Spellbook.CastSpell(spell.Slot);
                            }
                            else if (spell.TargetType == SpellDataTargetType.Location)
                            {
                                Player.Spellbook.CastSpell(spell.Slot, mob);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        private static void OnDraw(EventArgs args)
        {
            try
            {
                var smiteActive = InitializeMenu.Menu.Item("ElSmite.Activated").GetValue<KeyBind>().Active;
                var drawSmite = InitializeMenu.Menu.Item("ElSmite.Draw.Range").GetValue<Circle>();
                var drawText = InitializeMenu.Menu.Item("ElSmite.Draw.Text").IsActive();
                var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                var drawDamage = InitializeMenu.Menu.Item("ElSmite.Draw.Damage").IsActive();

                if (smiteActive)
                {
                    if (drawText && Player.Spellbook.CanUseSpell(SmiteSpell.Slot) == SpellState.Ready)
                    {
                        Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, Color.GhostWhite, "Smite active");
                    }

                    if (drawText && Player.Spellbook.CanUseSpell(SmiteSpell.Slot) != SpellState.Ready)
                    {
                        Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, Color.Red, "Smite cooldown");
                    }

                    if (drawDamage && SmiteDamage() != 0)
                    {
                        var minions =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(
                                    m =>
                                    m.Team == GameObjectTeam.Neutral && m.IsValidTarget()
                                    && SmiteObjects.Contains(m.CharData.BaseSkinName));

                        foreach (var Minion in minions.Where(m => m.IsHPBarRendered))
                        {
                            var hpBarPosition = Minion.HPBarPosition;
                            var maxHealth = Minion.MaxHealth;
                            var smiteDamage = SmiteDamage();
                            //SmiteDamage : MaxHealth = x : 100
                            //Ratio math for this ^
                            var x = SmiteDamage() / maxHealth;
                            var barWidth = 0;

                            /*
                            * DON'T STEAL THE OFFSETS FOUND BY ASUNA DON'T STEAL THEM JUST GET OUT WTF MAN.
                            * EL SMITE IS THE BEST SMITE ASSEMBLY ON LEAGUESHARP AND YOU WILL NOT FIND A BETTER ONE.
                            * THE DRAWINGS ACTUALLY MAKE FUCKING SENSE AND THEY ARE FUCKING GOOD
                            * GTFO HERE SERIOUSLY OR I CALL DETUKS FOR YOU GUYS
                            * NO STEAL OR DMC FUCKING A REPORT.
                            * HELLO COPYRIGHT BY ASUNA 2015 ALL AUSTRALIAN RIGHTS RESERVED BY UNIVERSAL GTFO SERIOUSLY THO
                            * NO ALSO NO CREDITS JUST GET OUT DUDE GET OUTTTTTTTTTTTTTTTTTTTTTTT
                            */

                            //Console.WriteLine(Minion.CharData.BaseSkinName);

                            switch (Minion.CharData.BaseSkinName)
                            {
                                case "SRU_RiftHerald":
                                    barWidth = 145;
                                    Drawing.DrawLine(
                                        new Vector2(hpBarPosition.X + 3 + (float)(barWidth * x), hpBarPosition.Y + 17),
                                        new Vector2(hpBarPosition.X + 3 + (float)(barWidth * x), hpBarPosition.Y + 30),
                                        2f,
                                        Color.Chartreuse);
                                    Drawing.DrawText(
                                        hpBarPosition.X - 22 + (float)(barWidth * x),
                                        hpBarPosition.Y - 5,
                                        Color.Chartreuse,
                                        smiteDamage.ToString());
                                    break;

                                case "SRU_Dragon_Air":
                                case "SRU_Dragon_Water":
                                case "SRU_Dragon_Fire":
                                case "SRU_Dragon_Earth":
                                case "SRU_Dragon_Elder":
                                    barWidth = 145;
                                    Drawing.DrawLine(
                                        new Vector2(hpBarPosition.X + 3 + (float)(barWidth * x), hpBarPosition.Y + 22),
                                        new Vector2(hpBarPosition.X + 3 + (float)(barWidth * x), hpBarPosition.Y + 30),
                                        2f,
                                        Color.Chartreuse);
                                    Drawing.DrawText(
                                        hpBarPosition.X - 22 + (float)(barWidth * x),
                                        hpBarPosition.Y - 5,
                                        Color.Chartreuse,
                                        smiteDamage.ToString());
                                    break;

                                case "SRU_Red":
                                case "SRU_Blue":
                                    barWidth = 145;
                                    Drawing.DrawLine(
                                        new Vector2(hpBarPosition.X + 3 + (float)(barWidth * x), hpBarPosition.Y + 20),
                                        new Vector2(hpBarPosition.X + 3 + (float)(barWidth * x), hpBarPosition.Y + 30),
                                        2f,
                                        Color.Chartreuse);
                                    Drawing.DrawText(
                                        hpBarPosition.X - 22 + (float)(barWidth * x),
                                        hpBarPosition.Y - 5,
                                        Color.Chartreuse,
                                        smiteDamage.ToString());
                                    break;

                                case "SRU_Baron":
                                    barWidth = 194;
                                    Drawing.DrawLine(
                                        new Vector2(hpBarPosition.X + 18 + (float)(barWidth * x), hpBarPosition.Y + 20),
                                        new Vector2(hpBarPosition.X + 18 + (float)(barWidth * x), hpBarPosition.Y + 35),
                                        2f,
                                        Color.Chartreuse);
                                    Drawing.DrawText(
                                        hpBarPosition.X - 22 + (float)(barWidth * x),
                                        hpBarPosition.Y - 3,
                                        Color.Chartreuse,
                                        smiteDamage.ToString());
                                    break;

                                case "SRU_Gromp":
                                    barWidth = 87;
                                    Drawing.DrawLine(
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 11),
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 4),
                                        2f,
                                        Color.Chartreuse);
                                    Drawing.DrawText(
                                        hpBarPosition.X + (float)(barWidth * x),
                                        hpBarPosition.Y - 15,
                                        Color.Chartreuse,
                                        smiteDamage.ToString());
                                    break;

                                case "SRU_Murkwolf":
                                    barWidth = 75;
                                    Drawing.DrawLine(
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 11),
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 4),
                                        2f,
                                        Color.Chartreuse);
                                    Drawing.DrawText(
                                        hpBarPosition.X + (float)(barWidth * x),
                                        hpBarPosition.Y - 15,
                                        Color.Chartreuse,
                                        smiteDamage.ToString());
                                    break;

                                case "Sru_Crab":
                                    barWidth = 61;
                                    Drawing.DrawLine(
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 8),
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 4),
                                        2f,
                                        Color.Chartreuse);
                                    Drawing.DrawText(
                                        hpBarPosition.X + (float)(barWidth * x),
                                        hpBarPosition.Y - 15,
                                        Color.Chartreuse,
                                        smiteDamage.ToString());
                                    break;

                                case "SRU_Razorbeak":
                                    barWidth = 75;
                                    Drawing.DrawLine(
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 11),
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 4),
                                        2f,
                                        Color.Chartreuse);
                                    Drawing.DrawText(
                                        hpBarPosition.X + (float)(barWidth * x),
                                        hpBarPosition.Y - 15,
                                        Color.Chartreuse,
                                        smiteDamage.ToString());
                                    break;

                                case "SRU_Krug":
                                    barWidth = 81;
                                    Drawing.DrawLine(
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 11),
                                        new Vector2(hpBarPosition.X + (float)(barWidth * x), hpBarPosition.Y + 4),
                                        2f,
                                        Color.Chartreuse);
                                    Drawing.DrawText(
                                        hpBarPosition.X + (float)(barWidth * x),
                                        hpBarPosition.Y - 15,
                                        Color.Chartreuse,
                                        smiteDamage.ToString());
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    if (drawText)
                    {
                        Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, Color.Red, "Smite not active");
                    }
                }

                if (smiteActive && drawSmite.Active && Player.Spellbook.CanUseSpell(SmiteSpell.Slot) == SpellState.Ready)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 570, Color.Green);
                }

                if (drawSmite.Active && Player.Spellbook.CanUseSpell(SmiteSpell.Slot) != SpellState.Ready)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 570, Color.Red);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }          
        }
        
        private static double SmiteDamage()
        {
            try
            {
                return Player.Spellbook.GetSpell(SmiteSpell.Slot).State == SpellState.Ready
                           ? (float)Player.GetSummonerSpellDamage(Minion, Damage.SummonerSpell.Smite)
                           : 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }

            return 0;
        }

        private static void SmiteKill()
        {
            try
            {
                if (!InitializeMenu.Menu.Item("ElSmite.KS.Activated").IsActive())
                {
                    return;
                }

                if (InitializeMenu.Menu.Item("Smite.Ammo").IsActive() && Player.GetSpell(SmiteSpell.Slot).Ammo == 1)
                {
                    return;
                }

                if (Player.GetSpell(SmiteSpell.Slot).Name.ToLower() != "s5_summonersmiteplayerganker")
                {
                    return;
                }

                var kSableEnemy =
                    HeroManager.Enemies.FirstOrDefault(
                        hero => !hero.IsZombie && hero.IsValidTarget(570) && 20 + 8 * Player.Level >= hero.Health);
                if (kSableEnemy != null)
                {
                    Player.Spellbook.CastSpell(SmiteSpell.Slot, kSableEnemy);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion
    }
}
