//badziewie ale jest:
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;

namespace KinectFundamentals
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D kinectRGBVideo;
        Texture2D overlay;
        Texture2D hand;
        Texture2D hand2;

        Vector2 handPosition = new Vector2();
        float handDepth = 0.0f;
        //Vector2 overPosition = new Vector2();
        Vector2 hand2Position = new Vector2();
        float hand2Depth;
        Vector2 headPosition = new Vector2();
        float headDepth;

        Vector2 elbowLposition = new Vector2();
        float elbowLdepth;
        Vector2 elbowRposition = new Vector2();
        float elbowRdepth;

        Vector2 armLposition = new Vector2();
        float armLdepth;
        Vector2 armRposition = new Vector2();
        float armRdepth;

        Vector2 centerPosition = new Vector2();
        float centerDepth;

        //do pisania:
        SpriteFont sf;


        KinectSensor kinectSensor;


        bool nad = true; //czy ramiona s¹ nad d³oñmi
        SpriteFont font;
        //MODEL
        Model mdRH;
        Model mdBox;
        // Effect effect;

        ///////// secondary kinect
      //  string pos = "";
        string result = "";


        private Client pipeClient;

        void DisplayReceivedMessage(string message)
        {
            //   this.tbReceived.Text += message + "\r\n";
            //uaktualniæ UI
            result = message;
        }
        void pipeClient_MessageReceived(string message)
        {
            // this.Invoke(new Client.MessageReceivedHandler(DisplayReceivedMessage),
            //     new object[] { message });
            DisplayReceivedMessage(message);
        }


        //k¹ty
        #region k¹ty

        static int angle = 360;
        static int angle2 = 0;
        //int a = 0;
       // float b = 0;

       // int begin = 0;

        Effect effect2;

        VertexPositionColor[] vertices;

        private void updateVertices(int angle, int begin, float x, float y)
        {
           // int verts = angle / 2;
            //begin = a;

            float mnoznik = 1.33333333f;//640/480
            double pi18 = Math.PI * 18.0;
            int bpv2 = begin + angle;
            for (int i = begin; i < bpv2; i++)
                vertices[i - begin].Color = Color.Red;
            for (int i = begin; i < bpv2; i += 2)
                vertices[i - begin].Position = new Vector3(x + (float)Math.Sin(i / pi18) * 0.1f, y + (float)Math.Cos(i /pi18) * 0.1f*  mnoznik, -50);

            for (int i = begin + 1; i < bpv2; i += 2)
                vertices[i - begin].Position = new Vector3(x + (float)Math.Sin(i / pi18) * 0.12f, y + (float)Math.Cos(i / pi18) * 0.12f * mnoznik, -50);

        }
       
        private void SetupVertices(int verts)
        {
            // verts = angle * 2;
            vertices = new VertexPositionColor[verts * 2];


            //updateVertices(angle, angle2, (armRposition.X - 100.0f) / 640.0f, -armRposition.Y / 240.0f);

            //updateVertices(angle, angle2, (160.0f + -armRposition.X * 0.5f) / 640.0f, -armRposition.Y / 240.0f);
        }
        #endregion k¹ty

        List<float> angles;

        private Model LoadModel(string assetName)
        {
            Model newModel = Content.Load<Model>(assetName);

            // effect = Content.Load<Effect>("effects"); 

            //   foreach (ModelMesh mesh in newModel.Meshes)
            //       foreach (ModelMeshPart meshPart in mesh.MeshParts)
            //           meshPart.Effect = effect.Clone();

            return newModel;
        }


        //string connectedStatus = "Not connected";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;//640
            graphics.PreferredBackBufferHeight = 960;//480


            this.pipeClient = new Client();

            this.pipeClient.MessageReceived +=
                new Client.MessageReceivedHandler(pipeClient_MessageReceived);
            //pod³¹cz siê
            if (!this.pipeClient.Connected)
            {
                this.pipeClient.PipeName = @"\\.\pipe\myNamedPipe";
                this.pipeClient.Connect();
            }
            

            angles = new List<float>();
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

        private bool InitializeKinect()
        {
            // Color stream
            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);

            // Skeleton Stream
            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
            {
                Smoothing = 0.2f, //0.5
                Correction = 0.5f,
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
             //   connectedStatus = "Unable to start the Kinect Sensor";
                return false;
            }
            return true;
        }
        Vector2 positionFromJoint(Joint j)
        {
            Vector2 resPosition = new Vector2(j.Position.X, -j.Position.Y);
            return resPosition;
        }
        Vector2 positionFromJoint(Vector2 v)
        {
            Vector2 resPosition = new Vector2((((v.X))), (((-v.Y))));
            return resPosition;
        }

        bool ok = true;
        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                   // int skeletonSlot = 0;
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                    Skeleton playerSkeleton = (from s in skeletonData where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                    ok = false;
                    if (playerSkeleton != null)
                    {
                        ok = true;
                        Joint rightHand = playerSkeleton.Joints[JointType.HandRight];
                        Joint leftHand = playerSkeleton.Joints[JointType.HandLeft];
                        Joint leftElbow = playerSkeleton.Joints[JointType.ElbowLeft];
                        Joint rightElbow = playerSkeleton.Joints[JointType.ElbowRight];

                        Joint leftArm = playerSkeleton.Joints[JointType.ShoulderLeft];
                        Joint rightArm = playerSkeleton.Joints[JointType.ShoulderRight];

                        Joint center = playerSkeleton.Joints[JointType.ShoulderCenter];

                        handPosition = positionFromJoint(rightHand);// Vector2((((0.5f * rightHand.Position.X) + 0.5f) * (640)), (((-0.5f * rightHand.Position.Y) + 0.5f) * (480)));
                        handDepth = rightHand.Position.Z;//
                        hand2Position = positionFromJoint(leftHand);//new Vector2((((0.5f * leftHand.Position.X) + 0.5f) * (640)), (((-0.5f * leftHand.Position.Y) + 0.5f) * (480)));
                        hand2Depth = leftHand.Position.Z;


                        elbowLposition = positionFromJoint(leftElbow);//new Vector2((((0.5f * leftElbow.Position.X) + 0.5f) * (640)), (((-0.5f * leftElbow.Position.Y) + 0.5f) * (480)));
                        elbowLdepth = leftElbow.Position.Z;
                        elbowRposition = positionFromJoint(rightElbow);// Vector2((((0.5f * rightElbow.Position.X) + 0.5f) * (640)), (((-0.5f * rightElbow.Position.Y) + 0.5f) * (480)));
                        elbowRdepth = rightElbow.Position.Z;

                        armLposition = positionFromJoint(leftArm);//new Vector2((((0.5f * leftElbow.Position.X) + 0.5f) * (640)), (((-0.5f * leftElbow.Position.Y) + 0.5f) * (480)));
                        armLdepth = leftArm.Position.Z;
                        armRposition = positionFromJoint(rightArm);// Vector2((((0.5f * rightElbow.Position.X) + 0.5f) * (640)), (((-0.5f * rightElbow.Position.Y) + 0.5f) * (480)));
                        armRdepth = rightArm.Position.Z;

                        centerPosition = positionFromJoint(center);
                        centerDepth = center.Position.Z;

                        // do "przysiadów"
                        /*
                        int gr = 50;
                        if (Math.Abs(elbowLposition.X - hand2Position.X) < gr || Math.Abs(elbowRposition.X - handPosition.X) < gr)
                            nad = true;
                        else
                            nad = false;

                        if (Math.Abs(elbowLposition.X - hand2Position.X) < gr && Math.Abs(elbowRposition.X - handPosition.X) < gr
                            &&
                            Math.Abs(elbowLposition.Y - hand2Position.Y) < gr && Math.Abs(elbowRposition.Y - handPosition.Y) < gr

                        )
                            nad = false;
                        */
                        Joint head = playerSkeleton.Joints[JointType.Head];
                        headPosition = positionFromJoint(head);// Vector2((((0.5f * head.Position.X) + 0.5f) * (640)), (((-0.5f * head.Position.Y) + 0.5f) * (480)));
                        headDepth = head.Position.Z;
                    }
                    //f = skeletonFrame;
                }
            }
        }

        byte[] pixelsFromFrame = new byte[1228800];//   byte[] pixelsFromFrame = new byte[colorImageFrame.PixelDataLength];

        int h2 = 960;
        int w2 = 1280;
        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame != null)
                {


                    colorImageFrame.CopyPixelDataTo(pixelsFromFrame);

                    //utworzyæ sobie zmienne na height*2 i width*2
                    

                    Color[] color = new Color[h2 * w2];//bez *4
                    kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, w2, h2);//bez *2*2

                    // Go through each pixel and set the bytes correctly
                    // Remember, each pixel got a Rad, Green and Blue
                    int index = 0;

                    for (int y = 0; y < h2; y += 2)
                    {
                        int yw2 = y * w2;
                        for (int x = 0; x < w2; x += 2)
                        {
                            //int y2 = y * 2;
                            //int x2 = x * 2;


                            int yw2x = yw2 + x;
                            color[yw2x + w2] =
                                color[yw2x + 1] =
                                    color[yw2x] =
                                        color[yw2x + w2 + 1] =
                                            new Color(pixelsFromFrame[index + 2], pixelsFromFrame[index + 1], pixelsFromFrame[index + 0]);

                            //        color[yw2x];
                            //        color[yw2x];
                            //       color[yw2x + w2 + 1] = color[yw2x];
                            index += 4;
                        }
                    }

                    // Set pixeldata from the ColorImageFrame to a Texture2D
                    kinectRGBVideo.SetData(color);
                }
            }
        }

        private void DiscoverKinectSensor()
        {
            int i = 0;
            foreach (KinectSensor sensor in KinectSensor.KinectSensors)
            {
                if (i == 0)
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
                //connectedStatus = "Found none Kinect Sensors connected to USB";
                return;
            }
            /*
            // You can use the kinectSensor.Status to check for status
            // and give the user some kind of feedback
            switch (kinectSensor.Status)
            {
                case KinectStatus.Connected:
                    {
                    //    connectedStatus = "Status: Connected";
                        break;
                    }
                case KinectStatus.Disconnected:
                    {
                      //  connectedStatus = "Status: Disconnected";
                        break;
                    }
                case KinectStatus.NotPowered:
                    {
                        //connectedStatus = "Status: Connect the power";
                        break;
                    }
                default:
                    {
                 //       connectedStatus = "Status: Error";
                        break;
                    }
            }
            */
            // Init the found and connected device
            if (kinectSensor.Status == KinectStatus.Connected)
            {
                InitializeKinect();
            }
        }

        protected override void Initialize()
        {
            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
            DiscoverKinectSensor();

            base.Initialize();
        }
        Matrix[] modelAbsTrans;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            kinectRGBVideo = new Texture2D(GraphicsDevice, 1337, 1337);

            overlay = Content.Load<Texture2D>("overlay2");
            hand = Content.Load<Texture2D>("hand");
            hand2 = Content.Load<Texture2D>("hand");
            font = Content.Load<SpriteFont>("SpriteFont1");



            //effect = Content.Load<Effect>("effects");
            mdRH = LoadModel("sphere");
            mdBox = LoadModel("box");
            modelAbsTrans = new Matrix[mdRH.Bones.Count];
            mdRH.CopyAbsoluteBoneTransformsTo(modelAbsTrans);

            sf = Content.Load<SpriteFont>("SpriteFont1");


            //k¹ty
            //trójk¹ty
            effect2 = Content.Load<Effect>("Effect1");
            SetupVertices(angle);
        }

        protected override void UnloadContent()
        {
            kinectSensor.Stop();
            kinectSensor.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {

           // pos = res; //secondary Kinect
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {

              //  kinectSensor.Stop();
              //  kinectSensor.Dispose();
                this.Exit();
            }

            base.Update(gameTime);
        }


        void renderJoint(Vector2 which, float depth, Matrix view, Matrix projection, float scale, Vector3 color, bool alpha)
        { //niby to jest ok.

           // depth *= 100;
         //   scale *= 2;
    
            foreach (ModelMesh mesh in mdRH.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                {
                    //effect.CurrentTechnique //AM
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(1, 1, 1);


                  //  if (blue)

                        effect.DiffuseColor = color;
                        if (alpha)
                            effect.Alpha = 0.3f;
                    //else
                     //   effect.DiffuseColor = new Vector3(1, 0, 0);
                    
                    
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = modelAbsTrans[0]
                        *
                        Matrix.CreateScale(30) 

                        * Matrix.CreateTranslation(
                        //by³o depth*=100
                        -1*depth*2000, //to (*100) daje bardzo ³adne wyniki dla 1 m... niby wszystkie w metrach, ale coœ nie bangla chyba
                     -1*which.Y*2000,//1*which.Y*400.0f, //100-Y*2.5
                     -1*which.X * 2000.0f //120-X*2.5
                     //centrum ekranu to 0,0
                     )
              //      320.0f - which.Y*2.0f,
              //          480.0f + -which.X*4.0f)//240.0-w.X*4
                        //  *
                       *Matrix.CreateScale(1.0f/(30)) 
                        ;
                  //  System.Console.WriteLine(depth); //depth jest w m (depth * 100 w cm)
                }

                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        public static void draw(ModelMesh mesh, float scaleLength, float rotationAddition,
            Vector2 upperInThisCase, Vector2 lowerInThisCase, Vector2 basePos, float baseDepth, float secondDepth, Matrix view, Matrix projection,
            Matrix[] modelAbsTrans)
        {
            float dX = upperInThisCase.X - lowerInThisCase.X;
            foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
            {
                effect.EnableDefaultLighting();
                effect.View = view;
                effect.Projection = projection;
                effect.Alpha = 0.8f;


                effect.World = modelAbsTrans[mesh.ParentBone.Index] *
                    Matrix.CreateScale(1, 1, scaleLength)  * Matrix.CreateTranslation(
                    0 - baseDepth * 100,
                    100.0f - basePos.Y * 2.5f,
                    120.0f + -basePos.X * 2.5f) *
                    Matrix.CreateRotationX
                    (
                    rotationAddition +
                    (float)Math.Atan
                            (
                                dX /
                                (upperInThisCase.Y - lowerInThisCase.Y)
                            )
                    ) *
                    Matrix.CreateRotationY
                    (
                    (float)Math.Atan
                            (
                                (baseDepth - secondDepth) /
                                dX/40.0f
                            )
                    )
                  
                  ;

            }
            mesh.Draw();
        }

        int i = -1;
        bool nagrywanie = true;
        //bool pureView = true;

        void render(Vector2 jointPosFromState, float jointDepthFromState, Vector2 jointPos, float jointDepth,
             Matrix view, Matrix proj, bool elements)
        {
            Vector3 jointNew = new Vector3(jointPosFromState.X, jointPosFromState.Y, jointDepthFromState);
            Vector2 correctPos = new Vector2(jointNew.X, jointNew.Y);
            float correctDepth = jointNew.Z;
            jointNew = Vector3.Transform(jointNew, Matrix.CreateTranslation(-30.01f, -10f, 0f));
            if (elements)
            {
                renderJoint(
                        new Vector2(jointNew.X, jointNew.Y), jointNew.Z,

                    view, proj, 1.5f, new Vector3(1, 0, 0), true); // mniej-wiêcej

                renderJoint(
                      new Vector2(jointPos.X, jointPos.Y), jointDepth,

                  view, proj, 1.5f, new Vector3(1, 0, 0), true); // mniej-wiêcej
            }
            Vector2 jointFinal = new Vector2((jointNew.X + jointPos.X) / 2.0f,
              (jointNew.Y + jointPos.Y) / 2.0f);
            float jointDepthFinal = (jointNew.Z + jointDepth) / 2.0f;

            renderJoint(
              new Vector2(jointFinal.X, jointFinal.Y), jointDepthFinal,
              view, proj, 1.5f, new Vector3(0, 1, 0), false); // mniej-wiêcej


        }
        // camera viewMatrix lookAt
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 1, 0)); //?
        static float kat = 43.0f; //k¹t widzenia w pionie
        float floatPi = (float)Math.PI;
        float floatPiPol = (float)Math.PI / 2.0f;
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(kat), 4f / 3f, 0.1f, 10000); //57.8
            
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Red);
                                            
            spriteBatch.Begin();
            spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 1280, 960), Color.White);

            float rot = floatPiPol;
            //rot =(float) Math.Atan(handPosition.X / handPosition.Y);
            if (handPosition.Y > 200)
                rot += floatPi;
            float scale = Math.Abs(handPosition.Y - 320.0f) / 100.0f;
            float scale2 = Math.Abs(hand2Position.Y - 320.0f) / 100.0f;

            #region jakies_spritey
            //System.Console.WriteLine(handPosition.Y);
            if (ok == true)
            {
                if (nad == false)
                {
                    spriteBatch.Draw(overlay,
                        new Rectangle((int)(2.5 * handPosition.X) + 0, (int)(handPosition.Y) + 200, (int)(640 * scale), 480),
                        null,
                        Color.LightGreen,
                        rot,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0
                        );
                    spriteBatch.Draw(overlay,
                        new Rectangle((int)(2.5 * hand2Position.X) + 0, (int)(hand2Position.Y) + 200, (int)(640 * scale2), 480),
                        null,
                        Color.White,
                        rot,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0
                        );
                }
            }
            // spriteBatch.DrawString(font, connectedStatus, new Vector2(20, 80), Color.White);
            spriteBatch.End();
            #endregion jakies_spritey

            renderJoint( //PUSTY
              new Vector2(1, 0), 0,
              view, projection, 1.0f, new Vector3(1, 0, 0), true); // mniej-wiêcej
                

            if (ok == true)
            {
                #region old
              //  renderJoint(hand2Position, hand2Depth, view, projection, 1, false);
               // renderJoint(elbowLposition, elbowLdepth, view, projection, 1, new Vector3(1,0,0));
               // renderJoint(armLposition, armLdepth, view, projection, 1, new Vector3(1, 0, 0));
               // renderJoint(headPosition, headDepth, view, projection, 1, new Vector3(1, 0, 0));

           //     renderJoint(elbowRposition, elbowRdepth, view, projection, 1, new Vector3(1, 0, 0), true);
           //     renderJoint(armRposition, armRdepth, view, projection, 1, new Vector3(1, 0, 0), true);
           //     renderJoint(handPosition, handDepth, view, projection, 1, new Vector3(1, 0, 0), true);
                #endregion old

                renderJoint( // ten. 
                  new Vector2(handPosition.X, handPosition.Y), handDepth,
                  view, projection, 1.0f, new Vector3(1, 0.5f, 0.5f), false); // mniej-wiêcej
                renderJoint( // ten. 
                  new Vector2(elbowRposition.X, elbowRposition.Y), elbowRdepth,
                  view, projection, 1.0f, new Vector3(0.5f, 1.0f, 0.5f), false); // mniej-wiêcej
                  
                //drugi Kinect
                   BodyState bs3 = new BodyState(result);
              
                   //render(correctPositionHand, bs3.handRight.Z, handPosition, handDepth, view, projection, false);

                   Vector2 correctPositionElbow = positionFromJoint(new Vector2(bs3.elbowRight.X, bs3.elbowRight.Y));
                   float correctDepthElbow = bs3.elbowRight.Z;
                   Vector3 rightElbowNew = new Vector3(correctPositionElbow.X, correctPositionElbow.Y, correctDepthElbow);
                //render(correctPositionHand, bs3.elbowRight.Z, elbowRposition, elbowRdepth, view, projection, false
                   Vector2 correctPositionArm = positionFromJoint(new Vector2(bs3.armRight.X, bs3.armRight.Y));
                 //  render(correctPositionArm, bs3.armRight.Z, armRposition, armRdepth, view, projection, false);

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                   Vector2 correctPositionHand = positionFromJoint(new Vector2(bs3.handRight.X, bs3.handRight.Y));
                   float correctDepthHand = bs3.handRight.Z;
                   Vector3 rightHandNew = new Vector3(correctPositionHand.X, correctPositionHand.Y, correctDepthHand);
                
                //wybraæ coœ z tego:

                //transformacja: 50 [cm],  15-20 (70-75) stopni
                
              //  rightHandNew = Vector3.Transform(rightHandNew, Matrix.CreateRotationY((float)(MathHelper.ToRadians(2))));
           
                //mo¿e to nie jest do koñca ok -> jakiœ kompromis?
                rightHandNew = Vector3.Transform(rightHandNew, Matrix.CreateRotationY((float)(MathHelper.ToRadians(30))));
                rightHandNew = Vector3.Transform(rightHandNew, Matrix.CreateTranslation(-0.5f, 0f, 0f));//50 //cm (jakiœ przelicznik x3?)

                rightElbowNew = Vector3.Transform(rightElbowNew, Matrix.CreateRotationY((float)(MathHelper.ToRadians(30))));
                rightElbowNew = Vector3.Transform(rightElbowNew, Matrix.CreateTranslation(-0.5f, 0f, 0f));//50 //cm (jakiœ przelicznik x3?)
                

                //  rightHandNew = Vector3.Transform(rightHandNew, Matrix.CreateTranslation(-0.0f,0.5f,0.0f));
              
                
                //  correctPositionHand = new Vector2(rightHandNew.X, rightHandNew.Y);
              //  correctDepthHand = rightHandNew.Z;
                
                  renderJoint(
                      new Vector2(rightHandNew.X, rightHandNew.Y),   rightHandNew.Z,  //NIE OK
                      view, projection, 1.0f, new Vector3(1, 0, 0), true); // mniej-wiêcej

                  renderJoint(
                      new Vector2(rightElbowNew.X, rightElbowNew.Y), rightElbowNew.Z,  //NIE OK
                      view, projection, 1.0f, new Vector3(0, 1, 0), true); // mniej-wiêcej

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


               // Vector2 rightHandFinal = new Vector2((rightHandNew.X + handPosition.X) / 2.0f,
               //       (rightHandNew.Y + handPosition.Y) / 2.0f);
               //   float rightHandDepthFinal = (rightHandNew.Z + handDepth) / 2.0f;

              //    renderJoint(
              //      new Vector2(rightHandFinal.X, rightHandFinal.Y), rightHandDepthFinal,
              //      view, projection, 1.5f, new Vector3(0, 0, 1), true); // mniej-wiêcej
        

            //    //wypadkowy
            //if (!pureView)
            //    renderJoint(correctPositionElbow,
            //        correctDepthElbow, view, projection, 1, new Vector3(0, 1, 0), true);

            //    //wypadkowy

            //    if (elbowRposition.X != 0) //0, gdy b³¹d, chocia¿ te¿ na oœce
            //        oldOneElbow = (correctPositionElbow + elbowRposition) / 2.0f; //uœrednienie wyników
            //    float depthResElbow = (correctDepthElbow + elbowRdepth) / 2.0f;
            //    if (depthResElbow != 0) //0, gdy b³¹d (poza takim przypadkiem, raczej nie 0)
            //        oldDepthElbow = depthResElbow;

           //     renderJoint(oldOneElbow,
           //             oldDepthElbow, view, projection, 1, new Vector3(1, 1, 1), false);
                /////////////////////////////////

           //     Vector2 correctPositionArm = positionFromJoint(new Vector2(bs3.armRight.X, bs3.armRight.Y));
           //     float correctDepthArm = bs3.armRight.Z;
           //     if (!pureView)
           //     renderJoint(correctPositionArm,
           //         correctDepthArm, view, projection, 1, new Vector3(0, 1, 0), true);

           //     //wypadkowy
           //     //tu bad:
           //     if (armRposition.X != 0) //0, gdy b³¹d, chocia¿ te¿ na oœce
           //         oldOneArm = (correctPositionArm + armRposition) / 2.0f; //uœrednienie wyników
           //     float depthResArm = (correctDepthArm + armRdepth) / 2.0f;
           //     if (depthResArm != 0) //0, gdy b³¹d (poza takim przypadkiem, raczej nie 0)
           //         oldDepthArm = depthResArm;

           ////     renderJoint(oldOneArm,
           ////             oldDepthArm, view, projection, 1, new Vector3(1, 1, 1), false);
           //     //handPosition = oldOneHand;
           //     elbowRposition = oldOneElbow;
           //     armRposition = oldOneArm;
           //     //handDepth = oldDepthHand;
           //     elbowRdepth = oldDepthElbow;
           //     armRdepth = oldDepthArm;
           //     /////////////////////////////////

                #region stare
                //foreach (ModelMesh mesh in mdRH.Meshes) //right hand
                //{
                //    //This is where the mesh orientation is set
                //    foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                //    {
                //        //effect.CurrentTechnique //AM
                //        effect.EnableDefaultLighting();
                //        effect.AmbientLightColor = new Vector3(0, 1, 0);
                //        effect.View = view;
                //        effect.Projection = projection;

                //        effect.World = modelAbsTrans[mesh.ParentBone.Index] * Matrix.CreateTranslation(
                //            0 - handDepth * 100,
                //            120.0f - handPosition.Y * 0.6f,
                //            160.0f + -handPosition.X * 0.5f)
                //            *
                //            Matrix.CreateScale(4);

                //    }

                //    //Draw the mesh, will use the effects set above.
                //    mesh.Draw();
                //}

                //foreach (ModelMesh mesh in mdRH.Meshes) //right elbow
                //{
                //    //This is where the mesh orientation is set
                //    foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                //    {
                //        //effect.CurrentTechnique //AM

                //        effect.EnableDefaultLighting();
                //        effect.View = view;
                //        effect.Projection = projection;

                //        effect.World = modelAbsTrans[mesh.ParentBone.Index] * Matrix.CreateTranslation(
                //            0 - elbowRdepth * 100,
                //            120.0f - elbowRposition.Y * 0.6f,
                //            160.0f + -elbowRposition.X * 0.5f)
                //            *
                //            Matrix.CreateScale(4);

                //    }

                //    //Draw the mesh, will use the effects set above.
                ////    mesh.Draw();
                //}

                //foreach (ModelMesh mesh in mdRH.Meshes) //left hand
                //{
                //    //This is where the mesh orientation is set
                //    foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                //    {
                //        //effect.CurrentTechnique //AM

                //        effect.EnableDefaultLighting();
                //        effect.View = view;
                //        effect.Projection = projection;

                //        effect.World = modelAbsTrans[mesh.ParentBone.Index] * Matrix.CreateTranslation
                //            (0 - hand2Depth * 100,
                //            120.0f - hand2Position.Y * 0.6f,
                //            160.0f + -hand2Position.X * 0.5f) *
                //            Matrix.CreateScale(3);

                //    }

                //    //Draw the mesh, will use the effects set above.
                //    mesh.Draw();
                //}
                //foreach (ModelMesh mesh in mdRH.Meshes) //left elbow
                //{
                //    //This is where the mesh orientation is set
                //    foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                //    {
                //        //effect.CurrentTechnique //AM

                //        effect.EnableDefaultLighting();
                //        effect.View = view;
                //        effect.Projection = projection;

                //        effect.World = modelAbsTrans[mesh.ParentBone.Index] * Matrix.CreateTranslation(
                //            0 - elbowRdepth * 100,
                //            120.0f - elbowLposition.Y * 0.6f,
                //            150.0f + -elbowLposition.X * 0.5f)
                //            *
                //            Matrix.CreateScale(4);

                //    }

                //    //Draw the mesh, will use the effects set above.
                //    mesh.Draw();
                //}

                #endregion stare
                #region stare2
                //foreach (ModelMesh mesh in mdRH.Meshes) //head
                //{
                //    //This is where the mesh orientation is set
                //    foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                //    {
                //        effect.EnableDefaultLighting();
                //        effect.View = view;
                //        effect.Projection = projection;

                //        effect.World = modelAbsTrans[mesh.ParentBone.Index] * Matrix.CreateTranslation(
                //            0 - headDepth * 100,
                //            100.0f - headPosition.Y * 0.5f,
                //            150.0f + -headPosition.X * 0.5f)
                //            *
                //            Matrix.CreateScale(9);
                //    }

                //    //Draw the mesh, will use the effects set above.
                //   // mesh.Draw();
                //}
                ///////
                //klocek prawego przedramienia
                #endregion stare2

                float kat2 = 0;

                float katRamienia = 0;

                float katPrzestrzenny = 0;
                Vector3 hand = new Vector3(handPosition.X, handPosition.Y, handDepth);
                Vector3 elbow = new Vector3(elbowRposition.X, elbowRposition.Y, elbowRdepth);
                Vector3 arm = new Vector3(armRposition.X, armRposition.Y, armRdepth);
                Vector3 toHand = -elbow+hand;
                Vector3 toArm = -elbow+arm;
                float res=0;
                res = Vector3.Dot(toArm, toHand);
                float handL = toHand.Length();
                float armL = toArm.Length();
                katPrzestrzenny = (float)(Math.Acos(res/(handL*armL))*180.0/Math.PI);

                spriteBatch.Begin();

                if (nagrywanie)
                {
                    float tempKat = (float)Math.Atan(
                                                 (armRposition.X - elbowRposition.X) /
                                                 (armRposition.Y - elbowRposition.Y)
                                              );
                    kat = (
                          (float)Math.Atan(
                                                 (elbowRposition.X - handPosition.X) /
                                                 (elbowRposition.Y - handPosition.Y)
                                              ) - tempKat
                                              );
                    kat = kat * 180.0f / floatPi;
                    kat = (float)Math.Round(kat, 0);
                    if (handPosition.Y < elbowRposition.Y && kat < 0)
                    {
                        // kat *= -1;
                        kat += 180;
                    }

                    //k¹t do rsunku k¹ta:
                    katRamienia = tempKat;
                    katRamienia = katRamienia * 180.0f / floatPi;
                    katRamienia = (float)Math.Round(katRamienia, 0);

                    // kat -= 90;
                    #region prawePrzedramie
                    if (handPosition.Y < elbowRposition.Y) // na górze
                    {

                        foreach (ModelMesh mesh in mdBox.Meshes)
                        {
                            #region old
                            //This is where the mesh orientation is set
                            //    foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                            //    {
                            //        //effect.CurrentTechnique //AM

                            //        effect.EnableDefaultLighting();
                            //        effect.View = view;
                            //        effect.Projection = projection;

                            //        effect.World = modelAbsTrans[mesh.ParentBone.Index] *
                            //           Matrix.CreateScale(2, 2, 30) *
                            //           Matrix.CreateRotationX
                            //           ((float)(Math.PI / 2.0) +
                            //           (float)Math.Atan(
                            //                       (elbowRposition.X - handPosition.X) /
                            //                       (elbowRposition.Y - handPosition.Y)
                            //                    ))
                            //           * Matrix.CreateTranslation(
                            //           0 - elbowRdepth * 100,
                            //           120.0f - elbowRposition.Y * 0.6f,
                            //           160.0f + -elbowRposition.X * 0.5f);

                            //    }

                            //    //Draw the mesh, will use the effects set above.
                            //    mesh.Draw();
                            #endregion old
                    ////  TO JEST OK, KLOCEK:
                    //        draw(mesh, 30, (float)(Math.PI / 2.0), elbowRposition, handPosition,
                    //            elbowRposition, elbowRdepth, handDepth,
                    //             view, projection,
                    //            modelAbsTrans);
                        }

                    }
                    else //na dole
                    {
                        foreach (ModelMesh mesh in mdBox.Meshes)
                        {
                            #region old
                            ////This is where the mesh orientation is set
                            //foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                            //{
                            //    //effect.CurrentTechnique //AM

                            //    effect.EnableDefaultLighting();
                            //    effect.View = view;
                            //    effect.Projection = projection;

                            //    effect.World = modelAbsTrans[mesh.ParentBone.Index] *
                            //       Matrix.CreateScale(2, 2, 30) *
                            //       Matrix.CreateRotationX
                            //       (
                            //       -(float)(Math.PI/2.0)+
                            //       (float)Math.Atan(
                            //                   (elbowRposition.X - handPosition.X) /
                            //                   (elbowRposition.Y - handPosition.Y)
                            //                ))
                            //       *
                            //        // problem g³êbokoœci (ta druga oœ obrotu)
                            //    #region dupa
                            //        /*
                            //       Matrix.CreateRotationZ
                            //       (
                            //       -(float)(Math.PI / 2.0) +
                            //       (float)Math.Atan(
                            //                   0.1f*(elbowRdepth - handDepth) /
                            //                   (elbowRposition.X - handPosition.X)
                            //                ))

                            //       *
                            //  */
                            //    #endregion dupa
                            //       Matrix.CreateTranslation(
                            //       0 - elbowRdepth * 100,
                            //       120.0f - elbowRposition.Y * 0.6f,
                            //       160.0f + -elbowRposition.X * 0.5f);

                            //}

                            ////Draw the mesh, will use the effects set above.
                            //mesh.Draw();
                            #endregion old
                        //    // TO JEST OK, KLOCEK:
                        //    if (nagrywanie)
                        //        draw(mesh, 30, -(float)(Math.PI / 2.0), elbowRposition, handPosition, elbowRposition,
                        //     elbowRdepth,handDepth, view, projection, modelAbsTrans);
                        //    else
                        //        draw(mesh, 30, -(float)(Math.PI / 2.0) + angles[i], elbowRposition, handPosition, elbowRposition,
                        //elbowRdepth,handDepth, view, projection, modelAbsTrans);



                        }
                    }

                    #endregion prawePrzedramie
                    ///////
                    //klocek prawego ramienia
                    #region praweRamie
                    if (elbowRposition.Y < armRposition.Y) // na górze!
                    {
                        foreach (ModelMesh mesh in mdBox.Meshes)
                        {
                            #region old
                            ////This is where the mesh orientation is set
                            //foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                            //{
                            //    //effect.CurrentTechnique //AM

                            //    effect.EnableDefaultLighting();
                            //    effect.View = view;
                            //    effect.Projection = projection;

                            //    effect.World = modelAbsTrans[mesh.ParentBone.Index] *
                            //       Matrix.CreateScale(2, 2, 18) *
                            //       Matrix.CreateRotationX
                            //       (
                            //       -(float)(Math.PI / 2.0) +
                            //       (float)Math.Atan(
                            //                   (armRposition.X - elbowRposition.X) /
                            //                   (armRposition.Y - elbowRposition.Y)
                            //                ))
                            //       * Matrix.CreateTranslation(
                            //       
                            #endregion old
                            // TO JEST OK, KLOCEK:
                            //draw(mesh, 18, -(float)(Math.PI / 2.0), armRposition, elbowRposition,
                            //    elbowRposition, elbowRdepth, view, projection, modelAbsTrans);
                        }

                    }
                    else //na dole!
                    {
                        foreach (ModelMesh mesh in mdBox.Meshes)
                        {
                            // TO JEST OK, KLOCEK:
                            //draw(mesh, 18, (float)(Math.PI / 2.0), elbowRposition, armRposition,
                            //    elbowRposition,
                            //    elbowRdepth,
                            //    view, projection, modelAbsTrans);
                            #region old
                            ////This is where the mesh orientation is set
                            //foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                            //{
                            //    //effect.CurrentTechnique //AM

                            //    effect.EnableDefaultLighting();
                            //    effect.View = view;
                            //    effect.Projection = projection;

                            //    effect.World = modelAbsTrans[mesh.ParentBone.Index] *
                            //       Matrix.CreateScale(2, 2, 18) *
                            //       Matrix.CreateRotationX
                            //       (
                            //       (float)(Math.PI / 2.0) +
                            //       (float)Math.Atan(
                            //                   (armRposition.X - elbowRposition.X) /
                            //                   (armRposition.Y - elbowRposition.Y)
                            //                ))
                            //       *
                            //        // problem g³êbokoœci (ta druga oœ obrotu)?

                            //       Matrix.CreateTranslation(
                            //       0 - elbowRdepth * 100,
                            //       120.0f - elbowRposition.Y * 0.6f,
                            //       160.0f + -elbowRposition.X * 0.5f);

                            //}

                            ////Draw the mesh, will use the effects set above.
                            //mesh.Draw();
                            #endregion old
                        }
                    }
                    //prawy obojczyk
                    #endregion praweRamie
                    #region prawyobojczyk
                    //     if (armRposition.Y < centerPosition.Y) // na górze!
                    //     {
                    //         foreach (ModelMesh mesh in mdBox.Meshes)
                    //         {
                    //             #region old
                    //             ////This is where the mesh orientation is set
                    //             //foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                    //             //{
                    //             //    //effect.CurrentTechnique //AM

                    //             //    effect.EnableDefaultLighting();
                    //             //    effect.View = view;
                    //             //    effect.Projection = projection;

                    //             //    effect.World = modelAbsTrans[mesh.ParentBone.Index] *
                    //             //       Matrix.CreateScale(2, 2, 18) *
                    //             //       Matrix.CreateRotationX
                    //             //       (
                    //             //       -(float)(Math.PI / 2.0) +
                    //             //       (float)Math.Atan(
                    //             //                   (armRposition.X - elbowRposition.X) /
                    //             //                   (armRposition.Y - elbowRposition.Y)
                    //             //                ))
                    //             //       * Matrix.CreateTranslation(
                    //             //       
                    //             #endregion old
                    //             draw(mesh, 10, -(float)(Math.PI / 2.0), centerPosition, armRposition,
                    //                 armRposition, elbowRdepth, view, projection, modelAbsTrans);
                    //         }

                    //     }
                    //     else //na dole!
                    //     {
                    //         foreach (ModelMesh mesh in mdBox.Meshes)
                    //         {
                    //             draw(mesh, 10, (float)(Math.PI / 2.0), armRposition, centerPosition,
                    //                armRposition, elbowRdepth, view, projection, modelAbsTrans);

                    //         }
                    //     }
                    #endregion prawyobojczyk
                    angle = (720 - (int)kat - 180) / 2;
                    angle2 = -(int)katRamienia;
                    string temp = "" + kat;

                    #region nagrywanie
                    if(false)  /////////////////////////////////////////////////////////////////////////////////////////////////cyk
                    if (5 < gameTime.TotalGameTime.Seconds && gameTime.TotalGameTime.Seconds <= 115) //nagrywanie
                    {
                        string temps = "";
                        if (gameTime.TotalGameTime.Milliseconds % 200 < 20) // tu trzeba dodaæ flagê, ¿eby w danym interwale tylko raz czyta³o
                        {
                            float f = new float();
                            f += kat;
                            angles.Add(f);

                        }
                        //  if (!temp.Contains(char.ConvertFromUtf32(0x0105)))
                        spriteBatch.DrawString(sf, "Rec", new Vector2(0, 200), Color.White);
                    }
                    #endregion nagrywanie
                    if (!temp.Contains(char.ConvertFromUtf32(0x0105)))
                    {
                      //  spriteBatch.DrawString(sf, "Right arm - right forearm angle:" + kat, new Vector2(10, 10), Color.Black);

                        spriteBatch.DrawString(sf, "Right arm - right forearm angle:" + (int)katPrzestrzenny, new Vector2(10, 10), Color.White);
                        spriteBatch.DrawString(sf, "" + (int)katPrzestrzenny, new Vector2(10, 90), Color.White);

                        // else
                        //   spriteBatch.DrawString(sf, "Right arm - right forearm angle:" + kat, new Vector2(armRposition.X, 750), Color.White);

                        spriteBatch.DrawString(sf, "o", new Vector2(920, 0), Color.White);

                    }

                }
                #region odtworzenie
                string temps2 = "";
                if (25 < gameTime.TotalGameTime.Seconds && gameTime.TotalGameTime.Seconds <= 15) //odt //25 -> 15
                {
                    nagrywanie = false;

                    //    string temps = "";
                    if (gameTime.TotalGameTime.Milliseconds % 200 < 20)
                    {
                        //    angles.Add(kat);
                        i++;
                        //       if (!temp.Contains(char.ConvertFromUtf32(0x0105)))
                        if (i < angles.Count && i >= 0)
                            temps2 = "Odtworzenie:" + angles[i];
                    }
                    else
                    {
                        if (i < angles.Count && i >= 0)
                            temps2 = "Odtworzenie: " + angles[i];
                        #region prawePrzedramie
                        if (elbowRposition.Y < armRposition.Y) // na górze
                        {

                            foreach (ModelMesh mesh in mdBox.Meshes)
                            {
                    //            if (i > 0 && i < angles.Count)
                    //                draw(mesh, 30, (float)(Math.PI) + angles[i], elbowRposition, handPosition,
                    //                    elbowRposition, elbowRdepth,handDepth,
                    //                     view, projection,
                    //                    modelAbsTrans);
                            }

                        }
                        else //na dole
                        {
                            foreach (ModelMesh mesh in mdBox.Meshes)
                            {
                     //           if (i > 0 && i < angles.Count)
                     //               draw(mesh, 30, (float)(Math.PI) + angles[i], elbowRposition, handPosition, elbowRposition,
                     //     elbowRdepth, handDepth, view, projection, modelAbsTrans);



                      }
                }

                        #endregion prawePrzedramie
                    }

                    if (!temps2.Contains(char.ConvertFromUtf32(0x0105)))
                        //   if(i<angles.Count && i>=0)
                        spriteBatch.DrawString(sf, temps2, new Vector2(0, 200), Color.White);
                }
                #endregion odtworzenie
                spriteBatch.End();
             //   spriteBatch.Begin();
                //    string temp2 = "lala";
                //  if (!temp.Contains(char.ConvertFromUtf32(0x0105)))
                //      spriteBatch.DrawString(sf, "" + temp2, new Vector2(armRposition.X, armRposition.Y), Color.White);
             //   spriteBatch.End();

            }

            //katy!

            // updateVertices(angle, angle2,);
          //  updateVertices(angle, angle2, (armRposition.X * 0.5f - 160.0f) / 320.0f, (60 - armRposition.Y * 0.5f) / 120.0f);

            effect2.CurrentTechnique = effect2.Techniques["Pretransformed"];

            foreach (EffectPass pass in effect2.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            if (angle - 181 <= 0)
                angle = 182;
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, angle * 2 - 362, VertexPositionColor.VertexDeclaration);

            ///////////////
            base.Draw(gameTime);
        }
    }
}
