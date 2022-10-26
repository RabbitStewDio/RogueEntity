namespace RogueEntity.Core.Tests;

public static class TextUtils
{
    public static string NormalizeMultilineText(this string ml)
    {
        return ml.Trim().Replace("\r\n", "\n");
    }
}