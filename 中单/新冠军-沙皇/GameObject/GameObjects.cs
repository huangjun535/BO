namespace YuLeAzir
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    public static class GameObjects
    {
        #region Static Fields

        /// <summary>
        ///     The ally heroes list.
        /// </summary>
        private static readonly List<Obj_AI_Hero> AllyHeroesList = new List<Obj_AI_Hero>();

        /// <summary>
        ///     The ally inhibitors list.
        /// </summary>
        private static readonly List<Obj_BarracksDampener> AllyInhibitorsList = new List<Obj_BarracksDampener>();

        /// <summary>
        ///     The ally list.
        /// </summary>
        private static readonly List<Obj_AI_Base> AllyList = new List<Obj_AI_Base>();

        /// <summary>
        ///     The ally minions list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> AllyMinionsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The ally shops list.
        /// </summary>
        private static readonly List<Obj_Shop> AllyShopsList = new List<Obj_Shop>();

        /// <summary>
        ///     The ally spawn points list.
        /// </summary>
        private static readonly List<Obj_SpawnPoint> AllySpawnPointsList = new List<Obj_SpawnPoint>();

        /// <summary>
        ///     The ally turrets list.
        /// </summary>
        private static readonly List<Obj_AI_Turret> AllyTurretsList = new List<Obj_AI_Turret>();

        /// <summary>
        ///     The ally wards list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> AllyWardsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The attackable unit list.
        /// </summary>
        private static readonly List<AttackableUnit> AttackableUnitsList = new List<AttackableUnit>();

        /// <summary>
        ///     The enemy heroes list.
        /// </summary>
        private static readonly List<Obj_AI_Hero> EnemyHeroesList = new List<Obj_AI_Hero>();

        /// <summary>
        ///     The enemy inhibitors list.
        /// </summary>
        private static readonly List<Obj_BarracksDampener> EnemyInhibitorsList = new List<Obj_BarracksDampener>();

        /// <summary>
        ///     The enemy list.
        /// </summary>
        private static readonly List<Obj_AI_Base> EnemyList = new List<Obj_AI_Base>();

        /// <summary>
        ///     The enemy minions list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> EnemyMinionsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The enemy shops list.
        /// </summary>
        private static readonly List<Obj_Shop> EnemyShopsList = new List<Obj_Shop>();

        /// <summary>
        ///     The enemy spawn points list.
        /// </summary>
        private static readonly List<Obj_SpawnPoint> EnemySpawnPointsList = new List<Obj_SpawnPoint>();

        /// <summary>
        ///     The enemy turrets list.
        /// </summary>
        private static readonly List<Obj_AI_Turret> EnemyTurretsList = new List<Obj_AI_Turret>();

        /// <summary>
        ///     The enemy wards list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> EnemyWardsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The game objects list.
        /// </summary>
        private static readonly List<GameObject> GameObjectsList = new List<GameObject>();

        /// <summary>
        ///     The heroes list.
        /// </summary>
        private static readonly List<Obj_AI_Hero> HeroesList = new List<Obj_AI_Hero>();

        /// <summary>
        ///     The inhibitors list.
        /// </summary>
        private static readonly List<Obj_BarracksDampener> InhibitorsList = new List<Obj_BarracksDampener>();

        /// <summary>
        ///     The jungle large list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> JungleLargeList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The jungle legendary list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> JungleLegendaryList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The jungle list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> JungleList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The jungle small list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> JungleSmallList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The minions list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> MinionsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The nexus list.
        /// </summary>
        private static readonly List<Obj_HQ> NexusList = new List<Obj_HQ>();

        /// <summary>
        ///     The general particle emitters list.
        /// </summary>
        private static readonly List<Obj_GeneralParticleEmitter> ParticleEmittersList =
            new List<Obj_GeneralParticleEmitter>();

        /// <summary>
        ///     The shops list.
        /// </summary>
        private static readonly List<Obj_Shop> ShopsList = new List<Obj_Shop>();

        /// <summary>
        ///     The spawn points list.
        /// </summary>
        private static readonly List<Obj_SpawnPoint> SpawnPointsList = new List<Obj_SpawnPoint>();

        /// <summary>
        ///     The turrets list.
        /// </summary>
        private static readonly List<Obj_AI_Turret> TurretsList = new List<Obj_AI_Turret>();

        /// <summary>
        ///     The wards list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> WardsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     Indicates whether the <see cref="GameObjects" /> stack was initialized and saved required instances.
        /// </summary>
        private static bool initialized = false;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="GameObjects" /> class.
        /// </summary>
        static GameObjects()
        {
            Initialize();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the game objects.
        /// </summary>
        public static IEnumerable<GameObject> AllGameObjects => GameObjectsList;

        /// <summary>
        ///     Gets the ally.
        /// </summary>
        public static IEnumerable<Obj_AI_Base> Ally => AllyList;

        /// <summary>
        ///     Gets the ally heroes.
        /// </summary>
        public static IEnumerable<Obj_AI_Hero> AllyHeroes => AllyHeroesList;

        /// <summary>
        ///     Gets the ally inhibitors.
        /// </summary>
        public static IEnumerable<Obj_BarracksDampener> AllyInhibitors => AllyInhibitorsList;

        /// <summary>
        ///     Gets the ally minions.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> AllyMinions => AllyMinionsList;

        /// <summary>
        ///     Gets or sets the ally nexus.
        /// </summary>
        public static Obj_HQ AllyNexus { get; set; }

        /// <summary>
        ///     Gets the ally shops.
        /// </summary>
        public static IEnumerable<Obj_Shop> AllyShops => AllyShopsList;

        /// <summary>
        ///     Gets the ally spawn points.
        /// </summary>
        public static IEnumerable<Obj_SpawnPoint> AllySpawnPoints => AllySpawnPointsList;

        /// <summary>
        ///     Gets the ally turrets.
        /// </summary>
        public static IEnumerable<Obj_AI_Turret> AllyTurrets => AllyTurretsList;

        /// <summary>
        ///     Gets the ally wards.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> AllyWards => AllyWardsList;

        /// <summary>
        ///     Gets the attackable units.
        /// </summary>
        public static IEnumerable<AttackableUnit> AttackableUnits => AttackableUnitsList;

        /// <summary>
        ///     Gets the enemy.
        /// </summary>
        public static IEnumerable<Obj_AI_Base> Enemy => EnemyList;

        /// <summary>
        ///     Gets the enemy heroes.
        /// </summary>
        public static IEnumerable<Obj_AI_Hero> EnemyHeroes => EnemyHeroesList;

        /// <summary>
        ///     Gets the enemy inhibitors.
        /// </summary>
        public static IEnumerable<Obj_BarracksDampener> EnemyInhibitors => EnemyInhibitorsList;

        /// <summary>
        ///     Gets the enemy minions.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> EnemyMinions => EnemyMinionsList;

        /// <summary>
        ///     Gets or sets the enemy nexus.
        /// </summary>
        public static Obj_HQ EnemyNexus { get; set; }

        /// <summary>
        ///     Gets the enemy shops.
        /// </summary>
        public static IEnumerable<Obj_Shop> EnemyShops => EnemyShopsList;

        /// <summary>
        ///     Gets the enemy spawn points.
        /// </summary>
        public static IEnumerable<Obj_SpawnPoint> EnemySpawnPoints => EnemySpawnPointsList;

        /// <summary>
        ///     Gets the enemy turrets.
        /// </summary>
        public static IEnumerable<Obj_AI_Turret> EnemyTurrets => EnemyTurretsList;

        /// <summary>
        ///     Gets the enemy wards.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> EnemyWards => EnemyWardsList;

        /// <summary>
        ///     Gets the heroes.
        /// </summary>
        public static IEnumerable<Obj_AI_Hero> Heroes => HeroesList;

        /// <summary>
        ///     Gets the inhibitors.
        /// </summary>
        public static IEnumerable<Obj_BarracksDampener> Inhibitors => InhibitorsList;

        /// <summary>
        ///     Gets the jungle.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> Jungle => JungleList;

        /// <summary>
        ///     Gets the jungle large.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> JungleLarge => JungleLargeList;

        /// <summary>
        ///     Gets the jungle legendary.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> JungleLegendary => JungleLegendaryList;

        /// <summary>
        ///     Gets the jungle small.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> JungleSmall => JungleSmallList;

        /// <summary>
        ///     Gets the minions.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> Minions => MinionsList;

        /// <summary>
        ///     Gets the nexuses.
        /// </summary>
        public static IEnumerable<Obj_HQ> Nexuses => NexusList;

        /// <summary>
        ///     Gets the general particle emitters.
        /// </summary>
        public static IEnumerable<Obj_GeneralParticleEmitter> ParticleEmitters => ParticleEmittersList;

        /// <summary>
        ///     Gets or sets the player.
        /// </summary>
        public static Obj_AI_Hero Player { get; set; }

        /// <summary>
        ///     Gets the shops.
        /// </summary>
        public static IEnumerable<Obj_Shop> Shops => ShopsList;

        /// <summary>
        ///     Gets the spawn points.
        /// </summary>
        public static IEnumerable<Obj_SpawnPoint> SpawnPoints => SpawnPointsList;

        /// <summary>
        ///     Gets the turrets.
        /// </summary>
        public static IEnumerable<Obj_AI_Turret> Turrets => TurretsList;

        /// <summary>
        ///     Gets the wards.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> Wards => WardsList;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Compares two <see cref="GameObject" /> and returns if they are identical.
        /// </summary>
        /// <param name="gameObject">The GameObject</param>
        /// <param name="object">The Compare GameObject</param>
        /// <returns>Whether the <see cref="GameObject" />s are identical.</returns>
        public static bool Compare(this GameObject gameObject, GameObject @object)
        {
            return gameObject != null && gameObject.IsValid && @object != null && @object.IsValid
                   && gameObject.NetworkId == @object.NetworkId;
        }

        /// <summary>
        ///     The get operation from the GameObjects stack.
        /// </summary>
        /// <typeparam name="T">
        ///     The requested <see cref="GameObject" /> type.
        /// </typeparam>
        /// <returns>
        ///     The List containing the requested type.
        /// </returns>
        public static IEnumerable<T> Get<T>() where T : GameObject, new()
        {
            return AllGameObjects.OfType<T>();
        }

        /// <summary>
        ///     Get get operation from the native GameObjects stack.
        /// </summary>
        /// <typeparam name="T">
        ///     The requested <see cref="GameObject" /> type.
        /// </typeparam>
        /// <returns>
        ///     The List containing the requested type.
        /// </returns>
        public static IEnumerable<T> GetNative<T>() where T : GameObject, new()
        {
            return ObjectManager.Get<T>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The initialize method.
        /// </summary>
        internal static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            CustomEvents.Game.OnGameLoad += (args) =>
            {
                Player = ObjectManager.Player;

                HeroesList.AddRange(ObjectManager.Get<Obj_AI_Hero>());
                MinionsList.AddRange(
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            o => o.Team != GameObjectTeam.Neutral && !o.GetMinionType().HasFlag(MinionTypes.Ward)));
                TurretsList.AddRange(ObjectManager.Get<Obj_AI_Turret>());
                InhibitorsList.AddRange(ObjectManager.Get<Obj_BarracksDampener>());
                JungleList.AddRange(
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(o => o.Team == GameObjectTeam.Neutral && o.Name != "WardCorpse"));
                WardsList.AddRange(
                    ObjectManager.Get<Obj_AI_Minion>().Where(o => o.GetMinionType().HasFlag(MinionTypes.Ward)));
                ShopsList.AddRange(ObjectManager.Get<Obj_Shop>());
                SpawnPointsList.AddRange(ObjectManager.Get<Obj_SpawnPoint>());
                GameObjectsList.AddRange(ObjectManager.Get<GameObject>());
                NexusList.AddRange(ObjectManager.Get<Obj_HQ>());
                AttackableUnitsList.AddRange(ObjectManager.Get<AttackableUnit>());
                ParticleEmittersList.AddRange(ObjectManager.Get<Obj_GeneralParticleEmitter>());

                EnemyHeroesList.AddRange(HeroesList.Where(o => o.IsEnemy));
                EnemyMinionsList.AddRange(MinionsList.Where(o => o.IsEnemy));
                EnemyTurretsList.AddRange(TurretsList.Where(o => o.IsEnemy));
                EnemyInhibitorsList.AddRange(InhibitorsList.Where(o => o.IsEnemy));
                EnemyList.AddRange(
                    EnemyHeroesList.Cast<Obj_AI_Base>().Concat(EnemyMinionsList).Concat(EnemyTurretsList));
                EnemyNexus = NexusList.FirstOrDefault(n => n.IsEnemy);

                AllyHeroesList.AddRange(HeroesList.Where(o => o.IsAlly));
                AllyMinionsList.AddRange(MinionsList.Where(o => o.IsAlly));
                AllyTurretsList.AddRange(TurretsList.Where(o => o.IsAlly));
                AllyInhibitorsList.AddRange(InhibitorsList.Where(o => o.IsAlly));
                AllyList.AddRange(
                    AllyHeroesList.Cast<Obj_AI_Base>().Concat(AllyMinionsList).Concat(AllyTurretsList));
                AllyNexus = NexusList.FirstOrDefault(n => n.IsAlly);

                JungleSmallList.AddRange(JungleList.Where(o => o.GetJungleType() == JungleType.Small));
                JungleLargeList.AddRange(JungleList.Where(o => o.GetJungleType() == JungleType.Large));
                JungleLegendaryList.AddRange(JungleList.Where(o => o.GetJungleType() == JungleType.Legendary));

                AllyWardsList.AddRange(WardsList.Where(o => o.IsAlly));
                EnemyWardsList.AddRange(WardsList.Where(o => o.IsEnemy));

                AllyShopsList.AddRange(ShopsList.Where(o => o.IsAlly));
                EnemyShopsList.AddRange(ShopsList.Where(o => o.IsEnemy));

                AllySpawnPointsList.AddRange(SpawnPointsList.Where(o => o.IsAlly));
                EnemySpawnPointsList.AddRange(SpawnPointsList.Where(o => o.IsEnemy));

                GameObject.OnCreate += OnCreate;
                GameObject.OnDelete += OnDelete;
            };
        }

        /// <summary>
        ///     OnCreate event.
        /// </summary>
        /// <param name="sender">
        ///     The sender
        /// </param>
        /// <param name="args">
        ///     The event data
        /// </param>
        private static void OnCreate(GameObject sender, EventArgs args)
        {
            GameObjectsList.Add(sender);

            var attackableUnit = sender as AttackableUnit;
            if (attackableUnit != null)
            {
                AttackableUnitsList.Add(attackableUnit);
            }

            var hero = sender as Obj_AI_Hero;
            if (hero != null)
            {
                HeroesList.Add(hero);
                if (hero.IsEnemy)
                {
                    EnemyHeroesList.Add(hero);
                    EnemyList.Add(hero);
                }
                else
                {
                    AllyHeroesList.Add(hero);
                    AllyList.Add(hero);
                }

                return;
            }

            var minion = sender as Obj_AI_Minion;
            if (minion != null)
            {
                if (minion.Team != GameObjectTeam.Neutral)
                {
                    if (minion.GetMinionType().HasFlag(MinionTypes.Ward))
                    {
                        WardsList.Add(minion);
                        if (minion.IsEnemy)
                        {
                            EnemyWardsList.Add(minion);
                        }
                        else
                        {
                            AllyWardsList.Add(minion);
                        }
                    }
                    else
                    {
                        MinionsList.Add(minion);
                        if (minion.IsEnemy)
                        {
                            EnemyMinionsList.Add(minion);
                            EnemyList.Add(minion);
                        }
                        else
                        {
                            AllyMinionsList.Add(minion);
                            AllyList.Add(minion);
                        }
                    }
                }
                else if (minion.Name != "WardCorpse")
                {
                    switch (minion.GetJungleType())
                    {
                        case JungleType.Small:
                            JungleSmallList.Add(minion);
                            break;
                        case JungleType.Large:
                            JungleLargeList.Add(minion);
                            break;
                        case JungleType.Legendary:
                            JungleLegendaryList.Add(minion);
                            break;
                    }

                    JungleList.Add(minion);
                }

                return;
            }

            var particle = sender as Obj_GeneralParticleEmitter;
            if (particle != null)
            {
                ParticleEmittersList.Add(particle);
                return;
            }

            var turret = sender as Obj_AI_Turret;
            if (turret != null)
            {
                TurretsList.Add(turret);
                if (turret.IsEnemy)
                {
                    EnemyTurretsList.Add(turret);
                    EnemyList.Add(turret);
                }
                else
                {
                    AllyTurretsList.Add(turret);
                    AllyList.Add(turret);
                }

                return;
            }

            var shop = sender as Obj_Shop;
            if (shop != null)
            {
                ShopsList.Add(shop);
                if (shop.IsAlly)
                {
                    AllyShopsList.Add(shop);
                }
                else
                {
                    EnemyShopsList.Add(shop);
                }

                return;
            }

            var spawnPoint = sender as Obj_SpawnPoint;
            if (spawnPoint != null)
            {
                SpawnPointsList.Add(spawnPoint);
                if (spawnPoint.IsAlly)
                {
                    AllySpawnPointsList.Add(spawnPoint);
                }
                else
                {
                    EnemySpawnPointsList.Add(spawnPoint);
                }
            }

            var inhibitor = sender as Obj_BarracksDampener;
            if (inhibitor != null)
            {
                InhibitorsList.Add(inhibitor);
                if (inhibitor.IsAlly)
                {
                    AllyInhibitorsList.Add(inhibitor);
                }
                else
                {
                    EnemyInhibitorsList.Add(inhibitor);
                }
            }

            var nexus = sender as Obj_HQ;
            if (nexus != null)
            {
                NexusList.Add(nexus);
                if (nexus.IsAlly)
                {
                    AllyNexus = nexus;
                }
                else
                {
                    EnemyNexus = nexus;
                }
            }
        }

        /// <summary>
        ///     OnDelete event.
        /// </summary>
        /// <param name="sender">
        ///     The sender
        /// </param>
        /// <param name="args">
        ///     The event data
        /// </param>
        private static void OnDelete(GameObject sender, EventArgs args)
        {
            foreach (var gameObject in GameObjectsList.Where(o => o.Compare(sender)).ToList())
            {
                GameObjectsList.Remove(gameObject);
            }

            foreach (var attackableUnitObject in AttackableUnitsList.Where(a => a.Compare(sender)).ToList())
            {
                AttackableUnitsList.Remove(attackableUnitObject);
            }

            var hero = sender as Obj_AI_Hero;
            if (hero != null)
            {
                foreach (var heroObject in HeroesList.Where(h => h.Compare(hero)).ToList())
                {
                    HeroesList.Remove(heroObject);
                    if (hero.IsEnemy)
                    {
                        EnemyHeroesList.Remove(heroObject);
                        EnemyList.Remove(heroObject);
                    }
                    else
                    {
                        AllyHeroesList.Remove(heroObject);
                        AllyList.Remove(heroObject);
                    }
                }

                return;
            }

            var minion = sender as Obj_AI_Minion;
            if (minion != null)
            {
                if (minion.Team != GameObjectTeam.Neutral)
                {
                    if (minion.GetMinionType().HasFlag(MinionTypes.Ward))
                    {
                        foreach (var ward in WardsList.Where(w => w.Compare(minion)).ToList())
                        {
                            WardsList.Remove(ward);
                            if (minion.IsEnemy)
                            {
                                EnemyWardsList.Remove(ward);
                            }
                            else
                            {
                                AllyWardsList.Remove(ward);
                            }
                        }
                    }
                    else
                    {
                        foreach (var minionObject in MinionsList.Where(m => m.Compare(minion)).ToList())
                        {
                            MinionsList.Remove(minionObject);
                            if (minion.IsEnemy)
                            {
                                EnemyMinionsList.Remove(minionObject);
                                EnemyList.Remove(minionObject);
                            }
                            else
                            {
                                AllyMinionsList.Remove(minionObject);
                                AllyList.Remove(minionObject);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var jungleObject in JungleList.Where(j => j.Compare(minion)).ToList())
                    {
                        switch (jungleObject.GetJungleType())
                        {
                            case JungleType.Small:
                                JungleSmallList.Remove(jungleObject);
                                break;
                            case JungleType.Large:
                                JungleLargeList.Remove(jungleObject);
                                break;
                            case JungleType.Legendary:
                                JungleLegendaryList.Remove(jungleObject);
                                break;
                        }

                        JungleList.Remove(jungleObject);
                    }
                }

                return;
            }

            var particle = sender as Obj_GeneralParticleEmitter;
            if (particle != null)
            {
                ParticleEmittersList.Remove(particle);
                return;
            }

            var turret = sender as Obj_AI_Turret;
            if (turret != null)
            {
                foreach (var turretObject in TurretsList.Where(t => t.Compare(turret)).ToList())
                {
                    TurretsList.Remove(turretObject);
                    if (turret.IsEnemy)
                    {
                        EnemyTurretsList.Remove(turretObject);
                        EnemyList.Remove(turretObject);
                    }
                    else
                    {
                        AllyTurretsList.Remove(turretObject);
                        AllyList.Remove(turretObject);
                    }
                }

                return;
            }

            var shop = sender as Obj_Shop;
            if (shop != null)
            {
                foreach (var shopObject in ShopsList.Where(s => s.Compare(shop)).ToList())
                {
                    ShopsList.Remove(shopObject);
                    if (shop.IsAlly)
                    {
                        AllyShopsList.Remove(shopObject);
                    }
                    else
                    {
                        EnemyShopsList.Remove(shopObject);
                    }
                }

                return;
            }

            var spawnPoint = sender as Obj_SpawnPoint;
            if (spawnPoint != null)
            {
                foreach (var spawnPointObject in SpawnPointsList.Where(s => s.Compare(spawnPoint)).ToList())
                {
                    SpawnPointsList.Remove(spawnPointObject);
                    if (spawnPoint.IsAlly)
                    {
                        AllySpawnPointsList.Remove(spawnPointObject);
                    }
                    else
                    {
                        EnemySpawnPointsList.Remove(spawnPointObject);
                    }
                }
            }

            var inhibitor = sender as Obj_BarracksDampener;
            if (inhibitor != null)
            {
                foreach (var inhibitorObject in InhibitorsList.Where(i => i.Compare(inhibitor)).ToList())
                {
                    InhibitorsList.Remove(inhibitorObject);
                    if (inhibitor.IsAlly)
                    {
                        AllyInhibitorsList.Remove(inhibitorObject);
                    }
                    else
                    {
                        EnemyInhibitorsList.Remove(inhibitorObject);
                    }
                }
            }

            var nexus = sender as Obj_HQ;
            if (nexus != null)
            {
                foreach (var nexusObject in NexusList.Where(n => n.Compare(nexus)).ToList())
                {
                    NexusList.Remove(nexusObject);
                    if (nexusObject.IsAlly)
                    {
                        AllyNexus = null;
                    }
                    else
                    {
                        EnemyNexus = null;
                    }
                }
            }
        }

        #endregion
    }

    public enum JungleType
    {
        /// <summary>
        ///     The unknown type.
        /// </summary>
        Unknown,

        /// <summary>
        ///     The small type.
        /// </summary>
        Small,

        /// <summary>
        ///     The large type.
        /// </summary>
        Large,

        /// <summary>
        ///     The legendary type.
        /// </summary>
        Legendary
    }
    [Flags]
    public enum MinionTypes
    {
        /// <summary>
        ///     The unknown type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     The normal type.
        /// </summary>
        Normal = 1 << 0,

        /// <summary>
        ///     The ranged type.
        /// </summary>
        Ranged = 1 << 1,

        /// <summary>
        ///     The melee type.
        /// </summary>
        Melee = 1 << 2,

        /// <summary>
        ///     The siege type.
        /// </summary>
        Siege = 1 << 3,

        /// <summary>
        ///     The super type.
        /// </summary>
        Super = 1 << 4,

        /// <summary>
        ///     The ward type.
        /// </summary>
        Ward = 1 << 5
    }
    public static class Minion
    {
        #region Static Fields

        private static readonly List<string> CloneList = new List<string> { "leblanc", "shaco", "monkeyking" };

        /// <summary>
        ///     The normal minion list.
        /// </summary>
        private static readonly List<string> NormalMinionList = new List<string>
                                                                    {
                                                                        "SRU_ChaosMinionMelee", "SRU_ChaosMinionRanged",
                                                                        "SRU_OrderMinionMelee", "SRU_OrderMinionRanged",
                                                                        "HA_ChaosMinionMelee", "HA_ChaosMinionRanged",
                                                                        "HA_OrderMinionMelee", "HA_OrderMinionRanged"
                                                                    };

        private static readonly List<string> PetList = new List<string>
                                                           {
                                                               "annietibbers", "elisespiderling", "heimertyellow",
                                                               "heimertblue", "malzaharvoidling", "shacobox",
                                                               "yorickspectralghoul", "yorickdecayedghoul",
                                                               "yorickravenousghoul", "zyrathornplant",
                                                               "zyragraspingplant"
                                                           };

        /// <summary>
        ///     The siege minion list.
        /// </summary>
        private static readonly List<string> SiegeMinionList = new List<string>
                                                                   {
                                                                       "SRU_ChaosMinionSiege", "SRU_OrderMinionSiege",
                                                                       "HA_ChaosMinionSiege", "HA_OrderMinionSiege"
                                                                   };

        /// <summary>
        ///     The super minion list.
        /// </summary>
        private static readonly List<string> SuperMinionList = new List<string>
                                                                   {
                                                                       "SRU_ChaosMinionSuper", "SRU_OrderMinionSuper",
                                                                       "HA_ChaosMinionSuper", "HA_OrderMinionSuper"
                                                                   };

        #endregion

        #region Public Methods and Operators


        /// <summary>
        ///     Gets the minion type.
        /// </summary>
        /// <param name="minion">
        ///     The minion.
        /// </param>
        /// <returns>
        ///     The <see cref="MinionTypes" />
        /// </returns>
        public static MinionTypes GetMinionType(this Obj_AI_Minion minion)
        {
            var baseSkinName = minion.CharData.BaseSkinName;

            if (NormalMinionList.Any(n => baseSkinName.Equals(n)))
            {
                return MinionTypes.Normal
                       | (minion.CharData.BaseSkinName.Contains("Melee") ? MinionTypes.Melee : MinionTypes.Ranged);
            }

            if (SiegeMinionList.Any(n => baseSkinName.Equals(n)))
            {
                return MinionTypes.Siege | MinionTypes.Ranged;
            }

            if (SuperMinionList.Any(n => baseSkinName.Equals(n)))
            {
                return MinionTypes.Super | MinionTypes.Melee;
            }

            if (baseSkinName.ToLower().Contains("ward") || baseSkinName.ToLower().Contains("trinket"))
            {
                return MinionTypes.Ward;
            }

            return MinionTypes.Unknown;
        }

        /// <summary>
        ///     Tells whether the <see cref="Obj_AI_Minion" /> is an actual minion.
        /// </summary>
        /// <param name="minion">The Minion</param>
        /// <returns>Whether the <see cref="Obj_AI_Minion" /> is an actual minion.</returns>
        public static bool IsMinion(this Obj_AI_Minion minion)
        {
            return minion.GetMinionType().HasFlag(MinionTypes.Melee)
                   || minion.GetMinionType().HasFlag(MinionTypes.Ranged);
        }

        /// <summary>
        ///     Tells whether the <see cref="Obj_AI_Minion" /> is an actual minion.
        /// </summary>
        /// <param name="minion">The Minion</param>
        /// <param name="includeClones">Whether to include clones.</param>
        /// <returns>Whether the <see cref="Obj_AI_Minion" /> is an actual pet.</returns>
        public static bool IsPet(this Obj_AI_Minion minion, bool includeClones = true)
        {
            var name = minion.CharData.BaseSkinName.ToLower();
            return PetList.Contains(name) || (includeClones && CloneList.Contains(name));
        }

        #endregion
    }

    /// <summary>
    ///     The farm location.
    /// </summary>
    public struct FarmLocation
    {
        #region Fields

        /// <summary>
        ///     The minions hit.
        /// </summary>
        public int MinionsHit;

        /// <summary>
        ///     The position.
        /// </summary>
        public Vector2 Position;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FarmLocation" /> struct.
        /// </summary>
        /// <param name="position">
        ///     The position.
        /// </param>
        /// <param name="minionsHit">
        ///     The minions hit.
        /// </param>
        public FarmLocation(Vector2 position, int minionsHit)
        {
            this.Position = position;
            this.MinionsHit = minionsHit;
        }

        #endregion
    }
    public static class Jungle
    {
        #region Static Fields

        /// <summary>
        ///     The large name regex list.
        /// </summary>
        private static readonly string[] LargeNameRegex =
            {
                "SRU_Murkwolf[0-9.]{1,}", "SRU_Gromp", "SRU_Blue[0-9.]{1,}",
                "SRU_Razorbeak[0-9.]{1,}", "SRU_Red[0-9.]{1,}",
                "SRU_Krug[0-9]{1,}"
            };

        /// <summary>
        ///     The legendary name regex list.
        /// </summary>
        private static readonly string[] LegendaryNameRegex = { "SRU_Dragon", "SRU_Baron", "SRU_RiftHerald" };

        /// <summary>
        ///     The small name regex list.
        /// </summary>
        private static readonly string[] SmallNameRegex = { "SRU_[a-zA-Z](.*?)Mini", "Sru_Crab" };

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Get the minion jungle type.
        /// </summary>
        /// <param name="minion">
        ///     The minion
        /// </param>
        /// <returns>
        ///     The <see cref="JungleType" />
        /// </returns>
        public static JungleType GetJungleType(this Obj_AI_Minion minion)
        {
            if (SmallNameRegex.Any(regex => Regex.IsMatch(minion.Name, regex)))
            {
                return JungleType.Small;
            }

            if (LargeNameRegex.Any(regex => Regex.IsMatch(minion.Name, regex)))
            {
                return JungleType.Large;
            }

            if (LegendaryNameRegex.Any(regex => Regex.IsMatch(minion.Name, regex)))
            {
                return JungleType.Legendary;
            }

            return JungleType.Unknown;
        }

        /// <summary>
        ///     Indicates whether the object is a jungle buff carrier.
        /// </summary>
        /// <param name="minion">
        ///     The minion.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsJungleBuff(this Obj_AI_Minion minion)
        {
            var @base = minion.CharData.BaseSkinName;
            return @base.Equals("SRU_Blue") || @base.Equals("SRU_Red");
        }

        #endregion
    }

    public static class Soldiers
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static int LastWTick;
        public static List<GameObject> soldier = new List<GameObject>();

        public static List<Obj_AI_Hero> enemies = new List<Obj_AI_Hero>();

        public static List<Obj_AI_Minion> autoattackminions = new List<Obj_AI_Minion>();

        public static List<Obj_AI_Minion> soldierattackminions = new List<Obj_AI_Minion>();

        public static List<SplashAutoAttackMinion> splashautoattackminions = new List<SplashAutoAttackMinion>();

        public static List<SplashAutoAttackChampion> splashautoattackchampions = new List<SplashAutoAttackChampion>();

        public static void AzirSoldier()
        {
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnDelete += GameObject_OnDelete;
            GameObject.OnCreate += GameObject_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            //Game.PrintChat(args.SData.Name);
			if (/*args.Slot == SpellSlot.W*/args.SData.Name == "AzirW")
                LastWTick = Environment.TickCount;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
			//if (sender.Name.ToLower().Contains("azir"))
			//    Game.PrintChat(sender.Name + " oncreate");	
            if (sender.Name == "Azir_Base_P_Soldier_Ring.troy" && Math.Abs(Environment.TickCount - LastWTick) <= 250)
                soldier.Add(sender);
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
			//if (sender.Name.ToLower().Contains("azir"))
			//    Game.PrintChat(sender.Name + " ondelete");	
            if (sender.Name == "Azir_Base_P_Soldier_Ring.troy")
                soldier.RemoveAll(x => x.NetworkId == sender.NetworkId);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            var soldierandtargetminion = new List<SoldierAndTargetMinion>();
            var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget()).ToList();
            minions.AddRange(GameObjects.Jungle.Where(x => x.IsValidTarget()));
            var minionspredictedposition = new List<MinionPredictedPosition>();
            foreach (var x in minions)
            {
                minionspredictedposition
                    .Add(new MinionPredictedPosition
                        (x, Prediction.GetPrediction(x, Player.AttackCastDelay + Game.Ping / 1000).UnitPosition));
            }
            var championpredictedposition = new List<ChampionPredictedPosition>();
            foreach (var x in HeroManager.Enemies.Where(x => x.IsValidTarget()))
            {
                championpredictedposition
                    .Add(new ChampionPredictedPosition
                        (x, Prediction.GetPrediction(x, Player.AttackCastDelay + Game.Ping / 1000).UnitPosition));
            }
            enemies = new List<Obj_AI_Hero>();
            foreach (var hero in HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsZombie))
            {
                if (soldier.Any(x => x.Position.Distance(hero.Position) <= 300 + hero.BoundingRadius && Player.Distance(x.Position) <= 925))
                    enemies.Add(hero);
            }
            soldierattackminions = new List<Obj_AI_Minion>();
            foreach (var minion in minions)
            {
                var Soldiers = soldier.Where
                    (x => x.Position.Distance(minion.Position) <= 300 + minion.BoundingRadius && Player.Distance(x.Position) <= 925)
                    .ToList();
                if (Soldiers.Any())
                {
                    soldierattackminions.Add(minion);
                    soldierandtargetminion.Add(new SoldierAndTargetMinion(minion, Soldiers));
                }
            }
            autoattackminions = new List<Obj_AI_Minion>();
            foreach (var minion in minions.Where(x => x.IsValidTarget(Orbwalking.GetRealAutoAttackRange(x))))
            {
                if (!soldierattackminions.Any(x => x.NetworkId == minion.NetworkId))
                    autoattackminions.Add(minion);
            }
            splashautoattackchampions = new List<SplashAutoAttackChampion>();
            foreach (var mainminion in soldierandtargetminion)
            {
                var mainminionpredictedposition = Prediction.GetPrediction(mainminion.Minion, Player.AttackCastDelay + Game.Ping / 1000).UnitPosition;
                List<Obj_AI_Hero> splashchampions = new List<Obj_AI_Hero>();
                foreach (var hero in championpredictedposition)
                {
                    foreach (var mainminionsoldier in mainminion.Soldier)
                        if (Geometry.Distance(hero.Position.To2D(), mainminionsoldier.Position.To2D(), mainminionsoldier.Position.To2D().Extend(mainminionpredictedposition.To2D(), 450), false) <= hero.Hero.BoundingRadius + 50)
                        {
                            splashchampions.Add(hero.Hero);
                        }
                }
                if (splashchampions.Any())
                    splashautoattackchampions.Add(new SplashAutoAttackChampion(mainminion.Minion, splashchampions));
            }
            splashautoattackminions = new List<SplashAutoAttackMinion>();
            foreach (var mainminion in soldierandtargetminion)
            {
                var mainminionpredictedposition =
                    Prediction.GetPrediction(mainminion.Minion, Player.AttackCastDelay + Game.Ping / 1000).UnitPosition;
                List<Obj_AI_Minion> splashminions = new List<Obj_AI_Minion>();
                foreach (var minion in minionspredictedposition)
                {
                    foreach (var mainminionsoldier in mainminion.Soldier)
                        if (Geometry.Distance(minion.Position.To2D(), mainminionsoldier.Position.To2D(),
                            mainminionsoldier.Position.To2D().Extend(mainminionpredictedposition.To2D(), 450), false)
                            <= minion.Minion.BoundingRadius + 50)
                        {
                            splashminions.Add(minion.Minion);
                            break;
                        }
                }
                splashautoattackminions.Add(new SplashAutoAttackMinion(mainminion.Minion, splashminions));
            }
        }
    }

    public class SplashAutoAttackMinion
    {
        public Obj_AI_Minion MainMinion;
        public List<Obj_AI_Minion> SplashAutoAttackMinions;
        public SplashAutoAttackMinion(Obj_AI_Minion mainminion, List<Obj_AI_Minion> splashautoattackminions)
        {
            MainMinion = mainminion;
            SplashAutoAttackMinions = splashautoattackminions;
        }
    }

    public class SplashAutoAttackChampion
    {
        public Obj_AI_Minion MainMinion;
        public List<Obj_AI_Hero> SplashAutoAttackChampions;
        public SplashAutoAttackChampion(Obj_AI_Minion mainminion, List<Obj_AI_Hero> splashAutoAttackChampions)
        {
            MainMinion = mainminion;
            SplashAutoAttackChampions = splashAutoAttackChampions;
        }
    }

    public class MinionPredictedPosition
    {
        public Obj_AI_Minion Minion;
        public Vector3 Position;
        public MinionPredictedPosition(Obj_AI_Minion minion, Vector3 position)
        {
            Minion = minion;
            Position = position;
        }
    }

    public class ChampionPredictedPosition
    {
        public Obj_AI_Hero Hero;
        public Vector3 Position;
        public ChampionPredictedPosition(Obj_AI_Hero hero, Vector3 position)
        {
            Hero = hero;
            Position = position;
        }
    }

    public class SoldierAndTargetMinion
    {
        public Obj_AI_Minion Minion;
        public List<GameObject> Soldier;
        public SoldierAndTargetMinion(Obj_AI_Minion minion, List<GameObject> soldier)
        {
            Minion = minion;
            Soldier = soldier;
        }
    }
}
