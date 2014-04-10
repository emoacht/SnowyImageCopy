using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	public class Shortcut
	{
		/// <summary>
		/// Check if a specified shortcut exists.
		/// </summary>
		/// <param name="shortcutPath">Path of shortcut file</param>
		/// <param name="targetPath">Target path of shortcut</param>
		/// <param name="argument">Argument of shortcut</param>
		/// <param name="appId">AppUserModelID of shortcut</param>
		/// <returns>True if exists</returns>
		public bool CheckShortcut(string shortcutPath, string targetPath, string argument, string appId)
		{
			if (!File.Exists(shortcutPath))
				return false;

			try
			{
				using (var shortcut = new ShellLink(shortcutPath))
				{
					// File path casing may be different from that when installed the shortcut.
					return (shortcut.TargetPath.Equals(targetPath, StringComparison.OrdinalIgnoreCase) &&
						shortcut.Arguments.Equals(argument, StringComparison.Ordinal) &&
						shortcut.AppUserModelID.Equals(appId, StringComparison.Ordinal));
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to check a shortcut.", ex);
			}
		}

		/// <summary>
		/// Install a specified shortcut.
		/// </summary>
		/// <param name="shortcutPath">Path of shortcut file</param>
		/// <param name="targetPath">Target path of shortcut</param>
		/// <param name="argument">Argument of shortcut</param>
		/// <param name="appId">AppUserModelID of shortcut</param>
		/// <param name="iconPath">Path of file that contains an icon to be used for shortcut file</param>
		public void InstallShortcut(string shortcutPath, string targetPath, string argument, string appId, string iconPath)
		{
			try
			{
				using (var shortcut = new ShellLink()
				{
					TargetPath = targetPath,
					Arguments = argument,
					AppUserModelID = appId,
					IconPath = iconPath,
					IconIndex = 0, // 1st icon in the file
					WindowStyle = ShellLink.SW.SW_SHOWMINNOACTIVE,
				})
				{
					shortcut.Save(shortcutPath);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to install a shortcut.", ex);
			}
		}

		/// <summary>
		/// Delete a specified shortcut.
		/// </summary>
		/// <param name="shortcutPath">Path of shortcut file</param>
		/// <param name="targetPath">Target path of shortcut</param>
		/// <param name="argument">Argument of shortcut</param>
		/// <param name="appId">AppUserModelID of shortcut</param>
		/// <remarks>If contents of shortcut file do not match, the file will not be deleted.</remarks>
		public void DeleteShortcut(string shortcutPath, string targetPath, string argument, string appId)
		{
			if (!CheckShortcut(shortcutPath, targetPath, argument, appId))
				return;

			File.Delete(shortcutPath);
		}
	}
}
