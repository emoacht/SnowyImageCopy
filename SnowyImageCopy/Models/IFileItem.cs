using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	internal interface IFileItem : IComparable<IFileItem>
	{
		string Directory { get; }
		string FileName { get; }
		int Size { get; }

		bool IsReadOnly { get; }
		bool IsHidden { get; }
		bool IsSystemFile { get; }
		bool IsVolume { get; }
		bool IsDirectory { get; }
		bool IsArchive { get; }

		DateTime Date { get; }
		FileExtension FileExtension { get; }

		bool IsImported { get; }

		string FilePath { get; }
		string Signature { get; }
		bool IsImageFile { get; }
		bool IsJpeg { get; }
		bool IsFlashAirSystemFolder { get; }
	}
}