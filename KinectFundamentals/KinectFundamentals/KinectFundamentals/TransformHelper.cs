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
            float l = 0.47f; //odległość w metrach pomiędzy kinectami w osi x (lewo-prawo)

            float alfa = -MathHelper.ToRadians(18); 
            // kąt nachylenia wokół osi pionowej drugiego kinecta (względem pierwszego)
            float back = -0.1f; // drugi kinect jest 10 cm z tyłu
            float sinAlfa = (float)Math.Sin(alfa); float cosAlfa = (float)Math.Cos(alfa);
            float tanAlfa = (float)Math.Tan(alfa); float cotAlpha = 1 / tanAlfa;
            float xbis = 0, zbis = 0;
            float x2 = remote.X; //remote
            float z2 = remote.Z;
          
            xbis = l - (z2 * sinAlfa - x2 * cosAlfa); // było l-(A1-B1) // (+ -)
            zbis = z2 / cosAlfa - x2 * sinAlfa; //  (+ -)
            zbis += back; //trzeci parametr... 
            return new Vector3(xbis, remote.Y, zbis);
        }
    }
}