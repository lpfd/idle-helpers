using System;

namespace LeapForward.IdleHelpers
{
    /// <summary>
    /// Represents a high-precision number using scientific notation ($Mantissa \times 10^{Exponent}$).
    /// Designed for idle games to handle values exceeding the <see cref="double.MaxValue"/>.
    /// </summary>
    public struct BigNumber : IEquatable<BigNumber>
    {
        /// <summary>
        /// The base value of the number. After normalization, this is typically in the range [1, 10).
        /// </summary>
        public double Mantissa { get; private set; }

        /// <summary>
        /// The power of 10 that the <see cref="Mantissa"/> is multiplied by.
        /// </summary>
        public long Exponent { get; private set; }

        /// <summary> Represents the value 0. </summary>
        public static readonly BigNumber Zero = new BigNumber(0, 0);

        /// <summary> Represents the maximum possible value of <see cref="BigNumber"/>. </summary>
        public static readonly BigNumber MaxValue = new BigNumber(1.0, long.MaxValue);

        /// <summary> Represents the smallest possible positive value of <see cref="BigNumber"/>. </summary>
        public static readonly BigNumber Epsilone = new BigNumber(1.0, long.MinValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="BigNumber"/> struct and normalizes it.
        /// </summary>
        /// <param name="mantissa">The initial mantissa.</param>
        /// <param name="exponent">The initial exponent.</param>
        public BigNumber(double mantissa, long exponent)
        {
            Mantissa = mantissa;
            Exponent = exponent;
            Normalize();
        }

        #region Implicit Conversions
        /// <summary>
        /// Defines an implicit conversion from a double-precision floating-point value to a BigNumber instance.
        /// </summary>
        /// <remarks>This conversion allows a double value to be assigned directly to a BigNumber variable
        /// without an explicit cast. Precision may be limited by the representation of the double value.</remarks>
        /// <param name="value">The double-precision floating-point value to convert to a BigNumber.</param>
        public static implicit operator BigNumber(double value) => new BigNumber(value, 0);

        /// <summary>
        /// Defines an implicit conversion of a 64-bit signed integer to a BigNumber instance.  
        /// </summary>
        /// <remarks>This conversion allows a long value to be used wherever a BigNumber is expected,
        /// without requiring an explicit cast.</remarks>
        /// <param name="value">The 64-bit signed integer value to convert to a BigNumber.</param>
        public static implicit operator BigNumber(long value) => new BigNumber((double)value, 0);

        /// <summary>
        /// Defines an implicit conversion of a 32-bit signed integer to a BigNumber instance.
        /// </summary>
        /// <remarks>This conversion allows an int value to be used wherever a BigNumber is expected,
        /// without requiring an explicit cast.</remarks>
        /// <param name="value">The 32-bit signed integer value to convert to a BigNumber.</param>
        public static implicit operator BigNumber(int value) => new BigNumber((double)value, 0);

        /// <summary>
        /// Defines an implicit conversion of a single-precision floating-point value to a BigNumber instance.
        /// </summary>
        /// <remarks>This conversion allows a float value to be assigned directly to a BigNumber without
        /// explicit casting. The fractional value is preserved in the resulting BigNumber.</remarks>
        /// <param name="value">The single-precision floating-point value to convert to a BigNumber.</param>
        public static implicit operator BigNumber(float value) => new BigNumber((double)value, 0);
        #endregion

        #region Explicit Conversions
        /// <summary> Converts to double. Returns <see cref="double.PositiveInfinity"/> if value exceeds 1.8e308. </summary>
        public static explicit operator double(BigNumber value)
        {
            return value.Mantissa * Math.Pow(10, value.Exponent);
        }

        /// <summary> Converts to long. Will overflow if the number is larger than ~9e18. </summary>
        public static explicit operator long(BigNumber value)
        {
            double d = (double)value;
            return (long)d;
        }

        /// <summary> Converts to float. Returns <see cref="float.PositiveInfinity"/> if value exceeds ~3.4e38. </summary>
        public static explicit operator float(BigNumber value)
        {
            return (float)(value.Mantissa * Math.Pow(10, value.Exponent));
        }

        /// <summary> Returns true if the value is not zero. </summary>
        public static explicit operator bool(BigNumber value)
        {
            return value != BigNumber.Zero;
        }
        #endregion

        /// <summary>
        /// Adjusts the <see cref="Mantissa"/> and <see cref="Exponent"/> so the absolute mantissa 
        /// stays within the [1, 10) range for consistent comparisons and arithmetic.
        /// </summary>
        private void Normalize()
        {
            if (Mantissa == 0)
            {
                Exponent = 0;
                return;
            }

            double absMantissa = Math.Abs(Mantissa);
            while (absMantissa >= 10)
            {
                absMantissa /= 10;
                Exponent++;
            }
            while (absMantissa < 1 && absMantissa > 0)
            {
                absMantissa *= 10;
                Exponent--;
            }

            Mantissa = Math.Sign(Mantissa) * absMantissa;
        }

        #region Arithmetic Operators
        /// <summary>
        /// Multiplies two BigNumber values and returns the result.
        /// </summary>
        /// <param name="a">The first BigNumber value to multiply.</param>
        /// <param name="b">The second BigNumber value to multiply.</param>
        /// <returns>A BigNumber that is the product of the two specified BigNumber values.</returns>
        public static BigNumber operator *(BigNumber a, BigNumber b)
        {
            return new BigNumber(a.Mantissa * b.Mantissa, a.Exponent + b.Exponent);
        }

        /// <summary>
        /// Divides one BigNumber value by another and returns the result.
        /// </summary>
        /// <param name="a">The dividend. The BigNumber to be divided.</param>
        /// <param name="b">The divisor. The BigNumber by which to divide.</param>
        /// <returns>A BigNumber that is the result of dividing a by b.</returns>
        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            return new BigNumber(a.Mantissa / b.Mantissa, a.Exponent - b.Exponent);
        }

