using KinectStreams;
using Microsoft.Kinect;
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
/// Intermediate window is loaded for user to adjust in the Kinect's field of view and to see what the application expects from the user.
/// </summary>
	public partial class LoadGame : UserControl, ISwitchable
    {
        private static KinectSensor kinectSensor = null;
        private static Body[] bodies = null;
        bool _displayBody = true;
        /// <summary>
        /// Loads the LoadGame.xaml with audio and visual feedback.
        /// </summary>
        public LoadGame()
		{
			// Required to initialize variables
			InitializeComponent();
            MainMenu.objSynth.Speak("If you can't see your skeleton on the screen, please position yourself in the kinect camera's  field of view ");
            CallKinect();
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
        	Switcher.Switch(new Option());
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
        #endregion
    }
}