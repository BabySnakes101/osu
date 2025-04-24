// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Rulesets.Taiko.Difficulty.Data;
using osu.Game.Rulesets.Taiko.Difficulty.Data.Repetitiveness;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    /// <summary>
    /// Temporary debug utils for patterns.
    /// </summary>
    public static class PatternDebugUtils
    {

        public static void PrintPatterns(PatternInterpreter patternInterpreter, List<TaikoDifficultyHitObject> allHitObjects)
        {
            foreach (Pattern pattern in patternInterpreter.PatternList)
            {
                string patternInfo = "";
                foreach (TaikoDifficultyHitObject note in pattern.PatternObjects)
                {
                    patternInfo += note.IsKat ? "k" : "d";
                }
                Console.WriteLine($"[{patternInfo}]{(pattern.AverageDeltaTime is not null ? $" ({pattern.AverageDeltaTime} ms average)" : "")}");
                Console.WriteLine($"Entropy: {NoteEntropyUtils.Entropy(pattern.PatternObjects)}");
                Console.WriteLine($"Compression: {NoteEntropyUtils.Compression(pattern.PatternObjects)}");

                Console.WriteLine($"RepetitiveTest: {NoteEntropyUtils.FormatWithBrackets(patternInfo, NoteEntropyUtils.AnalyzeRepeats(pattern.PatternObjects))}");
                Console.WriteLine($"Test: {NoteEntropyUtils.AnalyzeRepeats(pattern.PatternObjects).ToString()}");

                NoteEntropyUtils.PrintPatternLayers(patternInfo, NoteEntropyUtils.AnalyzeRepeats(pattern.PatternObjects));
                var RepetitivenessEvaluator = new RepetitivenessEvaluator(pattern.PatternObjects);
                RepetitivenessEvaluator.DecideLayers();
                Console.WriteLine(NoteEntropyUtils.FormatWithBracketsByNote(patternInfo, RepetitivenessEvaluator.noteWindowRepetition, pattern.PatternObjects));
                NoteEntropyUtils.PrintPatternLayersByNote(patternInfo, pattern.PatternObjects, RepetitivenessEvaluator.noteWindowRepetition);

                //Console.WriteLine("Shannon Weighted Entropy: " + NoteEntropyUtils.CalculateTimeWeightedEntropy(allHitObjects.ToArray()));
                //Console.WriteLine("Shannon Entropy: " + NoteEntropyUtils.CalculateStandardEntropy(allHitObjects.ToArray()));

                /*foreach (var item in NoteEntropyUtils.AnalyzeRepeats(pattern.PatternObjects))
                {
                    Console.WriteLine($"Test: {item.ToString()}");

                }


            /*foreach (var item in NoteEntropyUtils.RunLengthScore(pattern.PatternObjects))
            {
                Console.WriteLine($"Test: {item}");

            }
            */
            }
            Console.WriteLine($"Entropy: {NoteEntropyUtils.Entropy(allHitObjects)}");
            Console.WriteLine("Weighted better entropy" + NoteEntropyUtils.CalculateWeightedPermutationEntropy(allHitObjects,7));
            var RepetitivenessEvaluator2 = new RepetitivenessEvaluator(allHitObjects);

            var scores = RepetitionScorer.ComputeRepetitionScores(allHitObjects, RepetitivenessEvaluator2.noteWindowRepetition, targetWindowSize: 1);

            foreach (var note in allHitObjects)
            {
                double score = scores.TryGetValue(note, out var s) ? s : 0.0;
                Console.WriteLine($"{note}: Repetition Score = {score:F2}");
            }
        }
    }
}
