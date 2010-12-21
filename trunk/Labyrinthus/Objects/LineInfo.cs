using System;
namespace Labyrinthus.Objects
{
  [Serializable]
  public class LineInfo : IEquatable<LineInfo>
  {
    #region Поля

    public int X0 { get; set; }
    public int Y0 { get; set; }
    public int X1 { get; set; }
    public int Y1 { get; set; }

    #endregion

    #region Конструкторы 

    public LineInfo()
    {
      
    }

    public LineInfo(int x0, int y0, int x1, int y1)
    {
      X0 = x0;
      Y0 = y0;
      X1 = x1;
      Y1 = y1;
    }

    #endregion

    #region Проверка на равенство

    public static bool operator ==(LineInfo a, LineInfo b)
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

    public static bool operator !=(LineInfo a, LineInfo b)
    {
      return !(a == b);
    }

    public bool Equals(LineInfo other)
    {
      return other.X0 == X0 && other.X1 == X1 && other.Y0 == Y0 && other.Y1 == Y1;
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
      {
        return false;
      }
      var other = obj as LineInfo;
      if (other == null)
      {
        return false;
      }
      return Equals(other);
    }

    public override int GetHashCode()
    {
      return X0 ^ Y0 ^ X1 ^ Y1;
    }

    #endregion
  }
}