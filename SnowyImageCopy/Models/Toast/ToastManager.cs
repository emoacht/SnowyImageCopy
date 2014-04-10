using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Notifications;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Models.Toast
{
	/// <summary>
	/// Show toast notifications.
	/// </summary>
	internal class ToastManager
	{
		#region Constant

		/// <summary>
		/// Shortcut file of this application to be installed in Start menu
		/// </summary>
		private readonly string shortcutFile;

		/// <summary>
		/// AppUserModelID of this application
		/// </summary>
		private readonly string appId;

		/// <summary>
		/// Waiting time before showing a toast after the shortcut file is newly installed.
		/// </summary>
		/// <remarks>As far as I observed, roughly 3 seconds are required.</remarks>
		private readonly TimeSpan waitTime = TimeSpan.FromSeconds(3);

		#endregion


		internal ToastManager()
		{
			// Read from Settings.settings.
			shortcutFile = Properties.Settings.Default.ShortcutFile;
			appId = Properties.Settings.Default.AppId;
		}

		/// <summary>
		/// Show a toast.
		/// </summary>
		/// <param name="headline">A toast's headline</param>
		/// <param name="body">A toast's body</param>
		/// <returns>Result of showing a toast</returns>
		internal async Task<ToastResult> ShowAsync(string headline, string body)
		{
			return await ShowAsync(headline, body, String.Empty);
		}

		/// <summary>
		/// Show a toast.
		/// </summary>
		/// <param name="headline">A toast's headline</param>
		/// <param name="body1st">A toast's body (1st line)</param>
		/// <param name="body2nd">A toast's body (2nd line, optional)</param>
		/// <returns>Result of showing a toast</returns>
		internal async Task<ToastResult> ShowAsync(string headline, string body1st, string body2nd)
		{
			if (!OsVersion.IsEightOrNewer)
				return ToastResult.Unavailable;

			var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), // Not CommonStartMenu
				"Programs", shortcutFile);
			var targetPath = Assembly.GetExecutingAssembly().Location;

			try
			{
				var s = new Shortcut();

				if (!s.CheckShortcut(shortcutPath, targetPath, String.Empty, appId))
				{
					s.InstallShortcut(shortcutPath, targetPath, String.Empty, appId, targetPath);

					await Task.Delay(waitTime);
				}

				return await ShowBaseAsync(headline, body1st, body2nd);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to show a toast {0}", ex);
				throw;
			}
		}

		/// <summary>
		/// Show a toast.
		/// </summary>
		/// <param name="headline">A toast's headline</param>
		/// <param name="body1st">A toast's body (1st line)</param>
		/// <param name="body2nd">A toast's body (2nd line, optional)</param>
		/// <returns>Result of showing a toast</returns>
		private async Task<ToastResult> ShowBaseAsync(string headline, string body1st, string body2nd)
		{
			// Get a toast XML template (ToastText02 or ToastText04).
			var document = ToastNotificationManager.GetTemplateContent(String.IsNullOrEmpty(body2nd)
				? ToastTemplateType.ToastText02
				: ToastTemplateType.ToastText04);

			// Fill in text elements.
			var textElements = document.GetElementsByTagName("text");
			if (textElements.Length >= 2)
			{
				textElements[0].AppendChild(document.CreateTextNode(headline));
				textElements[1].AppendChild(document.CreateTextNode(body1st));

				if (textElements.Length == 3)
				{
					textElements[2].AppendChild(document.CreateTextNode(body2nd));
				}
			}

			// Add audio element.
			var audioElement = document.CreateElement("audio");
			audioElement.SetAttribute("src", "ms-winsoundevent:Notification.Default");
			document.DocumentElement.AppendChild(audioElement);

			// Create a toast and prepare to handle toast events.
			var toast = new ToastNotification(document);
			var tcs = new TaskCompletionSource<ToastResult>();

			TypedEventHandler<ToastNotification, object> activated = null;
			activated = (sender, e) =>
				{
					tcs.SetResult(ToastResult.Activated);
				};
			toast.Activated += activated;

			TypedEventHandler<ToastNotification, ToastDismissedEventArgs> dismissed = null;
			dismissed = (sender, e) =>
				{
					switch (e.Reason)
					{
						case ToastDismissalReason.ApplicationHidden:
							tcs.SetResult(ToastResult.ApplicationHidden);
							break;
						case ToastDismissalReason.UserCanceled:
							tcs.SetResult(ToastResult.UserCanceled);
							break;
						case ToastDismissalReason.TimedOut:
							tcs.SetResult(ToastResult.TimedOut);
							break;
					}
				};
			toast.Dismissed += dismissed;

			TypedEventHandler<ToastNotification, ToastFailedEventArgs> failed = null;
			failed = (sender, e) =>
				{
					tcs.SetResult(ToastResult.Failed);
				};
			toast.Failed += failed;

			// Show a toast.
			ToastNotificationManager.CreateToastNotifier(appId).Show(toast);

			// Wait for the result.
			var result = await tcs.Task;

			Debug.WriteLine("Toast result: {0}", result);

			toast.Activated -= activated;
			toast.Dismissed -= dismissed;
			toast.Failed -= failed;

			return result;
		}
	}
}
