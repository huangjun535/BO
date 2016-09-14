#region License

/*
 Copyright 2014 - 2015 Nikita Bernthaler
 IChampion.cs is part of YuLeViktor.

 YuLeViktor is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 YuLeViktor is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with YuLeViktor. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion License

#region

using System.Collections.Generic;
using LeagueSharp.Common;
using Orbwalking = YuLeViktor.STargetSelector.Orbwalking;
using Spell = YuLeViktor.Wrappers.Spell;

#endregion

namespace YuLeViktor.Interfaces
{
    public interface IChampion
    {
        Menu SMenu { get; }
        Menu Menu { get; }
        Orbwalking.Orbwalker Orbwalker { get; }
        List<Spell> Spells { get; }
        void Combo();
        void Harass();
        void LaneClear();
        void JungleClear();
        void Flee();
        void Killsteal();
    }
}