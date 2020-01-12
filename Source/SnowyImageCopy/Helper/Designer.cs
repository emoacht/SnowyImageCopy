using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Designer information
	/// </summary>
	public static class Designer
	{
		/// <summary>
		/// Whether a Control is in design mode (running in Blend or Visual Studio)
		/// </summary>
		public static bool IsInDesignMode
		{
			get => _isInDesignMode ??= (bool)DependencyPropertyDescriptor
				.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement))
				.Metadata.DefaultValue;			
		}
		private static bool? _isInDesignMode;
	}
}