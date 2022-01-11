using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;

namespace SnowyImageCopy.Models
{
	internal static class ToastManager
	{
		private static Action _action;

		public static void RegisterToastActivated(Action action)
		{
			_action = action;
			ToastNotificationManagerCompat.OnActivated += OnActivatedBase;
		}

		public static void UnregisterToastActivated()
		{
			_action = null;
			ToastNotificationManagerCompat.OnActivated -= OnActivatedBase;
		}

		private static void OnActivatedBase(ToastNotificationActivatedEventArgsCompat e)
		{
			_action?.Invoke();
		}

		public static void Show(string title, string body, string attribution, CancellationToken cancellationToken)
		{
			new ToastContentBuilder()
				.AddText(title)
				.AddText(body)
				.AddAttributionText(attribution)
				.Show();
		}
	}
}