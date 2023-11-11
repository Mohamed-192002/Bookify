namespace Bookify.Web.Core.Consts
{
    public static class RegexPatterns
    {
        public const string Passward = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}";
    }
}
