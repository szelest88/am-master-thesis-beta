using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace Server
{
    public partial class Form1 : Form
    {

        //sensor
        KinectSensor kinectSensor;
        string connectedStatus;

        //JointCollection jc;
        BodyState bs;
        //float h=0;
        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                 //   int skeletonSlot = 0;
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                    Skeleton playerSkeleton = (from s in skeletonData where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                   // ok = false;
                    if (playerSkeleton != null)
                    {
                        //ok = true;
                        //Joint rH = playerSkeleton.Joints[JointType.HandRight];
                        //jc = playerSkeleton.Joints;
                        //h = rH.Position.Z;
                        bs = new BodyState(playerSkeleton);
                        //bs.handRight.X = rH.Position.X;
                        //bs.handRight.Y = rH.Position.Y;
                        //bs.handRight.Z = rH.Position.Z;
                        
                    }
                }
            }
        }
        private bool InitializeKinect()
        {
            //bs = new BodyState();
            // Color stream
           // kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
           // kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);

            // Skeleton Stream
            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
            {
                Smoothing = 0.4f, //0.5
                Correction = 1.0f,
                Prediction = 0.5f,
                JitterRadius = 0.1f, //0.05f
                MaxDeviationRadius = 0.04f
            });
            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);

            try
            {
                kinectSensor.Start();
            }
            catch
            {
                connectedStatus = "Unable to start the Kinect Sensor";
                return false;
            }
            return true;
        }

        private void DiscoverKinectSensor()
        {
            int i = 0;
            foreach (KinectSensor sensor in KinectSensor.KinectSensors)
            {
                if (i == 1)
                {

                    if (sensor.Status == KinectStatus.Connected)
                    {
                        // Found one, set our sensor to this
                        kinectSensor = sensor;
                        break;
                    }
                }

                i++;
            }

            if (this.kinectSensor == null)
            {
                connectedStatus = "Found none Kinect Sensors connected to USB";
                return;
            }

            // You can use the kinectSensor.Status to check for status
            // and give the user some kind of feedback
            switch (kinectSensor.Status)
            {
                case KinectStatus.Connected:
                    {
                        connectedStatus = "Status: Connected";
                        break;
                    }
                case KinectStatus.Disconnected:
                    {
                        connectedStatus = "Status: Disconnected";
                        break;
                    }
                case KinectStatus.NotPowered:
                    {
                        connectedStatus = "Status: Connect the power";
                        break;
                    }
                default:
                    {
                        connectedStatus = "Status: Error";
                        break;
                    }
            }

            // Init the found and connected device
            if (kinectSensor.Status == KinectStatus.Connected)
            {
                InitializeKinect();
            }
        }

        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (this.kinectSensor == e.Sensor)
            {
                if (e.Status == KinectStatus.Disconnected ||
                    e.Status == KinectStatus.NotPowered)
                {
                    this.kinectSensor = null;
                    this.DiscoverKinectSensor();
                }
            }
        }


        //nadawacz
        private Server server;

        void DisplayMetodReceived(string message)
        {
            this.Text = message;
        }
       
        public Form1()
        {
            InitializeComponent();

            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
            DiscoverKinectSensor();

               

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.server = new Server();
            if (!this.server.Running)
            {
                this.server.PipeName = @"\\.\pipe\myNamedPipe";
                this.server.Start();
            }
        }
       // int i = 0;
        private void button1_Click(object sender, EventArgs e)
        {
          //  i++;
          //  this.server.SendMessage("lol"+h);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(this.server.listenThread!=null)
                this.server.listenThread.Abort();

            if (this.server.readThread != null)
                this.server.readThread.Abort();
            //if (kinectSensor != null)
            //{
            //    kinectSensor.Stop();
            //    kinectSensor.Dispose();
            //}
            Application.Exit();
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //if(bs!=null)
            //this.server.SendMessage(""+bs.handRight.X);
            if (bs != null)
                this.server.SendMessage("" + bs.ToString());

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.server.listenThread != null)
                this.server.listenThread.Abort();

            if (this.server.readThread != null)
                this.server.readThread.Abort();
            if (kinectSensor != null)
            {
                kinectSensor.Stop();
                kinectSensor.Dispose();
            }

        }
    }
}
