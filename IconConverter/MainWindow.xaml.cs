using IconConverter.IconEx;
using ImageMagick;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Media;

namespace IconConverter {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>
        /// Custom checkBoxes with a 'Dimesion' property included.
        /// </summary>
        List<CCheckBox> checkBoxes = new List<CCheckBox>();
        MagickImage loadedImage;
        string loadedImageFilename;
        double scale = 1;

        public MainWindow() {
            InitializeComponent();

            checkBoxes.AddRange(new[] { cb256, cb128, cb64, cb48, cb32, cb16 });

            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IconConverter");
            Directory.CreateDirectory(dir);
            Directory.SetCurrentDirectory(dir);

            SizeChanged += WindowSizeChanged;
        }
      
        void loadImage(string fileName) {
            scale = 1;
            loadedImageFilename = fileName;

            MagickImage image = new MagickImage(fileName);
            loadedImage = image;

            updateScale();

            pictureBox.Source = image.ToBitmapSource();
        }

        /// <summary>
        /// When resizing the window, handles the scaling of the image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowSizeChanged(object sender, SizeChangedEventArgs e) => updateScale();
          
        void updateScale() {
            if (loadedImage != null) {
                pictureGrid.Children.Remove(rect);

                scale = (grid1.ActualWidth - 10) / loadedImage.Width;

                if (scale * loadedImage.Height > grid1.ActualHeight - bord.Margin.Top - 10)
                    scale = (grid1.ActualHeight - bord.Margin.Top - 10) / loadedImage.Height;

                pictureBox.Height = loadedImage.Height * scale;
                pictureBox.Width = loadedImage.Width * scale;
            }
        }

        #region ButtonActions

