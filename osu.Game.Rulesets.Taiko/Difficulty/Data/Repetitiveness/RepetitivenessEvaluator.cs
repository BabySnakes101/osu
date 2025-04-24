// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTabletDriver.Native.Windows.Input;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using static osu.Game.Rulesets.Taiko.Difficulty.Utils.NoteEntropyUtils;
using Vulkan.Xlib;
using osu.Game.Rulesets.UI;
using System.Diagnostics;

namespace osu.Game.Rulesets.Taiko.Difficulty.Data.Repetitiveness
{
    public class RepetitivenessEvaluator
    {
        private IReadOnlyList<TaikoDifficultyHitObject> notes;
        private string notesAsText;
        public Dictionary<TaikoDifficultyHitObject, Dictionary<int, RepeatInfo>> noteWindowRepetition;
        public RepetitivenessEvaluator(IReadOnlyList<TaikoDifficultyHitObject> notes)
        {
            noteWindowRepetition = new Dictionary<TaikoDifficultyHitObject, Dictionary<int, RepeatInfo>>();

            this.notes = notes;
            notesAsText = "";
            foreach (var note in notes)
            {
                notesAsText += note.IsKat ? "k" : "d";
                noteWindowRepetition.Add(note, new Dictionary<int, RepeatInfo>());
                //noteWindowRepetition.Add(note, new List<int>());
            }
        }

        public void DecideLayers()
        {
            if (notes.Count < 2)
                return;
            EvaluateLayer(1, 5);
            EvaluateLayer(2, 3);

            for (int i = 3; i < 9; i++)
            {
                EvaluateLayer(i, 2);
            }
        }

        public void EvaluateLayer(int layerWindowSize, int requiredRepeats)
        {
            if (layerWindowSize < 1 || notes.Count < layerWindowSize * 2)
                //throw new ArgumentOutOfRangeException();
                return;

            int i = 0;
            while (i <= notes.Count - layerWindowSize * 2)
            {
                string pattern = notesAsText.Substring(i, layerWindowSize);
                int repeatCount = 1;
                int j = i + layerWindowSize;

                while (j + layerWindowSize <= notes.Count &&
                       notesAsText.Substring(j, layerWindowSize) == pattern)
                {
                    repeatCount++;
                    j += layerWindowSize;
                }

                if (repeatCount >= requiredRepeats)
                {
                    var info = new RepeatInfo
                    {
                        Start = i,
                        Length = layerWindowSize,
                        RepeatCount = repeatCount
                    };

                    for (int k = i; k < i + layerWindowSize * repeatCount; k++)
                    {
                        noteWindowRepetition[notes[k]].Add(layerWindowSize, info);
                    }

                    //matches.Add(info);
                    i += layerWindowSize * repeatCount;
                }
                else
                    i++;
            }

        }

        public string FormatWithBrackets()
        {
            // Flatten unique patterns
            var allPatterns = noteWindowRepetition
                .SelectMany(kv => kv.Value.Values)
                .Distinct()
                .OrderBy(p => p.Start)
                .ThenBy(p => p.Length)
                .ToList();

            var result = "";
            int index = 0;

            foreach (var pattern in allPatterns)
            {
                while (index < pattern.Start)
                    result += notesAsText[index++];

                for (int r = 0; r < pattern.RepeatCount; r++)
                {
                    result += "[";
                    result += notesAsText.Substring(pattern.Start + r * pattern.Length, pattern.Length);
                    result += "]";
                }

                index = pattern.Start + pattern.Length * pattern.RepeatCount;
            }

            while (index < notesAsText.Length)
                result += notesAsText[index++];

            return result;
        }
    }
}
