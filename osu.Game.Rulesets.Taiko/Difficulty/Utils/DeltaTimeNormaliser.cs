// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using NUnit.Framework.Constraints;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;


namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    public class DeltaTimeNormaliser
    {
        private List<TaikoDifficultyHitObject> hitObjects;

        /// <summary>
        /// Dictionary mapping each <see cref ="TaikoDifficultyHitObject"/> to its corresponding normalised DeltaTime.
        /// </summary>
        public Dictionary<TaikoDifficultyHitObject, double> NormalizedDeltaTime { get; private set; }


        /// <param name="hitObjects">List of <see cref ="TaikoDifficultyHitObject"/> to perform the DeltaTime normalisation on.</param>
        /// <param name="tolerance">Maximum allowed deviation.</param>
        public DeltaTimeNormaliser(List<TaikoDifficultyHitObject> hitObjects, double tolerance)
        {
            this.hitObjects = hitObjects;
            NormalizedDeltaTime = getNormalisedDeltaTime(tolerance);
        }

        private Dictionary<TaikoDifficultyHitObject, double> getNormalisedDeltaTime(double tolerance)
        {
            var originalDeltas = hitObjects.Select(n => n.DeltaTime).Distinct().OrderBy(x => x).ToList();

            var clusters = binByInitialValueProximity(originalDeltas, tolerance);
            var deltaMap = new Dictionary<double, double>();

            foreach (var cluster in clusters)
            {
                double normalizedValue = Median(cluster); // Or use Median if preferred
                foreach (double val in cluster)
                {
                    deltaMap[val] = normalizedValue; // Creates the mapping for one delta time value to it's normalised counterpart.
                }
            }

            var result = new Dictionary<TaikoDifficultyHitObject, double>();
            foreach (var obj in hitObjects)
            {
                if (deltaMap.TryGetValue(obj.DeltaTime, out double normalized))
                {
                    result[obj] = normalized;
                }
                else
                {
                    result[obj] = obj.DeltaTime;
                }
            }
            return result;
        }

        /// <summary>
        /// Selects values into bins within the given tolerance from the initial value.
        /// </summary>
        /// <param name="sortedValues">List of sorted values.</param>
        /// <param name="tolerance">Maximum allowed deviation.</param>
        /// <returns>A list of binned, grouped values.</returns>
        private List<List<double>> binByInitialValueProximity(List<double> sortedValues, double tolerance)
        {
            var bins = new List<List<double>>();
            List<double>? current = null;

            foreach (double value in sortedValues)
            {
                // Add to the current group if within margin of errorMore actions
                if (current != null && Math.Abs(value - current[0]) <= tolerance)
                {
                    current.Add(value);
                    continue;
                }

                // Otherwise begin a new group
                current = new List<double> { value };
                bins.Add(current);

            }
            return bins;
        }

        /// <summary>
        /// Selects values into bins within the given tolerance from the last value.
        /// This allows bins to "stretch" as long as consecutive values remain within the allowed range.
        /// </summary>
        /// <param name="sortedValues">List of sorted values.</param>
        /// <param name="tolerance">Maximum allowed deviation.</param>
        /// <returns>A list of binned, grouped values.</returns>
        private List<List<double>> binByIncrementalProximity(List<double> sortedValues, double tolerance)
        {
            var bins = new List<List<double>>();
            List<double>? current = null;

            foreach (double value in sortedValues)
            {
                // Add to current bin if value is within tolerance of the last element
                if (current != null && Math.Abs(value - current.Last()) <= tolerance)
                {
                    current.Add(value);
                    continue;
                }

                // Otherwise begin a new group
                current = new List<double> { value };
                bins.Add(current);

            }
            return bins;
        }

        public static double Median(List<double> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            int count = sorted.Count;
            if (count % 2 == 1)
            {
                return sorted[count / 2];
            }
            else
            {
                return (sorted[(count / 2) - 1] + sorted[count / 2]) / 2.0;
            }
        }
    }
}

