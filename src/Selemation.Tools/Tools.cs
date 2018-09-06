namespace Selemation.Tools
{
    public class Tools
    {
        public static bool UrlIsSame(string url1, string url2)
        {
            return url1.Trim(' ', '/', '\\').Equals(url2.Trim(' ', '/', '\\'));
        }
    }
}
