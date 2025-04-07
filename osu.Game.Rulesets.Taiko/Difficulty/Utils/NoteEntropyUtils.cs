// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    public static class NoteEntropyUtils
    {

        public static double Entropy(IEnumerable<TaikoDifficultyHitObject> taikoHitObjects) //Copy paste
        {
            bool[] data = taikoHitObjects.Select(c => c.IsKat).ToArray();
            int order = 3;
            Dictionary<string, int> patternCounts = new Dictionary<string, int>();
            int totalPatterns = data.Length - order + 1;

            if (totalPatterns <= 0)
                return 0.0; // Not enough data to compute entropy

            for (int i = 0; i < totalPatterns; i++)
            {
                bool[] window = data.Skip(i).Take(order).ToArray();
                int[] pattern = GetPermutationPattern(window);
                string patternKey = string.Join(",", pattern);

                if (patternCounts.ContainsKey(patternKey))
                    patternCounts[patternKey]++;
                else
                    patternCounts[patternKey] = 1;
            }

            // Compute probabilities
            double entropy = 0.0;
            foreach (int count in patternCounts.Values)
            {
                double probability = (double)count / totalPatterns;
                entropy -= probability * Math.Log2(probability);
            }

            return entropy;
        }

        private static int[] GetPermutationPattern(bool[] window)
        {
            // Sort indices by boolean value: false < true, preserve order in case of ties
            return window
                .Select((value, index) => new { Value = value, Index = index })
                .OrderBy(x => x.Value)
                .ThenBy(x => x.Index)
                .Select(x => x.Index)
                .ToArray();
        }
    }
}
