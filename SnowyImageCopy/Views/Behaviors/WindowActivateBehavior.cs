using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Activate Window.
	/// </summary>
	[TypeConstraint(typeof(Window))]
	public class WindowActivateBehavior : Behavior<Window>
	{
		#region Property

		/// <summary>
		/// Whether activating Window is requested
		/// </summary>
		public bool IsRequested
		{
			get { return (bool)GetValue(IsRequestedProperty); }
			set { SetValue(IsRequestedProperty, value); }
		}
		public static readonly DependencyProperty IsRequestedProperty =
			DependencyProperty.Register(
				"IsRequested",
				typeof(bool),
				typeof(WindowActivateBehavior),
				new FrameworkPropertyMetadata(
					false,
					(d, e) =>
					{
						if ((bool)e.NewValue)
						{
							var window = ((WindowActivateBehavior)d).AssociatedObject;
							if (window.IsActive == false)
							{
								if (window.WindowState == WindowState.Minimized)
									window.WindowState = WindowState.Normal;

								window.Activate();
							}
						}
					}));

		#endregion

		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.Activated += OnActivatedChanged;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			this.AssociatedObject.Activated -= OnActivatedChanged;
		}

		private void OnActivatedChanged(object sender, EventArgs e)
		{
			// When Window is activated, clear the flag to prepare for next request.
			if (this.AssociatedObject.IsActive)
				IsRequested = false;
		}
	}
}