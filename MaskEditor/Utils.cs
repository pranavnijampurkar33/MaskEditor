using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MaskEditor
{
    class Utils
    {
        public static void SaveCanvas(Canvas canvas,string imgName)
        {
            // PNG形式で保存
            canvas.toImage(imgName);

            // JPEG形式で保存
            //var encoder = new JpegBitmapEncoder();
            //scanvas.toImage(@"c:¥Path¥To¥Test.jpg", encoder);
        }

        public static Polyline initPolyLine()
        {
            Polyline NewPolyline = new Polyline();
            NewPolyline.Stroke = System.Windows.Media.Brushes.Red;
            NewPolyline.StrokeThickness = 1;
            NewPolyline.StrokeDashArray = new DoubleCollection();
            NewPolyline.StrokeDashArray.Add(5);
            NewPolyline.StrokeDashArray.Add(5);
            return NewPolyline;
        }
        public static Path GetPath(Geometry geometry, Color color, double op = 1)
        {
            Path path = new Path();
            SolidColorBrush strokeBrush = new SolidColorBrush(color);
            strokeBrush.Opacity = op;
            path.Fill = strokeBrush;
            path.Data = geometry;
            return path;
        }
        public static CombinedGeometry GetUnionGeometry(List<Poly> polyList,Canvas canvas)
        {
            CombinedGeometry union = new CombinedGeometry();
            foreach (Poly poly in polyList)
            {
                Polygon temp = new Polygon();
                temp.Fill = new SolidColorBrush(Colors.Transparent);
                // Keep stroke thickness same as the Poly objects else line difference will be seen
                temp.StrokeThickness = 1.5;
                temp.Stroke = new SolidColorBrush(Colors.Transparent);
                temp.Points = new PointCollection(poly.GetPolygon().Points);
                // Need to add and render the shape BC we need Geometry Object by rendered object
                canvas.Children.Add(temp);
                temp.UpdateLayout();
                union = Utils.CombinedGeometryUnion(union, temp.RenderedGeometry);
            }
            return union;
        }
        public static CombinedGeometry CombinedGeometryExclude(Geometry excludeFrom, Geometry geometry)
        {
            CombinedGeometry combinedGeometry = new CombinedGeometry(
                GeometryCombineMode.Exclude,
                excludeFrom,
                geometry);
            return combinedGeometry;
        }
        public static CombinedGeometry CombinedGeometryUnion(Geometry geometry1, Geometry geometry2)
        {
            CombinedGeometry combinedGeometry = new CombinedGeometry(
                GeometryCombineMode.Union,
                geometry1,
                geometry2);
            return combinedGeometry;
        }

        
        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new System.Drawing.Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
