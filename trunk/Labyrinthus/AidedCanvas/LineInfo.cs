namespace Labyrinthus.AidedCanvas
{
  public class LineInfo
  {
    public int X0 { get; set; }
    public int Y0 { get; set; }
    public int X1 { get; set; }
    public int Y1 { get; set; }

    public LineInfo(int x0, int y0, int x1, int y1)
    {
      X0 = x0;
      Y0 = y0;
      X1 = x1;
      Y1 = y1;
    }

    public override bool Equals(object obj)
    {
      var other = obj as LineInfo;
      if (other == null)
        return false;
      return other.X0 == X0 && other.X1 == X1 && other.Y0 == Y0 && other.Y1 == Y1;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}
