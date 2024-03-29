// MIT License
// 
// Copyright (c) 2016 Bismur Studios Ltd.
// Copyright (c) 2016 Ioannis Giagkiozis
//
// This file is part of PCGSharp.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all copies or substantial 
//  portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
//  LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
//  NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
//  SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// This file is based on PCG C++, the original has the following license: 
/*
 * PCG Random Number Generation for C++
 *
 * Copyright 2014 Melissa O'Neill <oneill@pcg-random.org>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * For additional information about the PCG random number generation scheme,
 * including its license and other licensing options, visit
 *
 *     http://www.pcg-random.org
 */

using System;

// Note, you will see that API is duplicated in PCG and PCGExtended, this is not by accident or 
// neglect. It would be "cleaner" to have a base class and just implement the core of the generator
// in derived classes, however, this has a performance cost which for random number generators, in my
// view, is too high. That said, there is still some room for performance optimisation.

namespace RogueEntity.Core.Infrastructure.Randomness.PCGSharp
{
    /// <summary>
    /// PCG (Permuted Congruential Generator) Extended is a C# port from C++ of a part of the extended PCG family of
    /// generators presented in "PCG: A Family of Simple Fast Space-Efficient Statistically Good
    /// Algorithms for Random Number Generation" by Melissa E. O'Neill. The code follows closely the one 
    /// made available by O'Neill at her site: http://www.pcg-random.org/using-pcg-cpp.html 
    /// To understand how exactly this generator works read this:
    /// http://www.pcg-random.org/pdf/toms-oneill-pcg-family-v1.02.pdf its a fun read, enjoy!
    /// 
    /// The most important difference between this version and PCG is that the generators produced by this
    /// class can have extremely larger periods (extremely!) and have configurable k-dimensional equidistribution.
    /// All this at a very small speed penalty! (Upto 1024-dimensionally equidistributed generators perform faster
    /// than System.Random, at least on the machines I have access to).
    /// 
    /// Fun fact, the period of PCG Extended for _table_mask = 14 is 2^(524352) which is more than 10^(157846). To put this 
    /// in context, the number particles in the known Universe is about 10^(80 to 81ish) if we also count photons
    /// that number goes up (with a lot of effort) to 10^(90ish). Now, there are estimates that say that 
    /// visible matter (counting photons, energy-matter same thing...) accounts for 5% of the matter in the
    /// visible Universe, so if these estimates are correct the total visible+invisible particles/(energy particles)
    /// in the visible Universe would approximately be 10^(91). That means that this generator has a period
    /// that is 157755 orders of magnitude larger than the number of all particles in the visible Universe... 
    /// Also, if the universe had an equal number of particles (to the period of PCG 14), that would mean that
    /// its mean density (at its current volume) would be so high that the entire universe would collapse unto
    /// its weight into a black hole.
    /// </summary>
    public sealed class PcgExtended
    {
        readonly ulong tableMask;
        readonly ulong tickMask;
        readonly int tablePow2;
        readonly int tableSize;
        readonly uint[] data;
        readonly ulong increment;
        ulong state;

        // This shifted to the left and or'ed with 1ul results in the default increment.
        const ulong ShiftedIncrement = 721347520444481703ul;
        const ulong Multiplier = 6364136223846793005ul;
        const uint PcgMultiplier = 747796405u;
        const uint PcgIncrement = 2891336453u;
        const uint McgMultiplier = 277803737u;
        const uint McgUnmultiplier = 2897767785u;

        // 1 / (uint.MaxValue + 1)
        const double ToDouble01 = 1.0 / 4294967296.0;

        // This attribute ensures that every thread will get its own instance of PCG.
        // An alternative, since PCG supports streams, is to use a different stream per
        // thread. 
        [ThreadStatic]
        static PcgExtended? defaultInstance;

        /// <summary>
        /// Default instance.
        /// </summary>
        public static PcgExtended Default
        {
            get
            {
                return defaultInstance ??= new PcgExtended(PCGSeed.GuidBasedSeed());
            }
        }

        public int Next()
        {
            uint result = NextUInt();
            return (int)(result >> 1);
        }

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentException("Max Exclusive must be positive");

            uint uMaxExclusive = (uint)(maxExclusive);
            uint threshold = (uint)(-uMaxExclusive) % uMaxExclusive;

