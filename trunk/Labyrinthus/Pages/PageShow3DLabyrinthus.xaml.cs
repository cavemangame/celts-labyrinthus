using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Labyrinthus.Objects;
using System.Windows.Controls;

namespace Labyrinthus.Pages
{
  public partial class PageShow3DLabyrinthus
  {
    #region Константы
    /// <summary>
    /// Масштаб
    /// </summary>
    private const int ZOOM = 4;
    #endregion

    #region Поля
    
    /// <summary>
    /// Материал для стен лабиринта
    /// </summary>
    private readonly DiffuseMaterial primitiveEdgeMaterial;

    /// <summary>
    /// Материал для стен лабиринта
    /// </summary>
    private readonly DiffuseMaterial debugMaterial;

    /// <summary>
    /// Начальное положение мыши перед изменением взгляда камеры
    /// </summary>
    private Point moveLookStartPoint;
    #endregion

    #region Свойства
    /// <summary>
    /// Размер лабиринта
    /// </summary>
    public int LabyrinthusSize { get; set; }

    /// <summary>
    /// Толщина стен лабиринта
    /// </summary>
    public int EdgeWidth { get; set; }

    /// <summary>
    /// Высота стен лабиринта
    /// </summary>
    public int EdgeHeight { get; set; }
    
    /// <summary>
    /// Кисть для стен лабиринта
    /// </summary>
    public SolidColorBrush PrimitiveEdgeBrush { get; set; }

    public SolidColorBrush FloorBrush { get; set; }
    #endregion

    #region Конструктор
    public PageShow3DLabyrinthus()
    {
      InitializeComponent();

      LabyrinthusSize = 4;
      EdgeWidth = 1;
      EdgeHeight = 4;
      
      PrimitiveEdgeBrush = new SolidColorBrush(Colors.Gray);
      primitiveEdgeMaterial = new DiffuseMaterial(PrimitiveEdgeBrush);
      FloorBrush = new SolidColorBrush(Colors.Red);

      debugMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Green));

