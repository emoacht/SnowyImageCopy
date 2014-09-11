using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnowyImageCopy.Views.Controls
{
	/// <summary>
	/// Interaction logic for CircularProgressBar.xaml
	/// </summary>
	public partial class CircularProgressBar : UserControl
	{
		public CircularProgressBar()
		{
			InitializeComponent();
		}


		#region Dependency Property

		public double Radius // Outer radius
		{
			get { return (double)GetValue(RadiusProperty); }
			set { SetValue(RadiusProperty, value); }
		}
		public static readonly DependencyProperty RadiusProperty =
			DependencyProperty.Register(
				"Radius",
				typeof(double),
				typeof(CircularProgressBar),
				new FrameworkPropertyMetadata(25D, OnPropertyChanged));

		public double StrokeThickness
		{
			get { return (double)GetValue(StrokeThicknessProperty); }
			set { SetValue(StrokeThicknessProperty, value); }
		}
		public static readonly DependencyProperty StrokeThicknessProperty =
			DependencyProperty.Register(
				"StrokeThickness",
				typeof(double),
				typeof(CircularProgressBar),
				new FrameworkPropertyMetadata(10D, OnPropertyChanged));

		public Brush ArcSegmentColor
		{
			get { return (Brush)GetValue(ArcSegmentColorProperty); }
			set { SetValue(ArcSegmentColorProperty, value); }
		}
		public static readonly DependencyProperty ArcSegmentColorProperty =
			DependencyProperty.Register(
				"ArcSegmentColor",
				typeof(Brush),
				typeof(CircularProgressBar),
				new FrameworkPropertyMetadata(SystemColors.HighlightBrush, OnPropertyChanged));

		public Brush RingSegmentColor
		{
			get { return (Brush)GetValue(RingSegmentColorProperty); }
			set { SetValue(RingSegmentColorProperty, value); }
		}
		public static readonly DependencyProperty RingSegmentColorProperty =
			DependencyProperty.Register(
				"RingSegmentColor",
				typeof(Brush),
				typeof(CircularProgressBar),
				new FrameworkPropertyMetadata(null, OnPropertyChanged));

		public double RingSegmentOpacity
		{
			get { return (double)GetValue(RingSegmentOpacityProperty); }
			set { SetValue(RingSegmentOpacityProperty, value); }
		}
		public static readonly DependencyProperty RingSegmentOpacityProperty =
			DependencyProperty.Register(
				"RingSegmentOpacity",
				typeof(double),
				typeof(CircularProgressBar),
				new FrameworkPropertyMetadata(0D, OnPropertyChanged));

		public double Percentage
		{
			get { return (double)GetValue(PercentageProperty); }
			set { SetValue(PercentageProperty, value); }
		}
		public static readonly DependencyProperty PercentageProperty =
			DependencyProperty.Register(
				"Percentage",
				typeof(double),
				typeof(CircularProgressBar),
				new FrameworkPropertyMetadata(
					60D, // Sample percentage
					OnPercentageChanged,
					(d, baseValue) =>
					{
						var num = (double)baseValue;
						if ((num < 0D) || (100D < num))
							throw new ArgumentOutOfRangeException("Percentage", String.Format("Value: {0}", num));

						return num;
					}));

		public double Angle
		{
			get { return (double)GetValue(AngleProperty); }
			set { SetValue(AngleProperty, value); }
		}
		public static readonly DependencyProperty AngleProperty =
			DependencyProperty.Register(
				"Angle",
				typeof(double),
				typeof(CircularProgressBar),
				new FrameworkPropertyMetadata(
					216D, // Sample angle
					OnPropertyChanged,
					(d, baseValue) =>
					{
						var num = (double)baseValue;
						if ((num < 0D) || (360D < num))
							throw new ArgumentOutOfRangeException("Angle");

						return num;
					}));

		#endregion


		private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var circle = (CircularProgressBar)d;
			circle.Angle = (circle.Percentage * 360D) / 100D; // This will invoke OnPropertyChanged.
		}

		private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var circle = (CircularProgressBar)d;
			circle.RenderArc();
		}


		private void RenderArc()
		{
			CirclePathBox.Width = Radius * 2;
			CirclePathBox.Height = Radius * 2;

			var pathRadius = Radius - StrokeThickness / 2;

			var startPoint = new Point(pathRadius, 0D);

			var endPoint = GetCartesianCoordinate(Angle, pathRadius);
			endPoint.X += pathRadius;
			endPoint.Y += pathRadius;

			// Check if distance between start point and end point is very short (when angle is close to
			// 360) and if so, adjust end point because in such case arc segment will not be rendered.
			if ((Math.Abs(endPoint.X - startPoint.X) < 0.01) &&
				(Math.Abs(endPoint.Y - startPoint.Y) < 0.01))
				endPoint.X -= 0.01;

			CirclePathFigure.StartPoint = startPoint;

			CircleArcSegment.Point = endPoint;
			CircleArcSegment.Size = new Size(pathRadius, pathRadius);
			CircleArcSegment.IsLargeArc = (Angle > 180D);

			CirclePathTransform.X = StrokeThickness / 2;
			CirclePathTransform.Y = StrokeThickness / 2;
		}

		private Point GetCartesianCoordinate(double angle, double radius)
		{
			// Convert from degree to radian.
			var angleRadian = (Math.PI / 180D) * (angle - 90D);

			var x = radius * Math.Cos(angleRadian);
			var y = radius * Math.Sin(angleRadian);

			return new Point(x, y);
		}
	}
}
