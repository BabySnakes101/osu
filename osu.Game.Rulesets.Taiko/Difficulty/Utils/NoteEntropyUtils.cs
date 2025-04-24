// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    public static class NoteEntropyUtils
    {

        public static double Entropy(IEnumerable<TaikoDifficultyHitObject> taikoHitObjects)
        {
            byte[] data = ConvertToBinary(taikoHitObjects);
            int order = 3;
            Dictionary<string, int> patternCounts = new Dictionary<string, int>();
            int totalPatterns = data.Length - order + 1;

            for (int i = 0; i < totalPatterns; i++)
            {
                byte[] window = data.Skip(i).Take(order).ToArray();
                int[] pattern = getPermutationPattern(window);
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

        private static int[] getPermutationPattern(byte[] window)
        {
            // Sort indices by values in window (preserving order in case of ties)
            return window
                .Select((value, index) => new { Value = value, Index = index })
                .OrderBy(x => x.Value)
                .ThenBy(x => x.Index)
                .Select(x => x.Index)
                .ToArray();
        }

        public static byte[] ConvertToBinary(IEnumerable<TaikoDifficultyHitObject> taikoHitObjects)
        {
            return taikoHitObjects.Select(c => c.IsKat is true ? (byte)1 : (byte)0).ToArray();
        }

        public static double Compression(IEnumerable<TaikoDifficultyHitObject> taikoDifficultyHitObjects)
        {
            byte[] data = ConvertToBinary(taikoDifficultyHitObjects); // Or .UTF8 if needed

            byte[] compressed;
            using (var outStream = new MemoryStream())
            {
                using (var deflate = new DeflateStream(outStream, CompressionMode.Compress, leaveOpen: true))
                {
                    deflate.Write(data, 0, data.Length);
                } // auto-flushed here
                compressed = outStream.ToArray();
            }

            double compressionRatio = (double)compressed.Length / data.Length;
            return compressionRatio;
        }

        public static void Repetitiveness(List<TaikoDifficultyHitObject> taikoDifficultyHitObjects)
        {
            Dictionary<int, List<TaikoDifficultyHitObject>> dict = new Dictionary<int, List<TaikoDifficultyHitObject>>();
            for (int slidingWindow = 1; slidingWindow < Math.Min(taikoDifficultyHitObjects.Count, 15); slidingWindow++)
            {
                var isKat = taikoDifficultyHitObjects[0].IsKat;
                for (int i = 1; i < taikoDifficultyHitObjects.Count; i++)
                {
                    //if()
                }
            }
        }
        public static double[] RunLengthScore(IEnumerable<TaikoDifficultyHitObject> taikoDifficultyHitObjects)
        {
            byte[] data = ConvertToBinary(taikoDifficultyHitObjects); // Or .UTF8 if needed
            // Convert byte[] to bit-level string for simplicity
            var bits = new List<char>(data.Length * 8);
            foreach (var b in data)
            {
                string bitString = Convert.ToString(b, 2).PadLeft(8, '0');
                bits.AddRange(bitString);
            }

            int len = bits.Count;
            double[] scores = new double[len];

            int i = 0;
            while (i < len)
            {
                char bit = bits[i];
                int j = i;
                while (j < len && bits[j] == bit) j++;

                int runLength = j - i;
                for (int k = i; k < j; k++)
                {
                    scores[k] = runLength;
                }

                i = j;
            }

            return scores;
        }

        public class RepeatInfo
        {
            public int Start;
            public int Length;
            public int RepeatCount;

            public override string? ToString()
            {
                return Start.ToString() + " " + Length.ToString() + " " + RepeatCount.ToString();
            }
        }

        public static List<RepeatInfo> AnalyzeRepeats(IEnumerable<TaikoDifficultyHitObject> notes)
        {
            string input = "";
            foreach (var note in notes)
            {
                input += note.IsKat ? "k" : "d";
            }

            int n = input.Length;
            var matches = new List<RepeatInfo>();
            Dictionary<int, bool[]> dict = new Dictionary<int, bool[]>();

            for (int window = 1; window <= n; window++)
            {
                dict[window] = new bool[n];
            }

            // Define minimum repeat thresholds for each window size
            Dictionary<int, int> minRepeats = new Dictionary<int, int>
{
    { 1, 5 },  // Only allow length-1 patterns with 5+ repeats
    { 2, 3 },  // Only allow length-2 patterns with 3+ repeats
    // Length 3+ will default to 2 repeats
};

            for (int window = 1; window <= n / 2; window++)
            {
                for (int i = 0; i <= n - window * 2; i++)
                {
                    if (dict[window][i]) continue;

                    string pattern = input.Substring(i, window);
                    int repeatCount = 1;
                    int j = i + window;

                    while (j + window <= n && input.Substring(j, window) == pattern)
                    {
                        repeatCount++;
                        j += window;
                    }

                    int requiredRepeats = minRepeats.ContainsKey(window) ? minRepeats[window] : 2;
                    if (repeatCount >= requiredRepeats)
                    {
                        // Check for overlap with any existing pattern
                        bool overlaps = false;
                        for (int k = i; k < i + window * repeatCount; k++)
                        {
                            if (dict[window][k])
                            {
                                overlaps = true;
                                break;
                            }
                        }

                        if (overlaps) continue;

                        // Mark as used
                        for (int k = i; k < i + window * repeatCount; k++)
                            dict[window][k] = true;

                        matches.Add(new RepeatInfo
                        {
                            Start = i,
                            Length = window,
                            RepeatCount = repeatCount
                        });

                        i = j - 1;
                    }
                }
            }

            return matches;
        }

        public static string FormatWithBrackets(string input, List<RepeatInfo> patterns)
        {
            var result = "";
            int index = 0;

            foreach (var pattern in patterns)
            {
                while (index < pattern.Start)
                    result += input[index++];

                for (int r = 0; r < pattern.RepeatCount; r++)
                {
                    result += "[";
                    result += input.Substring(pattern.Start + r * pattern.Length, pattern.Length);
                    result += "]";
                }

                index = pattern.Start + pattern.Length * pattern.RepeatCount;
            }

            while (index < input.Length)
                result += input[index++];

            return result;
        }

        public static void PrintPatternLayers(string input, List<RepeatInfo> patterns)
        {
            Console.WriteLine("Input:      " + input);
            int n = input.Length;

            var groups = patterns.GroupBy(p => p.Length).OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                int windowSize = group.Key;
                char[] layerLine = Enumerable.Repeat(' ', n).ToArray();

                foreach (var match in group)
                {
                    for (int r = 0; r < match.RepeatCount; r++)
                    {
                        int start = match.Start + r * windowSize;

                        if (start >= n) continue;

                        if (windowSize == 1)
                        {
                            // Mark single chars as 'x'
                            layerLine[start] = 'x';
                        }
                        else
                        {
                            if (start < n)
                                layerLine[start] = '[';

                            for (int j = 1; j < windowSize - 1; j++)
                            {
                                if (start + j < n)
                                    layerLine[start + j] = '-';
                            }

                            if (start + windowSize - 1 < n)
                                layerLine[start + windowSize - 1] = ']';
                        }
                    }
                }

                Console.WriteLine($"Layer ({windowSize,2}): {new string(layerLine)}");
            }
        }

        public static string FormatWithBracketsByNote(string input, Dictionary<TaikoDifficultyHitObject, Dictionary<int, RepeatInfo>> repetitions, IReadOnlyList<TaikoDifficultyHitObject> notes)
        {
            var result = new StringBuilder();

            bool inBracket = false;
            RepeatInfo? currentPattern = null;

            for (int i = 0; i < notes.Count; i++)
            {
                var note = notes[i];

                // Get all patterns this note is in (sorted by window size, small to large)
                if (repetitions.TryGetValue(note, out var windows) && windows.Count > 0)
                {
                    var smallest = windows.OrderBy(w => w.Key).First().Value;

                    if (currentPattern == null || smallest != currentPattern)
                    {
                        if (inBracket) result.Append(']');
                        result.Append('[');
                        inBracket = true;
                        currentPattern = smallest;
                    }
                }
                else
                {
                    if (inBracket)
                    {
                        result.Append(']');
                        inBracket = false;
                        currentPattern = null;
                    }
                }

                result.Append(input[i]);
            }

            if (inBracket)
                result.Append(']');

            return result.ToString();
        }
        public static void PrintPatternLayersByNote(string input, IReadOnlyList<TaikoDifficultyHitObject> notes,
    Dictionary<TaikoDifficultyHitObject, Dictionary<int, RepeatInfo>> repetitions)
        {
            Console.WriteLine("Input:      " + input);
            int n = notes.Count;

            // Collect all unique window sizes
            var allWindowSizes = repetitions
                .SelectMany(kvp => kvp.Value.Keys)
                .Distinct()
                .OrderBy(w => w)
                .ToList();

            foreach (var windowSize in allWindowSizes)
            {
                char[] layerLine = Enumerable.Repeat(' ', n).ToArray();

                for (int i = 0; i < n; i++)
                {
                    var note = notes[i];

                    if (repetitions.TryGetValue(note, out var windowMap) &&
                        windowMap.TryGetValue(windowSize, out var info))
                    {
                        int relativeIndex = i - info.Start;
                        if (relativeIndex % windowSize == 0)
                            layerLine[i] = '[';
                        else if (relativeIndex % windowSize == windowSize - 1)
                            layerLine[i] = ']';
                        else
                            layerLine[i] = '-';
                    }
                }

                Console.WriteLine($"Layer ({windowSize,2}): {new string(layerLine)}");
            }
        }

        public static double CalculateTimeWeightedEntropy(TaikoDifficultyHitObject[] notes)
        {
            if (notes.Length == 0)
                return 0;

            int countKat = notes.Count(e => e.IsKat);
            int countDon = notes.Length - countKat;

            double entropy = 0;
            double totalTimeWeight = 0;

            foreach (var e in notes)
            {
                double timeWeight = 1.0 / (e.DeltaTime + 1e-6); // Avoid divide by zero
                totalTimeWeight += timeWeight;

                double p = e.IsKat
                    ? (double)countKat / notes.Length
                    : (double)countDon / notes.Length;

                if (p > 0)
                    entropy += -p * Math.Log(p, 2) * timeWeight;
            }

            return entropy / totalTimeWeight; // Normalize
        }

        public static double CalculateStandardEntropy(TaikoDifficultyHitObject[] notes)
        {
            if (notes.Length == 0)
                return 0;

            int countKat = notes.Count(e => e.IsKat);
            int countDon = notes.Length - countKat;

            double entropy = 0;

            // Probabilities
            double pKat = (double)countKat / notes.Length;
            double pDon = (double)countDon / notes.Length;

            if (pKat > 0)
                entropy += -pKat * Math.Log(pKat, 2);
            if (pDon > 0)
                entropy += -pDon * Math.Log(pDon, 2);

            return entropy;
        }

        public static double CalculateWeightedPermutationEntropy(List<TaikoDifficultyHitObject> events, int order)
        {
            if (events.Count < order)
                return 0;

            var patternWeights = new Dictionary<string, double>();
            double totalWeight = 0;
            double epsilon = 1e-6;

            for (int i = 0; i <= events.Count - order; i++)
            {
                var window = events.Skip(i).Take(order).ToList();
                var values = window.Select(e => e.IsKat ? 1.0 : 0.0).ToList();
                string pattern = GetPermutationPattern(values);

                // Weight by time density (inverse of average delta)
                double avgDelta = window.Average(e => e.DeltaTime);
                double weight = Math.Pow(1.0 / (avgDelta + epsilon), 33);


                if (!patternWeights.ContainsKey(pattern))
                    patternWeights[pattern] = 0;
                patternWeights[pattern] += weight;
                totalWeight += weight;
            }

            // Calculate entropy
            double entropy = 0;
            foreach (var kvp in patternWeights)
            {
                double p = kvp.Value / totalWeight;
                entropy += -p * Math.Log(p, 2);
            }

            return entropy;
        }
        public static string GetPermutationPattern(List<double> values)
        {
            return string.Join(",", values
                .Select((v, i) => new { Value = v, Index = i })
                .OrderBy(x => x.Value)
                .Select(x => x.Index));
        }
    }
}

