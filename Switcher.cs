using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace KnockOut
{/// <summary>
/// Switcher class uses PageSwitcher to control navigation.
/// </summary>
  	public static class Switcher
  	{
    	public static PageSwitcher pageSwitcher;

    	public static void Switch(UserControl newPage)
    	{
      		pageSwitcher.Navigate(newPage);
    	}

    	public static void Switch(UserControl newPage, object state)
    	{
      		pageSwitcher.Navigate(newPage, state);
    	}
  	}

}