      DataContext = this;
    }
    #endregion

    #region Обработка событий
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
      DrawLabyrinthus();
    }

    private void LabyrinthusParams_Changed(object sender, RoutedEventArgs e)
    {
      DrawLabyrinthus();
    }

    private void LabyrinthusGrid_KeyDown(object sender, KeyEventArgs e)
    {
      var pos = Camera.Position;
      var look = Camera.LookDirection;
      var maxLook = Math.Max(
        Math.Max(Math.Abs(look.X), Math.Abs(look.Y)), Math.Abs(look.Z));
      var productLook = Vector3D.CrossProduct(Camera.LookDirection, Camera.UpDirection);
      var maxProductLook = Math.Max(
        Math.Max(Math.Abs(productLook.X), Math.Abs(productLook.Y)), Math.Abs(productLook.Z));

      Point3D newPos;

      switch (e.Key)
      {
//        case Key.Up:
        case Key.W:
          newPos = new Point3D(pos.X + look.X / maxLook,
                               pos.Y + look.Y / maxLook,
                               pos.Z + look.Z / maxLook);
          break;
//        case Key.Down:
        case Key.S:
          newPos = new Point3D(pos.X - look.X / maxLook,
                               pos.Y - look.Y / maxLook,
                               pos.Z - look.Z / maxLook);
          break;
//        case Key.Left:
        case Key.A:
          newPos = new Point3D(pos.X - productLook.X / maxProductLook,
                               pos.Y - productLook.Y / maxProductLook,
                               pos.Z - productLook.Z / maxProductLook);
          break;
//        case Key.Right:
        case Key.D:
          newPos = new Point3D(pos.X + productLook.X / maxProductLook,
                               pos.Y + productLook.Y / maxProductLook,
                               pos.Z + productLook.Z / maxProductLook);
          break;
        default:
          newPos = pos;
          break;
      }

      if (newPos.Z <= 0)
      {
        newPos = pos;
      }

      Camera.Position = newPos;
    }


    private void labyrinthusGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
      // начало вращения камеры
      moveLookStartPoint = e.MouseDevice.GetPosition(LabyrinthusViewport);
    }



    private void labyrinthusGrid_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
      {
        return;
      }

      var moveLookEndPoint = e.MouseDevice.GetPosition(LabyrinthusViewport);
      var xShift = moveLookEndPoint.X - moveLookStartPoint.X;
      var xAngle = - Math.PI * xShift / LabyrinthusViewport.ActualWidth;
      var yShift = moveLookEndPoint.Y - moveLookStartPoint.Y;
      var yAngle = Math.PI / 2 * yShift / LabyrinthusViewport.ActualHeight;
      var look = Camera.LookDirection;
      var newLook = new Vector3D(look.X, look.Y, look.Z);

      // поворот влево-вправо
      if (xAngle != 0)
      {
        var x = newLook.X * Math.Cos(xAngle) + newLook.Y * Math.Sin(xAngle);
        var y = - newLook.X * Math.Sin(xAngle) + newLook.Y * Math.Cos(xAngle);

        newLook.X = x;
        newLook.Y = y;
      }

      // поворот вверх-вниз
      if (yAngle != 0)
      {
        var xy = Math.Sqrt(Math.Pow(newLook.X, 2) + Math.Pow(newLook.Y, 2));
        var curZAngle = Math.Atan(Math.Abs(newLook.Z) / xy);
        var newZAngle = curZAngle + yAngle * Math.Sign(newLook.Z);

        if (Math.Abs(newZAngle) < 85d * Math.PI / 180d)
        {
          var z = Math.Tan(newZAngle) * xy * Math.Sign(newLook.Z);

          newLook.Z = z;
        }
      }

      Camera.LookDirection = newLook;
      moveLookStartPoint = moveLookEndPoint;
    }
    #endregion

    #region private методы


    public Model3DGroup Maze
    {
        get { return (Model3DGroup)GetValue(MazeProperty); }
        set { SetValue(MazeProperty, value); }
    }
    public static readonly DependencyProperty MazeProperty =
        DependencyProperty.Register("Maze", typeof(Model3DGroup), typeof(PageShow3DLabyrinthus), new UIPropertyMetadata(null));

    public GeometryModel3D Floor
    {
        get { return (GeometryModel3D)GetValue(FloorProperty); }
        set { SetValue(FloorProperty, value); }
    }
    public static readonly DependencyProperty FloorProperty =
        DependencyProperty.Register("Floor", typeof(GeometryModel3D), typeof(PageShow3DLabyrinthus), new UIPropertyMetadata(null));

    public Vector3D LightDirection
    {
        get { return (Vector3D)GetValue(LightDirectionProperty); }
        set { SetValue(LightDirectionProperty, value); }
    }
    public static readonly DependencyProperty LightDirectionProperty =
        DependencyProperty.Register("LightDirection", typeof(Vector3D), typeof(PageShow3DLabyrinthus), new UIPropertyMetadata(new Vector3D(2, 3, -7)));

    private void DrawLabyrinthus()
    {
        var labyrinthusModelGroup = new Model3DGroup();

        for (int x = -LabyrinthusSize; x < LabyrinthusSize; x++)
        {
            for (int y = -LabyrinthusSize; y < LabyrinthusSize; y++)
            {
                DrawPrimitive(labyrinthusModelGroup, x, y);
            }
        }

        var wnd = (WindowMaster)Application.Current.MainWindow;
        var xSize = wnd.Primitive.Width * ZOOM * LabyrinthusSize * 2;
        var ySize = wnd.Primitive.Height * ZOOM * LabyrinthusSize * 2;
        var viewport3d = new Viewport3D
        {
            Opacity = 0.8,
            Camera = new OrthographicCamera
            {
                Position = new Point3D(LightDirection.X * -10, LightDirection.Y * -10, LightDirection.Z * -10),
                LookDirection = LightDirection,
                Width = xSize,
                UpDirection = new Vector3D(0, 0, -1)
            }
        };
        viewport3d.Children.Add(new ModelVisual3D { Content = labyrinthusModelGroup });

        var shadow = new Border
        {
            Width = 400,
            Height = 400,
            Background = new SolidColorBrush(Colors.Transparent),
            Child = viewport3d
        };

        Maze = labyrinthusModelGroup;
        Floor = GetFloor(shadow);
    }

    private GeometryModel3D GetFloor(Border shadow)
    {
      var wnd = (WindowMaster)Application.Current.MainWindow;
      var xSize = wnd.Primitive.Width * ZOOM * LabyrinthusSize * 2;
      var ySize = wnd.Primitive.Height * ZOOM * LabyrinthusSize * 2;

      var floorPositions = new Point3DCollection
      {
        new Point3D(-xSize, -ySize, 0),
        new Point3D( xSize, -ySize, 0),
        new Point3D(-xSize,  ySize, 0),
        new Point3D( xSize,  ySize, 0),
      };

      var floorIndices = new Int32Collection
      {
        0,1,2,
        1,3,2
      };
      var pointCollection = new PointCollection
        {
            new Point(0,1),
            new Point(1,1),
            new Point(0,0),
            new Point(1,0),
        };

      var floorGeometry = new MeshGeometry3D
      {
        Positions = floorPositions,
        TriangleIndices = floorIndices,
        TextureCoordinates = pointCollection
      };
      var shadowBrush = new VisualBrush(shadow);
      var angle = 180 + Math.Atan(LightDirection.X / LightDirection.Y) * 180 / 3.14;
        var groupTransform = new TransformGroup();
        groupTransform.Children.Add(new ScaleTransform(0.5, 0.5, 0.5, 0.5));// should be math
        groupTransform.Children.Add(new RotateTransform(angle, 0.5, 0.5)); // should be math
        groupTransform.Children.Add(new TranslateTransform(-0.002, 0.002)); // should be math
        shadowBrush.RelativeTransform = groupTransform;
      var material = new MaterialGroup();
      material.Children.Add(new DiffuseMaterial { Brush = FloorBrush });
      material.Children.Add(new DiffuseMaterial { Brush = shadowBrush });
      var floorGeometryModel = new GeometryModel3D
       {
         Geometry = floorGeometry,
         Material = material
       };

      return floorGeometryModel;
    }


    private void DrawPrimitive(Model3DGroup labyrinthusModelGroup, int startX, int startY)
    {
      var wnd = (WindowMaster)Application.Current.MainWindow;

      foreach (var lineInfo in wnd.Primitive.Lines)
      {
        var primitiveEdge = DrawPrimitiveEdge(lineInfo, startX, startY);

        labyrinthusModelGroup.Children.Add(primitiveEdge);
      }
    }


    private GeometryModel3D DrawPrimitiveEdge(LineInfo primitiveEdgeLine, int startX, int startY)
    {
      // 2D координаты основания ребра примитива
      double x00, x01, x10, x11, y00, y01, y10, y11;
      var wnd = (WindowMaster)Application.Current.MainWindow;
      var primitiveWidth = wnd.Primitive.Width;
      var primitiveHeight = wnd.Primitive.Height;

      x00 = x01 = (primitiveEdgeLine.X0 + primitiveWidth * startX) * ZOOM;
      x10 = x11 = (primitiveEdgeLine.X1 + primitiveWidth * startX) * ZOOM;
      y00 = y01 = (- primitiveEdgeLine.Y0 + primitiveHeight * startY) * ZOOM;
      y10 = y11 = (- primitiveEdgeLine.Y1 + primitiveHeight * startY) * ZOOM;

      if (primitiveEdgeLine.X0 == primitiveEdgeLine.X1)
      {
        // вертикальное ребро
        x00 -= EdgeWidth / 2d;
        x01 += EdgeWidth / 2d;
        x10 -= EdgeWidth / 2d;
        x11 += EdgeWidth / 2d;
      }
      else
      {
        // горизонтальное ребро
        y00 -= EdgeWidth / 2d;
        y01 += EdgeWidth / 2d;
        y10 -= EdgeWidth / 2d;
        y11 += EdgeWidth / 2d;
      }

      var primitiveEdgeGeometry = new MeshGeometry3D();

      // все точки уникальны
      var primitiveEdgePositions = new Point3DCollection
      {
        // передняя горизонталь
        new Point3D(x00, y00, 0),          // 0
        new Point3D(x01, y01, 0),          // 1
        new Point3D(x00, y00, EdgeHeight), // 2
        new Point3D(x01, y01, 0),          // 3
        new Point3D(x00, y00, EdgeHeight), // 4
        new Point3D(x01, y01, EdgeHeight), // 5

        // задняя горизонталь
        new Point3D(x10, y10, 0),          // 6
        new Point3D(x11, y11, 0),          // 7
        new Point3D(x10, y10, EdgeHeight), // 8
        new Point3D(x11, y11, 0),          // 9
        new Point3D(x10, y10, EdgeHeight), // 10
        new Point3D(x11, y11, EdgeHeight), // 11

        // левая вертикаль
        new Point3D(x10, y10, 0),          // 12
        new Point3D(x00, y00, 0),          // 13
        new Point3D(x10, y10, EdgeHeight), // 14
        new Point3D(x00, y00, 0),          // 15
        new Point3D(x10, y10, EdgeHeight), // 16
        new Point3D(x00, y00, EdgeHeight), // 17

        // правая вертикаль
        new Point3D(x11, y11, 0),          // 18
        new Point3D(x01, y01, 0),          // 19
        new Point3D(x11, y11, EdgeHeight), // 20
        new Point3D(x01, y01, 0),          // 21
        new Point3D(x11, y11, EdgeHeight), // 22
        new Point3D(x01, y01, EdgeHeight), // 23

        // верх
        new Point3D(x00, y00, EdgeHeight), // 24
        new Point3D(x01, y01, EdgeHeight), // 25
        new Point3D(x10, y10, EdgeHeight), // 26
        new Point3D(x01, y01, EdgeHeight), // 27
        new Point3D(x10, y10, EdgeHeight), // 28
        new Point3D(x11, y11, EdgeHeight), // 29
      };
      primitiveEdgeGeometry.Positions = primitiveEdgePositions;

      var primitiveEdgeIndices = new Int32Collection
      {
        0,2,1,    3,4,5,
        6,7,8,    9,11,10,
        12,14,13, 15,16,17,
        18,19,20, 21,23,22,
        24,26,25, 27,28,29,
      };

      primitiveEdgeGeometry.TriangleIndices = primitiveEdgeIndices;

      var labyrinthusGeometryModel = new GeometryModel3D
       {
         Geometry = primitiveEdgeGeometry,
         Material = primitiveEdgeMaterial,
         BackMaterial = debugMaterial
       };

      return labyrinthusGeometryModel;
    }
    #endregion
  }
}
