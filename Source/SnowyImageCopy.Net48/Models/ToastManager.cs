using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	internal class ToastManager
	{
		public static void RegisterToastActivated(Action action)
		{
		}

		public static void UnregisterToastActivated()
		{
		}

		public static void Show(string title, string body, string attribution, CancellationToken cancellationToken)
		{
		}
	}
}