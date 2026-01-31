using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf;
using LabControlsWPF;

namespace LabControlsWPF.Plot3D
{
    /// <summary>
    /// Klasa kontrolki wykresu płaszczyzny 3D
    /// </summary>
    public class SurfacePlot : ModelVisual3D
    {
        /// <summary>Flaga której ustawienie powoduje ponowne narysowanie wykresu</summary>
        public object InvalidateFlag
        {
            get { return (object)GetValue(InvalidateFlagProperty); }
            set { SetValue(InvalidateFlagProperty, value); }
        }
        public static readonly DependencyProperty InvalidateFlagProperty =
            DependencyProperty.Register("InvalidateFlag", typeof(object), typeof(SurfacePlot), new UIPropertyMetadata(null, ModelChanged));

        /// <summary>Płaszczyzna do plotowania</summary>
        public Point3D[,] Points
        {
            get { return (Point3D[,])GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(Point3D[,]), typeof(SurfacePlot),
                                        new PropertyMetadata(null));

        /// <summary>Tytuł osi X</summary>
        public string XAxisTitle
        {
            get { return (string)GetValue(XAxisTitleProperty); }
            set { SetValue(XAxisTitleProperty, value); }
        }
        public static readonly DependencyProperty XAxisTitleProperty =
            DependencyProperty.Register("XAxisTitle", typeof(string), typeof(SurfacePlot), new PropertyMetadata("X-Axis"));

        /// <summary>Tytuł osi Y</summary>
        public string YAxisTitle
        {
            get { return (string)GetValue(YAxisTitleProperty); }
            set { SetValue(YAxisTitleProperty, value); }
        }
        public static readonly DependencyProperty YAxisTitleProperty =
            DependencyProperty.Register("YAxisTitle", typeof(string), typeof(SurfacePlot), new PropertyMetadata("Y-Axis"));

        /// <summary>Tytuł osi Z</summary>
        public string ZAxisTitle
        {
            get { return (string)GetValue(ZAxisTitleProperty); }
            set { SetValue(ZAxisTitleProperty, value); }
        }
        public static readonly DependencyProperty ZAxisTitleProperty =
            DependencyProperty.Register("ZAxisTitle", typeof(string), typeof(SurfacePlot), new PropertyMetadata("Z-Axis"));



        public AxisType XAxisType
        {
            get { return (AxisType)GetValue(XAxisTypeProperty); }
            set { SetValue(XAxisTypeProperty, value); }
        }
        public static readonly DependencyProperty XAxisTypeProperty =
            DependencyProperty.Register("XAxisType", typeof(AxisType), typeof(SurfacePlot), new PropertyMetadata(AxisType.Linear));

        public AxisType YAxisType
        {
            get { return (AxisType)GetValue(YAxisTypeProperty); }
            set { SetValue(YAxisTypeProperty, value); }
        }
        public static readonly DependencyProperty YAxisTypeProperty =
            DependencyProperty.Register("YAxisType", typeof(AxisType), typeof(SurfacePlot), new PropertyMetadata(AxisType.Linear));

        public AxisType ZAxisType
        {
            get { return (AxisType)GetValue(ZAxisTypeProperty); }
            set { SetValue(ZAxisTypeProperty, value); }
        }
        public static readonly DependencyProperty ZAxisTypeProperty =
            DependencyProperty.Register("ZAxisType", typeof(AxisType), typeof(SurfacePlot), new PropertyMetadata(AxisType.Linear));



        public bool PlotXInfoLines
        {
            get { return (bool)GetValue(PlotXInfoLinesProperty); }
            set { SetValue(PlotXInfoLinesProperty, value); }
        }
        public static readonly DependencyProperty PlotXInfoLinesProperty =
            DependencyProperty.Register("PlotXInfoLines", typeof(bool), typeof(SurfacePlot), new PropertyMetadata(false));

        public bool PlotYInfoLines
        {
            get { return (bool)GetValue(PlotYInfoLinesProperty); }
            set { SetValue(PlotYInfoLinesProperty, value); }
        }
        public static readonly DependencyProperty PlotYInfoLinesProperty =
            DependencyProperty.Register("PlotYInfoLines", typeof(bool), typeof(SurfacePlot), new PropertyMetadata(false));

        public bool PlotVerdicalInfoLines
        {
            get { return (bool)GetValue(PlotVerdicalInfoLinesProperty); }
            set { SetValue(PlotVerdicalInfoLinesProperty, value); }
        }
        public static readonly DependencyProperty PlotVerdicalInfoLinesProperty =
            DependencyProperty.Register("PlotVerdicalInfoLines", typeof(bool), typeof(SurfacePlot), new PropertyMetadata(false));

        public bool ColorByLight
        {
            get { return (bool)GetValue(ColorByLightProperty); }
            set { SetValue(ColorByLightProperty, value); }
        }
        public static readonly DependencyProperty ColorByLightProperty =
            DependencyProperty.Register("ColorByLight", typeof(bool), typeof(SurfacePlot), new PropertyMetadata(true));


        private readonly ModelVisual3D visualChild;

        public SurfacePlot()
        {
            visualChild = new ModelVisual3D();
            Children.Add(visualChild);
        }

        private double[,] ColorValues { get; set; }

        /// <summary>Gets or sets the brush used for the surface</summary>
        public Brush SurfaceBrush { get; private set; }

        /// <summary> Interwał rysowania znaczników osi X </summary>
        private double IntervalX { get; } = 4;
        /// <summary> Interwał rysowania znaczników osi Y </summary>
        private double IntervalY { get; } = 2;
        /// <summary> Interwał rysowania znaczników osi Z </summary>
        private double IntervalZ { get; } = 2;
        /// <summary> Rozmiar rysowanej czcionki</summary>
        private double FontSize { get; } = 0.5;
        /// <summary> Grubość rysowanych lini pomocniczych </summary>
        private double LineThickness { get; } = 0.05;

        /// <summary>Jaka ma być wartość maksymalna X -> zakres (0, limit)</summary>
        private int NormalizationLimitX { get; } = 40;

        /// <summary>Jaka ma być wartość maksymalna Y -> zakres (0, limit)</summary>
        private double NormalizationLimitY { get; } = 40;
        /// <summary>Jaka ma być wartość maksymalna Z -> zakres (0, limit)</summary>
        private double NormalizationLimitZ { get; } = 40;

        /// <summary> Funkcja wywoływana przy żadaniu odświeżenia modelu </summary>
        private static void ModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SurfacePlot surfacePlot = (SurfacePlot)d;
            if (surfacePlot.ColorByLight)
            {
                surfacePlot.ColorValues = null;
                surfacePlot.SurfaceBrush = Brushes.White;
            }
            else
            {
                surfacePlot.ColorValues = surfacePlot.FindGradientY(surfacePlot.Points);
                surfacePlot.SurfaceBrush = BrushHelper.CreateGradientBrush(Colors.Red, Colors.White, Colors.Blue);
            }
            surfacePlot.UpdateModel();
        }

