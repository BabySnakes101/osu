// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    public class DeltaTimeNormalizer
    {
        private List<TaikoDifficultyHitObject> hitObjects;
        public DeltaTimeNormalizer(List<TaikoDifficultyHitObject> hitObjects)
        {
            this.hitObjects = hitObjects;
        }

        public Dictionary<TaikoDifficultyHitObject, double> GetNormalizedDeltaTime(double tolerance)
        {

            var deltaMap = new Dictionary<double, double>();
            var originalDeltas = hitObjects.Select(n => n.DeltaTime).Distinct().OrderBy(x => x).ToList();
            var clusters = new List<List<double>>();

            List<double> currentCluster = new List<double> { originalDeltas[0] };
            double clusterSeed = originalDeltas[0];

            for (int i = 1; i < originalDeltas.Count; i++)
            {
                if (Math.Abs(originalDeltas[i] - clusterSeed) <= tolerance)
                {
                    currentCluster.Add(originalDeltas[i]);
                }
                else
                {
                    clusters.Add(new List<double>(currentCluster));
                    currentCluster = new List<double> { originalDeltas[i] };
                    clusterSeed = originalDeltas[i];
                }
            }
            clusters.Add(currentCluster);

            foreach (var cluster in clusters)
            {
                double normalizedValue = Median(cluster); // Or use Median if preferred
                foreach (var val in cluster)
                {
                    deltaMap[val] = normalizedValue;
                }
            }

            // Step 3: Build final mapping from hit object to normalized deltaTime
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
        public void ModNormalizedDeltaTime(double tolerance)
        {
            var deltaMap = new Dictionary<double, double>();
            var originalDeltas = hitObjects.Select(n => n.DeltaTime).Distinct().OrderBy(x => x).ToList();
            var clusters = new List<List<double>>();

            List<double> currentCluster = new List<double> { originalDeltas[0] };
            double clusterSeed = originalDeltas[0];

            for (int i = 1; i < originalDeltas.Count; i++)
            {
                if (Math.Abs(originalDeltas[i] - clusterSeed) <= tolerance)
                {
                    currentCluster.Add(originalDeltas[i]);
                }
                else
                {
                    clusters.Add(new List<double>(currentCluster));
                    currentCluster = new List<double> { originalDeltas[i] };
                    clusterSeed = originalDeltas[i];
                }
            }
            clusters.Add(currentCluster);

            // Map each original deltaTime to its cluster's normalized value (average or median)
            foreach (var cluster in clusters)
            {
                double normalizedValue = Median(cluster); // You could use avg here
                foreach (var val in cluster)
                {
                    deltaMap[val] = normalizedValue;
                }
            }

            // Modify the original hitObjects directly
            foreach (var obj in hitObjects)
            {
                if (deltaMap.TryGetValue(obj.DeltaTime, out double normalized))
                {
                    obj.DeltaTime = normalized;
                }
            }
        }

        public void PrintDeltaTimeDeviations(double tolerance)
        {
            var normalizedMap = GetNormalizedDeltaTime(tolerance);

            foreach (var obj in hitObjects)
            {
                if (normalizedMap.TryGetValue(obj, out double normalized))
                {
                    double original = obj.DeltaTime;
                    double deviation = normalized - original;

                    Console.WriteLine($"Object: {obj}, Original: {original}, Normalized: {normalized}, Deviation: {deviation:+0.##;-0.##;0}");
                }
                else
                {
                    Console.WriteLine($"Object: {obj}, DeltaTime not found in normalized map.");
                }
            }
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
