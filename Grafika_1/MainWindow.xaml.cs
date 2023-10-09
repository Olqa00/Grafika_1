using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
//using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Grafika_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //rysowanie 
        private Shape SelectedShape { get; set; }
        private Point startPoint;
        private enum SelectedShapeType
        {
            Line,
            Rectangle,
            Circle
        }
        private SelectedShapeType currentSelectedShape;
        //przesuwanie
        private bool isDragging = false;
        public MainWindow()
        {
            InitializeComponent();

            MyCanvas.MouseDown += MyCanvas_MouseRightButtonDown;
            MyCanvas.MouseMove += MyCanvas_MouseMove;
            MyCanvas.MouseUp += MyCanvas_MouseRightButtonUp;

        }
        //funkcja do "odsłaniania TextBoxów"
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (LineRadioButton.IsChecked == true)
            {
                currentSelectedShape = SelectedShapeType.Line;

                LengthLabel.Visibility = Visibility.Visible;
                LengthTextBox.Visibility = Visibility.Visible;
                VectorLabel.Visibility = Visibility.Visible;
                VectorTextBox.Visibility = Visibility.Visible;
                HeightLabel.Visibility = Visibility.Collapsed;
                HeightTextBox.Visibility = Visibility.Collapsed;
                RadiusLabel.Visibility = Visibility.Collapsed;
                RadiusTextBox.Visibility = Visibility.Collapsed;

            }
            else if (RectangleRadioButton.IsChecked == true)
            {
                currentSelectedShape = SelectedShapeType.Rectangle;

                LengthLabel.Visibility = Visibility.Visible;
                LengthTextBox.Visibility = Visibility.Visible;
                VectorLabel.Visibility = Visibility.Collapsed;
                VectorTextBox.Visibility = Visibility.Collapsed;
                HeightLabel.Visibility = Visibility.Visible;
                HeightTextBox.Visibility = Visibility.Visible;
                RadiusLabel.Visibility = Visibility.Collapsed;
                RadiusTextBox.Visibility = Visibility.Collapsed;

            }
            else if (CircleRadioButton.IsChecked == true)
            {
                currentSelectedShape = SelectedShapeType.Circle;

                LengthLabel.Visibility = Visibility.Collapsed;
                LengthTextBox.Visibility = Visibility.Collapsed;
                VectorLabel.Visibility = Visibility.Collapsed;
                VectorTextBox.Visibility = Visibility.Collapsed;
                HeightLabel.Visibility = Visibility.Collapsed;
                HeightTextBox.Visibility = Visibility.Collapsed;
                RadiusLabel.Visibility = Visibility.Visible;
                RadiusTextBox.Visibility = Visibility.Visible;

            }
            else
            {
                SelectedShape = null;
                LengthLabel.Visibility = Visibility.Collapsed;
                LengthTextBox.Visibility = Visibility.Collapsed;
                VectorLabel.Visibility = Visibility.Collapsed;
                VectorTextBox.Visibility = Visibility.Collapsed;
                HeightLabel.Visibility = Visibility.Collapsed;
                HeightTextBox.Visibility = Visibility.Collapsed;
                RadiusLabel.Visibility = Visibility.Collapsed;
                RadiusTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void DrawLine(double length, double x, double y, double vector)
        {
            //Walidacja >0
            if (length <= 0 || x < 0 || y < 0)
            {
                MessageBox.Show("Please enter valid positive values for Length, X, Y, and Vector.");
                return;
            }
            //Walidacja <MaxCanvas
            double canvasWidth = MyCanvas.ActualWidth;
            double canvasHeight = MyCanvas.ActualHeight;

            // Oblicz współrzędne końcowe linii na podstawie długości, kąta i początkowych współrzędnych
            double endX = x + length * Math.Cos(vector);
            double endY = y + length * Math.Sin(vector);

            if (endX > canvasWidth || endY > canvasHeight || x > canvasWidth || y > canvasHeight || x < 0 || y < 0 || endX < 0 || endY < 0)
            {
                MessageBox.Show("The line goes beyond the Canvas dimensions. Please enter valid values.");
                return; 
            }
            Line line = new Line
            {
                X1 = x, //x początkowe
                Y1 = y, //y początkowe
                X2 = endX,
                Y2 = endY,
                Stroke = Brushes.Black, //kolor
                StrokeThickness = 2 //grubość
            };
            line.MouseDown += (sender, e) => SetShapeForEdit(line, length, x, y, vector);
            // Dodaj linię do Canvasu
            MyCanvas.Children.Add(line);
        }
        private void DrawRectangle(double length, double height, double x, double y)
        {
            if (length < 0 || height < 0 || x < 0 || y < 0)
            {
                MessageBox.Show("Please enter valid positive values for Length, Height, X, and Y.");
                return;
            }

            double endX = x + length;
            double endY = y + height;

            double canvasWidth = MyCanvas.ActualWidth;
            double canvasHeight = MyCanvas.ActualHeight;

            if (endX > canvasWidth || endY > canvasHeight || x > canvasWidth || y > canvasHeight)
            {
                MessageBox.Show("The rectangle goes beyond the Canvas dimensions. Please enter valid values.");
                return;
            }

            Rectangle rectangle = new Rectangle
            {
                Width = length,
                Height = height,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            //ustawienie pozycji dla prostokąta
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            rectangle.MouseDown += (sender, e) => SetShapeForEdit(rectangle, length, height, x, y);
            MyCanvas.Children.Add(rectangle);
        }
        private void DrawCircle(double radius, double x, double y)
        {
            if (radius <= 0 || x < 0 || y < 0)
            {
                MessageBox.Show("Please enter valid positive values for Radius, X, and Y.");
                return;
            }

            double endX = x + radius; // Współrzędna X prawego brzegu koła
            double endY = y + radius; // Współrzędna Y dolnego brzegu koła

            double canvasWidth = MyCanvas.ActualWidth;
            double canvasHeight = MyCanvas.ActualHeight;

            if (endX > canvasWidth || endY > canvasHeight || x > canvasWidth || y > canvasHeight)
            {
                MessageBox.Show("The circle goes beyond the Canvas dimensions. Please enter valid values.");
                return;
            }

            Ellipse ellipse = new Ellipse
            {
                Width = radius * 2, //średnica
                Height = radius * 2,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            // Ustaw pozycję kółka
            Canvas.SetLeft(ellipse, x - radius); //środek- promień, bo lewa
            Canvas.SetTop(ellipse, y - radius); // środek- promień, bo do góry

            ellipse.MouseDown += (sender, e) => SetShapeForEdit(ellipse, radius, x, y);
            MyCanvas.Children.Add(ellipse);
        }
        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            if (LineRadioButton.IsChecked == true)
            {
                if (double.TryParse(LengthTextBox.Text, out double length) &&
                    double.TryParse(XTextBox.Text, out double x) &&
                    double.TryParse(YTextBox.Text, out double y) &&
                    double.TryParse(VectorTextBox.Text, out double vector))
                {
                    DrawLine(length, x, y, vector);
                }
                else
                {
                    MessageBox.Show("Please enter valid numeric values for Length, X, Y, and Vector.");
                }
            }
            else if (RectangleRadioButton.IsChecked == true)
            {
                if (double.TryParse(LengthTextBox.Text, out double length) &&
                double.TryParse(HeightTextBox.Text, out double height) &&
                double.TryParse(XTextBox.Text, out double x) &&
                double.TryParse(YTextBox.Text, out double y))
                {
                    DrawRectangle(length, height, x, y);
                }
                else
                {
                    MessageBox.Show("Please enter valid numeric values for Length, Width, X, and Y.");
                }
            }
            else if (CircleRadioButton.IsChecked == true)
            {
                if (double.TryParse(RadiusTextBox.Text, out double radius) &&
                           double.TryParse(XTextBox.Text, out double x) &&
                           double.TryParse(YTextBox.Text, out double y))
                {
                    DrawCircle(radius, x, y);
                }
                else
                {
                    MessageBox.Show("Please enter valid numeric values for Radius, X, and Y.");
                }
            }
            else
            {
                MessageBox.Show("Select shape before drawing.");
            }
        }
        //Ustawienie Shape jako SelectedShape oraz zmiana TextBoxów
        private void SetShapeForEdit(Shape shape, params double[] values)
        {
            if (shape == null || values == null || values.Length == 0)
                return;
            SelectedShape = shape;
            if (shape is Line)
            {
                LineRadioButton.IsChecked = true;
                LengthTextBox.Text = values[0].ToString();
                XTextBox.Text = values[1].ToString();
                YTextBox.Text = values[2].ToString();
                VectorTextBox.Text = values[3].ToString();
            }
            else if (shape is Rectangle)
            {
                RectangleRadioButton.IsChecked = true;
                LengthTextBox.Text = values[0].ToString();
                HeightTextBox.Text = values[1].ToString();
                XTextBox.Text = values[2].ToString();
                YTextBox.Text = values[3].ToString();
            }
            else if (shape is Ellipse)
            {
                CircleRadioButton.IsChecked = true;
                RadiusTextBox.Text = values[0].ToString();
                XTextBox.Text = values[1].ToString();
                YTextBox.Text = values[2].ToString();
            }
        }
        private void RemoveSelectedShape()
        {
            if (SelectedShape != null)
            {
                MyCanvas.Children.Remove(SelectedShape);
                SelectedShape = null;
            }
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyCanvas.Children.Count == 0)
            {
                MessageBox.Show("No shape to edit. Please draw a shape first.");
                return;
            }

            if (SelectedShape == null)
            {
                MessageBox.Show("Select a shape to edit.");
                return;
            }

            if (SelectedShape is Line)
            {
                DrawLine(double.Parse(LengthTextBox.Text), double.Parse(XTextBox.Text),
                         double.Parse(YTextBox.Text), double.Parse(VectorTextBox.Text));
            }
            else if (SelectedShape is Rectangle)
            {
                DrawRectangle(double.Parse(LengthTextBox.Text), double.Parse(HeightTextBox.Text),
                              double.Parse(XTextBox.Text), double.Parse(YTextBox.Text));
            }
            else if (SelectedShape is Ellipse)
            {
                DrawCircle(double.Parse(RadiusTextBox.Text), double.Parse(XTextBox.Text),
                           double.Parse(YTextBox.Text));
            }
            RemoveSelectedShape();
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MyCanvas.Children.Clear();
            LengthTextBox.Clear();
            HeightTextBox.Clear();
            RadiusTextBox.Clear();
            XTextBox.Clear();
            YTextBox.Clear();
            VectorTextBox.Clear();
        }
        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //rysowanie
            if(e.ChangedButton == MouseButton.Right)
            {
                if (currentSelectedShape == SelectedShapeType.Line)
                {
                    startPoint = e.GetPosition(MyCanvas);
                    SelectedShape = new Line();
                    MyCanvas.MouseMove += MyCanvas_MouseMove;
                    MyCanvas.MouseUp += MyCanvas_MouseRightButtonUp;

                }
                else if (currentSelectedShape == SelectedShapeType.Rectangle)
                {
                    startPoint = e.GetPosition(MyCanvas);
                    SelectedShape = new Rectangle();
                    MyCanvas.MouseMove += MyCanvas_MouseMove;
                    MyCanvas.MouseUp += MyCanvas_MouseRightButtonUp;
                }
                else if (currentSelectedShape == SelectedShapeType.Circle)
                {
                    startPoint = e.GetPosition(MyCanvas);
                    SelectedShape = new Ellipse();
                    MyCanvas.MouseMove += MyCanvas_MouseMove;
                    MyCanvas.MouseUp += MyCanvas_MouseRightButtonUp;
                }
                else
                {
                    MessageBox.Show("Select a shape (line, rectangle, or circle) before drawing.");
                }
            }
            //zmiana pozycji
            else if (e.ChangedButton == MouseButton.Left && SelectedShape != null)
            {
                isDragging = true;
                startPoint = e.GetPosition(MyCanvas);
                MyCanvas.MouseMove += MyCanvas_MouseMove;
                MyCanvas.MouseUp += MyCanvas_MouseRightButtonUp;
            }
            //zmiana kształtu
            else if (e.ChangedButton == MouseButton.Middle && SelectedShape != null)
            {
                isDragging = true;
                startPoint = e.GetPosition(MyCanvas);
                MyCanvas.MouseMove += MyCanvas_MouseMove;
                MyCanvas.MouseUp += MyCanvas_MouseRightButtonUp;
            }

        }
        //Aktualizacja TextBoxów
        private void UpdateTextBoxesForShape()
        {
            if (SelectedShape != null)
            {
                if (SelectedShape is Line line)
                {
                    LengthTextBox.Text = Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2)).ToString();
                    // Oblicz kąt w radianach
                    double angleRad = Math.Atan2(line.Y2 - line.Y1, line.X2 - line.X1);

                    // Przekonwertuj kąt na stopnie
                    double angleDegrees = angleRad * (180 / Math.PI);

                    VectorTextBox.Text = angleDegrees.ToString();
                    XTextBox.Text = line.X1.ToString();
                    YTextBox.Text = line.Y1.ToString();
                }
                else if (SelectedShape is Rectangle rectangle)
                {
                    LengthTextBox.Text = rectangle.Width.ToString();
                    HeightTextBox.Text = rectangle.Height.ToString();
                    XTextBox.Text = Canvas.GetLeft(rectangle).ToString();
                    YTextBox.Text = Canvas.GetTop(rectangle).ToString();
                }
                else if (SelectedShape is Ellipse ellipse)
                {
                    RadiusTextBox.Text = (ellipse.Width / 2).ToString();
                    XTextBox.Text = (Canvas.GetLeft(ellipse) + ellipse.Width / 2).ToString();
                    YTextBox.Text = (Canvas.GetTop(ellipse) + ellipse.Height / 2).ToString();
                }
            }
            
        }
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            //Zmiana pozycji, przesunięcie
            if (isDragging && e.LeftButton == MouseButtonState.Pressed && SelectedShape != null)
            {
                Point currentPoint = e.GetPosition(MyCanvas);

                // Oblicz różnicę między aktualną pozycją a początkową pozycją (ruch myszy)
                double deltaX = currentPoint.X - startPoint.X;
                double deltaY = currentPoint.Y - startPoint.Y;

                // Usuń obecny kształt z Canvas
                MyCanvas.Children.Remove(SelectedShape);

                if (SelectedShape is Line)
                {
                    // Przesuń linię
                    Line selectedLine = SelectedShape as Line;
                    selectedLine.X1 += deltaX;
                    selectedLine.X2 += deltaX;
                    selectedLine.Y1 += deltaY;
                    selectedLine.Y2 += deltaY;
                }
                else
                {
                    // Przesuń inny kształt
                    Canvas.SetLeft(SelectedShape, Canvas.GetLeft(SelectedShape) + deltaX);
                    Canvas.SetTop(SelectedShape, Canvas.GetTop(SelectedShape) + deltaY);
                }

                // Dodaj zaktualizowany kształt z powrotem do Canvasa
                MyCanvas.Children.Add(SelectedShape);
                
                startPoint = currentPoint; // Aktualizuj punkt początkowy
            }
            //Zmiana kształtu
            else if (isDragging && e.MiddleButton == MouseButtonState.Pressed && SelectedShape != null)
            {
                if (SelectedShape is Rectangle rectangle)
                {
                    double deltaX = e.GetPosition(MyCanvas).X - startPoint.X;
                    double deltaY = e.GetPosition(MyCanvas).Y - startPoint.Y;

                    // Aktualizuj szerokość i wysokość, uwzględniając odbicie
                    rectangle.Width = Math.Abs(rectangle.Width + deltaX);
                    rectangle.Height = Math.Abs(rectangle.Height + deltaY);

                    startPoint = e.GetPosition(MyCanvas);  // Zaktualizuj punkt początkowy
                }
                else if (SelectedShape is Ellipse ellipse)
                {
                    double deltaX = e.GetPosition(MyCanvas).X - startPoint.X;
                    double deltaY = e.GetPosition(MyCanvas).Y - startPoint.Y;

                    // Aktualizuj szerokość i wysokość, uwzględniając odbicie
                    ellipse.Width = Math.Abs(ellipse.Width + deltaX);
                    ellipse.Height = Math.Abs(ellipse.Height + deltaY);

                    startPoint = e.GetPosition(MyCanvas);  
                }
                else if (SelectedShape is Line line)
                {
                    double deltaX = e.GetPosition(MyCanvas).X - startPoint.X;
                    double deltaY = e.GetPosition(MyCanvas).Y - startPoint.Y;

                    // Aktualizuj końce linii
                    line.X2 += deltaX;
                    line.Y2 += deltaY;

                    startPoint = e.GetPosition(MyCanvas);  
                }
            }
            //Rysowanie
            if (currentSelectedShape == SelectedShapeType.Line && e.RightButton == MouseButtonState.Pressed)
            {
                Point endPoint = e.GetPosition(MyCanvas);
                RemoveSelectedShape();
                double length = Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2) + Math.Pow(endPoint.Y - startPoint.Y, 2));//wzór długość odcinka
                double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X); //arcTangens do obliczenia wektora

                DrawLine(length, startPoint.X, startPoint.Y, angle);
                SelectedShape = MyCanvas.Children[MyCanvas.Children.Count - 1] as Shape;

            }
            else if (currentSelectedShape == SelectedShapeType.Rectangle && e.RightButton == MouseButtonState.Pressed)
            {
                Point endPoint = e.GetPosition(MyCanvas);

                // Oblicz długość i szerokość jako wartości dodatnie
                double width = Math.Max(0, Math.Abs(endPoint.X - startPoint.X));
                double height = Math.Max(0, Math.Abs(endPoint.Y - startPoint.Y));

                RemoveSelectedShape();

                // Ustaw punkty początkowe i końcowe w zależności od kierunku rysowania
                double x = startPoint.X;
                double y = startPoint.Y;
                if (endPoint.X < startPoint.X)
                    x = endPoint.X;
                if (endPoint.Y < startPoint.Y)
                    y = endPoint.Y;

                DrawRectangle(width, height, x, y);

                SelectedShape = MyCanvas.Children[MyCanvas.Children.Count - 1] as Shape;
            }
            else if (currentSelectedShape == SelectedShapeType.Circle && e.RightButton == MouseButtonState.Pressed)
            {
                Point endPoint = e.GetPosition(MyCanvas);

                // Oblicz promień jako wartość dodatnią
                double radius = Math.Max(0, Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2) + Math.Pow(endPoint.Y - startPoint.Y, 2)));

                RemoveSelectedShape();

                // Ustaw punkt środkowy okręgu
                double centerX = startPoint.X;
                double centerY = startPoint.Y;

                DrawCircle(radius, centerX, centerY);

                SelectedShape = MyCanvas.Children[MyCanvas.Children.Count - 1] as Shape;
            }
            UpdateTextBoxesForShape();
        }
        private void MyCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && SelectedShape != null)
            {
                isDragging = false;
            }
            MyCanvas.MouseMove -= MyCanvas_MouseMove;
            MyCanvas.MouseUp -= MyCanvas_MouseRightButtonUp;
        }
        //Upload/ Download last Shape
        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, "Shapes");
            openFileDialog.Filter = "JSON Files (*.json)|*.json";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    // Odczytaj zawartość pliku JSON
                    string json = File.ReadAllText(filePath);

                    // Deserializuj JSON do obiektu ShapeInfo
                    ShapeInfo shapeInfo = JsonConvert.DeserializeObject<ShapeInfo>(json);

                    // Dodaj kształt na podstawie wczytanych informacji
                    if (shapeInfo.ShapeType == SelectedShapeType.Line.ToString())
                    {
                        DrawLine(shapeInfo.Length, shapeInfo.X, shapeInfo.Y, shapeInfo.Vector);
                    }
                    else if (shapeInfo.ShapeType == SelectedShapeType.Rectangle.ToString())
                    {
                        DrawRectangle(shapeInfo.Length, shapeInfo.Height, shapeInfo.X, shapeInfo.Y);
                    }
                    else if (shapeInfo.ShapeType == SelectedShapeType.Circle.ToString())
                    {
                        DrawCircle(shapeInfo.Radius, shapeInfo.X, shapeInfo.Y);
                    }

                    // Zaktualizuj TextBoxy
                    UpdateTextBoxesForShape();

                    MessageBox.Show("Shape information loaded from " + filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading the shape information: " + ex.Message);
                }
            }
        }
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            ShapeInfo shapeInfo = null;
            if (SelectedShape == null)
            {
                MessageBox.Show("Select a shape before downloading.");
                return;
            }
            // Stwórz obiekt zawierający informacje o kształcie
            if (currentSelectedShape.ToString() == "Line")
            {
                 shapeInfo = new ShapeInfo
                {
                    ShapeType = currentSelectedShape.ToString(),
                    Length = double.Parse(LengthTextBox.Text),
                    Vector = double.Parse(VectorTextBox.Text),
                    X = double.Parse(XTextBox.Text),
                    Y = double.Parse(YTextBox.Text),
                    
                };
            }
            else if(currentSelectedShape.ToString() == "Rectangle"){
                 shapeInfo = new ShapeInfo
                {
                    ShapeType = currentSelectedShape.ToString(),
                    Length = double.Parse(LengthTextBox.Text),
                    Height = double.Parse(HeightTextBox.Text),
                    X = double.Parse(XTextBox.Text),
                    Y = double.Parse(YTextBox.Text),
                };
            }
            else if(currentSelectedShape.ToString() == "Circle")
            {
                 shapeInfo = new ShapeInfo
                {
                    ShapeType = currentSelectedShape.ToString(),
                    Radius = double.Parse(RadiusTextBox.Text),
                    X = double.Parse(XTextBox.Text),
                    Y = double.Parse(YTextBox.Text),
                };

            }
            if (shapeInfo != null)
            {
                // Serializuj obiekt do JSON
                string json = JsonConvert.SerializeObject(shapeInfo);

                // Określ ścieżkę do pliku z unikalną nazwą (data i godzina)
                string fileName = $"{currentSelectedShape}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = System.IO.Path.Combine("C:\\Users\\HP\\Documents\\Projekty_Visual\\Grafika_1\\Grafika_1\\Shapes", fileName);
                try
                {
                    // Zapisz JSON do pliku
                    File.WriteAllText(filePath, json);
                    MessageBox.Show("Shape information downloaded and saved to " + filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while saving the shape information: " + ex.Message);
                }
            }

        }

        //Upload/ Download full canvas
        private void SaveShapesToJson(string filePath)
        {
            List<ShapeInfo> shapesInfo = new List<ShapeInfo>();

            foreach (var child in MyCanvas.Children)
            {
                if (child is Line line)
                {
                    ShapeInfo shapeInfo = new ShapeInfo
                    {
                        ShapeType = SelectedShapeType.Line.ToString(),
                        Length = Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2)),
                        Vector = Math.Atan2(line.Y2 - line.Y1, line.X2 - line.X1),
                        X = line.X1,
                        Y = line.Y1
                    };
                    shapesInfo.Add(shapeInfo);
                }
                else if (child is Rectangle rectangle)
                {
                    ShapeInfo shapeInfo = new ShapeInfo
                    {
                        ShapeType = SelectedShapeType.Rectangle.ToString(),
                        Length = rectangle.Width,
                        Height = rectangle.Height,
                        X = Canvas.GetLeft(rectangle),
                        Y = Canvas.GetTop(rectangle)
                    };
                    shapesInfo.Add(shapeInfo);
                }
                else if (child is Ellipse ellipse)
                {
                    ShapeInfo shapeInfo = new ShapeInfo
                    {
                        ShapeType = SelectedShapeType.Circle.ToString(),
                        Radius = ellipse.Width / 2,
                        X = Canvas.GetLeft(ellipse) + ellipse.Width / 2,
                        Y = Canvas.GetTop(ellipse) + ellipse.Height / 2
                    };
                    shapesInfo.Add(shapeInfo);
                }
            }

            string json = JsonConvert.SerializeObject(shapesInfo);

            try
            {
                File.WriteAllText(filePath, json);
                MessageBox.Show("Shapes information saved to " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving the shapes information: " + ex.Message);
            }
        }

        private void LoadShapesFromJson(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                List<ShapeInfo> shapesInfo = JsonConvert.DeserializeObject<List<ShapeInfo>>(json);

                foreach (var shapeInfo in shapesInfo)
                {
                    if (shapeInfo.ShapeType == SelectedShapeType.Line.ToString())
                    {
                        DrawLine(shapeInfo.Length, shapeInfo.X, shapeInfo.Y, shapeInfo.Vector);
                    }
                    else if (shapeInfo.ShapeType == SelectedShapeType.Rectangle.ToString())
                    {
                        DrawRectangle(shapeInfo.Length, shapeInfo.Height, shapeInfo.X, shapeInfo.Y);
                    }
                    else if (shapeInfo.ShapeType == SelectedShapeType.Circle.ToString())
                    {
                        DrawCircle(shapeInfo.Radius, shapeInfo.X, shapeInfo.Y);
                    }
                }

                UpdateTextBoxesForShape();

                MessageBox.Show("Shapes information loaded from " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading the shapes information: " + ex.Message);
            }
        }
        private void UploadManyButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files (*.json)|*.json";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                LoadShapesFromJson(openFileDialog.FileName);
            }
        }
        private void DownloadManyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON Files (*.json)|*.json";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveShapesToJson(saveFileDialog.FileName);
            }
        }
    }
}

