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
        List<Poly> unionPolyList = new List<Poly>();
        List<Poly> excludePolyList = new List<Poly>();
        CombinedGeometry union = new CombinedGeometry();
        private PointCollection pc = new PointCollection();
        bool stopDrawing = true;
        private string currentImage = "";

        public MainWindow()
        {
            InitializeComponent();
        }
        private void btnBrowse(object sender, RoutedEventArgs e)
        {
            if(canDraw.Children.Count >= 1) {
                if (MessageBox.Show("編集中のマスクは初期化されます。よろしいですか？",
                    "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // Close the window  
                }
                else return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.png) | *.jpg; *.png; | 全て(*.*) | *.*";
            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;
                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                {
                    currentImage = openFileDialog.FileName;
                    txtEditor.Text = currentImage;
                    imageBrushRare.ImageSource = new BitmapImage(new Uri(currentImage, UriKind.RelativeOrAbsolute));
                    var ratio = Math.Min(canDraw.RenderSize.Width / imageBrushRare.ImageSource.Width, 
                        canDraw.RenderSize.Height / imageBrushRare.ImageSource.Height);
                    var imageBrushRareWidth = imageBrushRare.ImageSource.Width * ratio;
                    var imageBrushRareHeight = imageBrushRare.ImageSource.Height * ratio;
                    imageBrushRare.ImageSource = null;

                    canDraw.Width = imageBrushRareWidth;
                    canDraw.Height = imageBrushRareHeight;
                    imageBrush.ImageSource = new BitmapImage(new Uri(currentImage, UriKind.RelativeOrAbsolute));
                    // Enable Drawing if Image is selected
                    stopDrawing = false;

                    // Re-Initialize Union and Exclude Geometry
                    union = new CombinedGeometry();
                    unionPolyList.Clear();
                    excludePolyList.Clear();
                    
                    // Make things Ready for new Polygon Drawing
                    NewPolyline = null;
                    pc = new PointCollection();
                    canDraw.Children.Clear();
                    polyList.Clear();           // New polylist for every new Figure
                    canDraw.Children.Clear();
                }
            }                
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            if (rbAdd.IsChecked == true)
            {
                Console.WriteLine(rbAdd.Content.ToString() +" is "+ rbAdd.IsChecked);
            }
            else if(rbDel.IsChecked == true)
            {
                Console.WriteLine(rbDel.Content.ToString() + " is " + rbDel.IsChecked);
            }                
        }
        
        // Save button functionality
        // This will save Polygon(only) in the PNG image with name logo.png
        private void btnSave(object sender, RoutedEventArgs e)
        {
            // Initialize canvas To save mask image
            Canvas canvas = new Canvas();
            canvas.Height = canDraw.ActualHeight;
            canvas.Width = canDraw.ActualWidth;
            canvas.Background = new SolidColorBrush(Colors.White);

            // Render results(union final Geometry on the canvas)
            canvas.Children.Add(Utils.GetPath(union, Colors.Black));
            canvas.UpdateLayout();
            

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Mask Images (*.png) | *.png";
            try {
                if (saveDialog.ShowDialog() == true)
                {
                    var extension = System.IO.Path.GetExtension(saveDialog.FileName);
                    if (extension.ToLower() == ".png")
                    {
                        //Save temporary mask image
                        string mask_file = @"mask_image.png";
                        Utils.SaveCanvas(canvas, @mask_file);

                        // Resize mask image to Base image Height and Width
                        System.Drawing.Image mask = System.Drawing.Image.FromFile(@mask_file);

                        Bitmap bmp = Utils.ResizeImage(mask, (int)imageBrush.ImageSource.Width, (int)imageBrush.ImageSource.Height);
                        Console.WriteLine("Saving resized masked image");
                        bmp.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        // dispose 
                        mask.Dispose();
                        bmp.Dispose();

                        // Re-Initialize Union and Exclude Geometry
                        union = new CombinedGeometry();
                        unionPolyList.Clear();
                        excludePolyList.Clear();

                        canvas.Children.Clear();

                        // Make things Ready for new Polygon Drawing
                        NewPolyline = null;
                        pc = new PointCollection();
                        canDraw.Children.Clear();
                        polyList.Clear();           // New polylist for every new Figure
                        canDraw.Children.Clear();
                    }
                    else
                    {
                        MessageBox.Show("画像フォーマットはpngを指定してください。");
                    }
                }
            }
            catch (Exception exception) {
                Console.WriteLine("Resized masked image is not saved");
                Console.WriteLine(exception.GetBaseException());
            }
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
                    // Remove only last Element(Part of PolyLine) from Canvas 
                    // clear Newpolyline to start fresh
                    else
                    {
                        NewPolyline = null;
                        canDraw.Children.RemoveAt(canDraw.Children.Count-1);
                        pc.Clear();
                    }
                }
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
                    temp.Fill = this.FindResource("HatchBrushnew") as VisualBrush;
                    canDraw.Children.Add(temp);
                }
                // On Mouse Double Click
                if (e.ClickCount == 2)
                {
                    if (polyList.Count >= 1)
                    { 
                        canDraw.Children.RemoveAt(canDraw.Children.Count - 1);
                        polyList.RemoveAt(polyList.Count-1);
                        //stopDrawing = true;

                        // Get Union Geometry *INCLUDING* all the polygons Drawn if Add Enabled
                        if (rbAdd.IsChecked == true)
                        {
                            unionPolyList = polyList;
                            CombinedGeometry tempUnion = Utils.GetUnionGeometry(unionPolyList, canDraw);
                            union = Utils.CombinedGeometryUnion(union, tempUnion);
                            Console.WriteLine("In Save Button method " + rbAdd.Content.ToString() + " is " + rbAdd.IsChecked);
                            excludePolyList.Clear();
                        }
                        // Get Union Geometry *EXCLUDING* all the polygons Drawn if Delete Enabled
                        else if (rbDel.IsChecked == true)
                        {
                            excludePolyList = polyList;
                            CombinedGeometry excludes = Utils.GetUnionGeometry(excludePolyList, canDraw);
                            union = Utils.CombinedGeometryExclude(union, excludes);
                            Console.WriteLine("In Save Button method " + rbDel.Content.ToString() + " is " + rbDel.IsChecked);
                            unionPolyList.Clear();
                        }
                        // Make things Ready for new Polygon Drawing
                        NewPolyline = null;
                        pc = new PointCollection();
                        canDraw.Children.Clear();
                        polyList.Clear();           // New polylist for every new Figure
                        canDraw.Children.Add(Utils.GetPath(union, Colors.Blue, .4d));
                    }
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
    }
}