        /// <summary>
        /// Adds two BigNumbers. If the difference in exponents is greater than 15, 
        /// the smaller value is discarded as insignificant to maintain double precision.
        /// </summary>
        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            BigNumber larger = a.Exponent >= b.Exponent ? a : b;
            BigNumber smaller = a.Exponent >= b.Exponent ? b : a;

            long diff = larger.Exponent - smaller.Exponent;

            if (diff > 15) return larger;

            double newMantissa = larger.Mantissa + (smaller.Mantissa / Math.Pow(10, diff));
            return new BigNumber(newMantissa, larger.Exponent);
        }

        /// <summary>
        /// Negates the specified BigNumber value.
        /// </summary>
        /// <param name="a">The BigNumber instance to negate.</param>
        /// <returns>A BigNumber whose value is the negation of the specified value.</returns>
        public static BigNumber operator -(BigNumber a)
        {
            return new BigNumber(-a.Mantissa, a.Exponent);
        }

        /// <summary>
        /// Subtracts one BigNumber value from another and returns the result.
        /// </summary>
        /// <param name="a">The minuend. The value from which <paramref name="b"/> is subtracted.</param>
        /// <param name="b">The subtrahend. The value to subtract from <paramref name="a"/>.</param>
        /// <returns>A BigNumber that is the result of subtracting <paramref name="b"/> from <paramref name="a"/>.</returns>
        public static BigNumber operator -(BigNumber a, BigNumber b)
        {
            return a + (-b);
        }

        /// <summary>
        /// Returns the absolute value of this BigNumber instance.
        /// </summary>
        /// <returns>A new BigNumber whose value is the absolute value of the current instance.</returns>
        public BigNumber Abs()
        {
            return new BigNumber(Math.Abs(Mantissa), Exponent);
        }

        /// <summary>
        /// Raises the BigNumber to the specified power.
        /// </summary>
        /// <param name="power">The exponent to raise this number to.</param>
        /// <returns>A new BigNumber representing (this ^ power).</returns>
        public BigNumber Pow(double power)
        {
            // 1. Handle Edge Cases
            if (power == 0) return new BigNumber(1, 0);
            if (power == 1) return this;
            if (Mantissa == 0) return Zero;

            // 2. Mathematical Logic: A^y = (m * 10^e)^y = m^y * 10^(e * y)
            // We calculate the new exponent and new mantissa separately.

            // Calculate new exponent part from the existing exponent
            double newExponent = Exponent * power;

            // Calculate new mantissa by raising the current mantissa to the power
            double newMantissa = Math.Pow(Mantissa, power);

            // 3. Handle potential overflow of Math.Pow 
            // If newMantissa becomes Infinity or very large, we convert it back 
            // to scientific notation using Log10.
            if (double.IsInfinity(newMantissa) || double.IsNaN(newMantissa) || Math.Abs(newMantissa) > 1e10)
            {
                // Fallback to Logarithms: log10(m^y) = y * log10(m)
                double logValue = power * Math.Log10(Math.Abs(Mantissa));
                double logFloor = Math.Floor(logValue);

                newMantissa = Math.Pow(10, logValue - logFloor) * Math.Sign(Mantissa);
                newExponent += logFloor;
            }

            // Split the newExponent into integer and fractional parts
            long exponentInt = (long)Math.Floor(newExponent);
            double exponentFraction = newExponent - exponentInt;

            // Apply the fractional part of the exponent to the mantissa
            newMantissa *= Math.Pow(10, exponentFraction);

            return new BigNumber(newMantissa, exponentInt);
        }
        #endregion

