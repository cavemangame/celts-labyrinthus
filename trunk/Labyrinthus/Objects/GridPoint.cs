namespace Labyrinthus.Objects
{
  public class GridPoint
  {
    public int X { get; set; }
    public int Y { get; set; }

    public GridPoint(int x, int y)
    {
      X = x;
      Y = y;
    }

    #region Проверка на равенство

    public static bool operator ==(GridPoint a, GridPoint b)
    {
      if (ReferenceEquals(a, b))
      {
        return true;
      }

      if ((object)a == null || (object)b == null)
      {
        return false;
      }

      return a.Equals(b);
    }

    public static bool operator !=(GridPoint a, GridPoint b)
    {
      return !(a == b);
    }

    public bool Equals(GridPoint other)
    {
      return other.X == X && other.Y == Y;
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
      {
        return false;
      }
      var other = obj as GridPoint;
      if (other == null)
      {
        return false;
      }
      return Equals(other);
    }

    public override int GetHashCode()
    {
      return X ^ Y;
    }

    #endregion
  }
}
