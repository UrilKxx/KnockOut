using Microsoft.Kinect;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Speech.Synthesis;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;
using static KnockOut.Constants;
using KinectStreams;

namespace KnockOut
{/// <summary>
/// The MainMenu class handles the interactions for the MainMenu.xaml. It contains event handlers for the all gestures and switches to the next screen when gesture is successfully performed..
/// 
/// </summary>
	public partial class MainMenu : UserControl
	{
        //public static String GestureName = "";
        public static bool bWave = false, bStance = false, bQuit = false;
        public static bool bInStart = false, bRecognized = false, bSwipeCt = false;
        private static KinectSensor kinectSensor = null;
        private static Body[] bodies = null;
        private static BodyFrameReader bodyFrameReader = null;
        private static KinectBodyView kinectBodyView = null;
        private static List<GestureDetector> gestureDetectorList = null;
        public static State nState = State.Wave;
		//skeletal tracking
        Mode _mode = Mode.Color;
        bool _displayBody = true;

        public static SpeechSynthesizer objSynth = new SpeechSynthesizer();
        public MainMenu()
		{
            
            InitializeComponent();
            bInStart = true;
            //GestureName = "Start";
            
            this.KeyDown += new KeyEventHandler(ChangeScreen);
            //Application.Current.LoadCompleted += Current_LoadCompleted;
        }/// <summary>
         /// The WaveDetected function is a event handler for Wave.
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
        void WaveDetected(object sender, EventArgs e)
		{
            if (bWave)
            {
                bInStart = false;
                bRecognized = true;
                nState = State.Limbo;
                Switcher.Switch(new LoadGame());
                bWave = false;
                //GestureName = "Stance";
            }
            CallKinect();
        }
        private static void QuitDetected(object sender, EventArgs e)
        {
            if (nState == State.Quit)
            {
                objSynth.Speak("Bye. See you soon!");
                Application.Current.Shutdown();
            }
        }

        private void ChangeScreen(object sender, KeyEventArgs e)
        {
            bInStart = false;
            
            //bInStance = true;
            Switcher.Switch(new Stance());
            //GestureName = "Stance";
        }

