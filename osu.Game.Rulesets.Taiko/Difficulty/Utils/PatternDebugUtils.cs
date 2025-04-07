// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Rulesets.Taiko.Difficulty.Data;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    /// <summary>
    /// Temporary debug utils for patterns.
    /// </summary>
    public static class PatternDebugUtils
    {

        public static void PrintPatterns(PatternInterpreter patternInterpreter)
        {
            foreach (Pattern pattern in patternInterpreter.PatternList)
            {
                string patternInfo = "";
                foreach (TaikoDifficultyHitObject note in pattern.PatternObjects)
                {
                    patternInfo += note.IsKat ? "k" : "d";
                }
                Console.WriteLine($"[{patternInfo}]{(pattern.AverageDeltaTime is not null ? $" ({pattern.AverageDeltaTime} ms average)" : "")}");
            }
        }
    }
}
