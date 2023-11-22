using Microsoft.AspNetCore.Hosting;
using System.Text.Encodings.Web;

namespace Bookify.Web.Services
{
	public class EmailBodyBuilder : IEmailBodyBuilder
	{
		private readonly IWebHostEnvironment webHostEnvironment;

		public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
		{
			this.webHostEnvironment = webHostEnvironment;
		}

		public string GetEmailBody(string imageUrl, string header, string body, string url, string linkTitle)
		{
			var filePath = $"{webHostEnvironment.WebRootPath}/templates/email.html";
			StreamReader streamReader = new(filePath);
			var templete = streamReader.ReadToEnd();
			streamReader.Close();
			templete = templete
				.Replace("[imageUrl]", imageUrl)
				.Replace("[header]", header)
				.Replace("[body]", body)
				.Replace("[url]", url)
				.Replace("[linkTitle]", linkTitle);

			return templete;
		}
	}
}
