using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.Net.Http.Headers;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Microsoft.Nnn.Web.Identity
{
    public class LoginHelper
    {
        public static string GetClaim(string token, string claimType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (tokenHandler.ReadToken(token) is JwtSecurityToken securityToken)
            {
                var stringClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
                return stringClaimValue;
            }

            return "";
        }
    }
}