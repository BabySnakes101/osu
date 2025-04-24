// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.Taiko.Difficulty.Data.Repetitiveness
{
    public class RepetitivenessLayer
    {
        public readonly int WindowSize;
        public readonly int RepetitionCount;
        public readonly int Length;

        public RepetitivenessLayer(int windowSize, int repetitionCount, int length)
        {
            WindowSize = windowSize;
            RepetitionCount = repetitionCount;
            Length = length;
        }
    }
}
