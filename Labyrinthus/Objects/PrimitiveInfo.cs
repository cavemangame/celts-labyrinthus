using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Labyrinthus.Objects
{
  [Serializable]
  public class PrimitiveInfo
  {
    #region Поля

    public int Width { get; set;}
    public int Height { get; set; }
    public List<LineInfo> Lines { get; set; }

    #endregion

    #region Конструктор

    public PrimitiveInfo()
    {
      Width = Height = 0;
      Lines = new List<LineInfo>();
    }

    #endregion

    #region Сериализация

    public void Serialize(string filePath)
    {
      using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
      {
        var xs = new XmlSerializer(typeof(PrimitiveInfo));
        xs.Serialize(fs, this);
      }
    }

    public void Deserialize(string filePath)
    {
      using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
      {
        var xs = new XmlSerializer(typeof(PrimitiveInfo));
        var info = (PrimitiveInfo)xs.Deserialize(fs);
        Width = info.Width;
        Height = info.Height;
        Lines = info.Lines;
      }
    }

    #endregion
  }
}