        #region Comparison Operators
        /// <summary> Performs a signed comparison of two BigNumbers. </summary>
        public static bool operator >(BigNumber a, BigNumber b)
        {
            bool aPos = a.Mantissa >= 0;
            bool bPos = b.Mantissa >= 0;

            if (aPos && !bPos) return true;
            if (!aPos && bPos) return false;

            if (aPos)
            {
                if (a.Exponent > b.Exponent) return true;
                if (a.Exponent < b.Exponent) return false;
                return a.Mantissa > b.Mantissa;
            }

            if (a.Exponent > b.Exponent) return false;
            if (a.Exponent < b.Exponent) return true;
            return a.Mantissa > b.Mantissa;
        }

        /// <summary>
        /// Determines whether one BigNumber value is less than another.
        /// </summary>
        /// <remarks>This operator enables the use of the less-than operator (<) with BigNumber instances.
        /// The comparison takes into account both the sign and magnitude of the numbers.</remarks>
        /// <param name="a">The first BigNumber to compare.</param>
        /// <param name="b">The second BigNumber to compare.</param>
        /// <returns>true if the value of a is less than the value of b; otherwise, false.</returns>
        public static bool operator <(BigNumber a, BigNumber b)
        {
            bool aPos = a.Mantissa >= 0;
            bool bPos = b.Mantissa >= 0;

            if (!aPos && bPos) return true;
            if (aPos && !bPos) return false;

            if (aPos)
            {
                if (a.Exponent < b.Exponent) return true;
                if (a.Exponent > b.Exponent) return false;
                return a.Mantissa < b.Mantissa;
            }
            else
            {
                if (a.Exponent < b.Exponent) return false;
                if (a.Exponent > b.Exponent) return true;
                return a.Mantissa < b.Mantissa;
            }
        }

        /// <summary>
        /// Determines whether one BigNumber instance is greater than or equal to another instance.
        /// </summary>
        /// <param name="a">The first BigNumber to compare.</param>
        /// <param name="b">The second BigNumber to compare.</param>
        /// <returns>true if the value of a is greater than or equal to the value of b; otherwise, false.</returns>
        public static bool operator >=(BigNumber a, BigNumber b) => !(a < b);
        public static bool operator <=(BigNumber a, BigNumber b) => !(a > b);
        public static bool operator ==(BigNumber left, BigNumber right) => left.Equals(right);
        public static bool operator !=(BigNumber left, BigNumber right) => !(left == right);
        #endregion

        /// <summary> Returns a string formatted in scientific notation (e.g., "1.23e45"). </summary>
        public override string ToString() => $"{Mantissa:F2}e{Exponent}";

        /// <summary>
        /// Determines whether the specified object is equal to the current BigNumber instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current BigNumber instance.</param>
        /// <returns>true if the specified object is a BigNumber and is equal to the current instance; otherwise, false.</returns>
        public override bool Equals(object obj) => obj is BigNumber number && Equals(number);

        /// <summary>
        /// Determines whether the current BigNumber instance is equal to another BigNumber instance.
        /// </summary>
        /// <param name="other">The BigNumber instance to compare with the current instance.</param>
        /// <returns>true if the current instance and the other instance represent the same value; otherwise, false.</returns>
        public bool Equals(BigNumber other)
        {
            return Mantissa == other.Mantissa && Exponent == other.Exponent;
        }

        /// <summary>
        /// Serves as the default hash function for the current object.
        /// </summary>
        /// <remarks>Use this method when inserting instances of this type into hash-based collections
        /// such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> or <see
        /// cref="System.Collections.Generic.HashSet{T}"/>. The hash code is based on the values of the Mantissa and
        /// Exponent properties.</remarks>
        /// <returns>A 32-bit signed integer hash code representing the current object.</returns>
        public override int GetHashCode() => HashCode.Combine(Mantissa, Exponent);
    }
}