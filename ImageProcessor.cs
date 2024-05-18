using System.Drawing;

namespace UVTileDiscardMapper
{
    internal static class ImageProcessor
    {
        internal static Image ReadImage(string Path)
        {
            Bitmap bitmap = null;

            ImageTypeChecker.FileType imageType = ImageTypeChecker.GetImageType(Path);

            switch (imageType)
            {
                case ImageTypeChecker.FileType.Unknown:
                    bitmap = null;
                    break;
                case ImageTypeChecker.FileType.Webp:
                    WebP webp = new WebP();
                    bitmap = webp.Load(Path);
                    break;
                case ImageTypeChecker.FileType.Svg:
                    bitmap = null;
                    break;
                default:
                    bitmap = LoadBitmapUnlocked(Path);
                    break;
            }

            return bitmap;
        }

        // Load a bitmap without locking it.
        internal static Bitmap LoadBitmapUnlocked(string path)
        {
            using (Bitmap bm = new Bitmap(path))
            {
                return new Bitmap(bm);
            }
        }
    }
}
