using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SnowyImageCopy.Views.Controls
{
	public class HideableGridSplitter : GridSplitter
	{
		public HideableGridSplitter()
		{
			this.Initialized += OnInitialized;
			this.IsVisibleChanged += OnVisibleChanged;
		}
		

		#region Property

		/// <summary>
		/// Minimum length of width of Column at the right of this splitter or height of Row at the bottom of this splitter
		/// </summary>
		public double MinLength
		{
			get { return (double)GetValue(MinLengthProperty); }
			set { SetValue(MinLengthProperty, value); }
		}
		public static readonly DependencyProperty MinLengthProperty =
			DependencyProperty.Register(
				"MinLength",
				typeof(double),
				typeof(HideableGridSplitter),
				new FrameworkPropertyMetadata(0D));

		#endregion


		private GridLength rightColumnWidth; // Width of Column at the right of this splitter
		private GridLength bottomRowHeight; // Height of Row at the bottom of this splitter
		
		private void OnInitialized(object sender, EventArgs e)
		{
			var parent = base.Parent as Grid;
			if (parent == null)
				return;

			switch (GetResizeDirection())
			{
				case GridResizeDirection.Columns:
					var columnIndex = Grid.GetColumn(this);
					if (columnIndex + 1 >= parent.ColumnDefinitions.Count)
						return;

					var rightColumn = parent.ColumnDefinitions[columnIndex + 1]; // Column at the right of this splitter

					// Record current column width.
					rightColumnWidth = rightColumn.Width;
					break;

				case GridResizeDirection.Rows:
					var rowIndex = Grid.GetRow(this);
					if (rowIndex + 1 >= parent.RowDefinitions.Count)
						return;

					var bottomRow = parent.RowDefinitions[rowIndex + 1]; // Row at the bottom of this splitter

					// Record current row height.
					bottomRowHeight = bottomRow.Height;
					break;
			}
		}

		private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var parent = base.Parent as Grid;
			if (parent == null)
				return;

			switch (GetResizeDirection())
			{
				case GridResizeDirection.Columns:
					int columnIndex = Grid.GetColumn(this);
					if (columnIndex + 1 >= parent.ColumnDefinitions.Count)
						return;

					var rightColumn = parent.ColumnDefinitions[columnIndex + 1]; // Column at right side of this splitter

					if (this.Visibility == Visibility.Visible)
					{
						// Restore previous column width.
						rightColumn.Width = rightColumnWidth;
						rightColumn.MinWidth = MinLength;
					}
					else
					{
						// Record current column width.
						rightColumnWidth = rightColumn.Width;

						// Hide the column.
						rightColumn.Width = new GridLength(0);
						rightColumn.MinWidth = 0D;
					}
					break;

				case GridResizeDirection.Rows:
					int rowIndex = Grid.GetRow(this);
					if (rowIndex + 1 >= parent.RowDefinitions.Count)
						return;

					var bottomRow = parent.RowDefinitions[rowIndex + 1]; // Row at the bottom of this splitter

					if (this.Visibility == Visibility.Visible)
					{
						// Restore previous row height.
						bottomRow.Height = bottomRowHeight;
						bottomRow.MinHeight = MinLength;
					}
					else
					{
						// Record height of the row.
						bottomRowHeight = bottomRow.Height;

						// Hide the column.
						bottomRow.Height = new GridLength(0);
						bottomRow.MinHeight = 0D;
					}
					break;
			}
		}

		private GridResizeDirection GetResizeDirection()
		{
			switch (this.ResizeDirection)
			{
				// This logic is based on http://msdn.microsoft.com/library/system.windows.controls.gridsplitter.aspx
				case GridResizeDirection.Auto:
					if (this.HorizontalAlignment != HorizontalAlignment.Stretch)
						return GridResizeDirection.Columns;
					if (this.VerticalAlignment != VerticalAlignment.Stretch)
						return GridResizeDirection.Rows;
					if (this.ActualWidth <= this.ActualHeight)
						return GridResizeDirection.Columns;
					else
						return GridResizeDirection.Rows;

				default:
					return this.ResizeDirection;
			}
		}
	}
}