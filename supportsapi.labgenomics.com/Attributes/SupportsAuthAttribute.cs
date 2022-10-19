using supportsapi.labgenomics.com.Filters;
using supportsapi.labgenomics.com.Services;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace supportsapi.labgenomics.com.Attributes
{
    public class SupportsAuthAttribute : Attribute, IAuthenticationFilter
    {
        public bool AllowMultiple => throw new NotImplementedException();

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            try
            {
                var request = context.Request;
                string authKey = request.Headers.Authorization.Parameter;

                //인증을 받을 수 없는 환경을 위한 발급 토큰
                if (authKey != "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFjaDEyMzAiLCJuYmYiOjE2NTgxMDk4MzgsImV4cCI6MTY1ODExMzQzOCwiaWF0IjoxNjU4MTA5ODM4fQ.2dFrjjZrg4Sp-OtKnzKf4O4lrphVKg94JvXFIeFjXk8")
                {
                    ClaimsPrincipal principal = ManageJwtToken.VerifyToken(authKey);

                    if (!principal.Identity.IsAuthenticated)
                    {
                        context.ErrorResult = new AuthenticationFailureResult("Authentication Failed.", request);
                    }
                }
                return Task.FromResult(0);
            }
            catch
            {
                var request = context.Request;
                context.ErrorResult = new AuthenticationFailureResult("Authentication Failed.", request);
                return Task.FromResult(0);
            }
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}