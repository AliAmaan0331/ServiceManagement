using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ServiceManagementAPI.Authorization
{
    public class APIRequirement : IAuthorizationRequirement
    {
    }

    public class APIHandler : AuthorizationHandler<APIRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public APIHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, APIRequirement requirement)
        {
            switch (AuthorizeToken(_httpContextAccessor))
            {
                case 0:
                    //DO NOTHING
                    break;
                case 1:
                    throw new UnauthorizedAccessException("Invalid Token");
                //break;

                case 2:

                    throw new UnauthorizedAccessException("Invalid API Key");
                //break;
                case 3:

                    throw new UnauthorizedAccessException("Missing Token");
                //break;
                case 4:

                    throw new UnauthorizedAccessException("Missing API Key");
                //break;
                case 5:

                    throw new UnauthorizedAccessException("Unauthorized");
                //break;
                case 6:

                    throw new UnauthorizedAccessException("Token has been expired");
                //break;
                default:
                    break;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        private int AuthorizeToken(IHttpContextAccessor actionContext)
        {
            string tokenKey = "accesstoken";
            if (!actionContext.HttpContext.Request.Headers.Any(x => x.Key == tokenKey))
            {
                return 3;
            }
            else
            {
                string? token = actionContext.HttpContext.Request.Headers[tokenKey][0];
                var tokenHandler = new JwtSecurityTokenHandler();
                List<Claim>? claims = new List<Claim>();
                var validationParameters = new TokenValidationParameters()
                {
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(""))
                };
                SecurityToken validatedToken;
                try
                {

                    ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                    claims = principal.Claims.ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //Sentry.SentrySdk.CaptureException(ex);
                    claims = null;
                }
                
                if (claims == null)
                {
                    JwtSecurityTokenHandler tokenHandlerJWT = new JwtSecurityTokenHandler();
                    bool IsTokenExpired;
                    try
                    {
                        IsTokenExpired = DateTime.UtcNow >= tokenHandler.ReadJwtToken(token).ValidTo ? true : false;
                    }
                    catch (Exception)
                    {
                        IsTokenExpired = false;
                    }
                    if (IsTokenExpired)
                        return 6;
                    else
                        return 1;
                }
                else
                {
                    //actionContext.HttpContext.Items.Add(new KeyValuePair<object, object?>("USERID", claims.FirstOrDefault(x => x.Type == "USERID")?.Value));
                }
                return 0;

            }
        }
    }
}
