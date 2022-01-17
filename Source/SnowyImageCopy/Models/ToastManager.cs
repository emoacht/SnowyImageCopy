using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Models
{
	internal static class ToastManager
	{
		private static Action _action;

		public static void RegisterToastActivated(Action action)
		{
			if (!OsVersion.Is10OrGreater)
				return;

			_action = action;
			ToastNotificationManagerCompat.OnActivated += OnActivatedBase;
		}

		public static void UnregisterToastActivated()
		{
			if (!OsVersion.Is10OrGreater)
				return;

			_action = null;
			ToastNotificationManagerCompat.OnActivated -= OnActivatedBase;
		}

		private static void OnActivatedBase(ToastNotificationActivatedEventArgsCompat e)
		{
			_action?.Invoke();
		}

		public static void Show(string title, string body, string attribution, CancellationToken cancellationToken)
		{
			if (!OsVersion.Is10OrGreater)
				return;

			new ToastContentBuilder()
				.AddText(title)
				.AddText(body)
				.AddAttributionText(attribution)
				.Show();
		}
	}
}