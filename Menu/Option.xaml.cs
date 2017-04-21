using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static KnockOut.Constants;

namespace KnockOut
{
	public partial class Option : UserControl, ISwitchable
    {  /// <summary>
       /// Loads th Option XAML and invokes the CallKinect method. This screen is for the final Quit gesture.
       /// </summary>
        public Option()
		{
			// Required to initialize variables
			InitializeComponent();
            MainMenu.objSynth.Speak("Bow to quit");

            MainMenu.nState = State.Quit;
        }

        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	Switcher.Switch(new Register());
        }
        #endregion
	}
}