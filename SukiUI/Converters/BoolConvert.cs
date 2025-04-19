using Avalonia.Data.Converters;

namespace SukiUI.Converters;

public static class BoolConvert
{
    public static readonly IValueConverter NotFalse =
        new FuncValueConverter<bool?, bool>(b => b != false);
}