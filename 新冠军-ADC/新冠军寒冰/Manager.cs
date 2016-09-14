using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Enumerations;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Utils;
using NLog;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;

namespace YuLeAshe
{
    internal static class SpellManager
    {
        internal static readonly Spell Q, W, E, R;

        static SpellManager()
        {
            Q = new Spell(SpellSlot.Q, 100f);
            W = new Spell(SpellSlot.W, 1200f);
            E = new Spell(SpellSlot.E, 300f);
            R = new Spell(SpellSlot.R, 2500f);

            W.SetSkillshot(0.25f, 60f, 1500f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 130f, 1600f, false, SkillshotType.SkillshotLine);
        }

        internal static void Initialize() { }
    }

    internal static class SkinManager
    {
        private static readonly WebClient Webclient = new WebClient();
        private static readonly JavaScriptSerializer JSerializer = new JavaScriptSerializer();

        private static readonly List<SkinInfo> SkinList = new List<SkinInfo>();

        private static int _skiniwant = -1;
        private static readonly int DefaultSkinId;

        private static MenuBool _enabledMenuBool;

        static SkinManager()
        {
            try
            {
                var versionJson = Webclient.DownloadString("http://ddragon.leagueoflegends.com/realms/na.json");
                var gameVersion = (string)((Dictionary<string, object>)JSerializer.Deserialize<Dictionary<string, object>>(versionJson)["n"])["champion"];
                var champJson = Webclient.DownloadString($"http://ddragon.leagueoflegends.com/cdn/{gameVersion}/data/en_US/champion/{GameObjects.Player.ChampionName}.json");
                var skins = (ArrayList)((Dictionary<string, object>)((Dictionary<string, object>)JSerializer.Deserialize<Dictionary<string, object>>(champJson)["data"])[GameObjects.Player.ChampionName])["skins"];

                foreach (Dictionary<string, object> skin in skins)
                {
                    SkinList.Add(new SkinInfo
                    {
                        Model = GameObjects.Player.ChampionName,
                        ID = (int)skin["num"],
                        Name = skin["name"].ToString().Contains("default") ? GameObjects.Player.ChampionName : skin["name"].ToString()
                    });
                }

                var firstOrDefault = SkinList.FirstOrDefault(x => x.Name == GameObjects.Player.SkinName);
                if (firstOrDefault != null)
                {
                    DefaultSkinId = firstOrDefault.ID;
                }

                Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            }
            catch
            {
                LogManager.GetCurrentClassLogger().Debug("GetSkin Failed");
            }
        }

        private static bool Enabled => _enabledMenuBool.Value;

        internal static void Initialize(Menu menu)
        {
            var submenu = menu.Add(new Menu("Skins", "Skins"));

            _enabledMenuBool = submenu.Add(new MenuBool("Enabled", "Enabled", true));
            var skinListItem = submenu.Add(new MenuList<string>("Skin", "Skin", SkinList.Select(x => x.Name)));

            _enabledMenuBool.ValueChanged += (sender, args) =>
            {
                if (_enabledMenuBool.Value)
                {
                    SetSkin(SkinList[skinListItem.Index].ID);
                }
                else
                {
                    GameObjects.Player.SetSkin(GameObjects.Player.ChampionName, DefaultSkinId);
                }
            };

            skinListItem.ValueChanged += (sender, args) =>
            {
                SetSkin(SkinList[skinListItem.Index].ID);
            };

            SetSkin(SkinList[skinListItem.Index].ID);
        }

        private static void SetSkin(int skinid)
        {
            if (Enabled)
            {
                GameObjects.Player.SetSkin(GameObjects.Player.ChampionName, skinid);
            }
            _skiniwant = skinid;
        }

        private static void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (Enabled && _skiniwant > -1 && sender.IsMe && args.Animation.ToLowerInvariant() == "respawn")
            {
                DelayAction.Add(250, () => GameObjects.Player.SetSkin(GameObjects.Player.ChampionName, _skiniwant));
            }
        }

        private class SkinInfo
        {
            internal int ID;
            internal string Model;
            internal string Name;
        }
    }
}
