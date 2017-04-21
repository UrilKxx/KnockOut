using Microsoft.Kinect;
using KinectStreams;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace KnockOut
{/// <summary>
/// RightJab class handles interactions for the RightJab.xaml
/// </summary>
	public partial class RightJab : UserControl, ISwitchable
	{
        private static KinectSensor kinectSensor = null;
        private static Body[] bodies = null;
        bool _displayBody = true;
        /// <summary>
        /// Loads th RightJab XAML and invokes the CallKinect method.
        /// </summary>
        public RightJab()
		{
            try
            {
                InitializeComponent();
                MainMenu.objSynth.Speak("That was good. Now onto the second lesson. Strike with right arm and place right foot ahead");
                CallKinect();
            }
            catch (Exception e)
            {

                throw;
            }
            //MainMenu
		}
        /// <summary>
        /// CallKinect Method invokes the KinectSensor open method and reads streams color,depth and infrared streams from Kinect used to draw skeleton on XAML canvas.
        /// </summary>
        public void CallKinect()
        {
            MultiSourceFrameReader _reader;

            kinectSensor = KinectSensor.GetDefault();
            // open the sensor
            kinectSensor.Open();
            // open the reader for the body frames
            //for body tracking
            _reader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
            _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

        }
        /// <summary>
        /// Color,depth and infrared streams from Kinect used to draw skeleton(Extensions.cs) on XAML canvas.
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


        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// For back door button click switching just in case.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	Switcher.Switch(new LoadGame());
        }
        #endregion

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            //MainMenu.GestureName = "Quit";
            Application.Current.Shutdown();
        }
    }
}