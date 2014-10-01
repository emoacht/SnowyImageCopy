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

namespace SnowyImageCopy.Views.Controls
{
	/// <summary>
	/// Interaction logic for ThumbnailBox.xaml
	/// </summary>
	public partial class ThumbnailBox : UserControl
	{
		#region Dependency Property

		/// <summary>
		/// Stroke brush of Box rectangle
		/// </summary>
		public Brush StrokeBrush
		{
			get { return Box.Stroke; }
			set { Box.Stroke = value; }
		}

		#endregion


		public ThumbnailBox()
		{
			InitializeComponent();

			StrokeBrush = new SolidColorBrush(Color.FromArgb(128, 100, 100, 100)); // Default brush
		}
	}
}