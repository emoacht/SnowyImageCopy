using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Get/Set selected dates of Calendar.
	/// </summary>
	public class CalendarSelectedDatesBehavior : Behavior<Calendar>
	{
		#region Dependency Property

		/// <summary>
		/// Selected dates
		/// </summary>
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

			this.AssociatedObject.Loaded += OnCalendarLoaded;
			this.AssociatedObject.SelectedDatesChanged += OnCalendarSelectedDatesChanged;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (this.AssociatedObject == null)
				return;

			this.AssociatedObject.Loaded -= OnCalendarLoaded;
			this.AssociatedObject.SelectedDatesChanged -= OnCalendarSelectedDatesChanged;
		}


		private void OnCalendarLoaded(object sender, RoutedEventArgs e)
		{
			if ((this.AssociatedObject == null) || !this.SelectedDates.Any())
				return;

			var calendar = this.AssociatedObject;

			// Make a copy of SelectedDates because SelectedDates will be changed by
			// OnCalendarSelectedDatesChanged during OnCalendarLoaded.
			var selectedDatesCopy = this.SelectedDates.ToArray();

			foreach (var date in selectedDatesCopy)
			{
				if (calendar.SelectedDates.Contains(date))
					continue;

				calendar.SelectedDates.Add(date);
			}
		}

		private void OnCalendarSelectedDatesChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((e.AddedItems != null) && (0 < e.AddedItems.Count))
			{
				foreach (var date in e.AddedItems.OfType<DateTime>())
					this.SelectedDates.Add(date);
			}

			if ((e.RemovedItems != null) && (0 < e.RemovedItems.Count))
			{
				foreach (var date in e.RemovedItems.OfType<DateTime>())
					this.SelectedDates.Remove(date);
			}

			this.SelectedDates = new ObservableCollection<DateTime>(this.SelectedDates.Distinct());

			// Release mouse capture because Calendar control captures mouse when it is clicked
			// and so prevents other controls from responding to the first click after Calendar.
			Mouse.Capture(null);
		}
	}
}
