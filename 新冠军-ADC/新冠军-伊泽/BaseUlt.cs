namespace YuLeEzreal
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using SharpDX.Direct3D9;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Collision = LeagueSharp.Common.Collision;
    using Color = System.Drawing.Color;
    using Font = SharpDX.Direct3D9.Font;

    internal class BaseUlt
    {
        #region 
        internal static Menu Menu = Program.Config;
        private static Menu TeamUlt;
        private static Menu DisabledChampions;
        private static Spell Ultimate;
        private static int LastUltCastT;
        private static Utility.Map.MapType Map;
        private static List<Obj_AI_Hero> Heroes;
        private static List<Obj_AI_Hero> Enemies;
        private static List<Obj_AI_Hero> Allies;
        private static List<EnemyInfo> EnemyInfo = new List<EnemyInfo>();
        private static Dictionary<int, int> RecallT = new Dictionary<int, int>();
        private static Vector3 EnemySpawnPos;
        private static Font Text;
        private static Color NotificationColor = Color.FromArgb(136, 207, 240);
        private static float BarX = Drawing.Width * 0.425f;
        private static float BarY = Drawing.Height * 0.80f;
        private static int BarWidth = (int)(Drawing.Width - 2 * BarX);
        private static int BarHeight = 6;
        private static int SeperatorHeight = 5;
        private static float Scale = (float)BarWidth / 8000;
        #endregion

        internal void Init()
        {
            Menu.SubMenu("基地大招").AddItem(new MenuItem("showRecalls", "显示回城状态").SetValue(true));
            Menu.SubMenu("基地大招").AddItem(new MenuItem("baseUlt", "启用基地大招").SetValue(true));
            Menu.SubMenu("基地大招").AddItem(new MenuItem("checkCollision", "检查碰撞").SetValue(true));
            Menu.SubMenu("基地大招").AddItem(new MenuItem("AutoUseBaseUit", "自动放大").SetValue(true));
            Menu.SubMenu("基地大招").AddItem(new MenuItem("panicKey", "禁止放大按键").SetValue(new KeyBind(32, KeyBindType.Press))); //32 == space
            Menu.SubMenu("基地大招").AddItem(new MenuItem("regardlessKey", "禁止放大按键2").SetValue(new KeyBind(17, KeyBindType.Press))); //17 == ctrl

            Heroes = ObjectManager.Get<Obj_AI_Hero>().ToList();
            Enemies = Heroes.Where(x => x.IsEnemy).ToList();
            Allies = Heroes.Where(x => x.IsAlly).ToList();

            EnemyInfo = Enemies.Select(x => new EnemyInfo(x)).ToList();

            bool compatibleChamp = IsCompatibleChamp(ObjectManager.Player.ChampionName);

            TeamUlt = Menu.SubMenu("基地大招").AddSubMenu(new Menu("队伍设置", "TeamUlt"));
            DisabledChampions = Menu.SubMenu("基地大招").AddSubMenu(new Menu("禁止基地大招目标", "DisabledChampions"));

            if (compatibleChamp)
            {
                foreach (Obj_AI_Hero champ in Allies.Where(x => !x.IsMe && IsCompatibleChamp(x.ChampionName)))
                    TeamUlt.AddItem(new MenuItem(champ.ChampionName, "友军: " + champ.ChampionName).SetValue(false).DontSave());

                foreach (Obj_AI_Hero champ in Enemies)
                    DisabledChampions.AddItem(new MenuItem(champ.ChampionName, "禁止大招: " + champ.ChampionName).SetValue(false).DontSave());
            }

            var NotificationsMenu = Menu.SubMenu("基地大招").AddSubMenu(new Menu("通知设置", "Notifications"));

            NotificationsMenu.AddItem(new MenuItem("notifRecFinished", "回城结束").SetValue(true));
            NotificationsMenu.AddItem(new MenuItem("notifRecAborted", "回城打断").SetValue(true));

            EnemySpawnPos = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position; //ObjectManager.Get<GameObject>().FirstOrDefault(x => x.Type == GameObjectType.obj_SpawnPoint && x.IsEnemy).Position;

            Map = Utility.Map.GetMap().Type;

            Ultimate = new Spell(SpellSlot.R);

            Text = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Calibri", Height = 13, Width = 6, OutputPrecision = FontPrecision.Default, Quality = FontQuality.Default });

            Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnDraw += Drawing_OnDraw;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_DomainUnload;

            if (compatibleChamp)
                Game.OnUpdate += Game_OnUpdate;
        }

        public static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        public static bool IsCompatibleChamp(string championName)
        {
            return UltSpellData.Keys.Any(x => x == championName);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            int time = Environment.TickCount;

            foreach (EnemyInfo enemyInfo in EnemyInfo.Where(x => x.Player.IsVisible))
                enemyInfo.LastSeen = time;

            if (!Menu.Item("baseUlt").GetValue<bool>())
                return;

            foreach (EnemyInfo enemyInfo in EnemyInfo.Where(x =>
                x.Player.IsValid<Obj_AI_Hero>() &&
                !x.Player.IsDead &&
                !DisabledChampions.Item(x.Player.ChampionName).GetValue<bool>() &&
                x.RecallInfo.Recall.Status == Packet.S2C.Teleport.Status.Start && x.RecallInfo.Recall.Type == Packet.S2C.Teleport.Type.Recall).OrderBy(x => x.RecallInfo.GetRecallCountdown()))
            {
                if (Environment.TickCount - LastUltCastT > 15000)
                    HandleUltTarget(enemyInfo);
            }
        }

        private struct UltSpellDataS
        {
            public int SpellStage;
            public float DamageMultiplicator;
            public float Width;
            public float Delay;
            public float Speed;
            public bool Collision;
        }

        private static Dictionary<string, UltSpellDataS> UltSpellData = new Dictionary<string, UltSpellDataS>
        {
            { "Ezreal",  new UltSpellDataS { SpellStage = 0, DamageMultiplicator = 0.7f, Width = 160f, Delay = 1000f/1000f, Speed = 2000f, Collision = false}}
        };

        private static bool CanUseUlt(Obj_AI_Hero hero) //use for allies when fixed: champ.Spellbook.GetSpell(SpellSlot.R) = Ready
        {
            return hero.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready ||
                (hero.Spellbook.GetSpell(SpellSlot.R).Level > 0 && hero.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Surpressed && hero.Mana >= hero.Spellbook.GetSpell(SpellSlot.R).ManaCost);
        }

        private static void HandleUltTarget(EnemyInfo enemyInfo)
        {
            bool ultNow = false;
            bool me = false;

            foreach (Obj_AI_Hero champ in Allies.Where(x => //gathering the damage from allies should probably be done once only with timers
                            x.IsValid<Obj_AI_Hero>() &&
                            !x.IsDead &&
                            ((x.IsMe && !x.IsStunned) || TeamUlt.Items.Any(item => item.GetValue<bool>() && item.Name == x.ChampionName)) &&
                            CanUseUlt(x)))
            {
                if (Menu.Item("checkCollision").GetValue<bool>() && UltSpellData[champ.ChampionName].Collision && IsCollidingWithChamps(champ, EnemySpawnPos, UltSpellData[champ.ChampionName].Width))
                {
                    enemyInfo.RecallInfo.IncomingDamage[champ.NetworkId] = 0;
                    continue;
                }

                //increase timeneeded if it should arrive earlier, decrease if later
                var timeneeded = GetUltTravelTime(champ, UltSpellData[champ.ChampionName].Speed, UltSpellData[champ.ChampionName].Delay, EnemySpawnPos) - 65;

                if (enemyInfo.RecallInfo.GetRecallCountdown() >= timeneeded)
                    enemyInfo.RecallInfo.IncomingDamage[champ.NetworkId] = (float)champ.GetSpellDamage(enemyInfo.Player, SpellSlot.R, UltSpellData[champ.ChampionName].SpellStage) * UltSpellData[champ.ChampionName].DamageMultiplicator;
                else if (enemyInfo.RecallInfo.GetRecallCountdown() < timeneeded - (champ.IsMe ? 0 : 125)) //some buffer for allies so their damage isnt getting reset
                {
                    enemyInfo.RecallInfo.IncomingDamage[champ.NetworkId] = 0;
                    continue;
                }

                if (champ.IsMe)
                {
                    me = true;

                    enemyInfo.RecallInfo.EstimatedShootT = timeneeded;

                    if (enemyInfo.RecallInfo.GetRecallCountdown() - timeneeded < 60)
                        ultNow = true;
                }
            }

            if (me)
            {
                if (!IsTargetKillable(enemyInfo))
                {
                    enemyInfo.RecallInfo.LockedTarget = false;
                    return;
                }

                enemyInfo.RecallInfo.LockedTarget = true;

                //AutoUseBaseUit
                if (!ultNow || Menu.Item("panicKey").GetValue<KeyBind>().Active || Menu.Item("AutoUseBaseUit").GetValue<bool>())
                    return;

                Ultimate.Cast(EnemySpawnPos, true);
                LastUltCastT = Environment.TickCount;
            }
            else
            {
                enemyInfo.RecallInfo.LockedTarget = false;
                enemyInfo.RecallInfo.EstimatedShootT = 0;
            }
        }

        private static bool IsTargetKillable(EnemyInfo enemyInfo)
        {
            float totalUltDamage = enemyInfo.RecallInfo.IncomingDamage.Values.Sum();

            float targetHealth = GetTargetHealth(enemyInfo, enemyInfo.RecallInfo.GetRecallCountdown());

            if (Environment.TickCount - enemyInfo.LastSeen > 20000 && !Menu.Item("regardlessKey").GetValue<KeyBind>().Active)
            {
                if (totalUltDamage < enemyInfo.Player.MaxHealth)
                    return false;
            }
            else if (totalUltDamage < targetHealth)
                return false;

            return true;
        }

        private static float GetTargetHealth(EnemyInfo enemyInfo, int additionalTime)
        {
            if (enemyInfo.Player.IsVisible)
                return enemyInfo.Player.Health;

            float predictedHealth = enemyInfo.Player.Health + enemyInfo.Player.HPRegenRate * ((Environment.TickCount - enemyInfo.LastSeen + additionalTime) / 1000f);

            return predictedHealth > enemyInfo.Player.MaxHealth ? enemyInfo.Player.MaxHealth : predictedHealth;
        }

        private static float GetUltTravelTime(Obj_AI_Hero source, float speed, float delay, Vector3 targetpos)
        {
            if (source.ChampionName == "Karthus")
                return delay * 1000;

            float distance = Vector3.Distance(source.ServerPosition, targetpos);

            float missilespeed = speed;

            if (source.ChampionName == "Jinx" && distance > 1350)
            {
                const float accelerationrate = 0.3f; //= (1500f - 1350f) / (2200 - speed), 1 unit = 0.3units/second

                var acceldifference = distance - 1350f;

                if (acceldifference > 150f) //it only accelerates 150 units
                    acceldifference = 150f;

                var difference = distance - 1500f;

                missilespeed = (1350f * speed + acceldifference * (speed + accelerationrate * acceldifference) + difference * 2200f) / distance;
            }

            return (distance / missilespeed + delay) * 1000;
        }

        private static bool IsCollidingWithChamps(Obj_AI_Hero source, Vector3 targetpos, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                Unit = source,
            };

            input.CollisionObjects[0] = CollisionableObjects.Heroes;

            return Collision.GetCollision(new List<Vector3> { targetpos }, input).Any(); //x => x.NetworkId != targetnetid, hard to realize with teamult
        }

        private static void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            var unit = sender as Obj_AI_Hero;

            if (unit == null || !unit.IsValid || unit.IsAlly)
            {
                return;
            }

            var recall = Packet.S2C.Teleport.Decoded(unit, args);
            var enemyInfo = EnemyInfo.Find(x => x.Player.NetworkId == recall.UnitNetworkId).RecallInfo.UpdateRecall(recall);

            if (recall.Type == Packet.S2C.Teleport.Type.Recall)
            {
                switch (recall.Status)
                {
                    case Packet.S2C.Teleport.Status.Abort:
                        if (Menu.Item("notifRecAborted").GetValue<bool>())
                        {
                            ShowNotification(enemyInfo.Player.ChampionName + ": 回程中", Color.Orange, 4000);
                        }

                        break;
                    case Packet.S2C.Teleport.Status.Finish:
                        if (Menu.Item("notifRecFinished").GetValue<bool>())
                        {
                            ShowNotification(enemyInfo.Player.ChampionName + ": 回程完毕", NotificationColor, 4000);
                        }

                        break;
                }
            }
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            Text.OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            Text.OnLostDevice();
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            Text.Dispose();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu.Item("showRecalls").GetValue<bool>() || Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
                return;

            bool indicated = false;

            float fadeout = 1f;
            int count = 0;

            foreach (EnemyInfo enemyInfo in EnemyInfo.Where(x =>
                x.Player.IsValid<Obj_AI_Hero>() &&
                x.RecallInfo.ShouldDraw() &&
                !x.Player.IsDead && //maybe redundant
                x.RecallInfo.GetRecallCountdown() > 0).OrderBy(x => x.RecallInfo.GetRecallCountdown()))
            {
                if (!enemyInfo.RecallInfo.LockedTarget)
                {
                    fadeout = 1f;
                    Color color = Color.White;

                    if (enemyInfo.RecallInfo.WasAborted())
                    {
                        fadeout = enemyInfo.RecallInfo.GetDrawTime() / (float)enemyInfo.RecallInfo.FADEOUT_TIME;
                        color = Color.Yellow;
                    }

                    DrawRect(BarX, BarY, (int)(Scale * enemyInfo.RecallInfo.GetRecallCountdown()), BarHeight, 1, Color.FromArgb((int)(255f * fadeout), Color.White));
                    DrawRect(BarX + Scale * enemyInfo.RecallInfo.GetRecallCountdown() - 1, BarY - SeperatorHeight, 0, SeperatorHeight + 1, 1, Color.FromArgb((int)(255f * fadeout), Color.White));

                    Text.DrawText(null, enemyInfo.Player.ChampionName, (int)BarX + (int)(Scale * enemyInfo.RecallInfo.GetRecallCountdown() - (float)(enemyInfo.Player.ChampionName.Length * Text.Description.Width) / 2), (int)BarY - SeperatorHeight - Text.Description.Height - 1, new ColorBGRA(color.R, color.G, color.B, (byte)(color.A * fadeout)));
                }
                else
                {
                    if (!indicated && enemyInfo.RecallInfo.EstimatedShootT != 0)
                    {
                        indicated = true;
                        DrawRect(BarX + Scale * enemyInfo.RecallInfo.EstimatedShootT, BarY + SeperatorHeight + BarHeight - 3, 0, SeperatorHeight * 2, 2, Color.Orange);
                    }

                    DrawRect(BarX, BarY, (int)(Scale * enemyInfo.RecallInfo.GetRecallCountdown()), BarHeight, 1, Color.FromArgb(255, Color.Red));
                    DrawRect(BarX + Scale * enemyInfo.RecallInfo.GetRecallCountdown() - 1, BarY + SeperatorHeight + BarHeight - 3, 0, SeperatorHeight + 1, 1, Color.IndianRed);

                    Text.DrawText(null, enemyInfo.Player.ChampionName, (int)BarX + (int)(Scale * enemyInfo.RecallInfo.GetRecallCountdown() - (float)(enemyInfo.Player.ChampionName.Length * Text.Description.Width) / 2), (int)BarY + SeperatorHeight + Text.Description.Height / 2, new ColorBGRA(255, 92, 92, 255));
                }

                count++;
            }

            if (count > 0)
            {
                if (count != 1) //make the whole bar fadeout when its only 1
                    fadeout = 1f;

                DrawRect(BarX, BarY, BarWidth, BarHeight, 1, Color.FromArgb((int)(40f * fadeout), Color.White));
                DrawRect(BarX - 1, BarY + 1, 0, BarHeight, 1, Color.FromArgb((int)(255f * fadeout), Color.White));
                DrawRect(BarX - 1, BarY - 1, BarWidth + 2, 1, 1, Color.FromArgb((int)(255f * fadeout), Color.White));
                DrawRect(BarX - 1, BarY + BarHeight, BarWidth + 2, 1, 1, Color.FromArgb((int)(255f * fadeout), Color.White));
                DrawRect(BarX + 1 + BarWidth, BarY + 1, 0, BarHeight, 1, Color.FromArgb((int)(255f * fadeout), Color.White));
            }
        }

        private static void DrawRect(float x, float y, int width, int height, float thickness, Color color)
        {
            for (int i = 0; i < height; i++)
                Drawing.DrawLine(x, y + i, x + width, y + i, thickness, color);
        }

    }

    class EnemyInfo
    {
        public Obj_AI_Hero Player;
        public int LastSeen;

        public RecallInfo RecallInfo;

        public EnemyInfo(Obj_AI_Hero player)
        {
            Player = player;
            RecallInfo = new RecallInfo(this);
        }

    }

    class RecallInfo
    {
        public EnemyInfo EnemyInfo;
        public Dictionary<int, float> IncomingDamage; //from, damage
        public Packet.S2C.Teleport.Struct Recall;
        public Packet.S2C.Teleport.Struct AbortedRecall;
        public bool LockedTarget;
        public float EstimatedShootT;
        public int AbortedT;
        public int FADEOUT_TIME = 3000;

        public RecallInfo(EnemyInfo enemyInfo)
        {
            EnemyInfo = enemyInfo;
            Recall = new Packet.S2C.Teleport.Struct(EnemyInfo.Player.NetworkId, Packet.S2C.Teleport.Status.Unknown, Packet.S2C.Teleport.Type.Unknown, 0);
            IncomingDamage = new Dictionary<int, float>();
        }

        public bool ShouldDraw()
        {
            return IsPorting() || (WasAborted() && GetDrawTime() > 0);
        }

        public bool IsPorting()
        {
            return Recall.Type == Packet.S2C.Teleport.Type.Recall && Recall.Status == Packet.S2C.Teleport.Status.Start;
        }

        public bool WasAborted()
        {
            return Recall.Type == Packet.S2C.Teleport.Type.Recall && Recall.Status == Packet.S2C.Teleport.Status.Abort;
        }

        public EnemyInfo UpdateRecall(Packet.S2C.Teleport.Struct newRecall)
        {
            IncomingDamage.Clear();
            LockedTarget = false;
            EstimatedShootT = 0;

            if (newRecall.Type == Packet.S2C.Teleport.Type.Recall && newRecall.Status == Packet.S2C.Teleport.Status.Abort)
            {
                AbortedRecall = Recall;
                AbortedT = Environment.TickCount;
            }
            else
                AbortedT = 0;

            Recall = newRecall;
            return EnemyInfo;
        }

        public int GetDrawTime()
        {
            int drawtime = 0;

            if (WasAborted())
                drawtime = FADEOUT_TIME - (Environment.TickCount - AbortedT);
            else
                drawtime = GetRecallCountdown();

            return drawtime < 0 ? 0 : drawtime;
        }

        public int GetRecallCountdown()
        {
            int time = Environment.TickCount;
            int countdown = 0;

            if (time - AbortedT < FADEOUT_TIME)
                countdown = AbortedRecall.Duration - (AbortedT - AbortedRecall.Start);
            else if (AbortedT > 0)
                countdown = 0; //AbortedT = 0
            else
                countdown = Recall.Start + Recall.Duration - time;

            return countdown < 0 ? 0 : countdown;
        }

        public override string ToString()
        {
            string drawtext = EnemyInfo.Player.ChampionName + ": " + Recall.Status; //change to better string and colored

            float countdown = GetRecallCountdown() / 1000f;

            if (countdown > 0)
                drawtext += " (" + countdown.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "s)";

            return drawtext;
        }

    }
}
