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
using System.Windows.Shapes;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Views.Controls
{
	/// <summary>
	/// Interaction logic for DottedPanel.xaml
	/// </summary>
	public partial class DottedPanel : UserControl
	{
		public DottedPanel()
		{
			InitializeComponent();
		}


		#region Dependency Property

		/// <summary>
		/// Number of dots to be rendered
		/// </summary>
		public int NumberDots
		{
			get { return (int)GetValue(NumberDotsProperty); }
			set { SetValue(NumberDotsProperty, value); }
		}
		public static readonly DependencyProperty NumberDotsProperty =
			DependencyProperty.Register(
				"NumberDots",
				typeof(int),
				typeof(DottedPanel),
				new FrameworkPropertyMetadata(0,
					(d, e) =>
					{
						((DottedPanel)d).RenderDot();
					},
					(d, baseValue) =>
					{
						var num = (int)baseValue;
						if (num < 0)
							throw new ArgumentOutOfRangeException("NumberDots");

						return num;
					}));

		#endregion


		private void RenderDot()
		{
			if (Designer.IsInDesignMode)
				return;

			if ((this.Width <= 0) || (this.Height <= 0))
				return;

			var rand = new Random();
			var pathList = new List<Path>();

			for (int i = 0; i < NumberDots; i++)
			{
				int x = rand.Next((int)this.Width);
				int y = rand.Next((int)this.Height);

				int diameter = rand.Next(3, 7); // This will produce 4 layers of different opacities.
				var opacity = diameter / 10D;
				var angle = (double)rand.Next(0, 180);

				var transform = new TransformGroup();
				transform.Children.Add(new SkewTransform() { AngleX = 25, AngleY = 15, CenterX = x, CenterY = y });
				transform.Children.Add(new RotateTransform() { Angle = angle, CenterX = x, CenterY = y });

				var dotForm = new EllipseGeometry()
				{
					RadiusX = diameter * 0.8,
					RadiusY = diameter * 0.5,
					Center = new Point(x, y),
					Transform = transform,
				};

				var pathOpacity = pathList.FirstOrDefault(p => p.Opacity == opacity);
				if (pathOpacity == null)
				{
					pathList.Add(new Path()
					{
						Opacity = opacity,
						Data = dotForm,
						StrokeThickness = 0,
						IsHitTestVisible = false,
						Fill = this.Foreground,
					});
				}
				else
				{
					pathOpacity.Data = new CombinedGeometry()
					{
						GeometryCombineMode = GeometryCombineMode.Union,
						Geometry1 = pathOpacity.Data,
						Geometry2 = dotForm,
					};
				}
			}

			pathList.ForEach(p => LayoutRoot.Children.Add(p));
		}
	}
}
