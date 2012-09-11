using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace Server
{
    class BodyState
    {

        public Vector3 handRight;
        public Vector3 elbowRight;
        public Vector3 armRight;

        public Vector3 handLeft;
        public override string ToString()
        {
            return "" + handRight.X + ":" + handRight.Y + ":" + handRight.Z + ":" +
                elbowRight.X + ":" + elbowRight.Y + ":" + elbowRight.Z + ":" +
                armRight.X + ":" + armRight.Y + ":" + armRight.Z + ":" +
                handLeft.X + ":" + handLeft.Y + ":" + handLeft.Z+":";
        }

        public void deString(string str)
        {
            float handX = float.Parse(str.Split(':')[0]);
            float handY = float.Parse(str.Split(':')[1]);
            float handZ = float.Parse(str.Split(':')[2]);

            handRight = new Vector3(handX, handY, handZ);

            float elbowX = float.Parse(str.Split(':')[3]);
            float elbowY = float.Parse(str.Split(':')[4]);
            float elbowZ = float.Parse(str.Split(':')[5]);

            elbowRight = new Vector3(elbowX, elbowY, elbowZ);

            float armX = float.Parse(str.Split(':')[6]);
            float armY = float.Parse(str.Split(':')[7]);
            float armZ = float.Parse(str.Split(':')[8]);

            armRight = new Vector3(armX, armY, armZ);

            float hand2X = float.Parse(str.Split(':')[9]);
            float hand2Y = float.Parse(str.Split(':')[10]);
            float hand2Z = float.Parse(str.Split(':')[11]);

            handLeft = new Vector3(hand2X, hand2Y, hand2Z);
        }

        public BodyState(Skeleton s)
        {
            handRight.X = s.Joints[JointType.HandRight].Position.X;
            handRight.Y = s.Joints[JointType.HandRight].Position.Y;
            handRight.Z = s.Joints[JointType.HandRight].Position.Z;

            elbowRight.X = s.Joints[JointType.ElbowRight].Position.X;
            elbowRight.Y = s.Joints[JointType.ElbowRight].Position.Y;
            elbowRight.Z = s.Joints[JointType.ElbowRight].Position.Z;

            armRight.X = s.Joints[JointType.ShoulderRight].Position.X;
            armRight.Y = s.Joints[JointType.ShoulderRight].Position.Y;
            armRight.Z = s.Joints[JointType.ShoulderRight].Position.Z;

            handLeft.X = s.Joints[JointType.WristLeft].Position.X;
            handLeft.Y = s.Joints[JointType.WristLeft].Position.Y;
            handLeft.Z = s.Joints[JointType.WristLeft].Position.Z;

        }
        public BodyState(string str)
        {
            if (str != "")
            {
                float handX = float.Parse(str.Split(':')[0]);
                float handY = float.Parse(str.Split(':')[1]);
                float handZ = float.Parse(str.Split(':')[2]);

                handRight = new Vector3(handX, handY, handZ);
                 
                float elbowX = float.Parse(str.Split(':')[3]);
                float elbowY = float.Parse(str.Split(':')[4]);
                float elbowZ = float.Parse(str.Split(':')[5]);

                elbowRight = new Vector3(elbowX, elbowY, elbowZ);

                float armX = float.Parse(str.Split(':')[6]);
                float armY = float.Parse(str.Split(':')[7]);
                float armZ = float.Parse(str.Split(':')[8]);

                armRight = new Vector3(armX, armY, armZ);

                float hand2X = float.Parse(str.Split(':')[9]);
                float hand2Y = float.Parse(str.Split(':')[10]);
                float hand2Z = float.Parse(str.Split(':')[11]);

                handLeft = new Vector3(hand2X, hand2Y, hand2Z);

            }
        }

    }
}
