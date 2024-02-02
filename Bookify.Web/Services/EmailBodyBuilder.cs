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

        public string GetEmailBody(string templete, Dictionary<string, string> Placeholders)
        {
            var filePath = $"{webHostEnvironment.WebRootPath}/templates/{templete}.html";
            StreamReader streamReader = new(filePath);
            var templeteContent = streamReader.ReadToEnd();
            streamReader.Close();
            foreach (var placeholder in Placeholders)
                templeteContent = templeteContent.Replace($"[{placeholder.Key}]", placeholder.Value);

            return templeteContent;
        }
    }
}
