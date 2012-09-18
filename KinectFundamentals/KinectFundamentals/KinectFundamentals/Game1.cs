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
using System.IO;

namespace KinectFundamentals
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch spriteBatch2;
        Texture2D kinectRGBVideo;
        

        Vector2 handPosition = new Vector2();
        float handDepth = 0.0f;
        //Vector2 overPosition = new Vector2();
        Vector2 hand2Position = new Vector2();
        Vector2 elbowRposition = new Vector2();
        float elbowRdepth;

        Vector2 armRposition = new Vector2();
        float armRdepth;
        
        //do pisania:
        SpriteFont sf;


        KinectSensor kinectSensor;


        bool nad = true; //czy ramiona s¹ nad d³oñmi
        SpriteFont font;
        //MODEL
        Model mdRH;
        Model mdBox;

        Model mdArrow;
        // Effect effect;

        ///////// secondary kinect
      //  string pos = "";
        string result = "";


        private Client pipeClient;
        /*
        void DisplayReceivedMessage(string message)
        {
            result = message;
        }
        */
        void pipeClient_MessageReceived(string message)
        {
            // this.Invoke(new Client.MessageReceivedHandler(DisplayReceivedMessage),
            //     new object[] { message });
            //DisplayReceivedMessage(message);
            result = message;
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
            vertices = new VertexPositionColor[verts * 2];
        }
        #endregion k¹ty

        List<float> angles;

        private Model LoadModel(string assetName)
        {
            Model newModel = Content.Load<Model>(assetName);

           return newModel;
        }

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
                Smoothing = 0.4f, //0.5
                Correction = 1.0f,
                Prediction = 0.5f,
                JitterRadius = 0.1f, //0.05f
                MaxDeviationRadius = 0.04f
                //was:
                //Smoothing = 0.2f, //0.5
                //Correction = 0.5f,
                //Prediction = 0.5f,
                //JitterRadius = 0.1f, //0.05f
                //MaxDeviationRadius = 0.04f
            });
            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);
             
            try
            {
                kinectSensor.Start();
            }
            catch
            {
             //   connectedStatus = "Unable to start the Kinect Sensor";
                ShowMissingRequirementMessage(new Exception("Nie ma Kinecta!"));
        
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
                        handPosition = positionFromJoint(rightHand);// Vector2((((0.5f * rightHand.Position.X) + 0.5f) * (640)), (((-0.5f * rightHand.Position.Y) + 0.5f) * (480)));
                        handDepth = rightHand.Position.Z;//
                        
                        Joint rightElbow = playerSkeleton.Joints[JointType.ElbowRight];
                        elbowRposition = positionFromJoint(rightElbow);// Vector2((((0.5f * rightElbow.Position.X) + 0.5f) * (640)), (((-0.5f * rightElbow.Position.Y) + 0.5f) * (480)));
                        elbowRdepth = rightElbow.Position.Z;

                        Joint rightArm = playerSkeleton.Joints[JointType.ShoulderRight];
                        armRposition = positionFromJoint(rightArm);// Vector2((((0.5f * rightElbow.Position.X) + 0.5f) * (640)), (((-0.5f * rightElbow.Position.Y) + 0.5f) * (480)));
                        armRdepth = rightArm.Position.Z;

                        Joint leftHand = playerSkeleton.Joints[JointType.HandLeft];
                        hand2Position = positionFromJoint(leftHand);
                        //centerPosition = positionFromJoint(center);
                        //centerDepth = center.Position.Z;

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
                        //Joint head = playerSkeleton.Joints[JointType.Head];
                        //headPosition = positionFromJoint(head);// Vector2((((0.5f * head.Position.X) + 0.5f) * (640)), (((-0.5f * head.Position.Y) + 0.5f) * (480)));
                        //headDepth = head.Position.Z;
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
            try
            {
                KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
                DiscoverKinectSensor();
            }
            catch (Exception e)
            {
                ShowMissingRequirementMessage(new Exception("Nie ma Kinecta"));
            }
            base.Initialize();
        }
        Matrix[] modelAbsTrans;
        Texture2D overlay;
        Texture2D hand;
        Texture2D hand2;
        Texture2D rec;
        Texture2D cursor;
        Texture2D play;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch2 = new SpriteBatch(GraphicsDevice);
            kinectRGBVideo = new Texture2D(GraphicsDevice, 1337, 1337);

            overlay = Content.Load<Texture2D>("overlay2");
            hand = Content.Load<Texture2D>("hand");
            hand2 = Content.Load<Texture2D>("hand");
            font = Content.Load<SpriteFont>("SpriteFont1");
            cursor = Content.Load<Texture2D>("cursor");
            play = Content.Load<Texture2D>("play");
            mdArrow = LoadModel("arrow2");

            rec = Content.Load<Texture2D>("rec");
            

            //effect = Content.Load<Effect>("effects");
            mdRH = LoadModel("sphere2");
            mdBox = LoadModel("cylinder");
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

      
        void renderArrow(Vector3 where, Vector3 fromWhere, Matrix view, Matrix projection, float scale, Vector3 color, bool alpha)
        {
            Vector3 dirWanted = where - fromWhere;
            float temp = dirWanted.Z;
            dirWanted.Z = dirWanted.X;
            dirWanted.X = temp;

            dirWanted.Normalize();
            Matrix finalOrientation = Matrix.CreateWorld(where, dirWanted, Vector3.Up);

            foreach (ModelMesh mesh in mdArrow.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                {

                    //effect.CurrentTechnique //AM
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = color;
                    effect.DiffuseColor = color; effect.View = view; effect.Projection = projection;
                    effect.World = modelAbsTrans[0]
                       * Matrix.CreateScale(10)
                           * Matrix.CreateFromYawPitchRoll(0, -MathHelper.ToRadians(90), 0)
                      * finalOrientation
                        *
                        Matrix.CreateTranslation(
                        -1 * where.Z * 2000, //to (*100) daje bardzo ³adne wyniki dla 1 m... niby wszystkie w metrach, ale coœ nie bangla chyba
                     -1 * where.Y * 2000,//1*which.Y*400.0f, //100-Y*2.5
                     -1 * where.X * 2000.0f //120-X*2.5
                     )
                      *
                        Matrix.CreateScale(1f / 10f)
                        ;
                }

                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
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
                    effect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);


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
                        Matrix.CreateScale(20) 

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
                       *Matrix.CreateScale(1.0f/(20)) 
                        ;
                  //  System.Console.WriteLine(depth); //depth jest w m (depth * 100 w cm)
                }

                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }


        void renderLine(Vector3 nr1, Vector3 nr2, Matrix view, Matrix projection, float scale, Vector3 color, bool alpha)
        {
            float odl = (float)Math.Sqrt(
                (nr1.X - nr2.X) * (nr1.X - nr2.X) +
                (nr1.Y - nr2.Y) * (nr1.Y - nr2.Y) +
                (nr1.Z - nr2.Z) * (nr1.Z - nr2.Z));
            float dX = nr1.X - nr2.X;
            float dY = nr1.Y - nr2.Y;
            float dZ = nr1.Z - nr2.Z;

            foreach (ModelMesh mesh in mdBox.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                {

                    Vector3 dirWanted = new Vector3(dZ, dY, dX);
                    float len = dirWanted.Length();
                    dirWanted.Normalize();
                    Matrix finalOrientation = Matrix.CreateWorld(nr2, -dirWanted, -Vector3.Up);

                    //effect.CurrentTechnique //AM
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
                    effect.DiffuseColor = color; effect.View = view; effect.Projection = projection;
                    effect.World = modelAbsTrans[0]
                        *
                        Matrix.CreateScale(20)
                        *
                        Matrix.CreateScale(1, odl * 40, 1)
                        *
                        Matrix.CreateFromYawPitchRoll(0, MathHelper.ToRadians(90), 0)
                       * finalOrientation

                        * Matrix.CreateTranslation(
                        -1 * nr1.Z * 2000, //to (*100) daje bardzo ³adne wyniki dla 1 m... niby wszystkie w metrach, ale coœ nie bangla chyba
                     -1 * nr1.Y * 2000,//1*which.Y*400.0f, //100-Y*2.5
                     -1 * nr1.X * 2000.0f //120-X*2.5
                     ) * Matrix.CreateScale(1.0f / (20)) 
                    
                        ;
                }

                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        void drawArc(Vector3 p1, Vector3 middle, Vector3 p2)
        {
            Vector3 j1 = p1;
            Vector3 j2 = p2;
            Vector3 j = middle;

            Vector3 prostopadly = Vector3.Cross(j2 - j, j1 - j);//wokó³ niego obrót

            Vector3 vec1 = j1 - j;
            Vector3 vec2 = j2 - j;
            float kat = (float)Math.Acos(Vector3.Dot(vec1, vec2) / (vec1.Length() * vec2.Length()));

            prostopadly.Normalize();
            // prostopadly /= 10.0f;
            float n = 90;
            //  renderLine(j, j+prostopadly,
            Vector3 nor = (j1 - j) / 2f;
            nor.Normalize();
            for (int i = 0; i < (int)n; i++)
            {
                Vector3 srodek = Vector3.Transform(j + nor / 4.0f,
                    //wybrano j1, ale mo¿e byæ sta³a odleg³oœæ
                    Matrix.CreateTranslation(-j) *
                    Matrix.CreateFromAxisAngle(prostopadly, -i / n * kat) *
                    Matrix.CreateTranslation(j)
                    );
                renderJoint(new Vector2(srodek.X, srodek.Y), srodek.Z, view, projection,
                    1f, new Vector3(0, 0, 1), false);
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

        //void render(Vector2 jointPosFromState, float jointDepthFromState, Vector2 jointPos, float jointDepth,
        //     Matrix view, Matrix proj, bool elements)
        //{
        //    Vector3 jointNew = new Vector3(jointPosFromState.X, jointPosFromState.Y, jointDepthFromState);
        //    Vector2 correctPos = new Vector2(jointNew.X, jointNew.Y);
        //    float correctDepth = jointNew.Z;
        //    jointNew = Vector3.Transform(jointNew, Matrix.CreateTranslation(-30.01f, -10f, 0f));
        //    if (elements)
        //    {
        //        renderJoint(
        //                new Vector2(jointNew.X, jointNew.Y), jointNew.Z,

        //            view, proj, 1.5f, new Vector3(1, 0, 0), true); // mniej-wiêcej

        //        renderJoint(
        //              new Vector2(jointPos.X, jointPos.Y), jointDepth,

        //          view, proj, 1.5f, new Vector3(1, 0, 0), true); // mniej-wiêcej
        //    }
        //    Vector2 jointFinal = new Vector2((jointNew.X + jointPos.X) / 2.0f,
        //      (jointNew.Y + jointPos.Y) / 2.0f);
        //    float jointDepthFinal = (jointNew.Z + jointDepth) / 2.0f;

        //    renderJoint(
        //      new Vector2(jointFinal.X, jointFinal.Y), jointDepthFinal,
        //      view, proj, 1.5f, new Vector3(0, 1, 0), false); // mniej-wiêcej


        //}
        // camera viewMatrix lookAt
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 1, 0)); //?
        static float kat = 45.8f; //k¹t widzenia w pionie 43?
        float floatPi = (float)Math.PI;
        float floatPiPol = (float)Math.PI / 2.0f;
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(kat), 60.8f / 45.8f, 0.1f, 10000); //57.8
            

        Vector2 correctPositionElbow;// = positionFromJoint(new Vector2(bs3.elbowRight.X, bs3.elbowRight.Y));
                float correctDepthElbow;// = bs3.elbowRight.Z;
                Vector3 rightElbowNew;// = new Vector3(bs3.elbowRight.X, bs3.elbowRight.Y, correctDepthElbow);
               // rightElbowNew = TransformHelper.transformToFirstKinect(new Vector3(correctPositionElbow, correctDepthElbow));
                Vector2 cursorPos = new Vector2(0,0);
                int initialTopRec = 200;
                int initialTopPlay = 200;
                bool recDown = false;
                bool playDown = false;


                int licznikod50do100 = 0;
                string[] linie = new string[100];
                string[] wczytane = new string[100];
                float[] wczytaneNum = new float[100];


                int i4 = 0;
                int j4 = 0;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);
                                            
            spriteBatch.Begin();
            try
            {
                spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 1280, 960), Color.White);
            }
            catch (Exception ex)
            {
                ShowMissingRequirementMessage(new Exception("Nie ma Kinecta"));
        
            } float rot = floatPiPol;
            //rot =(float) Math.Atan(handPosition.X / handPosition.Y);
            if (handPosition.Y > 200)
                rot += floatPi;
            float scale = Math.Abs(handPosition.Y - 320.0f) / 100.0f;
            float scale2 = Math.Abs(hand2Position.Y - 320.0f) / 100.0f;

            #region controlSprites
             spriteBatch.Draw(rec,
                        new Rectangle( 200, initialTopRec, 100, 100),
                        Color.Red
                        );

             spriteBatch.Draw(play,
                         new Rectangle(600, initialTopPlay, 100, 100),
                         Color.Green
                         );

            spriteBatch.Draw(cursor,
                new Rectangle((int)(640+(hand2Position.X)*640*2), (int)(480+(hand2Position.Y)*480*2), 100, 100),
                         Color.Green
                         );
            if (Math.Abs((int)(640 + (hand2Position.X) * 640 * 2) - 200) < 30 &&
                Math.Abs((int)(480 + (hand2Position.Y) * 480 * 2) - 200) < 30)
            {
                recDown = true;
                playDown = false;
            }
            if (Math.Abs((int)(640 + (hand2Position.X) * 640 * 2) - 600) < 30 &&
                Math.Abs((int)(480 + (hand2Position.Y) * 480 * 2) - 200) < 30)
            {
                recDown = false;
                playDown = true;
            }
            if (recDown == true)
                initialTopRec = 600;
            else
                initialTopRec = 200;
            if (playDown == true)
                initialTopPlay = 600;
            else
                initialTopPlay = 200;
            //System.Console.WriteLine(handPosition.Y);
            //if (ok == true)
            //{
            //    if (nad == false)
            //    {
            //        spriteBatch.Draw(overlay,
            //            new Rectangle((int)(2.5 * handPosition.X) + 0, (int)(handPosition.Y) + 200, (int)(640 * scale), 480),
            //            null,
            //            Color.LightGreen,
            //            rot,
            //            new Vector2(0, 0),
            //            SpriteEffects.None,
            //            0
            //            );
            //        spriteBatch.Draw(overlay,
            //            new Rectangle((int)(2.5 * hand2Position.X) + 0, (int)(hand2Position.Y) + 200, (int)(640 * scale2), 480),
            //            null,
            //            Color.White,
            //            rot,
            //            new Vector2(0, 0),
            //            SpriteEffects.None,
            //            0
            //            );
            //    }
            //}
            #endregion controlSprites
            spriteBatch.End();
            

            if (ok == true)
            {
                //renderJoint( // ten. 
                //  new Vector2(handPosition.X, handPosition.Y), handDepth,
                //  view, projection, 1.0f, new Vector3(1, 0.0f, 0.0f), false); // jest mniej-wiêcej ok
                //drugi Kinect
                   BodyState bs3 = new BodyState(result);

                   #region niewazne
                   //Vector2 correctPositionElbow = positionFromJoint(new Vector2(bs3.elbowRight.X, bs3.elbowRight.Y));
                   //float correctDepthElbow = bs3.elbowRight.Z;
                   //Vector3 rightElbowNew = new Vector3(correctPositionElbow.X, correctPositionElbow.Y, correctDepthElbow);
                   //Vector2 correctPositionArm = positionFromJoint(new Vector2(bs3.armRight.X, bs3.armRight.Y));
                   #endregion niewazne


                // ---------------------------- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ------------------------------------------------
                
                Vector2 correctPositionHand = new Vector2(bs3.handRight.X, bs3.handRight.Y); //odebrany, TA FUNKCJA ODWRACA Y !!!! (i po co?!)
                float correctDepthHand = bs3.handRight.Z;
                Vector3 rightHandNew = new Vector3(bs3.handRight.X, bs3.handRight.Y, correctDepthHand); //czy jest ok?
                rightHandNew = TransformHelper.transformToFirstKinect(new Vector3(correctPositionHand,correctDepthHand));
                
                
                // ---------------------------- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ------------------------------------------------

                correctPositionElbow = new Vector2(bs3.elbowRight.X, bs3.elbowRight.Y);
                correctDepthElbow = bs3.elbowRight.Z;
                rightElbowNew = new Vector3(bs3.elbowRight.X, bs3.elbowRight.Y, correctDepthElbow);
                rightElbowNew = TransformHelper.transformToFirstKinect(new Vector3(correctPositionElbow, correctDepthElbow));

                Vector2 correctPositionArm = new Vector2(bs3.armRight.X, bs3.armRight.Y);
                float correctDepthArm = bs3.armRight.Z;
                Vector3 rightArmNew = new Vector3(bs3.armRight.X, bs3.armRight.Y, correctDepthArm);
                rightArmNew = TransformHelper.transformToFirstKinect(new Vector3(correctPositionArm, correctDepthArm));

                renderLine(
                    new Vector3(elbowRposition.X, elbowRposition.Y, elbowRdepth),
                    new Vector3(handPosition.X, handPosition.Y, handDepth),
                    view, projection, 1.0f, new Vector3(0, 0, 1), false);

                renderLine(
                    new Vector3(elbowRposition.X, elbowRposition.Y, elbowRdepth),
                    new Vector3(armRposition.X, armRposition.Y, armRdepth),
                    view, projection, 1.0f, new Vector3(0, 0, 1), false);

                renderJoint(
                   new Vector2(
                       (elbowRposition.X)
                       , (elbowRposition.Y)
                       ), (elbowRdepth), //rightHandNew.Z -> handDepth
                   view, projection, 1.0f, new Vector3(1, 1, 1), false); // pi razy oko ok, ale ZA BARDZO ODSTAJE!!! 
                renderJoint(
               new Vector2(
                   (handPosition.X) 
                   , (handPosition.Y) 
                   ), (handDepth) , //rightHandNew.Z -> handDepth
               view, projection, 1.0f, new Vector3(1, 1, 1), false); // pi razy oko ok, ale ZA BARDZO ODSTAJE!!! 
                

                //SPRAWDZIÆ ARGUMENTY DLA PONI¯SZYCH (...New)
                //renderJoint(//tu nawala
                //   new Vector2(
                //       (rightArmNew.X + armRposition.X) / 2f
                //       , (rightArmNew.Y + armRposition.Y) / 2f
                //       ), (rightArmNew.Z + armRdepth) / 2f, //rightHandNew.Z -> handDepth
                //   view, projection, 1.0f, new Vector3(1, 0, 0), false); // pi razy oko ok, ale ZA BARDZO ODSTAJE!!! 
                //renderJoint(//tu te¿
                //        new Vector2(
                //            (rightElbowNew.X)
                //            , (rightElbowNew.Y)
                //            ), (rightElbowNew.Z) , //rightHandNew.Z -> handDepth
                //        view, projection, 1.0f, new Vector3(0, 1, 0), false); // pi razy oko ok, ale ZA BARDZO ODSTAJE!!! 
          

                Vector3 elbowFinal = new Vector3(
                    (rightElbowNew.X + elbowRposition.X) / 2f,
                    (rightElbowNew.Y + elbowRposition.Y) / 2f,
                    (rightElbowNew.Z + elbowRdepth) / 2f);
                Vector3 rightHandFinal = new Vector3(
                    (rightHandNew.X + handPosition.X) / 2f,         
                    (rightHandNew.Y + handPosition.Y) / 2f,
                    (rightHandNew.Z + handDepth) / 2f);
                Vector3 armFinal = new Vector3(
                                    (rightArmNew.X + armRposition.X) / 2f,
                                    (rightArmNew.Y + armRposition.Y) / 2f,
                                    (rightArmNew.Z + armRdepth) / 2f);



                  drawArc(
                      new Vector3(handPosition.X, handPosition.Y, handDepth),

                      new Vector3(elbowRposition.X, elbowRposition.Y, elbowRdepth),

                      new Vector3(armRposition.X, armRposition.Y, armRdepth));

                  Vector3 handP = new Vector3(handPosition.X, handPosition.Y, handDepth);//!!!!!!!!!!!!!!!!!!!!!!1
                  Vector3 elbowP = new Vector3(elbowRposition.X, elbowRposition.Y, elbowRdepth);//!!!!!!!!!!!!!!!!!!!!!!1
                  Vector3 armP = new Vector3(armRposition.X, armRposition.Y, armRdepth);//!!!!!!!!!!!!!!!!!!!!!!1
                  Vector3 toHand2 = -elbowP + handP;
                  Vector3 toArm2 = -elbowP + armP;
                  float resTemp = 0;
                  resTemp = Vector3.Dot(toArm2, toHand2);
              
                  float angle2 = (float)(Math.Acos(resTemp / (toArm2.Length() * toHand2.Length())) 
                      * 180.0 / Math.PI);
  
                spriteBatch2.Begin();
                string str = "Angle: "+angle2;
                
                if (!str.Contains(char.ConvertFromUtf32(0x0105)))
                  spriteBatch2.DrawString(sf, str, new Vector2(200, 700), Color.White);
                 
                  licznikod50do100++;
                  if (150 > licznikod50do100 && licznikod50do100 >= 50)
                  {
                      linie[licznikod50do100 - 50] = ""+angle2;
                  }
                  if (licznikod50do100 == 150)
                  {
                      spriteBatch2.DrawString(sf,"ZAPISANO",new Vector2(200,700),Color.White);
                      StreamWriter sw = File.CreateText(@"C:/saved.txt");
                      for (int i = 0; i < 100; i++)
                          sw.WriteLine("" + linie[i]);
                      sw.Close();
                  }

                  if (licznikod50do100 > 150 && licznikod50do100 < 175)
                  {
                      spriteBatch2.DrawString(sf, "WCZYTYWANIE.", new Vector2(200, 800), Color.White);
                      if (licznikod50do100 == 151)
                      {
                          wczytane = File.ReadAllLines(@"C:/saved.txt");
                          for(int i=0;i<wczytane.Length;i++)
                              wczytaneNum[i]=float.Parse(wczytane[i]);

                      }
                      
                  }

                  if (licznikod50do100 > 175)
                  {
                      spriteBatch2.DrawString(sf, "TESTOWANE", new Vector2(200, 800), Color.White);
                   
                      //i4++;
                      //drawArc(new Vector3(armRposition.X, armRposition.Y, armRdepth),
                     //     new Vector3(elbowRposition.X, elbowRposition.Y, elbowRdepth),




                      /*
                      if (i4 < wczytaneNum.Length - 1)
                      {
                          i4++;
                          if (wczytaneNum[i4] > angle2)
                          {
                              spriteBatch2.DrawString(sf, "PRAWO", new Vector2(200, 800), Color.White);

                          }
                          else
                          {
                              spriteBatch2.DrawString(sf, "LEWO", new Vector2(200, 800), Color.White);

                          }
                      }
                      else
                          spriteBatch2.DrawString(sf, "KONIEC", new Vector2(200, 800), Color.White);
                      */
                      /*
                      while (i4<wczytaneNum.Length-1 && !((angle2 < wczytaneNum[i4] && angle2 > wczytaneNum[i4 + 1]) || (angle2 > wczytaneNum[i4] && angle2 < wczytaneNum[i4 + 1])) )
                      {
                          i4++;

                      }
                      j4 = i4;
                      while (j4<wczytaneNum.Length-1 && i4<wczytaneNum.Length && Math.Abs(wczytaneNum[i4] - wczytaneNum[j4]) < 20)
                      {
                          j4++;
                      }
                      if (j4<wczytaneNum.Length && i4<wczytaneNum.Length  && wczytaneNum[j4] <= wczytaneNum[i4])
                      {
                          spriteBatch2.DrawString(sf, "LEWO", new Vector2(200, 800), Color.White);
                      
                          //mamy chyba: bie¿¹ce po³o¿enie, wszystkiego, to narysowaæ r¹czkê gdzieœtam ;F
                          //zaznaczaj_w_lewo (JAK? chyba kreska i kulka ;F)
                      }
                      else
                      {
                          spriteBatch2.DrawString(sf, "PRAWO", new Vector2(200, 800), Color.White);
                      
                          //zaznaczaj w prawo (JAK?)
                      }
                       */
                  }
                 spriteBatch2.End();

                // ---------------------------- !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ------------------------------------------------

                
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
                Vector3 hand = new Vector3(0,0,0);//!!!!!!!!!!!!!!!!!!!!!!1
                Vector3 elbow = new Vector3(0, 0, 0);//!!!!!!!!!!!!!!!!!!!!!!1
                Vector3 arm = new Vector3(0, 0, 0);//!!!!!!!!!!!!!!!!!!!!!!1
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
                    //KLOCEK
                  //  if (handPosition.Y < elbowRposition.Y) // na górze
                    {

                        foreach (ModelMesh mesh in mdBox.Meshes)
                        {
                            #region old
                        //    This is where the mesh orientation is set
                                foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                                {

                                    float dlugosc = (float)Math.Sqrt(
                                                   (elbowRposition.X - handPosition.X)*(elbowRposition.X - handPosition.X)+
                                                   (elbowRposition.Y - handPosition.Y)*(elbowRposition.Y - handPosition.Y)+
                                                   (elbowRdepth - handDepth)*(elbowRdepth - handDepth)
                                                );
                                    //effect.CurrentTechnique //AM

                                    effect.EnableDefaultLighting();
                                    effect.View = view;
                                    effect.Projection = projection;
                                    float kat1a = (float)Math.Atan(
                                                   (elbowRposition.Y - handPosition.Y) /
                                                   (elbowRposition.X - handPosition.X)
                                                );
                                    float kat2a = (float)Math.Atan(
                                                   (elbowRposition.X - handPosition.X) /
                                                   (elbowRdepth - handDepth)
                                                );

                                    effect.World = modelAbsTrans[mesh.ParentBone.Index] *

                                       Matrix.CreateScale(20)*
                        
                                  Matrix.CreateScale(1,2f/dlugosc,1)

                                       *
                                       Matrix.CreateFromYawPitchRoll(0, - kat1a + ((float)Math.PI / 2), - kat2a - ((float)Math.PI / 2))
                        
                                   
                       
                                       *Matrix.CreateTranslation(
                                       -1 * (correctDepthElbow * 0.5f + elbowRdepth) / 1.5f * 2000,
                                       -1 * (rightElbowNew.Y * 0.5f + elbowRposition.Y) / 1.5f * 2000,
                                       -1 * (rightElbowNew.X * 0.5f + elbowRposition.X) / 1.5f * 2000)*
 Matrix.CreateScale(1.0f / (20)) 
                      ;

                                }

                                //Draw the mesh, will use the effects set above.
                             //   mesh.Draw();
                            #endregion old
                            //  TO JEST OK, KLOCEK:
                            
                        }

                    }
                 //   else //na dole
                    //{
                    //    foreach (ModelMesh mesh in mdBox.Meshes)
                    //    {
                    //        #region old
                    //        ////This is where the mesh orientation is set
                    //        //foreach (BasicEffect effect in mesh.Effects) //próba BE -> E
                    //        //{
                    //        //    //effect.CurrentTechnique //AM

                    //        //    effect.EnableDefaultLighting();
                    //        //    effect.View = view;
                    //        //    effect.Projection = projection;

                    //        //    effect.World = modelAbsTrans[mesh.ParentBone.Index] *
                    //        //       Matrix.CreateScale(2, 2, 30) *
                    //        //       Matrix.CreateRotationX
                    //        //       (
                    //        //       -(float)(Math.PI/2.0)+
                    //        //       (float)Math.Atan(
                    //        //                   (elbowRposition.X - handPosition.X) /
                    //        //                   (elbowRposition.Y - handPosition.Y)
                    //        //                ))
                    //        //       *
                    //        //        // problem g³êbokoœci (ta druga oœ obrotu)
                    //        //    #region dupa
                    //        //        /*
                    //        //       Matrix.CreateRotationZ
                    //        //       (
                    //        //       -(float)(Math.PI / 2.0) +
                    //        //       (float)Math.Atan(
                    //        //                   0.1f*(elbowRdepth - handDepth) /
                    //        //                   (elbowRposition.X - handPosition.X)
                    //        //                ))

                    //        //       *
                    //        //  */
                    //        //    #endregion dupa
                    //        //       Matrix.CreateTranslation(
                    //        //       0 - elbowRdepth * 100,
                    //        //       120.0f - elbowRposition.Y * 0.6f,
                    //        //       160.0f + -elbowRposition.X * 0.5f);

                    //        //}

                    //        ////Draw the mesh, will use the effects set above.
                    //        //mesh.Draw();
                    //        #endregion old
                    //    //    // TO JEST OK, KLOCEK:
                    //    //    if (nagrywanie)
                    //    //        draw(mesh, 30, -(float)(Math.PI / 2.0), elbowRposition, handPosition, elbowRposition,
                    //    //     elbowRdepth,handDepth, view, projection, modelAbsTrans);
                    //    //    else
                    //    //        draw(mesh, 30, -(float)(Math.PI / 2.0) + angles[i], elbowRposition, handPosition, elbowRposition,
                    //    //elbowRdepth,handDepth, view, projection, modelAbsTrans);



                    //    }
                    //}

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
                      //  spriteBatch.DrawString(sf, "Rec", new Vector2(0, 200), Color.White);
                    }
                    #endregion nagrywanie
                    if (!temp.Contains(char.ConvertFromUtf32(0x0105)))
                    {
                      //  spriteBatch.DrawString(sf, "Right arm - right forearm angle:" + kat, new Vector2(10, 10), Color.Black);

                       // spriteBatch.DrawString(sf, "Right arm - right forearm angle:" + (int)katPrzestrzenny, new Vector2(10, 10), Color.White);
                       // spriteBatch.DrawString(sf, "" + (int)katPrzestrzenny, new Vector2(10, 90), Color.White);

                        // else
                        //   spriteBatch.DrawString(sf, "Right arm - right forearm angle:" + kat, new Vector2(armRposition.X, 750), Color.White);

                       // spriteBatch.DrawString(sf, "o", new Vector2(920, 0), Color.White);

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
