// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Data
{
    /// <summary>
    /// Represents a group of <see cref="TaikoDifficultyHitObject"/>s identified as a pattern by a <see cref="PatternInterpreter"/>.
    /// </summary>
    public class Pattern
    {
        /// <summary>
        /// Gets the index of this pattern within the sequence defined by its <see cref="PatternInterpreter"/>.
        /// </summary>
        public int PatternIndex { get; private set; }

        /// <summary>
        /// Gets the list of hit objects in this pattern as a read-only list.
        /// </summary>
        public IReadOnlyList<TaikoDifficultyHitObject> PatternObjects => patternObjects.AsReadOnly();

        private readonly List<TaikoDifficultyHitObject> patternObjects;

        /// <summary>
        /// Gets a sequence of delta times between consecutive hit objects within this pattern.
        /// </summary>
        /// <remarks>
        /// The first hit object is excluded since its delta time is considered part of the transition from the previous pattern.
        /// </remarks>
        public IEnumerable<double> DeltaTimes
        {
            get
            {
                for (int i = 1; i < patternObjects.Count; i++) //We ignore first one as it is the delta time between this pattern and previous
                {
                    yield return patternObjects[i].DeltaTime;
                }
            }
        }

        /// <summary>
        /// Gets the average delta time between objects in this pattern.
        /// </summary>
        /// <remarks>
        /// Returns null if the pattern contains only one object or no valid delta times.
        /// The result is cached after the first calculation.
        /// </remarks>
        public double? AverageDeltaTime
        {
            get
            {
                if (!DeltaTimes.Any())
                    return null;

                averageDeltaTime ??= DeltaTimes.Average();
                return averageDeltaTime;
            }
        }

        private double? averageDeltaTime;

        /// <summary>
        /// Gets the delta time of the final hit object in this pattern.
        /// </summary>
        /// <remarks>
        /// Returns null if the pattern contains only one hit object.
        /// </remarks>
        public double? FinalObjectDeltaTime
        {
            get
            {
                if (PatternObjects.Count <= 1)
                    return null;
                return patternObjects.LastOrDefault()?.DeltaTime;
            }
        }

        private readonly PatternInterpreter patternInterpreter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern"/> class.
        /// </summary>
        /// <param name="patternInterpreter">The <see cref="PatternInterpreter"/> responsible for identifying this pattern.</param>
        /// <param name="hitObjects">The hit objects that belong to this pattern.</param>
        /// <param name="patternIndex">The index of this pattern in its <see cref="PatternInterpreter"/> sequence of patterns.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hitObjects"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="hitObjects"/> is an empty list.</exception>
        public Pattern(PatternInterpreter patternInterpreter, List<TaikoDifficultyHitObject> hitObjects, int patternIndex)
        {
            ArgumentNullException.ThrowIfNull(hitObjects);
            if (hitObjects.Count == 0)
                throw new ArgumentException();

            this.patternInterpreter = patternInterpreter;
            PatternIndex = patternIndex;
            patternObjects = hitObjects;
            averageDeltaTime = null;
        }

        //TODO: REMOVE THIS (This was temporary testing only)
        public override string ToString()
        {
            string result = "";
            foreach (var patternObject in patternObjects)
            {
                result += patternObject.ToString();
            }

            return result;
        }

        /// <summary>
        /// Searches for previous patterns.
        /// </summary>
        /// <param name="backwardsIndex">Number of patterns to search backward.</param>
        /// <returns>Previous pattern number <paramref name="backwardsIndex"/>.</returns>
        public Pattern? PreviousPattern(int backwardsIndex) => SearchPattern(-backwardsIndex);

        /// <summary>
        /// Searches for next patterns.
        /// </summary>
        /// <param name="forwardsIndex">Number of patterns to search forward</param>
        /// <returns>Next pattern number <paramref name="forwardsIndex"/>.</returns>
        public Pattern? NextPattern(int forwardsIndex) => SearchPattern(forwardsIndex);

        /// <summary>
        /// Searches for patterns given an <paramref name="patternOffset"/> offset number.
        /// </summary>
        /// <param name="patternOffset">Offset index relative to the current pattern. (negative previous, positive next)</param>
        /// <returns>Pattern shifted by <paramref name="patternOffset"/> relative to the current pattern.</returns>
        public Pattern? SearchPattern(int patternOffset) => patternInterpreter.PatternList.ElementAtOrDefault(PatternIndex + patternOffset);
    }
}
