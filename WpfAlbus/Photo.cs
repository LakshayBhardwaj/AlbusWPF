using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfAlbus
{ /// <summary>
  ///     This class describes a single photo - its location, the image and
  ///     the metadata extracted from the image.
  /// </summary>
    public class Photo
    {
        private readonly Uri _source;

        public Photo(string path)
        {
            Source = path;
            _source = new Uri(path);
            Image = BitmapFrame.Create(_source);
            //BitmapMetadata _metadata = (BitmapMetadata)Image.Metadata;

            //var value = 0;
            //var val = _metadata.GetQuery("/app1/ifd/exif/subifd:{uint=40962}");
            //if (val == null)
            //{
            //    value =  0;
            //}
            //if (val.GetType() == typeof(int))
            //{
            //    value = (int)val;
            //}
            //value = Convert.ToInt32(val);
            //if (Image.PixelWidth == Image.PixelHeight)
            //{

            //   // Image = CreateResizedImage((ImageSource)Image, (int)Image.PixelWidth, (int)Image.PixelHeight, 0);
            //}
            Image_Name = System.IO.Path.GetFileName(path);
            
            //Metadata = new ExifMetadata(_source);
        }

        public string Source { get; }

        public string Image_Name { get; }
        public BitmapFrame Image { get; set; }
        //public ExifMetadata Metadata { get; }

        public override string ToString() => _source.ToString();

        //public BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int margin)
        //{
        //    var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

        //    var group = new DrawingGroup();
        //    RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
        //    group.Children.Add(new ImageDrawing(source, rect));

        //    var drawingVisual = new DrawingVisual();
        //    using (var drawingContext = drawingVisual.RenderOpen())
        //        drawingContext.DrawDrawing(group);

        //    var resizedImage = new RenderTargetBitmap(
        //        width, height,         // Resized dimensions
        //        96, 45,                // Default DPI values
        //        PixelFormats.Default); // Default pixel format
        //    resizedImage.Render(drawingVisual);

        //    return BitmapFrame.Create(resizedImage);
        //}
    }
}
