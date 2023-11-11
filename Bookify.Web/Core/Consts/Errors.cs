namespace Bookify.Web.Core.Consts
{
    public class Errors
    {
        public const string MaxLength = "Length cannot be more than {1} characters!";
        public const string MaxMinLengh = "The {0} must be at least {2} and at max {1} characters long.";
        public const string Duplicated = "This value is exist!";
        public const string AllowImagesExtensions = "The extentions .jpg , .png and .jpeg are only allowed";
        public const string AllowImagesSize = "Cannot allow size for image more than 2 MG";
        public const string AllowDate = "Date can't be in the future!";
        public const string RequiredField = "Requird field";

        public const string ConfirmPassward = "The password and confirmation password do not match.";
        public const string RegexPassword = "passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least six characters long.";
    }
}
