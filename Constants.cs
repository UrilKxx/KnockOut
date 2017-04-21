using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockOut
{
    public class Constants
    {
        private static bool Wave;
        public bool bWave
        {
            get { return Wave; }
            set { Wave = value; }
        }
        private static bool Quit;
        public bool bQuit
        {
            get { return Quit; }
            set { Quit = value; }
        }
        private static String strGesture;

        public String GestureName
        {
            get { return strGesture; }
            set { strGesture = value; }
        }
        public enum State
        {
            Wave = 0,
            Limbo,
            Help,
            Stance,
            Jab,
            UpperCut,
            Option,
            Quit
        }
    }
}
