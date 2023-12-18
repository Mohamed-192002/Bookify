namespace Bookify.Web.Core.Consts
{
    public static class RegexPatterns
    {
        public const string Passward = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}";
        public const string phoneRegex = "^(\\+201|01|00201)[0-2,5]{1}[0-9]{8}";
    }
}
