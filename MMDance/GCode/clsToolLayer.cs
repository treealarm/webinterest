using System.Drawing;
namespace GCode
{
    public class clsToolLayer
    {
        public System.Drawing.Color Color;
        public float Number;
        public bool Hidden;

        public clsToolLayer(float number, System.Drawing.Color color)
        {
            this.Number = number;
            this.Color = color;
            this.Hidden = false;
        }
    }
}