using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Bookify.Web.Helpers
{
    public class ApplicationUserClaimsPrincipleFactory : UserClaimsPrincipalFactory<ApplicationUsers, IdentityRole>
    {
        public ApplicationUserClaimsPrincipleFactory
            (UserManager<ApplicationUsers> userManager
            , RoleManager<IdentityRole> roleManager
            , IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUsers user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FullName));
            return identity;
        }
    }
}
