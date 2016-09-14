namespace LeagueSharp.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharpDX;

    /// <summary>
    /// An enum representing the order the minions should be listed.
    /// </summary>
    public enum MinionOrderTypes
    {
        /// <summary>
        /// No order.
        /// </summary>
        None,

        /// <summary>
        /// Ordered by the current health of the minion. (Least to greatest)
        /// </summary>
        Health,

        /// <summary>
        /// Ordered by the maximum health of the minions. (Greatest to least)
        /// </summary>
        MaxHealth
    }

    /// <summary>
    /// The team of the minion.
    /// </summary>
    public enum MinionTeam
    {
        /// <summary>
        /// The minion is not on either team.
        /// </summary>
        Neutral,

        /// <summary>
        /// The minions is an ally
        /// </summary>
        Ally,

        /// <summary>
        /// The minions is an enemy
        /// </summary>
        Enemy,

        /// <summary>
        /// The minion is not an ally
        /// </summary>
        NotAlly,

        /// <summary>
        /// The minions is not an ally for the enemy
        /// </summary>
        NotAllyForEnemy,

        /// <summary>
        /// Any minion.
        /// </summary>
        All
    }

    /// <summary>
    /// The type of minion.
    /// </summary>
    public enum MinionTypes
    {
        /// <summary>
        /// Ranged minions.
        /// </summary>
        Ranged,

        /// <summary>
        /// Melee minions.
        /// </summary>
        Melee,

        /// <summary>
        /// Any minion
        /// </summary>
        All,

        /// <summary>
        /// Any wards. (TODO)
        /// </summary>
        [Obsolete("Wards have not been implemented yet in the minion manager.")]
        Wards
    }

    /// <summary>
    /// Manages minions.
    /// </summary>
    public static class MinionManager
    {
        /// <summary>
        /// All Minions
        /// </summary>
        public static List<Obj_AI_Minion> AllMinionsObj = new List<Obj_AI_Minion>();

        /// <summary>
        /// Enemy Minions List
        /// </summary>
        public static List<Obj_AI_Minion> MinionsListEnemy = new List<Obj_AI_Minion>();

        /// <summary>
        /// Ally Minions List
        /// </summary>
        public static List<Obj_AI_Minion> MinionsListAlly = new List<Obj_AI_Minion>();

        /// <summary>
        /// Neutral Minions List
        /// </summary>
        public static List<Obj_AI_Minion> MinionsListNeutral = new List<Obj_AI_Minion>();

        /// <summary>
        /// Turret List
        /// </summary>
        public static List<Obj_AI_Turret> TurretList = ObjectManager.Get<Obj_AI_Turret>().ToList();

        /// <summary>
        /// Nexus List
        /// </summary>
        public static List<Obj_HQ> NexusList = ObjectManager.Get<Obj_HQ>().ToList();

        /// <summary>
        /// Inhi List
        /// </summary>
        public static List<Obj_BarracksDampener> InhiList = ObjectManager.Get<Obj_BarracksDampener>().ToList();

        /// <summary>
        /// MinionManager
        /// </summary>
        static MinionManager()
        {
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValid))
            {
                AddMinionObject(minion);
                if (!minion.IsAlly)
                    AllMinionsObj.Add(minion);
            }

            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnUpdate += Game_OnUpdate;
        }

        /// <summary>
        /// The Events
        /// </summary>
        /// <param name="args">The Args</param>
        private static void Game_OnUpdate(EventArgs args)
        {
            MinionsListEnemy.RemoveAll(minion => !IsValidMinion(minion));
            MinionsListNeutral.RemoveAll(minion => !IsValidMinion(minion));
            MinionsListAlly.RemoveAll(minion => !IsValidMinion(minion));
            AllMinionsObj.RemoveAll(minion => !IsValidMinion(minion));
        }

        /// <summary>
        /// Create Minions Obj
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">The Events</param>
        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            var minion = sender as Obj_AI_Minion;
            if (minion != null)
            {
                AddMinionObject(minion);
                if (!minion.IsAlly)
                    AllMinionsObj.Add(minion);
            }
        }

        /// <summary>
        /// Get The Minions Type Result
        /// </summary>
        /// <param name="minion">The Minion</param>
        private static void AddMinionObject(Obj_AI_Minion minion)
        {
            if (minion.MaxHealth >= 225)
            {
                if (minion.Team == GameObjectTeam.Neutral)
                {
                    MinionsListNeutral.Add(minion);
                }
                else if (minion.MaxMana == 0 && minion.MaxHealth >= 300)
                {
                    if (minion.Team == GameObjectTeam.Unknown)
                        return;
                    else if (minion.Team != ObjectManager.Player.Team)
                        MinionsListEnemy.Add(minion);
                    else if (minion.Team == ObjectManager.Player.Team)
                        MinionsListAlly.Add(minion);
                }
            }
        }

        /// <summary>
        ///  New Get Minion Logic
        /// </summary>
        /// <param name="from">The Position</param>
        /// <param name="range">The Range</param>
        /// <param name="team">The Team</param>
        /// <returns></returns>
        public static List<Obj_AI_Minion> GetMinion(Vector3 from, float range = float.MaxValue, MinionTeam team = MinionTeam.Enemy)
        {
            if (team == MinionTeam.Enemy)
            {

                return MinionsListEnemy.FindAll(minion => CanReturn(minion, from, range));
            }
            else if (team == MinionTeam.Ally)
            {

                return MinionsListAlly.FindAll(minion => CanReturn(minion, from, range));
            }
            else if (team == MinionTeam.Neutral)
            {

                return MinionsListNeutral.Where(minion => CanReturn(minion, from, range)).OrderByDescending(minion => minion.MaxHealth).ToList();
            }
            else
            {
                return AllMinionsObj.FindAll(minion => CanReturn(minion, from, range));
            }
        }

        /// <summary>
        /// Search Minions
        /// </summary>
        /// <param name="minion">The Minion</param>
        /// <returns></returns>
        private static bool IsValidMinion(Obj_AI_Base minion)
        {
            if (minion == null || !minion.IsValid || minion.IsDead)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Return Minions Result
        /// </summary>
        /// <param name="minion">The Minion</param>
        /// <param name="from">The Position</param>
        /// <param name="range">The Range</param>
        /// <returns></returns>
        private static bool CanReturn(Obj_AI_Base minion, Vector3 from, float range)
        {
            if (minion != null && minion.IsValid && !minion.IsDead && minion.IsVisible && minion.IsTargetable)
            {
                if (range == float.MaxValue)
                    return true;
                else if (range == 0)
                {
                    if (Orbwalking.InAutoAttackRange(minion))
                        return true;
                    else
                        return false;
                }
                else if (Vector2.DistanceSquared((@from).To2D(), minion.Position.To2D()) < range * range)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Gets minions based on range, type, team and then orders them.
        /// </summary>
        /// <param name="from">The point to get the minions from.</param>
        /// <param name="range">The range.</param>
        /// <param name="type">The type.</param>
        /// <param name="team">The team.</param>
        /// <param name="order">The order.</param>
        /// <returns>List&lt;Obj_AI_Base&gt;.</returns>
        public static List<Obj_AI_Base> GetMinions(Vector3 from,
            float range,
            MinionTypes type = MinionTypes.All,
            MinionTeam team = MinionTeam.Enemy,
            MinionOrderTypes order = MinionOrderTypes.Health)
        {
            var result = (from minion in ObjectManager.Get<Obj_AI_Minion>()
                          where minion.IsValidTarget(range, false, @from)
                          let minionTeam = minion.Team
                          where
                              team == MinionTeam.Neutral && minionTeam == GameObjectTeam.Neutral ||
                              team == MinionTeam.Ally &&
                              minionTeam ==
                              (ObjectManager.Player.Team == GameObjectTeam.Chaos ? GameObjectTeam.Chaos : GameObjectTeam.Order) ||
                              team == MinionTeam.Enemy &&
                              minionTeam ==
                              (ObjectManager.Player.Team == GameObjectTeam.Chaos ? GameObjectTeam.Order : GameObjectTeam.Chaos) ||
                              team == MinionTeam.NotAlly && minionTeam != ObjectManager.Player.Team ||
                              team == MinionTeam.NotAllyForEnemy &&
                              (minionTeam == ObjectManager.Player.Team || minionTeam == GameObjectTeam.Neutral) ||
                              team == MinionTeam.All
                          where
                              minion.IsMelee() && type == MinionTypes.Melee || !minion.IsMelee() && type == MinionTypes.Ranged ||
                              type == MinionTypes.All
                          where IsMinion(minion) || minionTeam == GameObjectTeam.Neutral && minion.MaxHealth > 5 && minion.IsHPBarRendered
                          select minion).Cast<Obj_AI_Base>().ToList();

            switch (order)
            {
                case MinionOrderTypes.Health:
                    result = result.OrderBy(o => o.Health).ToList();
                    break;
                case MinionOrderTypes.MaxHealth:
                    result = result.OrderByDescending(o => o.MaxHealth).ToList();
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets the minions.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="type">The type.</param>
        /// <param name="team">The team.</param>
        /// <param name="order">The order.</param>
        /// <returns>List&lt;Obj_AI_Base&gt;.</returns>
        public static List<Obj_AI_Base> GetMinions(float range,
            MinionTypes type = MinionTypes.All,
            MinionTeam team = MinionTeam.Enemy,
            MinionOrderTypes order = MinionOrderTypes.Health)
        {
            return GetMinions(ObjectManager.Player.ServerPosition, range, type, team, order);
        }

        /// <summary>
        /// Determines whether the specified object is a minion.
        /// </summary>
        /// <param name="minion">The minion.</param>
        /// <param name="includeWards">if set to <c>true</c> [include wards].</param>
        /// <returns><c>true</c> if the specified minion is minion; otherwise, <c>false</c>.</returns>
        public static bool IsMinion(Obj_AI_Minion minion, bool includeWards = false)
        {
            return minion.Name.Contains("Minion") || includeWards && IsWard(minion);
        }

        /// <summary>
        /// Determines whether the specified base skin name is ward.
        /// </summary>
        /// <param name="baseSkinName">Name of the base skin. Should be lowercase.</param>
        /// <returns><c>true</c> if the specified base skin name is ward; otherwise, <c>false</c>.</returns>
        [System.Obsolete("Use IsWard(Obj_AI_Minion)")]
        public static bool IsWard(string baseSkinName)
        {
            return baseSkinName.Contains("ward");
        }

        /// <summary>
        /// Determines whether the specified minion is a valid attackable ward.
        /// </summary>
        /// <param name="minion">The minion you want to check for</param>
        /// <returns><c>true</c> if the given minion is a valid attackable ward, otherwise returns <c>false</c>.</returns>
        public static bool IsWard(Obj_AI_Minion minion)
        {
            return minion.Name.Contains("Ward") && minion.IsHPBarRendered;
        }

        /// <summary>
        /// Returns the point where, when casted, the circular spell with hit the maximum amount of minions.
        /// </summary>
        /// <param name="minionPositions">The minion positions.</param>
        /// <param name="width">The width.</param>
        /// <param name="range">The range.</param>
        /// <param name="useMECMax">The use mec maximum.</param>
        /// <returns>FarmLocation.</returns>
        public static FarmLocation GetBestCircularFarmLocation(List<Vector2> minionPositions,
            float width,
            float range,
            int useMECMax = 9)
        {
            var result = new Vector2();
            var minionCount = 0;
            var startPos = ObjectManager.Player.ServerPosition.To2D();

            range = range * range;

            if (minionPositions.Count == 0)
            {
                return new FarmLocation(result, minionCount);
            }

            /* Use MEC to get the best positions only when there are less than 9 positions because it causes lag with more. */
            if (minionPositions.Count <= useMECMax)
            {
                var subGroups = GetCombinations(minionPositions);
                foreach (var subGroup in subGroups)
                {
                    if (subGroup.Count > 0)
                    {
                        var circle = MEC.GetMec(subGroup);

                        if (circle.Radius <= width && circle.Center.Distance(startPos, true) <= range)
                        {
                            minionCount = subGroup.Count;
                            return new FarmLocation(circle.Center, minionCount);
                        }
                    }
                }
            }
            else
            {
                foreach (var pos in minionPositions)
                {
                    if (pos.Distance(startPos, true) <= range)
                    {
                        var count = minionPositions.Count(pos2 => pos.Distance(pos2, true) <= width * width);

                        if (count >= minionCount)
                        {
                            result = pos;
                            minionCount = count;
                        }
                    }
                }
            }

            return new FarmLocation(result, minionCount);
        }

        /// <summary>
        /// Returns the point where, when casted, the linear spell with hit the maximum amount of minions.
        /// </summary>
        /// <param name="minionPositions">The minion positions.</param>
        /// <param name="width">The width.</param>
        /// <param name="range">The range.</param>
        /// <returns>FarmLocation.</returns>
        public static FarmLocation GetBestLineFarmLocation(List<Vector2> minionPositions, float width, float range)
        {
            var result = new Vector2();
            var minionCount = 0;
            var startPos = ObjectManager.Player.ServerPosition.To2D();

            var posiblePositions = new List<Vector2>();
            posiblePositions.AddRange(minionPositions);

            var max = minionPositions.Count;
            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (minionPositions[j] != minionPositions[i])
                    {
                        posiblePositions.Add((minionPositions[j] + minionPositions[i]) / 2);
                    }
                }
            }

            foreach (var pos in posiblePositions)
            {
                if (pos.Distance(startPos, true) <= range * range)
                {
                    var endPos = startPos + range * (pos - startPos).Normalized();

                    var count =
                        minionPositions.Count(pos2 => pos2.Distance(startPos, endPos, true, true) <= width * width);

                    if (count >= minionCount)
                    {
                        result = endPos;
                        minionCount = count;
                    }
                }
            }

            return new FarmLocation(result, minionCount);
        }

        /// <summary>
        /// Gets the minions predicted positions.
        /// </summary>
        /// <param name="minions">The minions.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="width">The width.</param>
        /// <param name="speed">The speed.</param>
        /// <param name="from">From.</param>
        /// <param name="range">The range.</param>
        /// <param name="collision">if set to <c>true</c>, checks for collision.</param>
        /// <param name="stype">The skillshot type.</param>
        /// <param name="rangeCheckFrom">The position to check the range from.</param>
        /// <returns>List&lt;Vector2&gt;.</returns>
        public static List<Vector2> GetMinionsPredictedPositions(List<Obj_AI_Base> minions,
            float delay,
            float width,
            float speed,
            Vector3 from,
            float range,
            bool collision,
            SkillshotType stype,
            Vector3 rangeCheckFrom = new Vector3())
        {
            from = from.To2D().IsValid() ? from : ObjectManager.Player.ServerPosition;

            return (from minion in minions
                    select
                        Prediction.GetPrediction(
                            new PredictionInput
                            {
                                Unit = minion,
                                Delay = delay,
                                Radius = width,
                                Speed = speed,
                                From = @from,
                                Range = range,
                                Collision = collision,
                                Type = stype,
                                RangeCheckFrom = rangeCheckFrom
                            })
                into pos
                    where pos.Hitchance >= HitChance.High
                    select pos.UnitPosition.To2D()).ToList();
        }

        /*
         from: https://stackoverflow.com/questions/10515449/generate-all-combinations-for-a-list-of-strings :^)
         */

        /// <summary>
        /// Returns all the subgroup combinations that can be made from a group
        /// </summary>
        /// <param name="allValues">All values.</param>
        /// <returns>List&lt;List&lt;Vector2&gt;&gt;.</returns>
        private static List<List<Vector2>> GetCombinations(List<Vector2> allValues)
        {
            var collection = new List<List<Vector2>>();
            for (var counter = 0; counter < (1 << allValues.Count); ++counter)
            {
                var combination = allValues.Where((t, i) => (counter & (1 << i)) == 0).ToList();

                collection.Add(combination);
            }
            return collection;
        }

        /// <summary>
        ///     A struct that represents the best position to cast a skillshot to hit the best number of minions, as well as the
        ///     number of minions hit.
        /// </summary>
        public struct FarmLocation
        {
            /// <summary>
            /// The minions hit
            /// </summary>
            public int MinionsHit;

            /// <summary>
            /// The position
            /// </summary>
            public Vector2 Position;

            /// <summary>
            /// Initializes a new instance of the <see cref="FarmLocation"/> struct.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="minionsHit">The minions hit.</param>
            public FarmLocation(Vector2 position, int minionsHit)
            {
                Position = position;
                MinionsHit = minionsHit;
            }
        }
    }
}
