using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Trigame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class TriPoint
    {
        public Point Position { get; set; }
        public Ellipse Visual { get; set; }
        public Ellipse Aura { get; set; }
        public Ellipse AuraCapture { get; set; }
        public TextBox Text;
        public short LeftToConnect { get; set; } = 5;

        public List<Line> ConnectedLines = new List<Line>();
        public List<TriPoint> ConnectedPoints = new List<TriPoint>();
    }
    public partial class MainWindow : Window
    {
        Canvas MyCanvas;
        TriPoint? DragStartPoint = null;
        Line? dragLine = null;
        bool DragPrevStarted = false;
        TriPoint? mouseOver = null;
        List<TriPoint> points = new List<TriPoint>
        {
            new TriPoint { Position = new Point(100, 100) },
            new TriPoint { Position = new Point(100, 100) },
            new TriPoint { Position = new Point(200, 150) },
            new TriPoint { Position = new Point(300, 200) },
            new TriPoint { Position = new Point(400, 250) },
            new TriPoint { Position = new Point(500, 300) }
        };
       
        public MainWindow()
        {
            InitializeComponent();
            MyCanvas = new Canvas
            {
                Width = 1024,
                Height = 1024,
                Background = Brushes.LightGray
            };
            this.Content = MyCanvas;
            MyCanvas.MouseMove += (s, e) => CheckAll(s, e: e);
            MyCanvas.MouseUp += (s,b) => CheckAll(s,b);
            DrawPoints(points);
        }

        public void CheckAll(object s, MouseButtonEventArgs b = null, MouseEventArgs e = null)
        {
            var currentPosition = b != null ? b.GetPosition(MyCanvas) : e.GetPosition(MyCanvas);
            MyCanvas.Children.Remove(dragLine);
            var mouseUp = b != null ? true : e.MouseDevice.LeftButton == MouseButtonState.Released;
            var overPoint = points.FirstOrDefault(x => x.AuraCapture.IsMouseOver);
            if (!DragPrevStarted && e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                if (overPoint != null)
                {
                    DragPrevStarted = true;
                    DragStartPoint = overPoint;
                    MyCanvas.Children.Remove(dragLine);
                }
                else
                {
                    DragPrevStarted = true;
                }
            }
            if (overPoint != null)
            {
                if (mouseOver != null && mouseOver != overPoint)
                {
                    mouseOver.Aura.RenderTransform = new ScaleTransform(1, 1, mouseOver.Aura.Width / 2, mouseOver.Aura.Height / 2);
                }
                else
                {
                    mouseOver = overPoint;
                    mouseOver.Aura.RenderTransform = new ScaleTransform(1.5, 1.5, mouseOver.Aura.Width / 2, mouseOver.Aura.Height / 2);
                }
            }
            else
            {
                if (mouseOver != null)
                {
                    mouseOver.Aura.RenderTransform = new ScaleTransform(1, 1, mouseOver.Aura.Width / 2, mouseOver.Aura.Height / 2);
                    mouseOver = null;
                }
            }
            if (!mouseUp && DragStartPoint != null)
            {
                dragLine = new Line
                {
                    X1 = DragStartPoint.Position.X + 1,
                    Y1 = DragStartPoint.Position.Y + 1,
                    X2 = currentPosition.X + 1,
                    Y2 = currentPosition.Y + 1,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };
                MyCanvas.Children.Add(dragLine);
            }
            if (mouseUp)
            {
                if (DragStartPoint != null && overPoint != null && overPoint != DragStartPoint && DragStartPoint.LeftToConnect > 0 && overPoint.LeftToConnect > 0 && !DragStartPoint.ConnectedPoints.Any(x => x == overPoint))
                {
                    var permanentLine = new Line
                    {
                        X1 = DragStartPoint.Position.X,
                        Y1 = DragStartPoint.Position.Y,
                        X2 = overPoint.Position.X,
                        Y2 = overPoint.Position.Y,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 2,
                        IsHitTestVisible = false
                    };
                    MyCanvas.Children.Add(permanentLine);
                    DragStartPoint.LeftToConnect--;
                    overPoint.LeftToConnect--;
                    DragStartPoint.ConnectedLines.Add(permanentLine);
                    overPoint.ConnectedLines.Add(permanentLine);
                    DragStartPoint.ConnectedPoints.Add(overPoint);
                    overPoint.ConnectedPoints.Add(DragStartPoint);
                    overPoint.Text.Text = overPoint.LeftToConnect.ToString();
                    DragStartPoint.Text.Text = DragStartPoint.LeftToConnect.ToString();
                }
                DragPrevStarted = false;
                DragStartPoint = null;
                MyCanvas.Children.Remove(dragLine);
            }

        }


        public void DrawPoints(List<TriPoint> points)
        {
            foreach (var point in points)
            {
                Ellipse ellipse = new Ellipse
                {
                    Stroke = Brushes.Black,
                    Width = 25,
                    Height = 25,
                    IsHitTestVisible = false,

                };
                Ellipse auraCaptureEllipse = new Ellipse
                {
                    Width = 75,
                    Height = 75,
                    Fill = Brushes.Transparent,
                    Cursor = Cursors.Hand
                };
                Ellipse auraEllipse = new Ellipse
                {                   
                    Width = 50,
                    Height = 50,
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection() { 2, 2 },
                    Cursor = Cursors.Hand,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(ellipse, point.Position.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, point.Position.Y - ellipse.Height / 2);
                Canvas.SetLeft(auraEllipse, point.Position.X - auraEllipse.Width / 2);
                Canvas.SetTop(auraEllipse, point.Position.Y - auraEllipse.Height / 2);                
                Canvas.SetLeft(auraCaptureEllipse, point.Position.X - auraCaptureEllipse.Width / 2);
                Canvas.SetTop(auraCaptureEllipse, point.Position.Y - auraCaptureEllipse.Height / 2);
                point.Aura = auraEllipse;
                point.AuraCapture = auraCaptureEllipse;
                point.Visual = ellipse;

                var text = new TextBox()
                {
                    Text = point.LeftToConnect.ToString(),
                    Width = 20,
                    Height = 20,
                    IsReadOnly = true,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(text, point.Position.X - text.Width / 2);
                Canvas.SetTop(text, point.Position.Y - text.Height / 2);
                point.Text = text;

                MyCanvas.Children.Add(auraEllipse);
                MyCanvas.Children.Add(ellipse);
                MyCanvas.Children.Add(auraCaptureEllipse);
                MyCanvas.Children.Add(text);
                

            }
        }
    }
}