        private void btn_OpenFile_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog().Value == true) {
                loadImage(ofd.FileName);
            }
        }

        private void btn_SaveWithoutCrop_Click(object sender, RoutedEventArgs e) {
            foreach (CCheckBox cb in checkBoxes)
                if (cb.IsChecked.Value)
                    ImageOperations.SaveWithoutCrop(loadedImageFilename, cb.Dimension);

            Process.Start(Path.GetFileNameWithoutExtension(loadedImageFilename));
        }

        private void btn_CropAndSave_Click(object sender, RoutedEventArgs e) {

            if (rect != null && pictureGrid.Children.Contains(rect)) {
                foreach (var cb in checkBoxes) {
                    if (cb.IsChecked.Value) {
                        ImageOperations.CropAndSave(loadedImage, loadedImageFilename, cb.Dimension, rect, scale, pictureBox);
                    }
                }
                Process.Start(Path.GetFileNameWithoutExtension(loadedImageFilename));
            } else MessageBox.Show("Select an area before using this feature.");
        }

        private void btn_MergeIntoBMP_Click(object sender, RoutedEventArgs e) {
            string[] selectedFiles = selectedFilesOnDialog();
            if (selectedFiles != null) {
                IconEx.IconEx icon = new IconEx.IconEx();
                foreach (string fileName in selectedFiles) {
                    icon.iconCollection.Add(ImageOperations.GetIconBMP(fileName));

                }
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.AddExtension = true;
                sfd.InitialDirectory = Directory.GetCurrentDirectory();
                sfd.DefaultExt = ".ico";
                sfd.Filter = "Icon |*.ico";

                if (sfd.ShowDialog().Value)
                    icon.Save(sfd.FileName);
            }
        }

        private void btn_MergeIntoPNG_Click(object sender, RoutedEventArgs e) {
            string[] selectedFiles = selectedFilesOnDialog();
            if (selectedFiles !=null) {
                IconFileWriter icon = new IconFileWriter();
                
                foreach (string fileName in selectedFiles) {
                    icon.Images.Add(ImageOperations.GetIconPNG(fileName));

                }
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = ".ico";
                sfd.Filter = "Icon |*.ico";
                if (sfd.ShowDialog().Value)
                    icon.Save(sfd.FileName);
            }
        }

        #endregion

        private string[] selectedFilesOnDialog() {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Multiselect = true;
            OFD.InitialDirectory = Environment.CurrentDirectory;
            OFD.DefaultExt = ".ico";
            OFD.Filter = "Icon |*.ico";
            if (OFD.ShowDialog().Value) {
                return OFD.FileNames.Reverse().ToArray();
            } else return null;
        }


        #region Rectangle Selection
        Point first;
        System.Windows.Shapes.Rectangle rect;
        public bool isPressed = false;

        /// <summary>
        /// Starts the selection when pressing the mouse button.
        /// </summary>
        private void pictureBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            isPressed = true;
            first = e.GetPosition((Image)sender);

            if (rect == null) {
                rect = new System.Windows.Shapes.Rectangle();
                rect.Opacity = 0.5;
                rect.IsHitTestVisible = false;
                rect.Fill = Brushes.Black;
            }
            rect.Fill = Brushes.Black;

            rect.VerticalAlignment = VerticalAlignment.Top;
            rect.Margin = new Thickness(first.X, first.Y, 0, 0);
            rect.Width = 0;
            rect.Height = 0; rect.HorizontalAlignment = HorizontalAlignment.Left;

            if (!pictureGrid.Children.Contains(rect))
                pictureGrid.Children.Add(rect);
        }

        /// <summary>
        /// Finished the selection when releasing the mouse button.
        /// </summary>
        private void window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            isPressed = false;
            if (rect !=null) rect.Fill = Brushes.White;
        }

        /// <summary>
        /// Handles the selection square margins and size.
        /// </summary>
        private void pictureBox_MouseMove(object sender, MouseEventArgs e) {
            Image imagem = (Image)sender;

            if (isPressed) {

                double horizontalDiff = e.GetPosition(imagem).X - first.X;
                double verticalDiff = e.GetPosition(imagem).Y - first.Y;

                //Up right
                if (horizontalDiff >= 0 && verticalDiff < 0) {
                    rect.HorizontalAlignment = HorizontalAlignment.Left;
                    rect.VerticalAlignment = VerticalAlignment.Bottom;
                    rect.Margin = new Thickness(first.X, 0, 0, imagem.ActualHeight - bord.BorderThickness.Right * 2 - first.Y);

                    if (Math.Abs(verticalDiff) > horizontalDiff)
                        rect.Width = Math.Abs(verticalDiff);
                    else rect.Width = horizontalDiff;

                    rect.Height = rect.Width;

                    double outOfBounds = 0;
                    if (rect.Height + rect.Margin.Bottom > imagem.ActualHeight) {
                        outOfBounds = imagem.ActualHeight - rect.Margin.Bottom;
                        rect.Height = outOfBounds;
                        rect.Width = outOfBounds;
                    }

                    if (rect.Width + rect.Margin.Left > imagem.ActualWidth) {
                        outOfBounds = imagem.ActualWidth - rect.Margin.Left;
                        rect.Height = outOfBounds;
                        rect.Width = outOfBounds;
                    }



                }
                //Down right
                if (horizontalDiff >= 0 && verticalDiff >= 0) {
                    rect.HorizontalAlignment = HorizontalAlignment.Left;
                    rect.VerticalAlignment = VerticalAlignment.Top;
                    rect.Margin = new Thickness(first.X, first.Y, 0, 0);

                    if (verticalDiff > horizontalDiff)
                        rect.Width = verticalDiff;
                    else rect.Width = horizontalDiff;

                    rect.Height = rect.Width;

                    double outOfBounds = 0;
                    if (rect.Height + rect.Margin.Top > imagem.ActualHeight) {
                        outOfBounds = imagem.ActualHeight - rect.Margin.Top;
                        rect.Height = outOfBounds;
                        rect.Width = outOfBounds;
                    }

                    if (rect.Width + rect.Margin.Left > imagem.ActualWidth) {
                        outOfBounds = imagem.ActualWidth - rect.Margin.Left;
                        rect.Height = outOfBounds;
                        rect.Width = outOfBounds;
                    }
                }
                //Down left
                if (horizontalDiff < 0 && verticalDiff >= 0) {
                    rect.HorizontalAlignment = HorizontalAlignment.Right;
                    rect.VerticalAlignment = VerticalAlignment.Top;
                    rect.Margin = new Thickness(0, first.Y, imagem.ActualWidth - first.X, 0);

                    if (verticalDiff > Math.Abs(horizontalDiff))
                        rect.Width = verticalDiff;
                    else rect.Width = Math.Abs(horizontalDiff);

                    rect.Height = rect.Width;

                    double outOfBounds = 0;
                    if (rect.Height + rect.Margin.Top > imagem.ActualHeight) {
                        outOfBounds = imagem.ActualHeight - rect.Margin.Top;
                        rect.Height = outOfBounds;
                        rect.Width = outOfBounds;
                    }

                    if (rect.Width + rect.Margin.Right > imagem.ActualWidth) {
                        outOfBounds = imagem.ActualWidth - rect.Margin.Right;
                        rect.Height = outOfBounds;
                        rect.Width = outOfBounds;
                    }
                }
                //Up left
                if (horizontalDiff < 0 && verticalDiff < 0) {
                    rect.HorizontalAlignment = HorizontalAlignment.Right;
                    rect.VerticalAlignment = VerticalAlignment.Bottom;
                    rect.Margin = new Thickness(0, 0, imagem.ActualWidth - first.X, imagem.ActualHeight - bord.BorderThickness.Right * 2 - first.Y);

                    if (Math.Abs(verticalDiff) > Math.Abs(horizontalDiff))
                        rect.Width = Math.Abs(verticalDiff);
                    else rect.Width = Math.Abs(horizontalDiff);

                    rect.Height = rect.Width;

                    double outOfBounds = 0;
                    if (rect.Height + rect.Margin.Bottom > imagem.ActualHeight) {
                        outOfBounds = imagem.ActualHeight - rect.Margin.Bottom;
                        rect.Height = outOfBounds;
                        rect.Width = outOfBounds;
                    }

                    if (rect.Width + rect.Margin.Right > imagem.ActualWidth) {
                        outOfBounds = imagem.ActualWidth - rect.Margin.Right;
                        rect.Height = outOfBounds;
                        rect.Width = outOfBounds;
                    }
                }
            }
        }

        /// <summary>
        /// Removes the square selection if Escape is pressed.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                isPressed = false;
                pictureGrid.Children.Remove(rect);
            }
        }

        /// <summary>
        /// Opens the file that was dragged to the window.
        /// </summary>
        private void Window_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                loadImage(files[0]);
            }
        }
        #endregion

      

    

    }
}

