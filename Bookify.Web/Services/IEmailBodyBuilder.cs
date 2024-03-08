namespace Bookify.Web.Services
{
	public interface IEmailBodyBuilder
	{
		string GetEmailBody(string templete, Dictionary<string, string> Placeholders);
	}
}
