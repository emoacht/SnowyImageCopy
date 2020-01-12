using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Gets/Sets SelectedDates property of <see cref="System.Windows.Controls.Calendar"/> as Dependency Property.
	/// </summary>
	[TypeConstraint(typeof(Calendar))]
	public class CalendarSelectedDatesBehavior : Behavior<Calendar>
	{
		#region Property

		public ObservableCollection<DateTime> SelectedDates
		{
			get { return (ObservableCollection<DateTime>)GetValue(SelectedDatesProperty); }
			set { SetValue(SelectedDatesProperty, value); }
		}
		public static readonly DependencyProperty SelectedDatesProperty =
			DependencyProperty.Register(
				"SelectedDates",
				typeof(ObservableCollection<DateTime>),
				typeof(CalendarSelectedDatesBehavior),
				new FrameworkPropertyMetadata(
					new ObservableCollection<DateTime>(),
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		#endregion

		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.Loaded += OnLoaded;
			this.AssociatedObject.SelectedDatesChanged += OnSelectedDatesChanged;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (this.AssociatedObject is null)
				return;

			this.AssociatedObject.Loaded -= OnLoaded;
			this.AssociatedObject.SelectedDatesChanged -= OnSelectedDatesChanged;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (!SelectedDates.Any())
				return;

			var associatedSelectedDates = this.AssociatedObject.SelectedDates; // SelectedDates property of Calendar

			// Make a cache of SelectedDates because SelectedDates will be changed by OnSelectedDatesChanged
			// during OnLoaded.
			var selectedDatesCache = SelectedDates.ToArray();

			foreach (var date in selectedDatesCache)
			{
				if (associatedSelectedDates.Contains(date))
					continue;

				associatedSelectedDates.Add(date);
			}
		}

		private void OnSelectedDatesChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems?.Count > 0)
			{
				foreach (var date in e.AddedItems.OfType<DateTime>())
					SelectedDates.Add(date);
			}
			if (e.RemovedItems?.Count > 0)
			{
				foreach (var date in e.RemovedItems.OfType<DateTime>())
					SelectedDates.Remove(date);
			}

			SelectedDates = new ObservableCollection<DateTime>(SelectedDates.Distinct());

			// Release mouse capture because Calendar control captures mouse when it is clicked and so
			// prevents other controls from responding to the first click after it.
			Mouse.Capture(null);
		}
	}
}