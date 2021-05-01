/*
    Copyright (C) 2021 by Dawid Andrzejczak

    This file is part of S4Pavir.

    S4Pavir is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    S4Pavir is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with S4Pavir.  If not, see <https://www.gnu.org/licenses />.
*/

using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace S4Pavir
{
	public partial class About_Window : Window
	{
		public About_Window()
		{
			InitializeComponent();
		}

		private void OnNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
			e.Handled = true;
		}

		private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
			{
				maximizeButton.Visibility = Visibility.Visible;
				restoreButton.Visibility = Visibility.Hidden;
				this.WindowState = WindowState.Normal;
			}
			else
			{
				maximizeButton.Visibility = Visibility.Hidden;
				restoreButton.Visibility = Visibility.Visible;
				this.WindowState = WindowState.Maximized;
			}
		}

		private void OnCloseButtonClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}