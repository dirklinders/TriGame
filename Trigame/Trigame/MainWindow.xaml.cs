using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Trigame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class TriPoint
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public Ellipse Visual { get; set; }
        public Ellipse Aura { get; set; }
        public Ellipse AuraCapture { get; set; }

        public TextBox Text;
        public byte LeftToConnect { get; set; }
        public byte OriginalLeftToConnect { get; set; } = 5;

        public bool Completed = false;

        public List<Tuple<TriPoint, Line>> ConnectedPoints = new List<Tuple<TriPoint, Line>>();
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

            new TriPoint { Id = 1,Position = new Point(100, 100), OriginalLeftToConnect = 2 },
            new TriPoint { Id = 2,Position = new Point(200, 400) },
            new TriPoint { Id = 3,Position = new Point(300, 200) },
            new TriPoint { Id = 4,Position = new Point(400, 250) },
            new TriPoint { Id = 5,Position = new Point(500, 300) },
            new TriPoint { Id = 6,Position = new Point(600, 350) },
            new TriPoint { Id = 7,Position = new Point(700, 400) },
        };

        List<Tuple<int, int, int, Color>> KnownTrangles = new List<Tuple<int, int, int, Color>>
        {
            new Tuple<int, int, int, Color>(1,2,3, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(2,3,4, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(2,4,5, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(2,5,6, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(2,6,7, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(3,4,5, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(3,5,6, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(3,6,7, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(4,5,6, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(4,6,7, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(5,6,7, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(1,3,4, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(1,4,5, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(1,5,6, Color.FromRgb(0,100,0)),
            new Tuple<int, int, int, Color>(1,6,7, Color.FromRgb(0,100,0)),
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
            MyCanvas.MouseUp += (s, b) => CheckAll(s, b);
            MyCanvas.MouseDown += (s, b) => CheckAll(s, b);
            DrawPoints(points);
        }

        public void CheckAll(object s, MouseButtonEventArgs b = null, MouseEventArgs e = null)
        {
            var currentPosition = b != null ? b.GetPosition(MyCanvas) : e.GetPosition(MyCanvas);

            MyCanvas.Children.Remove(dragLine);
            var mouseUp = b != null ? b.ButtonState == MouseButtonState.Released : e.MouseDevice.LeftButton == MouseButtonState.Released;
            var overPoint = points.FirstOrDefault(x => x.AuraCapture.IsMouseOver);
            #region DoubleClick undo
            // Handle double click to undo all connections from a point
            if (b != null && b.ButtonState == MouseButtonState.Pressed && b.ClickCount >= 2 && overPoint != null && !overPoint.Completed)
            {
                foreach (var point in overPoint.ConnectedPoints)
                {

                    point.Item1.ConnectedPoints.RemoveAll(x => x.Item1 == overPoint);
                    point.Item1.LeftToConnect++;
                    point.Item1.Text.Text = point.Item1.LeftToConnect.ToString();
                    point.Item1.Aura.Visibility = Visibility.Visible;
                    point.Item1.AuraCapture.Width = 75;
                    point.Item1.AuraCapture.Height = 75;
                    Canvas.SetLeft(point.Item1.AuraCapture, point.Item1.Position.X - point.Item1.AuraCapture.Width / 2);
                    Canvas.SetTop(point.Item1.AuraCapture, point.Item1.Position.Y - point.Item1.AuraCapture.Height / 2);
                    MyCanvas.Children.Remove(point.Item2);

                }
                overPoint.Aura.Visibility = Visibility.Visible;
                overPoint.ConnectedPoints.Clear();
                overPoint.LeftToConnect = overPoint.OriginalLeftToConnect;
                overPoint.Text.Text = overPoint.LeftToConnect.ToString();
                overPoint.AuraCapture.Width = 75;
                overPoint.AuraCapture.Height = 75;
                Canvas.SetLeft(overPoint.AuraCapture, overPoint.Position.X - overPoint.AuraCapture.Width / 2);
                Canvas.SetTop(overPoint.AuraCapture, overPoint.Position.Y - overPoint.AuraCapture.Height / 2);

            }
            #endregion

            #region Start Drag
            if (!DragPrevStarted && !mouseUp)
            {
                if (overPoint != null && overPoint.LeftToConnect > 0)
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
            #endregion

            #region Aura Scaling on hover
            if (overPoint != null)
            {
                if (mouseOver != null && mouseOver != overPoint)
                {
                    mouseOver.Aura.RenderTransform = new ScaleTransform(1, 1, mouseOver.Aura.Width / 2, mouseOver.Aura.Height / 2);                    
                }
                if (overPoint.LeftToConnect > 0)
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
            #endregion

            #region DragLine on dragging
            if (!mouseUp && DragStartPoint != null)
            {
                dragLine = new Line
                {
                    X1 = DragStartPoint.Position.X,
                    Y1 = DragStartPoint.Position.Y,
                    X2 = currentPosition.X,
                    Y2 = currentPosition.Y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    IsHitTestVisible = false
                };
                MyCanvas.Children.Add(dragLine);
            }
            #endregion

            #region Mouse Up
            if (mouseUp)
            {
                #region complete drag on another point
                if (DragStartPoint != null && overPoint != null && overPoint != DragStartPoint && DragStartPoint.LeftToConnect > 0 && overPoint.LeftToConnect > 0 && !DragStartPoint.ConnectedPoints.Any(x => x.Item1 == overPoint))
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
                    if (DragStartPoint.LeftToConnect == 0)
                    {
                        DragStartPoint.Aura.Visibility = Visibility.Hidden;
                        DragStartPoint.AuraCapture.Width = 25;
                        DragStartPoint.AuraCapture.Height = 25;
                        Canvas.SetLeft(DragStartPoint.AuraCapture, DragStartPoint.Position.X - DragStartPoint.AuraCapture.Width / 2);
                        Canvas.SetTop(DragStartPoint.AuraCapture, DragStartPoint.Position.Y - DragStartPoint.AuraCapture.Height / 2);
                    }
                    if (overPoint.LeftToConnect == 0)
                    {
                        overPoint.Aura.Visibility = Visibility.Hidden;
                        overPoint.AuraCapture.Width = 25;
                        overPoint.AuraCapture.Height = 25;
                        Canvas.SetLeft(overPoint.AuraCapture, overPoint.Position.X - overPoint.AuraCapture.Width / 2);
                        Canvas.SetTop(overPoint.AuraCapture, overPoint.Position.Y - overPoint.AuraCapture.Height / 2);
                    }
                    var dragTuple = new Tuple<TriPoint, Line>(DragStartPoint, permanentLine);
                    var overTuple = new Tuple<TriPoint, Line>(overPoint, permanentLine);

                    DragStartPoint.ConnectedPoints.Add(overTuple);
                    overPoint.ConnectedPoints.Add(dragTuple);
                    overPoint.Text.Text = overPoint.LeftToConnect.ToString();
                    DragStartPoint.Text.Text = DragStartPoint.LeftToConnect.ToString();

                    var possibleMatches = KnownTrangles.Where(x => (x.Item1 == DragStartPoint.Id || x.Item2 == DragStartPoint.Id || x.Item3 == DragStartPoint.Id) && (x.Item1 == overPoint.Id || x.Item2 == overPoint.Id || x.Item3 == overPoint.Id));
                    foreach (var possibleMatch in possibleMatches)
                    {
                        var thirdPointId = possibleMatch.Item1;
                        if (thirdPointId == DragStartPoint.Id || thirdPointId == overPoint.Id)
                            thirdPointId = possibleMatch.Item2;
                        if (thirdPointId == DragStartPoint.Id || thirdPointId == overPoint.Id)
                            thirdPointId = possibleMatch.Item3;
                        var thirdPoint = points.FirstOrDefault(x => x.Id == thirdPointId);
                        if (thirdPoint != null && thirdPoint.ConnectedPoints.Any(x => x.Item1 == DragStartPoint) && thirdPoint.ConnectedPoints.Any(x => x.Item1 == overPoint))
                        {
                            // We have a triangle!
                            if (DragStartPoint.OriginalLeftToConnect == 2)
                            {
                                DragStartPoint.Completed = true;
                                DragStartPoint.Aura.Visibility = Visibility.Hidden;
                                DragStartPoint.AuraCapture.Visibility = Visibility.Hidden;
                                DragStartPoint.AuraCapture.IsHitTestVisible = false;
                                DragStartPoint.Text.Visibility = Visibility.Hidden;
                                DragStartPoint.Visual.Visibility = Visibility.Hidden;
                            }
                            if (overPoint.OriginalLeftToConnect == 2)
                            {
                                overPoint.Completed = true;
                                overPoint.Aura.Visibility = Visibility.Hidden;
                                overPoint.AuraCapture.Visibility = Visibility.Hidden;
                                overPoint.AuraCapture.IsHitTestVisible = false;
                                overPoint.Text.Visibility = Visibility.Hidden;
                                overPoint.Visual.Visibility = Visibility.Hidden;
                            }
                            if (thirdPoint.OriginalLeftToConnect == 2)
                            {
                                thirdPoint.Completed = true;
                                thirdPoint.Aura.Visibility = Visibility.Hidden;
                                thirdPoint.AuraCapture.Visibility = Visibility.Hidden;
                                thirdPoint.AuraCapture.IsHitTestVisible = false;
                                thirdPoint.Text.Visibility = Visibility.Hidden;
                                thirdPoint.Visual.Visibility = Visibility.Hidden;
                            }

                            var linesToRemove = new List<Line>();
                            foreach (var line in DragStartPoint.ConnectedPoints)
                            {
                                if (line.Item1 == overPoint || line.Item1 == thirdPoint)
                                {
                                    linesToRemove.Add(line.Item2);
                                }
                            }
                            foreach (var line in overPoint.ConnectedPoints)
                            {
                                if (line.Item1 == DragStartPoint || line.Item1 == thirdPoint)
                                {
                                    linesToRemove.Add(line.Item2);
                                }
                            }
                            foreach (var line in thirdPoint.ConnectedPoints)
                            {
                                if (line.Item1 == DragStartPoint || line.Item1 == overPoint)
                                {
                                    linesToRemove.Add(line.Item2);
                                }
                            }
                            DragStartPoint.OriginalLeftToConnect--;
                            DragStartPoint.OriginalLeftToConnect--;
                            overPoint.OriginalLeftToConnect--;
                            overPoint.OriginalLeftToConnect--;
                            thirdPoint.OriginalLeftToConnect--;
                            thirdPoint.OriginalLeftToConnect--;
                            foreach (var line in linesToRemove.Distinct())
                            {
                                MyCanvas.Children.Remove(line);
                            }

                            // draw triangle
                            var triangle = new Polygon
                            {
                                Points = new PointCollection() { DragStartPoint.Position, overPoint.Position, thirdPoint.Position },
                                Stroke = Brushes.Green,
                                Fill = Brushes.LightGreen,
                                StrokeThickness = 2,
                                Opacity = 0.5,
                                IsHitTestVisible = false
                            };
                            MyCanvas.Children.Add(triangle);
                        }
                    }

                }
                #endregion
                DragPrevStarted = false;
                DragStartPoint = null;
                MyCanvas.Children.Remove(dragLine);
            }

            #endregion

        }


        public void DrawPoints(List<TriPoint> points)
        {
            foreach (var point in points)
            {
                point.LeftToConnect = point.OriginalLeftToConnect;
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