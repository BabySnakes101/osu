// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Data
{
    public class PatternInterpreter
    {
        private List<Pattern> patternList;
        public IReadOnlyList<Pattern> PatternList => patternList.AsReadOnly();
        public Dictionary<TaikoDifficultyHitObject, Pattern> ObjectPatternDict;

        public PatternInterpreter(List<TaikoDifficultyHitObject> hitObjects, double hitObjectDeltaTimeTolerance)
        {
            ArgumentNullException.ThrowIfNull(hitObjects);
            if (hitObjects.Count == 0 || hitObjectDeltaTimeTolerance < 0)
                throw new ArgumentException();

            ObjectPatternDict = new Dictionary<TaikoDifficultyHitObject, Pattern>();
            patternList = resolvePatterns(hitObjects, hitObjectDeltaTimeTolerance);
        }

        public PatternInterpreter(List<TaikoDifficultyHitObject> hitObjects, Dictionary<TaikoDifficultyHitObject, double> normalizedDeltaTimes, double hitObjectDeltaTimeTolerance)
        {
            ArgumentNullException.ThrowIfNull(hitObjects);
            if (hitObjects.Count == 0 || hitObjectDeltaTimeTolerance < 0)
                throw new ArgumentException();

            ObjectPatternDict = new Dictionary<TaikoDifficultyHitObject, Pattern>();
            patternList = resolveNormalizedPatterns(hitObjects, normalizedDeltaTimes, hitObjectDeltaTimeTolerance);
        }

        private List<Pattern> resolvePatterns(List<TaikoDifficultyHitObject> hitObjects, double hitObjectDeltaTimeTolerance)
        {
            var patterns = new List<Pattern>();

            int currentPatternStartIndex = 0;
            int patternIndex = 0;
            for (int i = 2; i < hitObjects.Count; i++)
            {
                var nextNote = hitObjects[i].NextNote(0);
                var previousNote = hitObjects[i].PreviousNote(0);

                bool sameDeltaTimeAsPrevious = previousNote is null || Math.Abs(hitObjects[i].DeltaTime - previousNote.DeltaTime) < hitObjectDeltaTimeTolerance; //null check not necessary since it will never happen
                bool slowerDeltaTimeThanNext = nextNote is null || nextNote.DeltaTime - hitObjects[i].DeltaTime < -hitObjectDeltaTimeTolerance;

                int currentPatternObjCount = i - currentPatternStartIndex;
                if (i == hitObjects.Count - 1)
                    currentPatternObjCount++; //Include last note


                if (slowerDeltaTimeThanNext || !(currentPatternObjCount == 1) && !sameDeltaTimeAsPrevious) //magic
                {
                    var patternObjects = hitObjects.GetRange(currentPatternStartIndex, currentPatternObjCount);
                    var pattern = new Pattern(this, patternObjects, patternIndex);
                    patterns.Add(pattern);
                    foreach (var patternObject in patternObjects)
                        ObjectPatternDict.Add(patternObject, pattern);

                    currentPatternStartIndex = i;
                    patternIndex++;
                }
            }
            return patterns;
        }
        private List<Pattern> resolveNormalizedPatterns(List<TaikoDifficultyHitObject> hitObjects, Dictionary<TaikoDifficultyHitObject, double> normalizedDeltaTimes, double hitObjectDeltaTimeTolerance)
        {
            var patterns = new List<Pattern>();

            int currentPatternStartIndex = 0;
            int patternIndex = 0;
            for (int i = 2; i < hitObjects.Count; i++)
            {
                var nextNote = hitObjects[i].NextNote(0);
                var previousNote = hitObjects[i].PreviousNote(0);

                bool sameDeltaTimeAsPrevious = previousNote is null || Math.Abs(normalizedDeltaTimes[hitObjects[i]] - normalizedDeltaTimes[previousNote]) < hitObjectDeltaTimeTolerance; //null check not necessary since it will never happen
                bool slowerDeltaTimeThanNext = nextNote is null || normalizedDeltaTimes[nextNote] - normalizedDeltaTimes[hitObjects[i]] < -hitObjectDeltaTimeTolerance;

                int currentPatternObjCount = i - currentPatternStartIndex;
                if (i == hitObjects.Count - 1)
                    currentPatternObjCount++; //Include last note


                if (slowerDeltaTimeThanNext || !(currentPatternObjCount == 1) && !sameDeltaTimeAsPrevious) //magic
                {
                    var patternObjects = hitObjects.GetRange(currentPatternStartIndex, currentPatternObjCount);
                    var pattern = new Pattern(this, patternObjects, patternIndex);
                    patterns.Add(pattern);
                    foreach (var patternObject in patternObjects)
                        ObjectPatternDict.Add(patternObject, pattern);

                    currentPatternStartIndex = i;
                    patternIndex++;
                }
            }
            return patterns;
        }

    }
}

