namespace Kit3D.Windows.Media.Media3D
{
    using System;

    //TODO: Move this somewhere into a different namespace

    public static class MathHelper
    {
        //TODO: Verify this constant is okay to use, too small?
        public const double Epsilon = 0.00001f;

        public const double OneDivThree = 1.0 / 3;
        
        public static bool IsZero(double value)
        {
            return System.Math.Abs(value) < Epsilon;
        }
        
        public static bool AreEqual(double x, double y)
        {
            return IsZero(x-y);
        }

        public static double SquareRoot(double value)
        {
            return System.Math.Sqrt(value);
        }

        public static double InverseSquareRoot(double value)
        {
            return (1.0 / System.Math.Sqrt(value));
        }
    }
}
