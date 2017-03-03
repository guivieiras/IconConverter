# IconConverter

A C# Application to convert image files to .ico and creating multi-sized icons.

###How to use:

You can drag and drop files directly to the window or click 'File -> Open' to load an image. To select a region, click and drag on the image, release to finish. Then, you can choose what dimensions you want to convert at the 'Export' menu. To merge multiple .ico, click the 'Merge' menu, choose the preferred option and select the files that you want to merge. After that, specify the location to save the result.

<p align="center">
<img align="center" src="http://i.imgur.com/yCbhyqR.png" width="551"/>

**Note:** It creates a folder at ***%AppData%*** only to store the converted icons, that folder can be deleted after use.

###Libraries used:

- [ImageMagick](https://github.com/ImageMagick/ImageMagick)     
- IconEx
