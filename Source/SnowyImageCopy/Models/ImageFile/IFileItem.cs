using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.ImageFile
{
	internal interface IFileItem : IComparable<IFileItem>
	{
		string Directory { get; }
		string FileName { get; }
		string FileExtension { get; }
		int Size { get; }

		bool IsReadOnly { get; }
		bool IsHidden { get; }
		bool IsSystem { get; }
		bool IsVolume { get; }
		bool IsDirectory { get; }
		bool IsArchive { get; }

		DateTime Date { get; }

		bool IsImported { get; }

		string FilePath { get; }
		HashItem Signature { get; }

		bool IsImageFile { get; }
		bool IsJpeg { get; }
		bool IsTiff { get; }
		bool IsLoadable { get; }
		bool IsFlashAirSystem { get; }

		bool Equals(IFileItem other);
	}
}