        private void newGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (bInStart)
            {
                bInStart = false;
                Switcher.Switch(new LoadGame());
                //GestureName = "Stance";
            }
        }
        /// <summary>
        /// After the MainMenu is loaded. The method prompts the user to Wave using audio and calls CallKinect method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainMenuLoaded(object sender, RoutedEventArgs e)
        {
            CallKinect();
            objSynth.Speak("Hi. Wave to start.");
        }
        /// <summary>
        /// CallKinect opens up Kinect Sensor which then uses MultiSourceFrameReader to display the user's skeleton back to the user giving visual feedback of the user's position.
        /// </summary>
        public void CallKinect()
        {
            try
            {
                MultiSourceFrameReader _reader;
                kinectSensor = KinectSensor.GetDefault();
                // open the sensor
                kinectSensor.Open();
                // open the reader for the body frames
                bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
                //for body tracking
                _reader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                // set the BodyFramedArrived event notifier
                bodyFrameReader.FrameArrived += Reader_BodyFrameArrived;
                
                // initialize the BodyViewer object for displaying tracked bodies in the UI
                kinectBodyView = new KinectBodyView(kinectSensor);
                // create a gesture detector for each body (6 bodies => 6 detectors) and create content controls to display results in the UI
                //GestureResultView result = new GestureResultView(0, false, false, 0.0f);
                gestureDetectorList = new List<GestureDetector>();
                //gestureDetectorList = new GestureDetector(kinectSensor, result);
                int maxBodies = kinectSensor.BodyFrameSource.BodyCount;
                for (int i = 0; i < maxBodies; ++i)
                {
                    GestureResultView result = new GestureResultView(i, false, false, 0.0f,"");
                    GestureDetector detector = new GestureDetector(kinectSensor, result);
                    gestureDetectorList.Add(detector);
                    gestureDetectorList[i].detectedWave += WaveDetected;
                    gestureDetectorList[i].detectedStance += StanceDetected;
                    gestureDetectorList[i].detectedQuit += QuitDetected;
                    gestureDetectorList[i].detectedRepeat += RepeatDetected;
                    gestureDetectorList[i].detectedHelp += HelpDetected;
                    gestureDetectorList[i].detectedElbowStrike += ElbowStrikeDetected;
                    gestureDetectorList[i].detectedJab += JabDetected;
                    gestureDetectorList[i].detectedUpperCut += UpperCutDetected;
                    gestureDetectorList[i].detectedElbowStrike += ElbowStrikeDetected;
                    gestureDetectorList[i].detectedSwipe += SwipeDetected;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                //throw;
            }
        }

        private void SwipeDetected(object sender, EventArgs e)
        {
            if (bSwipeCt)
            {
                switch (nState)
                {
                    case State.Limbo:
                        nState = State.Stance;
                        Switcher.Switch(new Stance());
                        break;
                    /*case State.Help:
                        nState = State.Stance;
                        Switcher.Switch(new Stance());
                        break;
                    case State.ElbowStrike:
                        nState = State.Stance;
                        Switcher.Switch(new Stance());
                        break;
                    case State.Jab:
                        nState = State.UpperCut;
                        Switcher.Switch(new UpperCut());
                        break;
                    case State.Stance:
                        nState = State.Jab;
                        Switcher.Switch(new RightJab());
                        break;
                    case State.UpperCut:
                        nState = State.ElbowStrike;
                        Switcher.Switch(new ElbowStrike());
                        break;*/
                }
                bSwipeCt = false; 
            }
        }
        /// <summary>
        /// Paints the user's skeleton in the canvas on the xaml 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(bodies);

                    foreach (var body in bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Draw skeleton.
                                if (_displayBody)
                                {
                                    canvas.DrawSkeleton(body);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Event handler for Help whcih switches to the HELP screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void HelpDetected(object sender, EventArgs e)
        {
            if(bRecognized)
                Switcher.Switch(new LoadGame());
        }
        /// <summary>
        /// Event Handler for ElbowStrike
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ElbowStrikeDetected(object sender, EventArgs e)
        {
            //objSynth.Speak("Good job! This is the end of the tutorial. Cross your arms to quit or perform swipe left to restart the tutorial.");
        }/// <summary>
        /// Event handler for Jab which switches to next gesture screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void JabDetected(object sender, EventArgs e)
        {
            nState = State.UpperCut;
            Switcher.Switch(new UpperCut());
        }
        /// <summary>
        /// Event handler for uppercut which switches to next gesture screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UpperCutDetected(object sender, EventArgs e)
        {
            //objSynth.Speak("That was nicely done! Your final lesson is the elbow strike.");
            Switcher.Switch(new Option());
        }
        /// <summary>
        /// Event handler for Repeat gesture which will reload the previous instruction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RepeatDetected(object sender, EventArgs e)
        {
            if (bRecognized)
            {
                switch (nState)
                {
                    case State.Stance:
                        Switcher.Switch(new Stance());
                        break;
                    case State.Jab:
                        Switcher.Switch(new RightJab());
                        break;
                    case State.Option:
                        Switcher.Switch(new Option());
                        break;
                    case State.UpperCut:
                        Switcher.Switch(new UpperCut());
                        break;
                }
            }
        }
        /// <summary>
        /// Event handler for Stance which switches the screen to the second lesson being the Jab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void StanceDetected(object sender, EventArgs e)
        {
            //objSynth.Speak("That was good. Now onto second lesson.");
            nState = State.Jab;
            Switcher.Switch(new RightJab());
        }
        /// <summary>
        /// Used from the Kinect Sensor 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    //MessageBox.Show("in body frame not null");
                    if (bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                // visualize the new body data
                kinectBodyView.UpdateBodyFrame(bodies);

                // we may have lost/acquired bodies, so update the corresponding gesture detectors
                if (bodies != null)
                {
                    int maxBodies = kinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; ++i)
                    {
                        Body body = bodies[i];
                        ulong trackingId = body.TrackingId;

                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != gestureDetectorList[i].TrackingId)
                        {
                            gestureDetectorList[i].TrackingId = trackingId;

                            // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }
                }
            }
        }
    }
    public static class ExtensionMethods
    {

        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

    public enum Mode
    {
        Color,
        Depth,
        Infrared
    }
}