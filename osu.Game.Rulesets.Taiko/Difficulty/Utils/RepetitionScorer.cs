// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using static osu.Game.Rulesets.Taiko.Difficulty.Utils.NoteEntropyUtils;

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    public static class RepetitionScorer
    {
        public static Dictionary<TaikoDifficultyHitObject, double> ComputeRepetitionScores(
            List<TaikoDifficultyHitObject> notes,
            Dictionary<TaikoDifficultyHitObject, Dictionary<int, RepeatInfo>> noteWindowRepetition,
            int targetWindowSize = 1,
            int repeatCap = 6,            // After this many repeats, score is maxed
            double decayTimeThreshold = 10000.0 // Time in ms that weakens decay if exceeded
        )
        {
            var scores = new Dictionary<TaikoDifficultyHitObject, double>();

            // Group notes into repetition patterns
            var groups = noteWindowRepetition
                .Where(kvp => kvp.Value.ContainsKey(targetWindowSize))
                .GroupBy(kvp => kvp.Value[targetWindowSize]);

            foreach (var group in groups)
            {
                RepeatInfo info = group.Key;
                int start = info.Start;
                int total = info.RepeatCount;
                int length = info.Length;

                double baseScore = Math.Min(1.0, (double)total / repeatCap);

                for (int i = 0; i < total * length; i++)
                {
                    int globalIndex = start + i;
                    if (globalIndex >= notes.Count) break;

                    var note = notes[globalIndex];
                    var segment = notes.Skip(start).Take(total * length).ToArray();

                    double decay = GetDecayFactorWithTiming(i, total * length, segment, decayTimeThreshold);
                    scores[note] = baseScore * decay;
                }
            }

            return scores;
        }

        private static double GetDecayFactorWithTiming(
            int position,
            int totalLength,
            TaikoDifficultyHitObject[] segment,
            double decayTimeThreshold)
        {
            if (totalLength <= 1) return 1.0;

            // Base decay (cosine curve: 0 → 1 → 0)
            double t = (double)position / (totalLength - 1);
            double baseDecay = 0.5 * (1.0 - Math.Cos(Math.PI * t));

            // Edge notes get modified by DeltaTime
            bool isStartEdge = position == 0;
            bool isEndEdge = position == totalLength - 1;

            double deltaTime = 0;

            if (isStartEdge)
                deltaTime = segment[0].DeltaTime;
            else if (isEndEdge)
                deltaTime = segment[^1].DeltaTime;

            Console.WriteLine("DeltaTime: " + deltaTime);
            double timingFactor = Math.Min(1.0, deltaTime / decayTimeThreshold);
            double timingAdjustment = 1.0 - timingFactor;

            return baseDecay * timingAdjustment;
        }
    }
}
