using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

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
			var selector = sender as Selector;
			if (selector?.SelectedItem == null)
				return;

			var item = selector.ItemContainerGenerator.ContainerFromItem(selector.SelectedItem) as FrameworkElement;
			item?.BringIntoView();
		}
	}
}