using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MaskEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        private Polyline NewPolyline = null;
        Poly poly = new Poly();
        List<Poly> polyList = new List<Poly>();
        private PointCollection pc = new PointCollection();
        bool stopDrawing = false;

        public MainWindow()
        {
            InitializeComponent();
            //canDraw.Background = new SolidColorBrush(Colors.Black);
        }
        private void btnBrowse(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                txtEditor.Text = openFileDialog.FileName;
                imageBrush.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.RelativeOrAbsolute));
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {

        }

        private void HandleUnchecked(object sender, RoutedEventArgs e)
        {

        }

        private void HandleThirdState(object sender, RoutedEventArgs e)
        {

        }

        // Save button functionality
        // This will save Polygon(only) in the PNG image with name logo.png
        private void btnSave(object sender, RoutedEventArgs e)
        {
            foreach(Point p in pc)
            {
                Console.WriteLine(p);
            }
            //CommandBinding_Executed();
            Canvas canvas = new Canvas();
            //canvas.RenderSize = new System.Windows.Size(500, 400);
            canvas.Height = 1000;
            canvas.Width = 1200;
            canvas.Background = new SolidColorBrush(Colors.DarkBlue);

            
            //canvas.Children.Add(temp);

            foreach(Poly poly in polyList) {
                Polygon temp = new Polygon();

                temp.Fill = new SolidColorBrush(Colors.Bisque);
                // Keep stroke thickness same as the Poly objects else line difference will be seen
                temp.StrokeThickness = 1.5;
                temp.Stroke = new SolidColorBrush(Colors.Bisque);
                temp.Points = new PointCollection(poly.GetPolygon().Points);

                canvas.Children.Add(temp);
            }

            canvas.UpdateLayout();
            SaveCanvas(canvas);
            stopDrawing = false;
        }

        // Reset the canvas 
        // remove all the elements
        private void btnReset(object sender, RoutedEventArgs e)
        {
            canDraw.Children.Clear();
        }

        private void canDraw_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Flag to stop mouse drawing activity on Double click
            

            // See which button was pressed.
            if (e.RightButton == MouseButtonState.Pressed)
            {
                stopDrawing = false;
                Console.WriteLine("Mouse Right Button click");
                // See if we are drawing a new polygon.
                if (NewPolyline != null)
                {
                    
                    if (NewPolyline.Points.Count > 2)
                    {
                        // Polygon1 = [A,B,C],Polygon2 = [A,C,D]  New Polyline = [A,MouseClick,D]
                        // Output
                        // remove polygon from polylist and canvas
                        // New Polyline = [A,MouseClick,C from polygon 1]
                        if (polyList.Count >= 1)
                        {
                            NewPolyline.Points[1] = e.GetPosition(canDraw);
                            NewPolyline.Points[NewPolyline.Points.Count - 1] = polyList[polyList.Count - 1].GetPolygon().Points[NewPolyline.Points.Count - 2];
                            pc[pc.Count - 1] = NewPolyline.Points[NewPolyline.Points.Count - 1];
                            polyList.RemoveAt(polyList.Count - 1);
                            canDraw.Children.RemoveAt(canDraw.Children.Count - 1);
                        }
                        // If no polygon 
                        // remove last point from newpolyline and from pc
                        else
                        {
                            NewPolyline.Points.RemoveAt(NewPolyline.Points.Count -1);
                            pc.RemoveAt(pc.Count - 1);

                        }
                        
                    }
                    // if NewPolyLine(Red dashed line) has 2 points [pt by lclick , mouse move point] remove both from newpolyline
                    // Empty global pc
                    // clear canvas canDraw
                    // clear Newpolyline to start fresh
                    else
                    {
                        NewPolyline = null;
                        canDraw.Children.Clear();
                        pc.Clear();
                    }
                }
                //return;
            }



            // Add a point to the new polygon.
            if (e.LeftButton == MouseButtonState.Pressed && !stopDrawing )
            {
                

                // If Left Single click add point to polyline (rough polygon)
                PointCollection temppc = new PointCollection();
                if (e.ClickCount == 1)
                {
                    Console.WriteLine("Mouse LeftButton Single click");

                    // If we don't have a new polygon, start one.
                    if (NewPolyline == null)
                    {
                        // We have no new polygon. Start one.
                        NewPolyline = new Polyline();
                        NewPolyline.Stroke = System.Windows.Media.Brushes.Red;
                        NewPolyline.StrokeThickness = 1;
                        NewPolyline.StrokeDashArray = new DoubleCollection();
                        NewPolyline.StrokeDashArray.Add(5);
                        NewPolyline.StrokeDashArray.Add(5);
                        NewPolyline.Points.Add(e.GetPosition(canDraw));

                        canDraw.Children.Add(NewPolyline);
                    }

                    // If 1 point in polyline then add new on click
                    if (NewPolyline.Points.Count < 3) {
                        NewPolyline.Points.Add(e.GetPosition(canDraw));
                    }
                    
                    pc.Add(e.GetPosition(canDraw));   
                }
                if (pc.Count() >= 3)
                {
                    temppc.Add(pc[0]);
                    temppc.Add(pc[pc.Count - 2]);
                    temppc.Add(pc[pc.Count - 1]);

                    Poly tempPoly = new Poly();
                    tempPoly.setPolygon(Colors.Black);
                    Polygon temp = tempPoly.GetPolygon();
                    polyList.Add(tempPoly);
                    temp.Points = temppc;

                    temp.Fill = this.FindResource("HatchBrush") as DrawingBrush;
                    //temp.Fill = new SolidColorBrush(Colors.Black);
                    canDraw.Children.Add(temp);
                }
                if (e.ClickCount == 2)
                {
                    // Stop Drawing functionality
                    canDraw.Children.RemoveAt(canDraw.Children.Count - 1);
                    polyList.RemoveAt(polyList.Count-1);
                    stopDrawing = true;
                }
            }
        }

        private void canDraw_MouseMove(object sender, MouseEventArgs e)
        {
            if (NewPolyline == null ) return;
            if (!stopDrawing)
            {
                if (NewPolyline.Points.Count > 2)
                {
                    NewPolyline.Points[0] = pc[0];
                    NewPolyline.Points[1] = e.GetPosition(canDraw);
                    NewPolyline.Points[2] = pc[pc.Count - 1];
                }
                else if (NewPolyline.Points.Count == 2)
                {
                    NewPolyline.Points[0] = pc[0];
                    NewPolyline.Points[1] = e.GetPosition(canDraw);
                }
            }
        }

        private void canDraw_MouseLeave(object sender, MouseEventArgs e)
        {
            // TO BE REMOVE
        }

        private void CommandBinding_Executed()
        {
            Canvas canvas = new Canvas();
            canvas.RenderSize = new System.Windows.Size(500, 400);
            canvas.Background = new SolidColorBrush(Colors.Black);




            Rect rect = new Rect(canvas.RenderSize);

            // Update Layout will update the polygon information and 
            // Canvas will save with the polygon 
            // Otherwise Canvas with only image will save
            canvas.UpdateLayout();

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right,
              (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);
            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();


            pngEncoder.Save(ms);
            System.IO.File.WriteAllBytes("logo2.png", ms.ToArray());

            ms.Close();
            Console.WriteLine("Image Saved Successfully");
        }
        private void CommandBinding_Executed(Canvas can)
        {
            Rect rect = new Rect(can.RenderSize);

            // Update Layout will update the polygon information and 
            // Canvas will save with the polygon 
            // Otherwise Canvas with only image will save
            //can.UpdateLayout();

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right,
              (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(can);
            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();


            pngEncoder.Save(ms);
            System.IO.File.WriteAllBytes("logo2.png", ms.ToArray());
            
            ms.Close();
            Console.WriteLine("Image Saved Successfully");
        }

        private void SaveCanvas(Canvas canvas)
        {
            // PNG形式で保存
            canvas.toImage(@"logo3.png");

            // JPEG形式で保存
            var encoder = new JpegBitmapEncoder();
            //scanvas.toImage(@"c:¥Path¥To¥Test.jpg", encoder);
        }

    }
    class Poly
    {
        private Polygon polygon = null;

        public void setPolygon()
        {
            polygon = new Polygon();
            polygon.Stroke = System.Windows.Media.Brushes.Blue;
            polygon.StrokeThickness = 2;
        }
        public void setPolygon(System.Windows.Media.Color fill)
        {
            polygon = new Polygon();
            polygon.StrokeThickness = 1.5;
            polygon.Stroke = System.Windows.Media.Brushes.Violet;
            polygon.Fill = new SolidColorBrush(fill);
        }
        public void setPoints(PointCollection points)
        {
            this.polygon.Points = points;

        }
        public Polygon GetPolygon()
        {
            return polygon;
        }
    }
    // Canvas クラスの拡張メソッドとして実装する
    public static class CanvasExtensions
    {
        // Canvas を画像ファイルとして保存する。
        public static void toImage(this Canvas canvas, string path, BitmapEncoder encoder = null)
        {
            // レイアウトを再計算させる
            var size = new Size(canvas.Width, canvas.Height);
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            // VisualObjectをBitmapに変換する
            var renderBitmap = new RenderTargetBitmap((int)size.Width,       // 画像の幅
                                                      (int)size.Height,      // 画像の高さ
                                                      96.0d,                 // 横96.0DPI
                                                      96.0d,                 // 縦96.0DPI
                                                      PixelFormats.Pbgra32); // 32bit(RGBA各8bit)
            renderBitmap.Render(canvas);

            // 出力用の FileStream を作成する
            using (var os = new FileStream(path, FileMode.Create))
            {
                // 変換したBitmapをエンコードしてFileStreamに保存する。
                // BitmapEncoder が指定されなかった場合は、PNG形式とする。
                encoder = encoder ?? new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(os);
            }
        }
    }


}