        /// <summary> Funkcja odświeża wykres 3D </summary>
        private void UpdateModel()
        {
            try
            {
                if(Points.GetUpperBound(0) < 2 || Points.GetUpperBound(1) < 2)
                {
                    MaterialMessageBox.NewFastMessage(
                        MaterialMessageFastType.InternalError,
                        $"Błąd podczas generownaia wykresu 3D: za mało danych: {Points.GetUpperBound(0) + 1}x{Points.GetUpperBound(1) + 1}"
                        );
                    // Brak punktów do narysowania
                    visualChild.Content = new Model3DGroup();
                    return;
                }
                else
                {
                    visualChild.Content = CreateModel();
                    return;
                }
            }
            catch(Exception ex)
            {
                Serilog.Log.Error(ex, "SurfacePlot.UpdateModel-Error during 3D plot generation");
                MaterialMessageBox.NewFastMessage(
                    MaterialMessageFastType.InternalError,
                    $"Błąd podczas generownaia wykresu 3D: {ex.Message}"
                    );
            }
        }

        /// <summary>
        /// Funkcja rysuje wykres 3D
        /// </summary>
        /// <returns></returns>
        private Model3D CreateModel()
        {
            var plotModel = new Model3DGroup();

            int rows = Points.GetUpperBound(0) + 1;
            int columns = Points.GetUpperBound(1) + 1;


            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            double minZ = double.MaxValue;
            double maxZ = double.MinValue;
            double minColorValue = double.MaxValue;
            double maxColorValue = double.MinValue;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    double x = Points[i, j].X;
                    double y = Points[i, j].Y;
                    double z = Points[i, j].Z;
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                    maxZ = Math.Max(maxZ, z);
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    minZ = Math.Min(minZ, z);
                    if (ColorValues != null)
                    {
                        maxColorValue = Math.Max(maxColorValue, ColorValues[i, j]);
                        minColorValue = Math.Min(minColorValue, ColorValues[i, j]);
                    }
                }

