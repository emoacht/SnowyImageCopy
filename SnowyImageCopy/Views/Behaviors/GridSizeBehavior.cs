using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Relay change of Grid size.
	/// </summary>
	public class GridSizeBehavior : Behavior<Grid>
	{
		#region Dependency Property

		/// <summary>
		/// Size of this Grid
		/// </summary>
		public Size GridSize
		{
			get { return (Size)GetValue(GridSizeProperty); }
			set { SetValue(GridSizeProperty, value); }
		}
		public static readonly DependencyProperty GridSizeProperty =
			DependencyProperty.Register(
				"GridSize",
				typeof(Size),
				typeof(GridSizeBehavior),
				new FrameworkPropertyMetadata(Size.Empty));

		/// <summary>
		/// Maximum width of this Grid
		/// </summary>
		public double MaxWidth
		{
			get { return (double)GetValue(MaxWidthProperty); }
			set { SetValue(MaxWidthProperty, value); }
		}
		public static readonly DependencyProperty MaxWidthProperty =
			DependencyProperty.Register(
				"MaxWidth",
				typeof(double),
				typeof(GridSizeBehavior),
				new FrameworkPropertyMetadata(
					double.NaN,
					(d, e) => ((GridSizeBehavior)d).AdjustSize()));

		/// <summary>
		/// Inner frame size of this Grid calculated by padding
		/// </summary>
		public Size FrameSize
		{
			get { return (Size)GetValue(FrameSizeProperty); }
			set { SetValue(FrameSizeProperty, value); }
		}
		public static readonly DependencyProperty FrameSizeProperty =
			DependencyProperty.Register(
				"FrameSize",
				typeof(Size),
				typeof(GridSizeBehavior),
				new FrameworkPropertyMetadata(Size.Empty));

		/// <summary>
		/// Padding of this Grid for calculation
		/// </summary>
		/// <remarks>This must match with Margin of inner element.</remarks>
		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingProperty); }
			set { SetValue(PaddingProperty, value); }
		}
		public static readonly DependencyProperty PaddingProperty =
			DependencyProperty.Register(
				"Padding",
				typeof(Thickness),
				typeof(GridSizeBehavior),
				new FrameworkPropertyMetadata(new Thickness(0D)));

		/// <summary>
		/// Whether change of Grid size is reliable for relaying.
		/// </summary>
		/// <remarks>Without setting this property, relaying will never happen.</remarks>
		public bool IsReliable
		{
			get { return (bool)GetValue(IsReliableProperty); }
			set { SetValue(IsReliableProperty, value); }
		}
		public static readonly DependencyProperty IsReliableProperty =
			DependencyProperty.Register(
				"IsReliable",
				typeof(bool),
				typeof(GridSizeBehavior),
				new FrameworkPropertyMetadata(false));

		#endregion


		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.SizeChanged += OnSizeChanged;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (this.AssociatedObject == null)
				return;

			this.AssociatedObject.SizeChanged -= OnSizeChanged;
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			AdjustSize();
		}


		private void AdjustSize()
		{
			if (!IsReliable)
				return;

			if ((0 < this.AssociatedObject.ActualWidth) && (0 < this.AssociatedObject.ActualHeight))
			{
				FrameSize = new Size(
					Math.Min(this.AssociatedObject.ActualWidth, MaxWidth) - Padding.Left - Padding.Right,
					this.AssociatedObject.ActualHeight - Padding.Top - Padding.Bottom);
			}
		}
	}
}