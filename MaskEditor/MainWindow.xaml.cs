using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    
    public partial class MainWindow : Window
    {
        private Polyline NewPolyline = null;
        List<Poly> polyList = new List<Poly>();
        private PointCollection pc = new PointCollection();
        bool stopDrawing = true;
        private string currentImage = "";

        public MainWindow()
        {
            InitializeComponent();
            //canDraw.Background = new SolidColorBrush(Colors.Black);
        }
        private void btnBrowse(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;
                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                {
                    currentImage = openFileDialog.FileName;
                    txtEditor.Text = currentImage;
                    imageBrush.ImageSource = new BitmapImage(new Uri(currentImage, UriKind.RelativeOrAbsolute));

                    // Enable Drawing if Image is selected
                    stopDrawing = false;

                }
            }                
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            //var checkedValue = panel.Children.OfType<RadioButton>()
              //   .FirstOrDefault(r => r.IsChecked.HasValue && r.IsChecked.Value);
            if (rbAdd.IsChecked == true)
            {
                //MessageBox.Show(rbAdd.Content.ToString());
                stopDrawing = false;
                Console.WriteLine(rbAdd.Content.ToString() +" is "+ rbAdd.IsChecked);
            }
            else if(rbDel.IsChecked == true)
            {
                //MessageBox.Show(rbDel.Content.ToString());
                stopDrawing = true;
                Console.WriteLine(rbDel.Content.ToString() + " is " + rbDel.IsChecked);
            }                
        }
        
        // Save button functionality
        // This will save Polygon(only) in the PNG image with name logo.png
        private void btnSave(object sender, RoutedEventArgs e)
        {
            
            Canvas canvas = new Canvas();
            canvas.Height = canDraw.ActualHeight;
            canvas.Width = canDraw.ActualWidth;
            canvas.Background = new SolidColorBrush(Colors.White);

            // Get Union Geometry of all the polygons
            CombinedGeometry union = Utils.GetUnionGeometry(polyList,canDraw);

            canvas.Children.Add(Utils.GetPath(union, Colors.Black));
            canvas.UpdateLayout();
            //Save mask image
            string mask_file = "mask_image.png";
            Utils.SaveCanvas(canvas, @"mask_image.png");

            // Resize mask image to Base image Height and Width
            System.Drawing.Image mask = System.Drawing.Image.FromFile(mask_file);

            var ratio = Math.Min(canDraw.RenderSize.Width / imageBrush.ImageSource.Width , canDraw.RenderSize.Height / imageBrush.ImageSource.Height);
            var imageBrushWidth = imageBrush.ImageSource.Width * ratio;
            var imageBrushHeight = imageBrush.ImageSource.Height * ratio;

            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                Bitmap bmp = Utils.ResizeImage(mask, 1980, 1080);
                bmp.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }

            // Using same canvas to add Image BG
            // Create separate canvas for operations
            canvas.Children.Clear();
            
            
            // Set Base image as current CANVAS background
            

            // Make things Ready for new Polygon Drawing
            stopDrawing = false;
            NewPolyline = null;
            pc = new PointCollection(); 
            canDraw.Children.Clear();
            polyList.Clear();           // New polylist for every new Figure
            canDraw.Children.Add(Utils.GetPath(union, Colors.Blue, .4d));
        }

        // Reset the canvas 
        // remove all the elements
        private void btnReset(object sender, RoutedEventArgs e)
        {
            stopDrawing = false;
            NewPolyline = null;
            pc = new PointCollection();
            canDraw.Children.Clear();
        }

        private void canDraw_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // See which button was pressed.
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Console.WriteLine("Mouse Right Button click");
                // See if we are drawing a new polygon.
                if (NewPolyline != null)
                {
                    // Flag to stop mouse drawing activity on Double click
                    stopDrawing = false;    
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
                        NewPolyline = Utils.initPolyLine();
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
    }
}