            while (true)
            {
                uint result = NextUInt();
                if (result >= threshold)
                    return (int)(result % uMaxExclusive);
            }
        }

        public int Next(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new ArgumentException("MaxExclusive cannot be smaller or equal to MinInclusive");

            uint uMaxExclusive = unchecked((uint)(maxExclusive - minInclusive));
            uint threshold = (uint)(-uMaxExclusive) % uMaxExclusive;

            while (true)
            {
                uint result = NextUInt();
                if (result >= threshold)
                    return (int)(unchecked((result % uMaxExclusive) + minInclusive));
            }
        }

        public int[] NextInts(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new int[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = Next();
            }

            return resultA;
        }

        public int[] NextInts(int count, int maxExclusive)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");
            if (maxExclusive <= 0)
                throw new ArgumentException("Max Exclusive must be positive");

            var resultA = new int[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = Next(maxExclusive);
            }

            return resultA;
        }

        public int[] NextInts(int count, int minInclusive, int maxExclusive)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new int[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = Next(minInclusive, maxExclusive);
            }

            return resultA;
        }

        public uint NextUInt()
        {
            var index = state & tableMask;
            bool tick = (state & tickMask) == 0ul;
            if (tick)
            {
                bool carry = false;
                for (int i = 0; i < tableSize; i++)
                {
                    if (carry)
                        carry = ExternalStep(ref data[i], i + 1);

                    bool carry2 = ExternalStep(ref data[i], i + 1);
                    carry = carry || carry2;
                }
            }

            uint rhs = data[index];

            ulong oldState = state;
            state = unchecked(state * Multiplier + increment);
            int rot = (int)(oldState >> 59);
            uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            uint lhs = (xorShifted >> rot) | (xorShifted << ((-rot) & 31));

            uint result = lhs ^ rhs;
            return result;
        }

        public uint NextUInt(uint maxExclusive)
        {
            uint threshold = (uint)(-maxExclusive) % maxExclusive;

            while (true)
            {
                uint result = NextUInt();
                if (result >= threshold)
                    return result % maxExclusive;
            }
        }

        public uint NextUInt(uint minInclusive, uint maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new ArgumentException();

            uint diff = (maxExclusive - minInclusive);
            uint threshold = (uint)(-diff) % diff;

            while (true)
            {
                uint result = NextUInt();
                if (result >= threshold)
                    return (result % diff) + minInclusive;
            }
        }

        public uint[] NextUInts(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new uint[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextUInt();
            }

            return resultA;
        }

        public uint[] NextUInts(int count, uint maxExclusive)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new uint[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextUInt(maxExclusive);
            }

            return resultA;
        }

        public uint[] NextUInts(int count, uint minInclusive, uint maxExclusive)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new uint[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextUInt(minInclusive, maxExclusive);
            }

            return resultA;
        }

        public float NextFloat()
        {
            return (float)(NextUInt() * ToDouble01);
        }

        public float NextFloat(float maxInclusive)
        {
            if (maxInclusive <= 0)
                throw new ArgumentException("Max must be larger than 0");

            return (float)(NextUInt() * ToDouble01) * maxInclusive;
        }

        public float NextFloat(float minInclusive, float maxInclusive)
        {
            if (maxInclusive < minInclusive)
                throw new ArgumentException("Max must be larger than min");

            return (float)(NextUInt() * ToDouble01) * (maxInclusive - minInclusive) + minInclusive;
        }

        public float[] NextFloats(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new float[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextFloat();
            }

            return resultA;
        }

        public float[] NextFloats(int count, float maxInclusive)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new float[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextFloat(maxInclusive);
            }

            return resultA;
        }

        public float[] NextFloats(int count, float minInclusive, float maxInclusive)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new float[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextFloat(minInclusive, maxInclusive);
            }

            return resultA;
        }

        public double NextDouble()
        {
            return (NextUInt() * ToDouble01);
        }

        public double NextDouble(double maxInclusive)
        {
            if (maxInclusive <= 0)
                throw new ArgumentException("Max must be larger than 0");

            return (NextUInt() * ToDouble01) * maxInclusive;
        }

        public double NextDouble(double minInclusive, double maxInclusive)
        {
            if (maxInclusive < minInclusive)
                throw new ArgumentException("Max must be larger than min");

            return (NextUInt() * ToDouble01) * (maxInclusive - minInclusive) + minInclusive;
        }

        public double[] NextDoubles(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new double[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextDouble();
            }

            return resultA;
        }

        public double[] NextDoubles(int count, double maxInclusive)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new double[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextDouble(maxInclusive);
            }

            return resultA;
        }

        public double[] NextDoubles(int count, double minInclusive, double maxInclusive)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new double[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextDouble(minInclusive, maxInclusive);
            }

            return resultA;
        }

        public byte NextByte()
        {
            uint result = NextUInt();
            return (byte)(result % 256);
        }

        public byte[] NextBytes(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new byte[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextByte();
            }

            return resultA;
        }

        public bool NextBool()
        {
            uint result = NextUInt();
            return ((result % 2) == 1);
        }

        public bool[] NextBools(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Zero count");

            var resultA = new bool[count];
            for (int i = 0; i < count; i++)
            {
                resultA[i] = NextBool();
            }

            return resultA;
        }

        public int Equidistribution()
        {
            return 1 << tablePow2;
        }

        public int EquidistributionPow2()
        {
            return tablePow2;
        }

        public int PeriodPow2()
        {
            return (1 << tablePow2) * 32 + 64;
        }

        public ulong CurrentStream()
        {
            return increment >> 1;
        }

        public PcgExtended() : this(PCGSeed.GuidBasedSeed())
        {
        }

        public PcgExtended(int seed) : this((ulong)seed)
        {
            
        }
        
        public PcgExtended(int seed, int sequence, int tablePow2 = 10, int advancePow2 = 16) : this((ulong)seed, (ulong)sequence, tablePow2, advancePow2)
        {
        }

        public PcgExtended(ulong seed, ulong sequence = ShiftedIncrement, int tablePow2 = 10, int advancePow2 = 16)
        {
            this.tablePow2 = tablePow2;
            increment = (sequence << 1) | 1;
            
            state = seed + increment;
            state = Bump(state);

            tableSize = (1 << tablePow2);
            data = new uint[tableSize];
            // The correct way to initialize this is to use a seed sequence, but this 
            // is more convenient. 
            uint xdiff = InternalNext() - InternalNext();
            for (int i = 0; i < tableSize; i++)
            {
                data[i] = (InternalNext() ^ xdiff);
            }

            tableMask = (1ul << tablePow2) - 1ul;
            tickMask = (1ul << advancePow2) - 1ul;
        }

        uint InternalNext()
        {
            ulong oldState = state;
            // Overflow is part of the design.
            state = unchecked(oldState * Multiplier + increment);
            uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rot = (int)(oldState >> 59);
            return ((xorShifted >> rot) | (xorShifted << ((-rot) & 31)));
        }

        ulong Bump(ulong nextState)
        {
            return (nextState * Multiplier + increment);
        }

        static bool ExternalStep(ref uint randval, int i)
        {
            uint localState = UnOutput(randval);
            localState = unchecked(localState * PcgMultiplier + PcgIncrement + (uint)i * 2u);

            int rshift = (int)(localState >> 28) + 4;
            localState ^= (localState >> rshift);
            localState *= McgMultiplier;
            uint result = localState ^ (localState >> 22);
            randval = result;

            return result == 0u;
        }

        static uint UnOutput(uint @internal)
        {
            // unxorshift
            @internal ^= (@internal >> 22);
            @internal *= McgUnmultiplier;
            int rshift = (int)(@internal >> 28);
            @internal = UnXorShift(@internal, 32, 4 + rshift);
            return @internal;
        }

        static uint UnXorShift(uint x, int bits, int shift)
        {
            if (2 * shift >= bits)
                return x ^ (x >> shift);

            uint lowmask1 = (1u << (bits - 2 * shift)) - 1u;
            uint highmask1 = ~lowmask1;
            uint top1 = x;
            uint bottom1 = x & lowmask1;
            top1 ^= top1 >> shift;
            top1 &= highmask1;
            x = top1 | bottom1;
            uint lowmask2 = (1u << (bits - shift)) - 1u;
            uint bottom2 = x & lowmask2;
            bottom2 = UnXorShift(bottom2, bits - shift, shift);
            bottom2 &= lowmask1;
            return top1 | bottom2;
        }
    }
}