/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
using DoubanFM.Core;

namespace DoubanFM
{
	public partial class Style : ResourceDictionary
	{
	    /// <summary>
		/// 使控制面板切换时有渐变效果
		/// </summary>
		private void ControlPanelStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.OriginalSource == sender)
			{
				((sender as FrameworkElement).Style.Resources["ControlPanelStoryboard"] as Storyboard).Begin(sender as FrameworkElement);
			}
		}

		private void Channel_MouseEnter(object sender, MouseEventArgs e)
		{
			Player player = (Player)App.Current.FindResource("Player");
			Channel channel = GetChannel(sender);
			if (channel != null)
			{
				GetCheckBox(sender).IsChecked = player.CanRemoveFromFavorites(channel);
			}
		}

		private static Channel GetChannel(object sender)
		{
			if (sender == null) return null;
			var dataContext = ((FrameworkElement)sender).DataContext;
			return dataContext as Channel;
		}

		private static CheckBox GetCheckBox(object sender)
		{
			return (CheckBox)((Control)sender).Template.FindName("CbFavorite", sender as FrameworkElement);
		}

		private void CbFavorite_Click(object sender, RoutedEventArgs e)
		{
			Channel channel = GetChannel(sender);
			if (channel == null) return;
			Player player = (Player)App.Current.FindResource("Player");
			if (((CheckBox)sender).IsChecked == true)
			{
				player.AddToFavorites(channel);
			}
			else
			{
				player.RemoveFromFavorites(channel);
			}
			(Application.Current.MainWindow as DoubanFMWindow).RefreshMyChannels();
		}
	}
}
