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
		/// Gets descendant visuals of a specified visual.
		/// </summary>
		/// <param name="reference">Parent visual</param>
		/// <returns>Descendant visuals</returns>
		public static IEnumerable<DependencyObject> GetDescendants(DependencyObject reference)
		{
			if (reference is null)
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

		/// <summary>
		/// Gets descendant visuals of a specified visual.
		/// </summary>
		/// <typeparam name="T">Type of descendant visuals</typeparam>
		/// <param name="reference">Parent visual</param>
		/// <returns>Descendant visuals</returns>
		public static IEnumerable<T> GetDescendants<T>(DependencyObject reference) where T : DependencyObject
		{
			if (reference is null)
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

					if (child is T buffer)
						yield return buffer;
				}
			}
			while (queue.Count > 0);
		}

		/// <summary>
		/// Gets ancestor visuals of a specified visual.
		/// </summary>
		/// <param name="reference">Child visual</param>
		/// <returns>Ancestor visuals</returns>
		/// <remarks>
		/// <see cref="System.Windows.Interactivity.DependencyObjectHelper.GetSelfAndAncestors"/> method
		/// will provide similar function if System.Windows.Interactivity is added to the project references.
		/// </remarks>
		public static IEnumerable<DependencyObject> GetAncestors(DependencyObject reference)
		{
			var parent = reference;

			while (true)
			{
				parent = VisualTreeHelper.GetParent(parent);
				if (parent is null)
					yield break;

				yield return parent;
			}
		}

		/// <summary>
		/// Gets ancestor visuals of a specified visual.
		/// </summary>
		/// <typeparam name="T">Type of ancestor visuals</typeparam>
		/// <param name="reference">Child visual</param>
		/// <returns>Ancestor visuals</returns>
		public static IEnumerable<T> GetAncestors<T>(DependencyObject reference) where T : DependencyObject
		{
			var parent = reference;

			while (true)
			{
				parent = VisualTreeHelper.GetParent(parent);
				if (parent is null)
					yield break;

				if (parent is T buffer)
					yield return buffer;
			}
		}
	}
}