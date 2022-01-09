using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	internal class ToastManager
	{
		private static Action _action;

		public static void RegisterToastActivated(Action action)
		{
			_action = action;
		}

		public static void UnregisterToastActivated()
		{
			_action = null;
		}

		public static void Show(string title, string body, string attribution, CancellationToken cancellationToken)
		{
			var request = new DesktopToast.ToastRequest
			{
				ToastTitle = title,
				ToastBodyList = new[] { body, attribution },
				ShortcutFileName = Workspace.ShortcutFileName,
				ShortcutTargetFilePath = Assembly.GetEntryAssembly().Location,
				AppId = Workspace.AppId
			};

			DesktopToast.ToastManager.ShowAsync(request, cancellationToken)
				.ContinueWith(task =>
				{
					if (task.Result == DesktopToast.ToastResult.Activated)
						_action?.Invoke();
				},
				cancellationToken);
		}
	}
}