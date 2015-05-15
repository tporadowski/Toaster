﻿using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace Toaster
{
	public class Toast
	{
		public static ObservableCollection<BaseForm> ActiveToasts = new ObservableCollection<BaseForm>();
		private static Boolean bound = false;

		public static BaseForm ShowNotification(String title, String body, String iconUrl = "", String tag = "", int duration = 150000, Icon icon = null)
		{
			if (!bound) { bound = true; ActiveToasts.CollectionChanged += ActiveToasts_CollectionChanged; }

			if (String.IsNullOrEmpty(tag)) { tag = Guid.NewGuid().ToString(); }

			var form = new BaseForm(title, body, duration)
			{
				ToastIconUrl = iconUrl,
				ToastIcon = icon
			};
			form.Tag = tag;
			form.FormClosed += delegate { ActiveToasts.Remove(form); };
			ActiveToasts.Add(form);
			form.Show();

			return form;
		}

		private static void ActiveToasts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
			{
				foreach (BaseForm form in ActiveToasts)
				{
					form.Reposition();
				}
			}
		}
	}
}