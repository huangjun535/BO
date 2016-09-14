﻿#region

using System;
using LeagueSharp;
using LeagueSharp.Common;
using Marksman.Orb;
using Orbwalking = Marksman.Orb.Orbwalking;

#endregion

namespace Marksman.Champions
{
    internal class Champion
    {
        public bool ComboActive;
        public bool HarassActive;
        public bool LaneClearActive;
        public bool JungleClearActive;
        public Menu Config;
        
        public string Id = "";
        public Orbwalking.Orbwalker Orbwalker;
        public bool ToggleActive;

        public T GetValue<T>(string item)
        {
            return Config.Item(item + Id).GetValue<T>();
        }

        public virtual bool ComboMenu(Menu config)
        {
            return false;
        }

        public virtual bool HarassMenu(Menu config)
        {
            return false;
        }

        public virtual bool JungleClearMenu(Menu config)
        {
            return false;
        }

        public virtual bool LaneClearMenu(Menu config)
        {
            return false;
        }

        public virtual bool MiscMenu(Menu config)
        {
            return false;
        }

        public virtual bool DrawingMenu(Menu config)
        {
            return false;
        }

        public virtual bool MainMenu(Menu config)
        {
            return false;
        }
        public virtual bool ToolsMenu(Menu config)
        {
            return false;
        }
        public virtual void Drawing_OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }
        }

        
        public virtual void Game_OnUpdate(EventArgs args)
        {
            PermaActive();

            if (JungleClearActive)
            {
                ExecuteJungleClear();
            }
            
            if (LaneClearActive)
            {
                
                ExecuteLaneClear();
            }
        }

        public virtual void ExecuteCombo() { }
        public virtual void ExecuteHarass() { }
        public virtual void ExecuteFlee() { }
        public virtual void ExecuteLaneClear() { }
        public virtual void ExecuteJungleClear() { }
        public virtual void PermaActive() { }

        public virtual void DrawingOnEndScene(EventArgs args)
        {
                    if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
            {
                return;
            }
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

        }

        public virtual void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target) { }
        public virtual void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args) { }

        public virtual void OnCreateObject(GameObject sender, EventArgs args) { }
        public virtual void OnDeleteObject(GameObject sender, EventArgs args) { }

        public virtual void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args) { }
        public virtual void Obj_AI_Base_OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args) { }

        public virtual void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args) { }
        public virtual void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) { }


    }
}
