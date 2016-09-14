﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Marksman.Common;

namespace Marksman.Champions
{
    internal static class PlayerSpells
    {
        public static void CastIfHitchanceGreaterOrEqual(this Spell spell, Obj_AI_Hero t)
        {
            var nPrediction = spell.GetPrediction(t);
            var nHitPosition = nPrediction.CastPosition.Extend(ObjectManager.Player.Position, -130);
            if (nPrediction.Hitchance >= spell.GetHitchance())
            {
                spell.Cast(nHitPosition);
            }
            
        }
    }
}
