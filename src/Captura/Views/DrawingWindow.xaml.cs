using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Captura.Views
{
    /// <summary>
    /// Interaction logic for DrawingWindow.xaml
    /// </summary>
    public partial class DrawingWindow : Window
    {
        /// <summary>
        /// possible state in this window
        /// </summary>
        enum DrawingState { None, Arrow, Text }
        Brush arrowColor = Brushes.Red;
        DrawingState drawingState;
        string currentEditingText;
        public DrawingWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //get settings
            drawingCanvas.Background = new SolidColorBrush(ToColor(Settings.Instance.DrawingBackgroundColor));
            this.arrowColor = new SolidColorBrush(ToColor(Settings.Instance.DrawingArrowColor));
            helpText.Visibility = Visibility.Collapsed;
        }

        private System.Windows.Media.Color ToColor(System.Drawing.Color color)
        {
            return new System.Windows.Media.Color() { A = color.A, R = color.R, G = color.G, B = color.B };
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var keyAsString = e.getKeyAsString();
            if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                //erase
                ReinitCanvas();
            }
            else if (e.Key == Key.T && Keyboard.Modifiers == ModifierKeys.Control)
            {
                StartEditingText();
            }
            else if (e.Key == Key.Add && Keyboard.Modifiers == ModifierKeys.Control)
            {
                IncreaseFontSize();
            }
            else if (e.Key == Key.Subtract && Keyboard.Modifiers == ModifierKeys.Control)
            {
                DecreaseFontSize();
            }
            else if (e.Key == Key.Enter)
            {
                ValidateEditingText();
            }
            else
            {
                if (e.Key == Key.F1)
                {
                    ShowHideHelp();
                } else if (e.Key == Key.Escape)
                {
                    if (!CancelDrawing() && !HideHelp())
                    {
                        this.Close();
                    }
                } else AddLetterToEditingText(e.Key, keyAsString);
            }
        }

        private void AddLetterToEditingText(Key key, string keyAsString)
        {
            if (drawingState == DrawingState.Text)
            {
                switch(key)
                {
                    case Key.Back:
                        if (this.currentEditingText.Length > 0)
                        {
                            this.currentEditingText = this.currentEditingText.Remove(this.currentEditingText.Length - 1);
                            UpdateEditingText();
                        }
                        break;
                    default:
                        if (!string.IsNullOrEmpty(keyAsString))
                        {
                            this.currentEditingText += keyAsString;
                            UpdateEditingText();
                        }
                        break;
                }
            }
        }

        private void ValidateEditingText()
        {
            if (this.drawingState == DrawingState.Text)
            {
                //copy text to drawingCanvas
                var tb = ((TextBlock)temporaryDrawingCanvas.Children[0]);
                tb.FontSize = this.FontSize;
                temporaryDrawingCanvas.Children.Clear();
                drawingCanvas.Children.Add(tb);
                this.CancelDrawing();
            }
        }

        private void DecreaseFontSize()
        {
            if (this.drawingState == DrawingState.Text && this.FontSize > 5)
            {
                this.FontSize -= 5;
                UpdateEditingText();
            }
        }

        private void IncreaseFontSize()
        {
            if (this.drawingState == DrawingState.Text)
            {
                this.FontSize += 5;
                UpdateEditingText();
            }
        }

        private void UpdateEditingText()
        {
            var tb = ((TextBlock)temporaryDrawingCanvas.Children[0]);
            tb.Text = currentEditingText?.Length > 0 ? currentEditingText : " ";
            var tSize = MeasureTextSize(tb.Text, tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch, tb.FontSize);

            ((Rectangle)temporaryDrawingCanvas.Children[1]).Height = tSize.Height;
            //move pseudo caret at the end of the text
            ((Rectangle)temporaryDrawingCanvas.Children[1]).RenderTransform = new TranslateTransform(tSize.Width, 0);
        }

        public Size MeasureTextSize(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            FormattedText ft = new FormattedText(text,
                                                 CultureInfo.CurrentCulture,
                                                 FlowDirection.LeftToRight,
                                                 new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                                 fontSize,
                                                 this.arrowColor);
            return new Size(ft.Width, ft.Height);
        }

        private void StartEditingText()
        {
            if (drawingState == DrawingState.None)
            {
                currentEditingText = string.Empty;
                var mousePos = Mouse.GetPosition(temporaryDrawingCanvas);
                drawingState = DrawingState.Text;
                var tb = new TextBlock();
                tb.Foreground = this.arrowColor;
                temporaryDrawingCanvas.Children.Add(tb);
                Canvas.SetLeft(tb, mousePos.X);
                Canvas.SetTop(tb, mousePos.Y);
                var tSize = MeasureTextSize("|", tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch, tb.FontSize);
                var r = new Rectangle() { Width = 2, Height = tSize.Height, Fill = this.arrowColor };
                Canvas.SetLeft(r, mousePos.X);
                Canvas.SetTop(r, mousePos.Y);
                temporaryDrawingCanvas.Children.Add(r);
            }
        }

        private bool CancelDrawing()
        {
            switch(drawingState)
            {
                case DrawingState.Arrow:
                    drawingCanvas.Children.RemoveRange(drawingCanvas.Children.Count - 2, 2);
                    break;
                case DrawingState.Text:
                    temporaryDrawingCanvas.Children.Clear();
                    break;
                default:
                    return false;
            }
            drawingState = DrawingState.None;

            return true;
        }

        private void ShowHideHelp()
        {
            if (helpText.Visibility == Visibility.Collapsed) ShowHelp();
            else HideHelp();
        }

        private bool HideHelp()
        {
            var result = helpText.Visibility != Visibility.Collapsed;
            helpText.Visibility = Visibility.Collapsed;
            return result;
        }

        private bool ShowHelp()
        {
            var result = helpText.Visibility == Visibility.Collapsed;
            helpText.Visibility = Visibility.Visible;
            return result;
        }

        private void ReinitCanvas()
        {
            //erase all and reinit drawing state
            drawingState = DrawingState.None;
            drawingCanvas.Children.Clear();
        }

        private static void DrawArrowHead(Canvas canvas, Line linePath,  Brush brush)
        {
            var innerLeft = linePath.X2;
            var innerTop = linePath.Y2;
            var outerLeft = linePath.X1;
            var outerTop = linePath.Y1;

            Polygon arrowHead = new Polygon();
            arrowHead.Points = new PointCollection();
            arrowHead.Points.Add(new Point(/*innerLeft*/0, /*innerTop*/0));
            arrowHead.Points.Add(new Point(/*innerLeft*/0 + 10, /*innerTop*/0 + 5));
            arrowHead.Points.Add(new Point(/*innerLeft*/0 + 10, /*innerTop*/0 - 5));
            arrowHead.Points.Add(new Point(/*innerLeft*/0, /*innerTop*/0));
            arrowHead.Stroke = brush;
            arrowHead.Fill = brush;

            arrowHead.RenderTransform = GetArrowHeadTransform(innerLeft, innerTop, outerLeft, outerTop);
            canvas.Children.Add(arrowHead);
        }

        private static Transform GetArrowHeadTransform(double innerLeft, double innerTop, double outerLeft, double outerTop)
        {
            // The differences between the intersection points on
            // the inner and outer shapes gives us the base and
            // perpendicular of the right-angled triangle
            double baseSize = innerLeft - outerLeft;
            double perpSize = innerTop - outerTop;
            // Calculate the angle in degrees using ATan
            double angle = Math.Atan(perpSize / baseSize) * 180 / Math.PI;


            // Rotate another 180 degrees for lines in the 3rd & 4th quadrants
            if (baseSize >= 0) angle += 180;

            TransformGroup arrowHeadTransform = new TransformGroup();
            arrowHeadTransform.Children.Add(new TranslateTransform(innerLeft, innerTop));
            arrowHeadTransform.Children.Add(new RotateTransform(angle, innerLeft, innerTop));
            // Apply the rotation to the arrow head
            return arrowHeadTransform;
        }

        private void drawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (drawingState == DrawingState.None)
            {
                //enter in arrow drawing
                var point = e.GetPosition(drawingCanvas);
                Line l = new Line() { Stroke = this.arrowColor };
                l.StrokeThickness = 1;
                l.X1 = point.X;
                l.Y1 = point.Y;
                l.X2 = point.X;
                l.Y2 = point.Y;
                drawingCanvas.Children.Add(l);
                DrawArrowHead(drawingCanvas, l, l.Stroke);
                drawingState = DrawingState.Arrow;
            }
        }

        private void drawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (drawingState == DrawingState.Arrow)
            {
                //end of drawing state
                drawingState = DrawingState.None;
            }
        }

        private void drawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && drawingState == DrawingState.Arrow)
            {
                var arrowHead = (Polygon)drawingCanvas.Children[drawingCanvas.Children.Count - 1];
                var l = (Line)drawingCanvas.Children[drawingCanvas.Children.Count - 2];
                var point = e.GetPosition(drawingCanvas);
                l.X2 = point.X;
                l.Y2 = point.Y;
                //then change the rotation of arrowhead
                arrowHead.RenderTransform = GetArrowHeadTransform(l.X2, l.Y2, l.X1, l.Y1);
            }
        }
    }
}
