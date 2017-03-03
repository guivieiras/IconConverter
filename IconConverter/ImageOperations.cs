using ImageMagick;
using System;
using System.Drawing;
using System.IO;
using IconConverter.IconEx;
using System.Windows;

namespace IconConverter {
    public static class ImageOperations {

        enum Direction {
            NE, SE, SW, NW
        }

        /// <summary>
        /// Gets the direction of the rectangle from the origin.
        /// </summary>
        private static Direction getDirection(System.Windows.Shapes.Rectangle rect) {
            if (rect.VerticalAlignment == VerticalAlignment.Top && rect.HorizontalAlignment == HorizontalAlignment.Left)
                return Direction.SE;
            if (rect.VerticalAlignment == VerticalAlignment.Top && rect.HorizontalAlignment == HorizontalAlignment.Right)
                return Direction.SW;
            if (rect.VerticalAlignment == VerticalAlignment.Bottom && rect.HorizontalAlignment == HorizontalAlignment.Left)
                return Direction.NE;

            return Direction.NW;
        }

        internal static void CropAndSave(MagickImage img, string fileName, int dimension, System.Windows.Shapes.Rectangle rect, double scale, System.Windows.Controls.Image pictureBox) {
            Directory.CreateDirectory(Path.GetFileNameWithoutExtension(fileName));

            img = new MagickImage(img);

            cropImage(img, rect, scale, pictureBox);

            string iconFilename = Path.Combine(Path.GetFileNameWithoutExtension(fileName), $"CroppedIcon_{dimension}.ico");

            img.Scale(new MagickGeometry(dimension) { IgnoreAspectRatio = true });
            Console.WriteLine(img.HasAlpha + " " + img.BitDepth());
            if (dimension == 256) {
                if ((img.Format == MagickFormat.Jpeg || img.Format == MagickFormat.Jpg) && MessageBox.Show("Compact into jpg .ico?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    IconFileWriter x = new IconFileWriter();
                    img.HasAlpha = true;
                    x.Images.Add(img.ToBitmap());
                    x.Save(iconFilename);

                    return;
                }
                else {
                    img.HasAlpha = true;
                }
            }

            img.Write(iconFilename);
        }

        private static void cropImage(MagickImage img, System.Windows.Shapes.Rectangle rect, double scale, System.Windows.Controls.Image pictureBox) {
            int height = (int)(rect.Height / scale);
            int width = (int)(rect.Width / scale);

            if (getDirection(rect) == Direction.NE) {
                int left = (int)(rect.Margin.Left / scale);
                int top = (int)((pictureBox.Height - (rect.Margin.Bottom + rect.Height)) / scale);

                img.Crop(left, top, height, width);
            }
            if (getDirection(rect) == Direction.SE) {
                int left = (int)(rect.Margin.Left / scale);
                int top = (int)((rect.Margin.Top) / scale);

                img.Crop(left, top, height, width);
            }
            if (getDirection(rect) == Direction.SW) {
                int left = (int)((pictureBox.Width - rect.Margin.Right - rect.Width) / scale);
                int top = (int)((rect.Margin.Top) / scale);

                img.Crop(left, top, height, width);
            }
            if (getDirection(rect) == Direction.NW) {
                int left = (int)((pictureBox.Width - (rect.Margin.Right + rect.Width)) / scale);
                int top = (int)((pictureBox.Height - (rect.Margin.Bottom + rect.Height)) / scale);

                img.Crop(left, top, height, width);
            }
        }

        internal static void SaveWithoutCrop(string fileName, int dimension) {
            Directory.CreateDirectory(Path.GetFileNameWithoutExtension(fileName));

            MagickImage img = new MagickImage(fileName);
            img.Scale(new MagickGeometry(dimension) { IgnoreAspectRatio = true });
            Bitmap bmp = img.ToBitmap();
            MagickImage final = new MagickImage(bmp);

            string iconFilename = Path.Combine(Path.GetFileNameWithoutExtension(fileName), $"WholeIcon_{dimension}.ico");

            final.Write(iconFilename);
        }

        internal static IconDeviceImage GetIconBMP(string fileName) {
            MagickImage ico = new MagickImage(fileName);
            IconDeviceImage IconDeviceImage = new IconDeviceImage(new System.Drawing.Size(ico.Width, ico.Height), System.Windows.Forms.ColorDepth.Depth32Bit);
            IconDeviceImage.IconImage = ico.ToBitmap();

            return IconDeviceImage;
        }

        internal static Image GetIconPNG(string fileName) {
            MagickImage ico = new MagickImage(fileName);
            return ico.ToBitmap();
        }      
    }
}
