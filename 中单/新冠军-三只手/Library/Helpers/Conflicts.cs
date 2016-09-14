#region License

/*
 Copyright 2014 - 2015 Nikita Bernthaler
 Conflicts.cs is part of YuLeViktor.

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

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using YuLeViktor.Library.Extensions.NET;

#endregion

namespace YuLeViktor.Helpers
{
    internal static class Conflicts
    {
        private static readonly List<string> AssemblyBlacklist;

        static Conflicts()
        {
            AssemblyBlacklist = new List<string>
            {
                "mscorlib",
                "System",
                "Microsoft",
                "SMDiagnostics",
                "LeagueSharp.Common",
                "Activator",
                "Utility",
                "Awareness",
                "Evade",
                "Tracker",
                "Smite",
                Global.Name
            };
        }
    }
}