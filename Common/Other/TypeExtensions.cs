namespace PowerUnit.Common.Other;

public static class TypeExtensions
{
    public static string GetFormattedName(this Type type)
    {
        if (type.IsGenericType)
        {
            var genericArguments = type.GetGenericArguments().Select(GetFormattedName).Aggregate((x1, x2) => $"{x1}, {x2}");
            return $"{type.Name[..type.Name.IndexOf('`')]}" + $"<{genericArguments}>";
        }

        return type.Name;
    }
}

