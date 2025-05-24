// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Taiko.Difficulty.Data;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data;
using osu.Game.Rulesets.Taiko.Difficulty.Utils;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm
{
    public static class TaikoRhythmDifficultyPreprocessor
    {
        public static void ProcessAndAssign(List<TaikoDifficultyHitObject> hitObjects)
        {
            var rhythmGroups = createSameRhythmGroupedHitObjects(hitObjects);

            foreach (var rhythmGroup in rhythmGroups)
            {
                foreach (var hitObject in rhythmGroup.HitObjects)
                    hitObject.RhythmData.SameRhythmGroupedHitObjects = rhythmGroup;
            }

            var patternGroups = createSamePatternGroupedHitObjects(rhythmGroups);

            foreach (var patternGroup in patternGroups)
            {
                foreach (var hitObject in patternGroup.AllHitObjects)
                    hitObject.RhythmData.SamePatternsGroupedHitObjects = patternGroup;
            }
        }

        private static List<SameRhythmHitObjectGrouping> createSameRhythmGroupedHitObjects(List<TaikoDifficultyHitObject> hitObjects)
        {
            var rhythmGroups = new List<SameRhythmHitObjectGrouping>();
            /*
            DeltaTimeNormalizer normalizer = new DeltaTimeNormalizer(hitObjects);
            normalizer.ModNormalizedDeltaTime(0);
            foreach (var hitObject in hitObjects)
            {
                hitObject.RhythmData =  new TaikoRhythmData(hitObject);
            }
            var normalizedDeltaTimes = normalizer.GetNormalizedDeltaTime(0);
            //normalizer.PrintDeltaTimeDeviations(8);
            */
            //PatternInterpreter interpreter = new PatternInterpreter(hitObjects, normalizedDeltaTimes, 5);
            PatternInterpreter interpreter = new PatternInterpreter(hitObjects, 2);


            List<List<TaikoDifficultyHitObject>> list = new List<List<TaikoDifficultyHitObject>>();

            foreach (var pattern in interpreter.PatternList)
            {
                List<TaikoDifficultyHitObject> sublist = new List<TaikoDifficultyHitObject>();
                foreach (var item in pattern.PatternObjects)
                {
                    sublist.Add(item);
                }
                list.Add(sublist);
            }
            //PatternDebugUtils.PrintPatterns(interpreter);

            foreach (var grouped in list)
            {
                rhythmGroups.Add(new SameRhythmHitObjectGrouping(rhythmGroups.LastOrDefault(), grouped));
                Console.WriteLine(new SameRhythmHitObjectGrouping(rhythmGroups.LastOrDefault(), grouped).ToString());
            }

            return rhythmGroups;
        }

        private static List<SamePatternsGroupedHitObjects> createSamePatternGroupedHitObjects(List<SameRhythmHitObjectGrouping> rhythmGroups)
        {
            var patternGroups = new List<SamePatternsGroupedHitObjects>();

            foreach (var grouped in IntervalGroupingUtils.GroupByInterval(rhythmGroups))
                patternGroups.Add(new SamePatternsGroupedHitObjects(patternGroups.LastOrDefault(), grouped));

            return patternGroups;
        }
    }
}
