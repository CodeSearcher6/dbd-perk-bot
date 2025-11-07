namespace DBDPerkBot.Utils;

public static class IconNameHelper
{
    public static string Normalize(string s) =>
        s.ToLower()
         .Replace("iconperks", "")
         .Replace("iconsperks", "")
         .Replace("iconperk", "")
         .Replace("iconsperk", "")
         .Replace("t_ui_iconperks", "")
         .Replace("t_iconperks", "")
         .Replace("t_ui_iconsperks", "")
         .Replace("t_iconsperks", "")
         .Replace("_", "")
         .Replace("-", "")
         .Replace(" ", "")
         .Replace(".png", "")
         .Replace(".jpg", "")
         .Replace(".jpeg", "")
         .Trim();
}
