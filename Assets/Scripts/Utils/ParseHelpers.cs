using System.Globalization;
using UnityEngine;


public static class ParseHelpers
{
    public static bool TryParseVector2(string s, out Vector2 result)
    {
        result = Vector2.zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        s = s.Trim();
        if (s.Length < 2 || s[0] != '(' || s[s.Length - 1] != ')')
            return false;

        s = s.Substring(1, s.Length - 2);

        int commaIndex = s.IndexOf(',');
        if (commaIndex < 0)
            return false;

        string xPart = s.Substring(0, commaIndex).Trim();
        string yPart = s.Substring(commaIndex + 1).Trim();

        if (!float.TryParse(xPart, NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
            return false;

        if (!float.TryParse(yPart, NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            return false;

        result = new Vector2(x, y);
        return true;
    }
}