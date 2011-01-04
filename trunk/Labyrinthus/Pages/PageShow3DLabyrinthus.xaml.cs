using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Labyrinthus.Objects;

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
    private readonly DiffuseMaterial debugMaterial; // TODO: удалить после отдалки
    #endregion

    #region Свойства
    /// <summary>
    /// Толщина стен лабиринта
    /// </summary>
    public int EdgeWidth { get; set; }

    /// <summary>
    /// Высота стен лабиринта
    /// </summary>
    public int EdgeHeight { get; set; }
    #endregion

    #region Конструктор
    public PageShow3DLabyrinthus()
    {
      InitializeComponent();

      EdgeWidth = 2;
      EdgeHeight = 2;

      primitiveEdgeMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.White));
      debugMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Green));
    }
    #endregion

    #region Обработка событий
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
      DrawLabyrinthus();
    }
    #endregion

    #region private методы
    private void DrawLabyrinthus()
    {
      LabyrinthusViewport.Children.Clear();

      var labyrinthusModelVisual = new ModelVisual3D();
      var labyrinthusModelGroup = new Model3DGroup();
      var labyrinthusGeometryModel = new GeometryModel3D();

      var myDirectionalLight = new DirectionalLight
      {
        Color = Colors.White,
        Direction = new Vector3D(2, 5, -3)
      };

      labyrinthusModelGroup.Children.Add(myDirectionalLight);


      DrawPrimitive(labyrinthusModelGroup);

      labyrinthusModelGroup.Children.Add(labyrinthusGeometryModel);
      labyrinthusModelVisual.Content = labyrinthusModelGroup;
      LabyrinthusViewport.Children.Add(labyrinthusModelVisual);
    }


    private void DrawPrimitive(Model3DGroup labyrinthusModelGroup)
    {
      var wnd = (WindowMaster)Application.Current.MainWindow;

      foreach (var lineInfo in wnd.Primitive.Lines)
      {
        var primitiveEdge = DrawPrimitiveEdge(lineInfo);

        labyrinthusModelGroup.Children.Add(primitiveEdge);
      }
    }

    private GeometryModel3D DrawPrimitiveEdge(LineInfo primitiveEdgeLine)
    {
      // 2D координаты основания ребра примитива
      double x00, x01, x10, x11, y00, y01, y10, y11;

      x00 = x01 = primitiveEdgeLine.X0 * ZOOM;
      x10 = x11 = primitiveEdgeLine.X1 * ZOOM;
      y00 = y01 = primitiveEdgeLine.Y0 * ZOOM;
      y10 = y11 = primitiveEdgeLine.Y1 * ZOOM;

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

      // TODO: поменять ориентацию для горизонтальных ребер
      var primitiveEdgeIndices = new Int32Collection
      {
        0,1,2,    3,5,4,
        6,8,7,    9,10,11,
        12,13,14, 15,17,16,
        18,20,19, 21,22,23,
        24,25,26, 27,29,28,
      };

      primitiveEdgeGeometry.TriangleIndices = primitiveEdgeIndices;

//      var myNormalCollection = new Vector3DCollection();
//      myNormalCollection.Add(new Vector3D(0, -1, 0));
//      myNormalCollection.Add(new Vector3D(0, -1, 0));
//      myNormalCollection.Add(new Vector3D(0, 1, 0));
//      myNormalCollection.Add(new Vector3D(0, 1, 0));
//      myNormalCollection.Add(new Vector3D(-1, 0, 0));
//      myNormalCollection.Add(new Vector3D(-1, 0, 0));
//      myNormalCollection.Add(new Vector3D(1, 0, 0));
//      myNormalCollection.Add(new Vector3D(1, 0, 0));
//      myNormalCollection.Add(new Vector3D(0, 0, 1));
//      myNormalCollection.Add(new Vector3D(0, 0, 1));
//      primitiveEdgeGeometry.Normals = myNormalCollection;

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
