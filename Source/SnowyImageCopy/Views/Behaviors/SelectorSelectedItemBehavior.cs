using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Brings selected item of Selector into view.
	/// </summary>
	[TypeConstraint(typeof(Selector))]
	public sealed class SelectorSelectedItemBehavior : Behavior<Selector>
	{
		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.SelectionChanged += OnSelectionChanged;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			this.AssociatedObject.SelectionChanged -= OnSelectionChanged;
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((sender is Selector selector) &&
				(selector.ItemContainerGenerator.ContainerFromItem(selector.SelectedItem) is FrameworkElement item))
			{
				item.BringIntoView();
			}
		}
	}
}