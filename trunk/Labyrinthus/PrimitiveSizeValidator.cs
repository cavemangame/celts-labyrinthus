using System.Globalization;
using System.Windows.Controls;

namespace Labyrinthus
{
  /// <summary>
  /// Валидатор для полей ввода размеров примитива на странице PagePreparePrimitives
  /// </summary>
  public class PrimitiveSizeValidator : ValidationRule
  {
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
      string sizeStr = value.ToString();
      int size;

      if (string.IsNullOrEmpty(sizeStr))
      {
        return new ValidationResult(false, @"Укажите размеры примитива");
      }
      if (!int.TryParse(sizeStr, out size))
      {
        return new ValidationResult(false, @"Неверный формат числа");
      }
      if (size <= 0)
      {
        return new ValidationResult(false, @"Размер примитива может быть только положительным");
      }

      return new ValidationResult(true, null);
    }
  }
}
