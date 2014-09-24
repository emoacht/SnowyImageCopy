using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Play SystemSound.
	/// </summary>
	public class SystemSoundPlayAction : TriggerAction<DependencyObject>
	{
		#region Dependency Property

		/// <summary>
		/// SystemSound to be played
		/// </summary>
		public SystemSound Sound
		{
			get { return (SystemSound)GetValue(SoundProperty); }
			set { SetValue(SoundProperty, value); }
		}
		public static readonly DependencyProperty SoundProperty =
			DependencyProperty.Register(
				"Sound",
				typeof(SystemSound),
				typeof(SystemSoundPlayAction),
				new FrameworkPropertyMetadata(null));

		#endregion


		protected override void Invoke(object parameter)
		{
			if (Sound == null)
				return;

			Sound.Play();
		}
	}
}