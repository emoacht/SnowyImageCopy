using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Bring selected item of ListBox into view.
	/// </summary>
	[TypeConstraint(typeof(ListBox))]
	public sealed class ListBoxSelectedItemBehavior : Behavior<ListBox>
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
			var box = sender as ListBox;
			if ((box == null) || (box.SelectedItem == null))
				return;

			var item = box.ItemContainerGenerator.ContainerFromItem(box.SelectedItem) as ListBoxItem;
			if (item == null)
				return;

			item.BringIntoView();
		}
	}
}