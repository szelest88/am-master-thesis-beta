using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;


namespace KinectFundamentals
{
    class TransformHelper
    {
        /**
        <summary>
        przyjmuje rezultat positionFromJoint i Z z tego jointa
        </summary>
        **/
        public static Vector3 transformToFirstKinect(Vector3 remote)
        {
            float l = 0.47f; //było 0.47

            float alfa = -MathHelper.ToRadians(18); // 18     !!! Pokminić nad tym. (było -1)
            float poprz = -0.1f;
            float sinAlfa = (float)Math.Sin(alfa); float cosAlfa = (float)Math.Cos(alfa);
            float tanAlfa = (float)Math.Tan(alfa); float cotAlpha = 1 / tanAlfa;
            float xbis = 0, zbis = 0;
            float x2 = remote.X; //remote
            float z2 = remote.Z;
          
            xbis = l - (z2 * sinAlfa - x2 * cosAlfa); // było l-(A1-B1) // (+ -)
            zbis = z2 / cosAlfa - x2 * sinAlfa; //  (+ -)
            zbis += poprz; //trzeci parametr... // (modify)
            return new Vector3(xbis, remote.Y, zbis);
        }
    }
}