﻿/* 
 
 Adapted from Unity MersenneTwister from
 https://github.com/tucano/UnityMersenneTwister/blob/master/Assets/Scripts/UnityMersenneTwister.cs

 Unity MersenneTwister
 
 Copyright 2011 Davide Rambaldi (davide.rambaldi@gmail.com)
 
 Adapted from: http://archive.msdn.microsoft.com/MersenneTwister

 and, of course:


   A C-program for MT19937, with initialization improved 2002/2/10.
   Coded by Takuji Nishimura and Makoto Matsumoto.
   This is a faster version by taking Shawn Cokus's optimization,
   Matthe Bellew's simplification, Isaku Wada's real version.

   Before using, initialize the state by using init_genrand(seed) 
   or init_by_array(init_key, key_length).

   Copyright (C) 1997 - 2002, Makoto Matsumoto and Takuji Nishimura,
   All rights reserved.                          

   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:

     1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.

     2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.

     3. The names of its contributors may not be used to endorse or promote 
        products derived from this software without specific prior written 
        permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
   A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


   Any feedback is very welcome.
   http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html
   email: m-mat @ math.sci.hiroshima-u.ac.jp (remove space)
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EvolutionaryStrategyEngine.Models;

namespace EvolutionaryStrategyEngine.Utils
{

    /// <summary>
    /// Generates pseudo-random numbers using the Mersenne Twister algorithm.
    /// See http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html and
    /// http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html for details on the algorithm.
    /// </summary>
    public class MersenneTwister : Random
    {
        [ThreadStatic]
        private static MersenneTwister _instance;

        public static MersenneTwister Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = Arguments.HasKey("Seed") ? new MersenneTwister(Arguments.Get<int>("Seed")) : new MersenneTwister(Defaults.Seed);
                Accord.Math.Random.Generator.Seed = Defaults.Seed;

                return _instance;
            }
        }

        /// <summary>
        /// Creates a new pseudo-random number generator with a given seed.
        /// </summary>
        /// <param name="seed">A value to use as a seed.</param>
        public MersenneTwister(int seed)
        {
            init((uint)seed);
        }

        /// <summary>
        /// Creates a new pseudo-random number generator with a default seed.
        /// </summary>
        /// <remarks>
        /// <c>new <see cref="System.Random"/>().<see cref="Random.Next()"/></c> 
        /// is used for the seed.
        /// </remarks>
        public MersenneTwister() : this(new Random().Next())    /* a default initial seed is used   */
        {
        }

        /// <summary>
        /// Creates a pseudo-random number generator initialized with the given array.
        /// </summary>
        /// <param name="initKey">The array for initializing keys.</param>
        public MersenneTwister(int[] initKey)
        {
            if (initKey == null)
            {
                throw new ArgumentNullException("initKey");
            }

            uint[] initArray = new uint[initKey.Length];

            for (int i = 0; i < initKey.Length; ++i)
            {
                initArray[i] = (uint)initKey[i];
            }

            init(initArray);
        }

        public static void Initialize(ExperimentParameters experimentParameters)
        {
            _instance = Arguments.HasKey("Seed") ? new MersenneTwister(Arguments.Get<int>("Seed")) : new MersenneTwister(experimentParameters.Seed);
            Accord.Math.Random.Generator.Seed = experimentParameters.Seed;
        }

        /// <summary>
        /// Returns the next pseudo-random <see cref="uint"/>.
        /// </summary>
        /// <returns>A pseudo-random <see cref="uint"/> value.</returns>
        public virtual uint Nextuint()
        {
            return Generateuint();
        }

        /// <summary>
        /// Returns the next pseudo-random <see cref="uint"/> 
        /// up to <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">
        /// The maximum value of the pseudo-random number to create.
        /// </param>
        /// <returns>
        /// A pseudo-random <see cref="uint"/> value which is at most <paramref name="maxValue"/>.
        /// </returns>
        public virtual uint Nextuint(uint maxValue)
        {
            return (uint)(Generateuint() / ((double)uint.MaxValue / maxValue));
        }

        /// <summary>
        /// Returns the next pseudo-random <see cref="uint"/> at least 
        /// <paramref name="minValue"/> and up to <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The minimum value of the pseudo-random number to create.</param>
        /// <param name="maxValue">The maximum value of the pseudo-random number to create.</param>
        /// <returns>
        /// A pseudo-random <see cref="uint"/> value which is at least 
        /// <paramref name="minValue"/> and at most <paramref name="maxValue"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <c><paramref name="minValue"/> &gt;= <paramref name="maxValue"/></c>.
        /// </exception>
        public virtual uint Nextuint(uint minValue, uint maxValue)  /* throws ArgumentOutOfRangeException */
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (uint)(Generateuint() / ((double)uint.MaxValue / (maxValue - minValue)) + minValue);
        }

        /// <summary>
        /// Returns the next pseudo-random <see cref="int"/>.
        /// </summary>
        /// <returns>A pseudo-random <see cref="int"/> value.</returns>
        public override int Next()
        {
            return Next(int.MaxValue);
        }

        /// <summary>
        /// Returns the next pseudo-random <see cref="int"/> up to <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="maxValue">The maximum value of the pseudo-random number to create.</param>
        /// <returns>
        /// A pseudo-random <see cref="int"/> value which is at most <paramref name="maxValue"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When <paramref name="maxValue"/> &lt; 0.
        /// </exception>
        public override int Next(int maxValue)
        {
            if (maxValue <= 1)
            {
                if (maxValue < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return 0;
            }

            return (int)(NextDouble() * maxValue);
        }

        /// <summary>
        /// Returns the next pseudo-random <see cref="int"/> 
        /// at least <paramref name="minValue"/> 
        /// and up to <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">The minimum value of the pseudo-random number to create.</param>
        /// <param name="maxValue">The maximum value of the pseudo-random number to create.</param>
        /// <returns>A pseudo-random int value which is at least <paramref name="minValue"/> and at 
        /// most <paramref name="maxValue"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <c><paramref name="minValue"/> &gt;= <paramref name="maxValue"/></c>.
        /// </exception>
        public override int Next(int minValue, int maxValue)
        {
            Debug.Assert(minValue <= maxValue);

            if (maxValue == minValue)
            {
                return minValue;
            }

            return Next(maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Fills a buffer with pseudo-random bytes.
        /// </summary>
        /// <param name="buffer">The buffer to fill.</param>
        /// <exception cref="ArgumentNullException">
        /// If <c><paramref name="buffer"/> == <see langword="null"/></c>.
        /// </exception>
        public override void NextBytes(byte[] buffer)
        {
            // [codekaizen: corrected this to check null before checking length.]
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            int bufLen = buffer.Length;

            for (int idx = 0; idx < bufLen; ++idx)
            {
                buffer[idx] = (byte)Next(256);
            }
        }

        /// <summary>
        /// Returns the next pseudo-random <see cref="double"/> value.
        /// </summary>
        /// <returns>A pseudo-random double floating point value.</returns>
        /// <remarks>
        /// <para>
        /// There are two common ways to create a double floating point using MT19937: 
        /// using <see cref="Generateuint"/> and dividing by 0xFFFFFFFF + 1, 
        /// or else generating two double words and shifting the first by 26 bits and 
        /// adding the second.
        /// </para>
        /// <para>
        /// In a newer measurement of the randomness of MT19937 published in the 
        /// journal "Monte Carlo Methods and Applications, Vol. 12, No. 5-6, pp. 385 ñ 393 (2006)"
        /// entitled "A Repetition Test for Pseudo-Random Number Generators",
        /// it was found that the 32-bit version of generating a double fails at the 95% 
        /// confidence level when measuring for expected repetitions of a particular 
        /// number in a sequence of numbers generated by the algorithm.
        /// </para>
        /// <para>
        /// Due to this, the 53-bit method is implemented here and the 32-bit method
        /// of generating a double is not. If, for some reason,
        /// the 32-bit method is needed, it can be generated by the following:
        /// <code>
        /// (Double)Nextuint() / ((UInt64)uint.MaxValue + 1);
        /// </code>
        /// </para>
        /// </remarks>
        public override double NextDouble()
        {
            return compute53BitRandom(0, InverseOnePlus53BitsOf1s);
        }

        /// <summary>
        /// Return double greater or equal to minValue and less or equal to maxValue
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public double NextDouble(double minValue, double maxValue)
        {
            return NextDouble(true) * (maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Returns a pseudo-random number greater than or equal to zero, and 
        /// either strictly less than one, or less than or equal to one, 
        /// depending on the value of the given parameter.
        /// </summary>
        /// <param name="includeOne">
        /// If <see langword="true"/>, the pseudo-random number returned will be 
        /// less than or equal to one; otherwise, the pseudo-random number returned will
        /// be strictly less than one.
        /// </param>
        /// <returns>
        /// If <paramref name="includeOne"/> is <see langword="true"/>, 
        /// this method returns a double-precision pseudo-random number greater than 
        /// or equal to zero, and less than or equal to one. 
        /// If <paramref name="includeOne"/> is <see langword="false"/>, this method
        /// returns a double-precision pseudo-random number greater than or equal to zero and
        /// strictly less than one.
        /// </returns>
        public double NextDouble(bool includeOne)
        {
            return includeOne ? compute53BitRandom(0, Inverse53BitsOf1s) : NextDouble();
        }

        /// <summary>
        /// Returns a pseudo-random number greater than 0.0 and less than 1.0.
        /// </summary>
        /// <returns>A pseudo-random number greater than 0.0 and less than 1.0.</returns>
        public double NextDoublePositive()
        {
            return compute53BitRandom(0.5, Inverse53BitsOf1s);
        }

        /// <summary>
        /// Returns a pseudo-random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A single-precision floating point number greater than or equal to 0.0, 
        /// and less than 1.0.
        /// </returns>
        public float NextSingle()
        {
            return (float)NextDouble();
        }

        /// <summary>
        /// Returns a pseudo-random number greater than or equal to zero, and either strictly
        /// less than one, or less than or equal to one, depending on the value of the
        /// given boolean parameter.
        /// </summary>
        /// <param name="includeOne">
        /// If <see langword="true"/>, the pseudo-random number returned will be 
        /// less than or equal to one; otherwise, the pseudo-random number returned will
        /// be strictly less than one.
        /// </param>
        /// <returns>
        /// If <paramref name="includeOne"/> is <see langword="true"/>, this method returns a
        /// single-precision pseudo-random number greater than or equal to zero, and less
        /// than or equal to one. If <paramref name="includeOne"/> is <see langword="false"/>, 
        /// this method returns a single-precision pseudo-random number greater than or equal to zero and
        /// strictly less than one.
        /// </returns>
        public float NextSingle(bool includeOne)
        {
            return (float)NextDouble(includeOne);
        }

        /// <summary>
        /// Returns a pseudo-random number greater than 0.0 and less than 1.0.
        /// </summary>
        /// <returns>A pseudo-random number greater than 0.0 and less than 1.0.</returns>
        public float NextSinglePositive()
        {
            return (float)NextDoublePositive();
        }

        /// <summary>
        /// Generates a new pseudo-random <see cref="uint"/>.
        /// </summary>
        /// <returns>A pseudo-random <see cref="uint"/>.</returns>
        protected uint Generateuint()
        {
            uint y;

            /* _mag01[x] = x * MatrixA  for x=0,1 */
            if (_mti >= N)      /* generate N words at one time */
            {
                Int16 kk = 0;

                for (; kk < N - M; ++kk)
                {
                    y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                    _mt[kk] = _mt[kk + M] ^ (y >> 1) ^ _mag01[y & 0x1];
                }

                for (; kk < N - 1; ++kk)
                {
                    y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                    _mt[kk] = _mt[kk + (M - N)] ^ (y >> 1) ^ _mag01[y & 0x1];
                }

                y = (_mt[N - 1] & UpperMask) | (_mt[0] & LowerMask);
                _mt[N - 1] = _mt[M - 1] ^ (y >> 1) ^ _mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];
            y ^= temperingShiftU(y);
            y ^= temperingShiftS(y) & TemperingMaskB;
            y ^= temperingShiftT(y) & TemperingMaskC;
            y ^= temperingShiftL(y);

            return y;
        }

        /* Period parameters */
        private const int N = 624;
        private const int M = 397;
        private const uint MatrixA = 0x9908b0df;
        /* constant vector a */
        private const uint UpperMask = 0x80000000;
        /* most significant w-r bits */
        private const uint LowerMask = 0x7fffffff;
        /* least significant r bits */
        /* Tempering parameters */
        private const uint TemperingMaskB = 0x9d2c5680;
        private const uint TemperingMaskC = 0xefc60000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint temperingShiftU(uint y)
        {
            return (y >> 11);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint temperingShiftS(uint y)
        {
            return (y << 7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint temperingShiftT(uint y)
        {
            return (y << 15);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint temperingShiftL(uint y)
        {
            return (y >> 18);
        }

        private readonly uint[] _mt = new uint[N];
        /* the array for the state vector  */
        private Int16 _mti;

        private static readonly uint[] _mag01 = { 0x0, MatrixA };

        private void init(uint seed)
        {
            _mt[0] = seed & 0xffffffffu;

            for (_mti = 1; _mti < N; _mti++)
            {
                _mt[_mti] = (uint)(1812433253u * (_mt[_mti - 1] ^ (_mt[_mti - 1] >> 30)) + _mti);
                // See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. 
                // In the previous versions, MSBs of the seed affect   
                // only MSBs of the array _mt[].                        
                // 2002/01/09 modified by Makoto Matsumoto             
                _mt[_mti] &= 0xffffffffu;
                // for >32 bit machines
            }
        }

        private void init(uint[] key)
        {
            int i, j, k;
            init(19650218u);

            int keyLength = key.Length;
            i = 1;
            j = 0;
            k = (N > keyLength ? N : keyLength);

            for (; k > 0; k--)
            {
                _mt[i] = (uint)((_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30)) * 1664525u)) + key[j] + j);
                /* non linear */
                _mt[i] &= 0xffffffffu;
                // for WORDSIZE > 32 machines
                i++;
                j++;
                if (i >= N)
                {
                    _mt[0] = _mt[N - 1];
                    i = 1;
                }
                if (j >= keyLength)
                    j = 0;
            }

            for (k = N - 1; k > 0; k--)
            {
                _mt[i] = (uint)((_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30)) * 1566083941u)) - i);
                /* non linear */
                _mt[i] &= 0xffffffffu;
                // for WORDSIZE > 32 machines
                i++;

                if (i < N)
                {
                    continue;
                }

                _mt[0] = _mt[N - 1];
                i = 1;
            }

            _mt[0] = 0x80000000u;
            // MSB is 1; assuring non-zero initial array
        }


        // 9007199254740991.0 is the maximum double value which the 53 significand
        // can hold when the exponent is 0.
        private const double FiftyThreeBitsOf1s = 9007199254740991.0;
        // Multiply by inverse to (vainly?) try to avoid a division.
        private const double Inverse53BitsOf1s = 1.0 / FiftyThreeBitsOf1s;
        private const double OnePlus53BitsOf1s = FiftyThreeBitsOf1s + 1;
        private const double InverseOnePlus53BitsOf1s = 1.0 / OnePlus53BitsOf1s;

        private double compute53BitRandom(double translate, double scale)
        {
            // get 27 pseudo-random bits
            ulong a = (ulong)Generateuint() >> 5;
            // get 26 pseudo-random bits
            ulong b = (ulong)Generateuint() >> 6;

            // shift the 27 pseudo-random bits (a) over by 26 bits (* 67108864.0) and
            // add another pseudo-random 26 bits (+ b).
            return ((a * 67108864.0 + b) + translate) * scale;

            // What about the following instead of the above? Is the multiply better? 
            // Why? (Is it the FMUL instruction? Does this count in .Net? Will the JITter notice?)
            //return BitConverter.Int64BitsToDouble((a << 26) + b));
        }
    }
}
