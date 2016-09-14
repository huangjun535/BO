﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace OneKeyToWin_AIO_Sebby.Core
{
    class ChampionInfo
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

    class OKTWtracker
    {
        public static List<ChampionInfo> ChampionInfoList = new List<ChampionInfo>();
        public static Obj_AI_Hero jungler;
        private Vector3 EnemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position;

        public void LoadOKTW()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    ChampionInfoList.Add(new ChampionInfo() { NetworkId = hero.NetworkId, LastVisablePos = hero.Position });
                    if (IsJungler(hero))
                        jungler = hero;
                }
            }

            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
        }

        private static void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            var unit = sender as Obj_AI_Hero;

            if (unit == null || !unit.IsValid || unit.IsAlly)
                return;

            var ChampionInfoOne = ChampionInfoList.Find(x => x.NetworkId == sender.NetworkId);

            var recall = Packet.S2C.Teleport.Decoded(unit, args);

            if (recall.Type == Packet.S2C.Teleport.Type.Recall)
            {
                switch (recall.Status)
                {
                    case Packet.S2C.Teleport.Status.Start:
                        ChampionInfoOne.StartRecallTime = Game.Time;
                        break;
                    case Packet.S2C.Teleport.Status.Abort:
                        ChampionInfoOne.AbortRecallTime = Game.Time;
                        break;
                    case Packet.S2C.Teleport.Status.Finish:
                        ChampionInfoOne.FinishRecallTime = Game.Time;
                        ChampionInfoOne.LastVisablePos = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position;
                        break;
                }
            }
        }

        private void OnUpdate(EventArgs args)
        {
            if (!Program.LagFree(0))
                return;

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

        private bool IsJungler(Obj_AI_Hero hero) { return hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite")); }
    }
}
