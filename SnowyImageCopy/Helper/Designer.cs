using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnowyImageCopy.Helper
{
	public static class Designer
	{
		/// <summary>
		/// Check if a Control is in design mode (running in Blend or Visual Studio).
		/// </summary>
		public static bool IsInDesignMode
		{
			get
			{
				if (!_isInDesignMode.HasValue)
				{
					_isInDesignMode = (bool)DependencyPropertyDescriptor
						.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement))
						.Metadata.DefaultValue;
				}

				return _isInDesignMode.Value;
			}
		}
		private static bool? _isInDesignMode;
	}
}
