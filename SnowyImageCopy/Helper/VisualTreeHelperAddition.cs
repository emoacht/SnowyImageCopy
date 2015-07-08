using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Additional method for <see cref="System.Windows.Media.VisualTreeHelper"/>
	/// </summary>
	public static class VisualTreeHelperAddition
	{
		/// <summary>
		/// Get descendant visuals of a specified visual.
		/// </summary>
		/// <param name="reference">Parent visual</param>
		/// <returns>Descendant visuals</returns>
		public static IEnumerable<DependencyObject> GetDescendants(DependencyObject reference)
		{
			if (reference == null)
				yield break;

			var queue = new Queue<DependencyObject>();

			do
			{
				var parent = (queue.Count == 0) ? reference : queue.Dequeue();

				var count = VisualTreeHelper.GetChildrenCount(parent);
				for (int i = 0; i < count; i++)
				{
					var child = VisualTreeHelper.GetChild(parent, i);
					queue.Enqueue(child);

					yield return child;
				}
			}
			while (queue.Count > 0);
		}
	}
}