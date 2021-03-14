
using System.Windows.Media;
using System.Windows.Shapes;

namespace MaskEditor
{
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
}
