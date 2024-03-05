using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;
using NotNullAttribute = Hangfire.Annotations.NotNullAttribute;

namespace Bookify.Web.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private string _PolicyName;

        public HangfireAuthorizationFilter(string policyName)
        {
            _PolicyName = policyName;
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var authServices = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var isAuthorized = authServices.AuthorizeAsync(httpContext.User, _PolicyName)
                .ConfigureAwait(false).GetAwaiter().GetResult().Succeeded;
            return isAuthorized;
        }
    }
}
