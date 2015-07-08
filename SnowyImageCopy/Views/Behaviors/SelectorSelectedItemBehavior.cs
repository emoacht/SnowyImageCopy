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
	/// Bring selected item of <see cref="System.Windows.Controls.Primitives.Selector"/> into view.
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
			var box = sender as Selector;
			if ((box == null) || (box.SelectedItem == null))
				return;

			var item = box.ItemContainerGenerator.ContainerFromItem(box.SelectedItem) as FrameworkElement;
			if (item == null)
				return;

			item.BringIntoView();
		}
	}
}