            // Normalizacja danych
            Point3D[,] normalizedPoints = new Point3D[rows, columns];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    double newX = Points[i, j].X;
                    if (XAxisType == AxisType.Logarytmic)
                    {
                        newX = Math.Log10(newX);
                        newX = ChangeRange(newX, Math.Log10(minX), Math.Log10(maxX), 0, NormalizationLimitX);
                    }
                    else
                        newX = ChangeRange(newX, minX, maxX, 0, NormalizationLimitX);

                    double newY = Points[i, j].Y;
                    if (YAxisType == AxisType.Logarytmic)
                    {
                        newY = Math.Log10(newY);
                        newY = ChangeRange(newY, Math.Log10(minY), Math.Log10(maxY), 0, NormalizationLimitY);
                    }
                    else
                        newY = ChangeRange(newY, minY, maxY, 0, NormalizationLimitY);

                    double newZ = Points[i, j].Z;
                    if(ZAxisType == AxisType.Logarytmic)
                    {
                        newZ = Math.Log10(newZ);
                        newZ = ChangeRange(newZ, Math.Log10(minZ), Math.Log10(maxZ), 0, NormalizationLimitZ);
                    }
                    else
                        newZ = ChangeRange(newZ, minZ, maxZ, 0, NormalizationLimitZ);

                    normalizedPoints[i, j] = new Point3D(newX, newY, newZ);
                }

            // make color value 0 at texture coordinate 0.5
            if (Math.Abs(minColorValue) < Math.Abs(maxColorValue))
                minColorValue = -maxColorValue;
            else
                maxColorValue = -minColorValue;

