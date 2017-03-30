
namespace SnowyImageCopy.Models
{
	/// <summary>
	/// File extension for image files
	/// </summary>
	internal enum FileExtension
	{
		other = 0,
		jpg,
		jpeg,
		bmp,
		png,
		tif,
		tiff,
		raw, // raw (Microsoft Camera Codec Pack supports some models)
		dng, // raw (Microsoft Camera Codec Pack supports some models)
		cr2, // Canon raw (Microsoft Camera Codec Pack supports some models)
		crw, // Canon raw (Microsoft Camera Codec Pack supports some models)
		erf, // Epson raw (Microsoft Camera Codec Pack supports some models)
		raf, // Fujifilm raw (Microsoft Camera Codec Pack supports a model)
		kdc, // Kodac raw (Microsoft Camera Codec Pack supports some models)
		rwl, // Leica raw
		nef, // Nikon raw (Microsoft Camera Codec Pack supports some models)
		orf, // Olympus raw (Microsoft Camera Codec Pack supports some models)
		rw2, // Panasonic raw (Microsoft Camera Codec Pack supports some models)
		pef, // Pentax raw (Microsoft Camera Codec Pack supports some models)
		srw, // Samsung raw (Microsoft Camera Codec Pack supports some models)
		x3f, // Sigma raw
		arw, // Sony raw (Microsoft Camera Codec Pack supports some models)
	}
}