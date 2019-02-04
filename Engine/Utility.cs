namespace Engine
{
    using System;

    /// <summary>
    /// Provides various utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Divides the two integers, rounding the result towards negative infinity.
        /// The operation is equivalent to <c>(int)Math.Floor((double)a / b)</c>, but operates on integers, is faster, and does not need rounding or conversion.
        /// </summary>
        /// <param name="a">The dividend.</param>
        /// <param name="b">The divisor.</param>
        /// <returns>The <c>Floor</c> value after the division.</returns>
        public static int DivFloor(int a, int b) => (a < 0) ^ (b < 0) && a % b != 0 ? a / b - 1 : a / b;

        /// <summary>
        /// Calculates the remainder after division of the two integers. The division result is first rounded towards negative infinity.
        /// This operation is the modulo of the corresponding <see cref="DivFloor"/> operation.
        /// </summary>
        /// <param name="a">The dividend.</param>
        /// <param name="b">The divisor.</param>
        /// <returns>The remainder of the floored division operation.</returns>
        public static int ModFloor(int a, int b) => (a < 0) ^ (b < 0) && a % b != 0 ? a - DivFloor(a, b) * b : a % b;

        /// <summary>
        /// Calculates the squared distance between two points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <returns>The squared distance.</returns>
        public static int SquaredDistance((int x, int y) p1, (int x, int y) p2)
        {
            var dx = Math.Abs(p2.x - p1.x);
            var dy = Math.Abs(p2.y - p1.y);

            return dx * dx + dy * dy;
        }
    }
}
