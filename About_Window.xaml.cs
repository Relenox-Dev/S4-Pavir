// Copyright (C) 2021 by Dawid Andrzejczak
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