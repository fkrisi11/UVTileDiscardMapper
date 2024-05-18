using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UVTileDiscardMapper
{
    public static class ImageTypeChecker
    {
        public static FileType GetImageType(string filePath)
        {
            FileType result = GetKnownFileType(filePath);

            switch (result)
            {
                case FileType.Unknown:
                    break;
                case FileType.Webp:
                case FileType.Webp2:
                case FileType.Webp3:
                case FileType.WebpLast:
                    return FileType.Webp;
                default:
                    return result;
            }

            if (IsSvgImage(filePath))
                return FileType.Svg;
            else
                return FileType.Unknown;
        }

        private static bool IsSvgImage(string filePath)
        {
            try
            {
                // Read the first few bytes of the file to ensure it's text-based
                using (StreamReader reader = new StreamReader(filePath))
                {
                    char[] buffer = new char[100];
                    int bytesRead = reader.Read(buffer, 0, 100);

                    // Check if the file is text-based
                    if (bytesRead < 2 || buffer[0] != '<' || buffer[1] != '?')
                        return false; // Not an XML file

                    // Check if the content starts with an SVG element
                    string content = new string(buffer, 0, bytesRead);
                    return (content.TrimStart().StartsWith("<svg", StringComparison.OrdinalIgnoreCase) || content.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase));
                }
            }
            catch (Exception)
            {
                // Error occurred while reading the file or parsing XML
                return false;
            }
        }

        public enum FileType
        {
            Unknown,
            Jpeg,
            JpegEOI,
            Bmp,
            Gif,
            Png,
            Webp,
            Webp2,
            Webp3,
            WebpLast,
            TiffI,
            TiffM,
            Svg // This is checked by another method
        }

        private static readonly Dictionary<FileType, byte[]> KNOWN_FILE_HEADERS = new Dictionary<FileType, byte[]>()
        {
        { FileType.Jpeg, new byte[]{ 0xFF, 0xD8 }}, // JPEG
        { FileType.JpegEOI, new byte[]{ 0xFF, 0xD9 }}, // JPEG EOI
        { FileType.TiffI, new byte[]{ 0x49, 0x49, 0x2A, 0x00 }}, // TiffI
        { FileType.TiffM, new byte[]{ 0x4D, 0x4D, 0x00, 0x2A }}, // TiffM
		{ FileType.Bmp, new byte[]{ 0x42, 0x4D }}, // BMP
		{ FileType.Gif, new byte[]{ 0x47, 0x49, 0x46 }}, // GIF
		{ FileType.Png, new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }}, // PNG
        { FileType.Webp, new byte[]{ 0x52, 0x49, 0x46, 0x46, 0xFF, 0xFF, 0xFF, 0xFF, 0x57, 0x45, 0x42, 0x50 } }, // WEBP
        { FileType.Webp2, new byte[]{ 0x52, 0x49, 0x46, 0x46, 0xBC, 0xCE, 0x06, 0x00, 0x57, 0x45, 0x42, 0x50 } }, // WEBP 2nd variation
        { FileType.Webp3, new byte[] { 0x52, 0x49, 0x46, 0x46, 0xBA, 0x2E, 0x08, 0x00, 0x57, 0x45, 0x42, 0x50 } }, // WEBP 3rd variation
        { FileType.WebpLast, new byte[] { 0x52, 0x49, 0x46 } } // WEBP 3rd variation
	    };

        public static FileType GetKnownFileType(string filePath)
        {
            Stream data = File.OpenRead(filePath);

            foreach (var check in KNOWN_FILE_HEADERS)
            {
                data.Seek(0, SeekOrigin.Begin);

                var slice = new byte[check.Value.Length];
                data.Read(slice, 0, check.Value.Length);
                if (slice.SequenceEqual(check.Value))
                {
                    data.Seek(0, SeekOrigin.Begin);
                    data.Close();
                    return check.Key;
                }
            }

            data.Seek(0, SeekOrigin.Begin);
            data.Close();
            return FileType.Unknown;
        }
    }
}