            // set the texture coordinates by z-value or ColorValue
            var texcoords = new Point[rows, columns];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    double u = normalizedPoints[i, j].Z / NormalizationLimitZ;
                    if (ColorValues != null)
                        u = (ColorValues[i, j] - minColorValue) / (maxColorValue - minColorValue);
                    texcoords[i, j] = new Point(u, u);
                }

            // Plotowanie siatki wykresu
            var surfaceMeshBuilder = new MeshBuilder();
            surfaceMeshBuilder.AddRectangularMesh(normalizedPoints, texcoords);

            var surfaceModel = new GeometryModel3D(surfaceMeshBuilder.ToMesh(),
                                                   MaterialHelper.CreateMaterial(SurfaceBrush, null, null, 1, 0));
            surfaceModel.BackMaterial = surfaceModel.Material;

            // Tworzenie siatki osi
            var axesMeshBuilder = new MeshBuilder();

            // Tworzenie osi X
            for (double x = 0; x <= NormalizationLimitX; x += IntervalX)
            {
                if(PlotXInfoLines)
                {
                    double j = x / NormalizationLimitX * (columns - 1);
                    var path = new List<Point3D>();
                    if(PlotVerdicalInfoLines)
                        path.Add(new Point3D(x, 0, 0));
                    for (int i = 0; i < rows; i++)
                    {
                        path.Add(BilinearInterpolation(normalizedPoints, i, j));
                    }
                    if (PlotVerdicalInfoLines)
                        path.Add(new Point3D(x, NormalizationLimitY, 0));

                    axesMeshBuilder.AddTube(path, LineThickness, 9, false);
                }
                double realXValue;
                if(XAxisType == AxisType.Logarytmic)
                {
                    realXValue = ChangeRange(x, 0, NormalizationLimitX, Math.Log10(minX), Math.Log10(maxX));
                    realXValue = Math.Pow(realXValue, 10);
                }
                else
                    realXValue  = ChangeRange(x, 0, NormalizationLimitX, minX, maxX);
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(realXValue.ToString("G6", CultureInfo.InvariantCulture), Brushes.Black, true, FontSize,
                                                                           new Point3D(x, -FontSize * 6, 0),
                                                                           new Vector3D(0, 1, 0), new Vector3D(1, 0, 0));
                plotModel.Children.Add(label);
            }
            {
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(XAxisTitle, Brushes.Black, true, FontSize,
                                                                           new Point3D(NormalizationLimitX * 0.5,
                                                                                       -FontSize * 15, 0),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                plotModel.Children.Add(label);
            }

            // Tworzenie siatki osi Y
            for (double y = 0; y <= NormalizationLimitY; y += IntervalY)
            {
                if(PlotYInfoLines)
                {
                    double i = y / NormalizationLimitY * (rows - 1);
                    var path = new List<Point3D>();
                    if(PlotVerdicalInfoLines)
                        path.Add(new Point3D(0, y, 0));
                    for (int j = 0; j < columns; j++)
                    {
                        path.Add(BilinearInterpolation(normalizedPoints, i, j));
                    }
                    if (PlotVerdicalInfoLines)
                        path.Add(new Point3D(NormalizationLimitX, y, 0));

                    axesMeshBuilder.AddTube(path, LineThickness, 9, false);
                }
                double realYValue;
                if (YAxisType == AxisType.Logarytmic)
                {
                    realYValue = ChangeRange(y, 0, NormalizationLimitY, Math.Log10(minY), Math.Log10(maxY));
                    realYValue = Math.Pow(realYValue, 10);
                }
                else
                    realYValue = ChangeRange(y, 0, NormalizationLimitY, minY, maxY);
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(realYValue.ToString("G6", CultureInfo.InvariantCulture), Brushes.Black, true, FontSize,
                                                                           new Point3D(-FontSize * 6, y, 0),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                plotModel.Children.Add(label);
            }
            {
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(YAxisTitle, Brushes.Black, true, FontSize,
                                                                           new Point3D(-FontSize * 15,
                                                                                       NormalizationLimitY * 0.5, 0),
                                                                           new Vector3D(0, 1, 0), new Vector3D(-1, 0, 0));
                plotModel.Children.Add(label);
            }

            // Tworzenie siatki osi Z
            double z0 = (int)Math.Pow(IntervalZ, 2);
            for (double z = z0; z <= NormalizationLimitZ + double.Epsilon; z += IntervalZ)
            {
                double realZValue;
                if (ZAxisType == AxisType.Logarytmic)
                {
                    realZValue = ChangeRange(z, 0, NormalizationLimitZ, Math.Log10(minZ), Math.Log10(maxZ));
                    realZValue = Math.Pow(realZValue, 10);
                }
                else
                    realZValue = ChangeRange(z, 0, NormalizationLimitZ, minZ, maxZ);
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(realZValue.ToString("G6", CultureInfo.InvariantCulture), Brushes.Black, true, FontSize,
                                                                           new Point3D(-FontSize * 6, NormalizationLimitY, z),
                                                                           new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
                plotModel.Children.Add(label);
            }
            {
                GeometryModel3D label = TextCreator.CreateTextLabelModel3D(ZAxisTitle, Brushes.Black, true, FontSize,
                                                                           new Point3D(-FontSize * 15, NormalizationLimitY,
                                                                                       NormalizationLimitZ * 0.5),
                                                                           new Vector3D(0, 0, 1), new Vector3D(1, 0, 0));
                plotModel.Children.Add(label);
            }

            // Dodawanie bounding box i plotowanie osi
            var bb = new Rect3D(0, 0, 0, NormalizationLimitX, NormalizationLimitY, 0);
            axesMeshBuilder.AddBoundingBox(bb, LineThickness);
            var axesModel = new GeometryModel3D(axesMeshBuilder.ToMesh(), Materials.Black);

            // Wrzucanie na ekran wykresu
            plotModel.Children.Add(surfaceModel);
            plotModel.Children.Add(axesModel);

            return plotModel;
        }

        private static Point3D BilinearInterpolation(Point3D[,] p, double i, double j)
        {
            int n = p.GetUpperBound(0);
            int m = p.GetUpperBound(1);
            var i0 = (int)i;
            var j0 = (int)j;
            if (i0 + 1 >= n) i0 = n - 2;
            if (j0 + 1 >= m) j0 = m - 2;

            if (i < 0) i = 0;
            if (j < 0) j = 0;
            double u = i - i0;
            double v = j - j0;
            Vector3D v00 = p[i0, j0].ToVector3D();
            Vector3D v01 = p[i0, j0 + 1].ToVector3D();
            Vector3D v10 = p[i0 + 1, j0].ToVector3D();
            Vector3D v11 = p[i0 + 1, j0 + 1].ToVector3D();
            Vector3D v0 = v00 * (1 - u) + v10 * u;
            Vector3D v1 = v01 * (1 - u) + v11 * u;
            return (v0 * (1 - v) + v1 * v).ToPoint3D();
        }

        // Kod dodany na potrzeby automatycznego generowania ColorValues
        public double[,] FindGradientY(Point3D[,] data)
        {
            int n = data.GetUpperBound(0) + 1;
            int m = data.GetUpperBound(1) + 1;
            var K = new double[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    // Finite difference approximation
                    var p10 = data[i + 1 < n ? i + 1 : i, j - 1 > 0 ? j - 1 : j];
                    var p00 = data[i - 1 > 0 ? i - 1 : i, j - 1 > 0 ? j - 1 : j];
                    //var p11 = data[i + 1 < n ? i + 1 : i, j + 1 < m ? j + 1 : j];
                    //var p01 = data[i - 1 > 0 ? i - 1 : i, j + 1 < m ? j + 1 : j];

                    //double dx = p01.X - p00.X;
                    //double dz = p01.Z - p00.Z;
                    //double Fx = dz / dx;

                    double dy = p10.Y - p00.Y;
                    double dz = p10.Z - p00.Z;

                    K[i, j] = dz / dy;
                }
            return K;
        }

        /// <summary>
        /// Funkcja zmienia zakres danej
        /// </summary>
        /// <param name="value">Wartość wejściowa</param>
        /// <param name="oldMin">Dolne ograniczenie zakresu wartości wejściowej</param>
        /// <param name="oldMax">Górne ograniczenie zakresu wartości wejściowej</param>
        /// <param name="newMin">Dolne ograniczenie nowego zakresu</param>
        /// <param name="newMax">Górne ograniczenie nowego zakresu</param>
        /// <returns>Rzut wartości na nowy zakres</returns>
        private double ChangeRange(double value, double oldMin, double oldMax, double newMin, double newMax)
        {
            double oldRange = oldMax - oldMin;
            double newRange = newMax - newMin;
            return ((value - oldMin) * newRange / oldRange) + newMin;
        }
    }

    /// <summary> Enum określający typ osi wykresu </summary>
    public enum AxisType
    {
        Linear,
        Logarytmic
    }
}
