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
				new PropertyMetadata(0D));

		#endregion

		private GridLength _rightColumnWidth; // Width of Column at the right of this splitter
		private GridLength _bottomRowHeight; // Height of Row at the bottom of this splitter

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.IsVisibleChanged += OnIsVisibleChanged;

			if (base.Parent is not Grid parent)
				return;

			switch (GetResizeDirection(this))
			{
				case GridResizeDirection.Columns:
					if (TryGetRightColumn(this, parent, out ColumnDefinition rightColumn))
					{
						// Record current column width.
						_rightColumnWidth = rightColumn.Width;
					}
					break;

				case GridResizeDirection.Rows:
					if (TryGetBottomRow(this, parent, out RowDefinition bottomRow))
					{
						// Record current row height.
						_bottomRowHeight = bottomRow.Height;
					}
					break;
			}
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (base.Parent is not Grid parent)
				return;

			switch (GetResizeDirection(this))
			{
				case GridResizeDirection.Columns:
					if (TryGetRightColumn(this, parent, out ColumnDefinition rightColumn))
					{
						if (this.Visibility == Visibility.Visible)
						{
							// Restore previous column width.
							rightColumn.Width = _rightColumnWidth;
							rightColumn.MinWidth = MinLength;
						}
						else
						{
							// Record current column width.
							_rightColumnWidth = rightColumn.Width;

							// Hide the column.
							rightColumn.Width = new GridLength(0);
							rightColumn.MinWidth = 0D;
						}
					}
					break;

				case GridResizeDirection.Rows:
					if (TryGetBottomRow(this, parent, out RowDefinition bottomRow))
					{
						if (this.Visibility == Visibility.Visible)
						{
							// Restore previous row height.
							bottomRow.Height = _bottomRowHeight;
							bottomRow.MinHeight = MinLength;
						}
						else
						{
							// Record height of the row.
							_bottomRowHeight = bottomRow.Height;

							// Hide the column.
							bottomRow.Height = new GridLength(0);
							bottomRow.MinHeight = 0D;
						}
					}
					break;
			}
		}

		private static bool TryGetRightColumn(UIElement child, Grid parent, out ColumnDefinition rightColumn)
		{
			int columnIndex = Grid.GetColumn(child);
			if (columnIndex + 1 < parent.ColumnDefinitions.Count)
			{
				rightColumn = parent.ColumnDefinitions[columnIndex + 1]; // Column at the right of child element
				return true;
			}
			rightColumn = default;
			return false;
		}

		private static bool TryGetBottomRow(UIElement child, Grid parent, out RowDefinition bottomRow)
		{
			int rowIndex = Grid.GetRow(child);
			if (rowIndex + 1 < parent.RowDefinitions.Count)
			{
				bottomRow = parent.RowDefinitions[rowIndex + 1]; // Row at the bottom of child element
				return true;
			}
			bottomRow = default;
			return false;
		}

		private static GridResizeDirection GetResizeDirection(GridSplitter splitter)
		{
			switch (splitter.ResizeDirection)
			{
				// This logic is based on http://msdn.microsoft.com/library/system.windows.controls.gridsplitter.aspx
				case GridResizeDirection.Auto:
					if (splitter.HorizontalAlignment != HorizontalAlignment.Stretch)
						return GridResizeDirection.Columns;
					if (splitter.VerticalAlignment != VerticalAlignment.Stretch)
						return GridResizeDirection.Rows;
					if (splitter.ActualWidth <= splitter.ActualHeight)
						return GridResizeDirection.Columns;
					else
						return GridResizeDirection.Rows;

				default:
					return splitter.ResizeDirection;
			}
		}
	}
}