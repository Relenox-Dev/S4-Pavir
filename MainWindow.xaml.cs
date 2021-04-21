// Copyright (C) 2021 by Dawid Andrzejczak
using CatalogResource;
using Microsoft.VisualBasic.FileIO;
using Ookii.Dialogs.Wpf;
using s4pi.Interfaces;
using s4pi.Package;
using s4pi.WrapperDealer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace S4Pavir
{
	public partial class MainWindow : Window
	{
		public VistaFolderBrowserDialog mod_dir;

		private static uint[] thumbnails = new uint[]
		{
			0x0D338A3A,
			0x16CCF748,
			0x35D45407,
			0x3C1AF1F2,
			0xC32A8647,
			0x56278554,
			0x58282D45,
			0x9C925813,
			0xA1FF2FC4,
			0xCD9DE247,
			0xE1BCAEE2,
			0xE254AE6E,
			0x3C2A8647
		};

		private static uint[] flags = new uint[]
		{
			0x034AEECB
		};

		private static uint[] types = new uint[]
		{
			0xC0DB5AE7
		};

		public IPackage p;

		public string regex_pattern = @"[^\\]+(?=\.package$)";
		public string mod_folder_path = "";

		public List<BitmapFrame> thumbnail_bitmaps = new List<BitmapFrame>();
		private List<string> names = new List<string>();
		public int num_of_images = 0;
		public int now_displayed = 0;

		private void Custom_Message_Box(string content)
		{
			using (TaskDialog dialog = new TaskDialog())
			{
				dialog.WindowTitle = "Information";
				dialog.Content = content;
				TaskDialogButton ok_bttn = new TaskDialogButton(ButtonType.Ok);
				dialog.CenterParent = true;
				dialog.Buttons.Add(ok_bttn);
				dialog.ShowDialog(this);
			}
		}

		private bool Custom_Yes_No_Box(string content)
		{
			using (TaskDialog dialog = new TaskDialog())
			{
				dialog.WindowTitle = "Information";
				dialog.Content = content;
				TaskDialogButton yes_bttn = new TaskDialogButton(ButtonType.Yes);
				TaskDialogButton no_bttn = new TaskDialogButton(ButtonType.No);
				dialog.CenterParent = true;
				dialog.Buttons.Add(yes_bttn);
				dialog.Buttons.Add(no_bttn);
				TaskDialogButton button = dialog.ShowDialog(this);
				if (button == yes_bttn)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		private void Scan()
		{
			names.Clear();
			list_of_mods.Items.Clear();

			string[] folder_list = Directory.GetDirectories(mod_folder_path);
			foreach (string folder in folder_list)
			{
				names.AddRange(Directory.GetFiles(folder).ToList());
			}

			// Mods in the main directory
			string[] mod_list = Directory.GetFiles(mod_folder_path);
			foreach (string mod_path in mod_list)
			{
				if (mod_path.Contains(".package"))
				{
					names.Add(mod_path);
				}
			}

			foreach (string name in names)
			{
				list_of_mods.Items.Add(Regex.Match(name, regex_pattern));
			}
		}

		private void Sort_mods()
		{
			list_of_mods.Items.Clear();
			string[] folders = Directory.GetDirectories(mod_folder_path);
			string[] mod_names = Directory.GetFiles(mod_folder_path);
			string current_body_type;
			List<string> new_folders = new List<string>();

			Custom_Message_Box("Please wait, this can take some time. " + mod_names.Length.ToString() + " mods need to be sorted.");

			foreach (string name in mod_names)
			{
				current_body_type = "";
				if (!name.Contains(".package"))
				{
					continue;
				}
				IPackage now_open = Package.OpenPackage(0, name);
				string only_mod_name = Regex.Match(name, regex_pattern).ToString();

				foreach (IResourceIndexEntry r in now_open.GetResourceList)
				{
					if (flags.Contains(r.ResourceType))
					{
						CASPartResource.CASPartResource text_file = (CASPartResource.CASPartResource)WrapperDealer.GetResource(1, now_open, r);

						current_body_type = text_file.BodyType.ToString();

						break;
					}
				}

				now_open.Dispose();

				if (folders.Contains(current_body_type) || new_folders.Contains(current_body_type))
				{
					File.Move(name, mod_folder_path + "\\" + current_body_type + "\\" + only_mod_name + ".package");
				}
				else
				{
					Directory.CreateDirectory(mod_folder_path + "\\" + current_body_type);
					new_folders.Add(current_body_type);
					File.Move(name, mod_folder_path + "\\" + current_body_type + "\\" + only_mod_name + ".package");
				}
			}

			Custom_Message_Box("All mods have been sorted!");
		}

		private void Remove_file()
		{
			try
			{
				object mod_in_list = list_of_mods.SelectedItem;
				int now_selected = list_of_mods.SelectedIndex;
				string to_delete = names[now_selected];
				p.Dispose();
				list_of_mods.Items.Remove(mod_in_list);
				FileSystem.DeleteFile(to_delete, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
				names.RemoveAt(now_selected);
			}
			catch (Exception)
			{
				Custom_Message_Box("Please select a mod from the list before deleting it.");
			}
		}

		private void Fill_Image_Array(IPackage p)
		{
			thumbnail_bitmaps.Clear();

			foreach (IResourceIndexEntry r in p.GetResourceList)
			{
				if (thumbnails.Contains(r.ResourceType))
				{
					IResource image_resource = WrapperDealer.GetResource(1, p, r);
					BitmapFrame my_image = BitmapFrame.Create(image_resource.Stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
					thumbnail_bitmaps.Add(my_image);
				}
			}

			if (thumbnail_bitmaps.Count() == 0)
			{
				thumbnail_bitmaps.Add(BitmapFrame.Create(new Uri("pack://application:,,,/Clear.bmp")));
			}

			img_thumbnail.Source = thumbnail_bitmaps[0];
			num_of_images = thumbnail_bitmaps.Count();
			now_displayed = 0;
			img_counter.Content = now_displayed + 1 + "/" + num_of_images;
		}

		private void Fill_Details(IPackage p)
		{
			foreach (IResourceIndexEntry r in p.GetResourceList)
			{
				if (flags.Contains(r.ResourceType))
				{
					CASPartResource.CASPartResource text_file = (CASPartResource.CASPartResource)WrapperDealer.GetResource(1, p, r);

					string[] flag_array = text_file.ParameterFlags.ToString().Split(',');
					if (flag_array.Contains<string>("AllowForCASRandom"))
					{
						tb_flags.Text = "AllowForCASRandom - True";
					}
					else
					{
						tb_flags.Text = "AllowForCASRandom - False";
					}

					tb_age_gender.Text = text_file.AgeGender.ToString();
					tb_body_type.Text = text_file.BodyType.ToString();

					break;
				}
				else if (types.Contains(r.ResourceType))
				{
					ObjectDefinitionResource text_file = (ObjectDefinitionResource)WrapperDealer.GetResource(1, p, r);

					try
					{
						tb_flags.Text = text_file.Tuning.ToString();
					}
					catch (Exception)
					{
						tb_flags.Text = null;
					}
					break;
				}

				tb_flags.Text = null;
				tb_age_gender.Text = null;
				tb_body_type.Text = null;
			}
		}

		public MainWindow()
		{
			InitializeComponent();
			this.list_of_mods.SelectionChanged += new SelectionChangedEventHandler(list_of_mods_SelectedIndexChanged);
		}

		private void Select_Click(object sender, RoutedEventArgs e)
		{
			mod_dir = new VistaFolderBrowserDialog();
			if ((bool)mod_dir.ShowDialog())
			{
				mod_folder_path = mod_dir.SelectedPath;
				custom_title.Text = "S4-Pavir - \"" + mod_folder_path + "\"";

				if (mod_folder_path.Length != 0)
				{
					Scan();
				}
			}
		}

		private void Count_Click(object sender, RoutedEventArgs e)
		{
			Custom_Message_Box("There are " + list_of_mods.Items.Count + " mods in this list.");
		}

		private void Sort_Click(object sender, RoutedEventArgs e)
		{
			// Custom popup ( YES / NO ) if yes scan if no discard
			if (Custom_Yes_No_Box("Sort the mods folder into subfolders ?"))
			{
				Sort_mods();
			}
		}

		private void Remove_Click(object sender, RoutedEventArgs e)
		{
			// Custom popup ( YES / NO ) if yes remove if no discard
			if (Custom_Yes_No_Box("Remove the selected mod ?"))
			{
				Remove_file();
			}
		}

		private void list_of_mods_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				string now_selected = list_of_mods.SelectedItem.ToString();
				int index_of_now_selected = list_of_mods.Items.IndexOf(list_of_mods.SelectedItem);

				tb_mod_name.Text = now_selected;

				p = Package.OpenPackage(0, names[index_of_now_selected]);
				Fill_Image_Array(p);
				Fill_Details(p);
				p.Dispose();
			}
			catch (Exception)
			{
				img_thumbnail.Source = BitmapFrame.Create(new Uri("pack://application:,,,/Clear.bmp"));
				tb_mod_name.Text = null;
				tb_flags.Text = null;
				tb_age_gender.Text = null;
				tb_body_type.Text = null;
			}
		}

		private void Switch_Image(bool forward)
		{
			try
			{
				if (forward)
				{
					if (now_displayed < num_of_images - 1)
					{
						now_displayed++;
					}
					else
					{
						now_displayed = 0;
					}
				}
				else
				{
					if (now_displayed > 0)
					{
						now_displayed--;
					}
					else
					{
						now_displayed = num_of_images - 1;
					}
				}

				img_thumbnail.Source = thumbnail_bitmaps[now_displayed];
				img_counter.Content = now_displayed + 1 + "/" + num_of_images;
			}
			catch (Exception)
			{
			}
		}

		private void Slideshow_Left_Click(object sender, RoutedEventArgs e)
		{
			Switch_Image(false);
		}

		private void Slideshow_Right_Click(object sender, RoutedEventArgs e)
		{
			Switch_Image(true);
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			About_Window subWindow = new About_Window();
			subWindow.Owner = this;
			subWindow.Show();
		}

		private void Donate_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://www.paypal.com/donate?hosted_button_id=W6GRMX5P7RHJN");
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