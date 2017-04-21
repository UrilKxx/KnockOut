//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace KnockOut
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;
    using System.Windows;
    using static Constants;

    /// <summary>
    /// Gesture Detector class which listens for VisualGestureBuilderFrame events from the service
    /// and updates the associated GestureResultView object with the latest results for the trained gestures that include Wave,Stance, Jab,Upper Cut,Elbow Strike,Quit,Repeat
    /// </summary>
    public class GestureDetector : IDisposable
    {
        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string gestureDatabase = @"C:\Users\vazra\Documents\Kinect Studio\Repository\Knockout_knocked.gbd";  

        /// <summary> Name of the discrete gesture in the database that we want to track </summary>
        //private readonly string seatedGestureName = "Quit";

        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /// <summary>
        /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        /// </summary>
        /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
        /// <param name="gestureResultView">GestureResultView object to store gesture results of a single body to</param>
        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            if (gestureResultView == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }

            this.GestureResultView = gestureResultView;

            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }

            // load the 'all gestures' from the gesture database
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase))
            {
                // we could load all available gestures in the database with a call to vgbFrameSource.AddGestures(database.AvailableGestures), 
                // but for this program, we only want to track one discrete gesture from the database, so we'll load it by name
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    //if (gesture.Name.Equals(this.seatedGestureName))
                    //{
                        this.vgbFrameSource.AddGesture(gesture);
                   //System.Windows.MessageBox.Show("Detected");
                    //}
                }
            }
        }

        /// <summary> Gets the GestureResultView object which stores the detector results for display in the UI </summary>
        public GestureResultView GestureResultView { get; private set; }

        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        /// <summary>
        /// Handles gesture detection results arriving from the sensor for the associated body tracking Id. Each gesture is associated with a confidence level. If the gesture performed surpasses the confidence level set the corresponding event handler is invoked.
        /// Certain gesture like Help, Repeat,Quit,Skip are intended to be global and function throughout the application.
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    // get the discrete gesture results which arrived with the latest frame
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                    if (discreteResults != null)
                    {
                        // we only have one gesture in this source object, but you can get multiple gestures
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            List<String> gestureList = new List<string>();
                            gestureList.AddRange(new String[] { "", "", "", "Swipe_Left" });
                            switch (MainMenu.nState)
                            {
                                case State.Stance:
                                    gestureList.Add("Stance");
                                    break;
                                case State.Option:
                                    gestureList.Add("Elbow_Strike_Left");
                                    break;
                                case State.Jab:
                                    gestureList.Add("Jab");
                                    break;
                                case State.UpperCut:
                                    gestureList.Add("Uppercut");
                                    break;
                                case State.Wave:
                                    gestureList = new List<string>();
                                    gestureList.Add("Start");
                                    break;
                                case State.Quit:
                                    gestureList = new List<string>();
                                    gestureList.Add("Quit");
                                    break;
                            }
                            if (gestureList.Contains(gesture.Name) && gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null && result.Confidence>0.2)
                                {
                                    // update the GestureResultView object with new gesture result values
                                    this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence,gesture.Name);

                                    if (result.Detected && result.Confidence > 0.5) 
                                    {
                                        MainMenu.bSwipeCt = false;
                                        if (gesture.Name == "Start" && result.Confidence > 0.9 && MainMenu.bRecognized == false)
                                        {
                                            MainMenu.bWave = true;
                                            OnWave(EventArgs.Empty);
                                            //MessageBox.Show(result.Confidence.ToString());
                                        }
                                        else if (gesture.Name == "Stance" && MainMenu.nState == State.Stance)
                                            OnStance(EventArgs.Empty);
                                        else if (gesture.Name == "Help" && result.Confidence >= 0.95)
                                        {
                                            OnHelp(EventArgs.Empty);
                                        }
                                        else if (gesture.Name == "Repeat" && result.Confidence >=0.9)
                                        {
                                            OnRepeat(EventArgs.Empty);
                                        }
                                        else if (gesture.Name == "Quit" && result.Confidence >=0.98)
                                        {
                                            OnQuit(EventArgs.Empty);
                                            //MessageBox.Show(result.Confidence.ToString());
                                        }
                                        else if (gesture.Name == "Elbow_Strike_Left" && result.Confidence > 0.5)
                                        {
                                            OnElbowStrike(EventArgs.Empty);
                                        }
                                        else if (gesture.Name == "Uppercut" && result.Confidence > 0.5)
                                            OnUpperCut(EventArgs.Empty);
                                        else if (gesture.Name == "Swipe_Left" && result.Confidence > 0.7)
                                        {
                                            MainMenu.bSwipeCt = true;
                                            OnSwipe(EventArgs.Empty);
                                        }
                                        else if (gesture.Name == "Jab" && result.Confidence > 0.5)
                                            OnJab(EventArgs.Empty);
                                        else
                                        {
                                            switch (MainMenu.nState)
                                            {
                                                case State.Stance:

                                                    break;
                                            }
                                        }
                                    }
                                    //System.Windows.MessageBox.Show("Detected");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the TrackingIdLost event for the VisualGestureBuilderSource object
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            // update the GestureResultView object to show the 'Not Tracked' image in the UI
            this.GestureResultView.UpdateGestureResult(false, false, 0.0f,"");

        }

        public EventHandler detectedWave;/// <summary>
        ///Invokes Event handler for Wave
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnWave(EventArgs e)
        {
            EventHandler handle = detectedWave;
            if (handle != null)
                handle(this, e);
        }

        public EventHandler detectedStance;
        /// <summary>
        /// Invokes Event handler for Stance
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStance(EventArgs e)
        {
            //MainMenu.nState = State.Stance;
            EventHandler handle = detectedStance;
            if (handle != null)
                handle(this, e);
        }
        public EventHandler detectedQuit;
        /// <summary>
        /// Invokes EventHandler for quit
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnQuit(EventArgs e)
        {
            EventHandler handle = detectedQuit;
            if (handle != null)
                handle(this, e);
        }
        
        public EventHandler detectedRepeat;
        /// <summary>
        /// Invokes Event handler for repeat
        /// </summary>
        protected virtual void OnRepeat(EventArgs e)
        {
            EventHandler handle = detectedRepeat;
            if (handle != null)
                handle(this, e);
        }
        public EventHandler detectedHelp;
        /// <summary>
        /// Invokes event handler for help
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHelp(EventArgs e)
        {
            //MainMenu.nState = State.Help;
            EventHandler handle = detectedHelp;
            if (handle != null)
                handle(this, e);
        }
        public EventHandler detectedSwipe;
        /// <summary>
        /// Invokes event handler for swipe
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSwipe(EventArgs e)
        {
            //MainMenu.nState = State.ElbowStrike;
            EventHandler handle = detectedSwipe;
            if (handle != null)
                handle(this, e);
        }
        public EventHandler detectedJab;
        /// <summary>
        /// Invokes Event handler for Jab
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnJab(EventArgs e)
        {
            //Application.Current.Shutdown();
            EventHandler handle = detectedJab;
            if (handle != null)
                handle(this, e);
        }
        public EventHandler detectedUpperCut;
        /// <summary>
        /// Invokes Even handler for UpperCut
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnUpperCut(EventArgs e)
        {
            //Application.Current.Shutdown();
            EventHandler handle = detectedUpperCut;
            if (handle != null)
                handle(this, e);
        }
        public EventHandler detectedElbowStrike;

        /// <summary>
        /// Invokes Event Handler for Elbow Strike gesture
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnElbowStrike(EventArgs e)
        {
            //Application.Current.Shutdown();
            EventHandler handle = detectedElbowStrike;
            if (handle != null)
                handle(this, e);
        }
    }
}
