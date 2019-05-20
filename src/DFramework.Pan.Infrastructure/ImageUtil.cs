using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace DFramework.Pan.Infrastructure
{
    public class ImageUtil
    {
        public static Stream ThumbnailOld(Stream source, Int32 width, Int32 height)
        {
            Image image = Image.FromStream(source);
            var size = GetImageSize(image, width, height);
            Image thumb = image.GetThumbnailImage(size.Width, size.Height, () => false, IntPtr.Zero);
            var ms = new MemoryStream();
            thumb.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public static Stream Thumbnail(Stream source, Int32 width, Int32 height, String contentType)
        {
            Image srcImage = Image.FromStream(source);
            var size = GetImageSize(srcImage, width, height);

            Bitmap newImage = new Bitmap(size.Width, size.Height);
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.DrawImage(srcImage, new Rectangle(new Point(), size));
            }
            var ms = new MemoryStream();
            newImage.Save(ms, GetImageFormat(contentType));
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public static Size GetImageSize(Image picture, int width, int height)
        {
            //不能超过原始宽高
            width = Math.Min(width, picture.Width);
            height = Math.Min(height, picture.Height);

            //0:原始大小
            if (width == 0)
                width = picture.Width;
            if (height == 0)
                height = picture.Height;

            if (width < 1 || height < 1)
                throw new Exception("尺寸不能小于1");

            Size imageSize;

            imageSize = new Size(width, height);

            double widthRatio = (double)picture.Width / picture.Height;
            double heightRatio = (double)picture.Height / picture.Width;

            int desiredHeight = imageSize.Height;
            int desiredWidth = imageSize.Width;

            imageSize.Height = desiredHeight;
            if (widthRatio > 0)
                imageSize.Width = Convert.ToInt32(imageSize.Height * widthRatio);

            if (imageSize.Width > desiredWidth)
            {
                imageSize.Width = desiredWidth;
                imageSize.Height = Convert.ToInt32(imageSize.Width * heightRatio);
            }

            return imageSize;
        }

        public static ImageFormat GetImageFormat(String contentType)
        {
            switch (contentType.ToLower())
            {
                case ".jpeg":
                case ".jpg":
                    return System.Drawing.Imaging.ImageFormat.Jpeg;

                case ".png":
                    return System.Drawing.Imaging.ImageFormat.Png;

                case ".bmp":
                    return System.Drawing.Imaging.ImageFormat.Bmp;

                case ".gif":
                    return System.Drawing.Imaging.ImageFormat.Gif;

                case ".tif":
                case ".tiff":
                    return System.Drawing.Imaging.ImageFormat.Tiff;

                case ".ico":
                case ".icon":
                    return System.Drawing.Imaging.ImageFormat.Icon;

                case ".emf":
                    return System.Drawing.Imaging.ImageFormat.Emf;

                case ".exif":
                    return System.Drawing.Imaging.ImageFormat.Exif;

                case ".wmf":
                    return System.Drawing.Imaging.ImageFormat.Wmf;

                default:
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
            }
        }
    }
}