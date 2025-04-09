namespace SukiUI.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Gets all the values of an enum type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="orderByName">True to order by enum name, otherwise it will order was defined on enum.</param>
    /// <returns></returns>
    public static IEnumerable<T> GetValues<T>(bool orderByName = false) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        return orderByName
            ? values.OrderBy(e => e.ToString(), StringComparer.Ordinal)
            : values;
    }

    /// <summary>
    /// Gets all the set flags of an enum value(s).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="flags"></param>
    /// <returns></returns>
    /// <remarks>For enums with <see cref="FlagsAttribute"/>.<br/>
    /// If you have an enum value set to 0 it will always return. (Filter it before or after calling this function)<br/>
    /// If you have negative value it can return all flags as undesired effect.</remarks>
    public static IEnumerable<T> GetSetFlags<T>(this T flags) where T : Enum
    {
        var flagsInt64 = Convert.ToInt64(flags);

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagInt64 = Convert.ToInt64(flag);
            if ((flagsInt64 & flagInt64) == flagInt64)
            {
                yield return flag;
            }
        }
    }

    /// <summary>
    /// Gets all the set flags of an enum value(s).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="flags"></param>
    /// <param name="ignoreFlag">Set a flag to exclude from returning, eg: If you want to ignore or remove an always return 0 flag.</param>
    /// <returns></returns>
    /// <remarks>For enums with <see cref="FlagsAttribute"/>.<br/>
    /// If you have an enum value set to 0 it will always return. (Filter it before or after calling this function)<br/>
    /// If you have negative value it can return all flags as undesired effect.</remarks>
    public static IEnumerable<T> GetSetFlagsIgnoring<T>(this T flags, T ignoreFlag) where T : Enum
    {
        var flagsInt64 = Convert.ToInt64(flags);
        var ignoreFlagInt64 = Convert.ToInt64(ignoreFlag);

        flagsInt64 &= ~ignoreFlagInt64;

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagInt64 = Convert.ToInt64(flag);
            if ((flagsInt64 & flagInt64) == flagInt64)
            {
                yield return flag;
            }
        }
    }

    /// <summary>
    /// Gets all the set flags of an enum value(s).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="flags"></param>
    /// <param name="ignoreFlags">Set flag(s) to exclude from returning, eg: If you want to ignore or remove an always return 0 flag.</param>
    /// <returns></returns>
    /// <remarks>For enums with <see cref="FlagsAttribute"/>.<br/>
    /// If you have an enum value set to 0 it will always return. (Filter it before or after calling this function)<br/>
    /// If you have negative value it can return all flags as undesired effect.</remarks>
    public static IEnumerable<T> GetSetFlagsIgnoring<T>(this T flags, params IEnumerable<T> ignoreFlags) where T : Enum
    {
        var flagsInt64 = Convert.ToInt64(flags);

        foreach (var ignoreFlag in ignoreFlags)
        {
            var ignoreFlagInt64 = Convert.ToInt64(ignoreFlag);
            flagsInt64 &= ~ignoreFlagInt64;
        }

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            var flagInt64 = Convert.ToInt64(flag);
            if ((flagsInt64 & flagInt64) == flagInt64)
            {
                yield return flag;
            }
        }
    }
}