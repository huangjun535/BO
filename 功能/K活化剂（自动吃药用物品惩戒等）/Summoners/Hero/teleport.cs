using System;
using System.Linq;
using Activator.Base;
using LeagueSharp;
using LeagueSharp.Common;

namespace Activator.Summoners
{
    class teleport : CoreSum
    {
        internal override string Name => "summonerteleport";
        internal override string DisplayName => "Teleport";
        internal override string[] ExtraNames => new[] { "" };
        internal override float Range => float.MaxValue;
        internal override int Duration => 3500;

        static bool IsLethal(Champion hero)
        {
            return hero.Player.Health/hero.Player.MaxHealth * 100 <= 35 && hero.IncomeDamage > 0 ||
                   hero.HitTypes.Contains(HitType.Ultimate) && hero.IncomeDamage > 0;
        }

        public override void AttachMenu(Menu menu)
        {
            menu.AddItem(new MenuItem("teleblank", "Disclaimer: Camera Position are Beta! :^)"));
            menu.AddItem(new MenuItem("teleqq2", "Show Recent Event"))
                .SetTooltip("Move camera to the most recent event")
                .SetValue(new KeyBind('Z', KeyBindType.Press));
            menu.AddItem(new MenuItem("teleqq", "Show Recent Event (Lock to Me)"))
                .SetTooltip("Move camera between you and the most recent event")
                .SetValue(new KeyBind(192, KeyBindType.Press))
                .ValueChanged += (sender, e) =>
                {
                    if (e.GetNewValue<KeyBind>().Active == e.GetOldValue<KeyBind>().Active) return;
                    if (e.GetNewValue<KeyBind>().Active == false) Camera.Position = Player.Position;
                };
            menu.AddItem(new MenuItem("teledraw", "Outline Ally in Danger (Minimap)")).SetValue(true);
        }

        public override void OnTick(EventArgs args)
        {
            if (Menu.Item("teleqq").GetValue<KeyBind>().Active || Menu.Item("teleqq2").GetValue<KeyBind>().Active)
            {
                var p = Activator.Allies().Where(h => !h.Player.IsMe && h.Player.Distance(Player) > 1800f)
                        .OrderByDescending(h => h.IncomeDamage)
                        .ThenByDescending(h => h.Player.CountEnemiesInRange(1450))
                        .ThenBy(h => h.Player.Health/h.Player.MaxHealth*100).FirstOrDefault();

                if (p != null)
                {
                    Camera.Position = p.Player.Position;
                }
            }
        }

        public override void OnDraw(EventArgs args)
        {                       
            if (IsReady() && Menu.Item("teledraw").GetValue<bool>())
            {
                foreach (var hero in Activator.Allies())
                {
                    if (hero.Player.IsValid && !hero.Player.IsZombie && !hero.Player.IsDead)
                    {
                        if (IsLethal(hero) && !hero.Player.IsMe)
                        {
                            Utility.DrawCircle(hero.Player.Position, 850f, System.Drawing.Color.Crimson, 3, 30, true);
                        }
                    }
                }
            }          
        }
    }
}
