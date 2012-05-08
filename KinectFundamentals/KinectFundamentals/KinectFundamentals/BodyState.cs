using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace KinectFundamentals
{
    class BodyState
    {

        public Vector3 handRight;
        public Vector3 elbowRight;
        public Vector3 armRight;

        public override string ToString()
        {
            return "" + handRight.X + ":" + handRight.Y + ":" + handRight.Z + ":" +
                elbowRight.X + ":" + elbowRight.Y + ":" + elbowRight.Z + ":" +
                armRight.X + ":" + armRight.Y + ":" + armRight.Z;
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

        }

        public BodyState(Skeleton s)
        {
            handRight.X = s.Joints[JointType.WristRight].Position.X;
            handRight.Y = s.Joints[JointType.WristRight].Position.Y;
            handRight.Z = s.Joints[JointType.WristRight].Position.Z;
            elbowRight.X = s.Joints[JointType.ElbowRight].Position.X;
            elbowRight.Y = s.Joints[JointType.ElbowRight].Position.Y;
            elbowRight.Z = s.Joints[JointType.ElbowRight].Position.Z;

            armRight.X = s.Joints[JointType.ShoulderRight].Position.X;
            armRight.Y = s.Joints[JointType.ShoulderRight].Position.Y;
            armRight.Z = s.Joints[JointType.ShoulderRight].Position.Z;

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
                float armZ=0;
                if(str.Split(':')[8]!=null && str.Split(':')[8]!="")
                    armZ = float.Parse(str.Split(':')[8]);

                armRight = new Vector3(armX, armY, armZ);
            }
        }

    }
}
