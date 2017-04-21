using System;
using System.Windows;
using System.Windows.Controls;

namespace KnockOut
{
    /// <summary>
    /// Interaction logic for Window.xaml
    /// </summary>
    public partial class PageSwitcher : Window
    {/// <summary>
    /// Switches to MainMenu xaml
    /// </summary>
        public PageSwitcher()
        {
            InitializeComponent();
            Switcher.pageSwitcher = this;
            Switcher.Switch(new MainMenu());            
        }

        public void Navigate(UserControl nextPage)
        {
            this.Content = nextPage;
        }

        public void Navigate(UserControl nextPage, object state)
        {
            this.Content = nextPage;
            ISwitchable s = nextPage as ISwitchable;

            if (s != null)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! "
                  + nextPage.Name.ToString());
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Height = SystemParameters.WorkArea.Height;
            this.Width = SystemParameters.WorkArea.Width;
            this.Left = (SystemParameters.WorkArea.Location.X);
            this.Top = (SystemParameters.WorkArea.Location.Y);
        }
    }
}
