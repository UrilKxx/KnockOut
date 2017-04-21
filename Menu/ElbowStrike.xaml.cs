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
{   /// <summary>
///
/// </summary>
	public partial class ElbowStrike : UserControl, ISwitchable
	{
        private static KinectSensor kinectSensor = null;
        private static Body[] bodies = null;
        bool _displayBody = true;
        public ElbowStrike()
		{
            try
            {
                InitializeComponent();
                MainMenu.objSynth.Speak("That was nicely done! Your final lesson is the elbow strike. Strike opponent with elbow with the corresponding foot behind the other foot.");
                CallKinect();
            }
            catch (Exception e)
            {

                throw;
            }
            //MainMenu
		}
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