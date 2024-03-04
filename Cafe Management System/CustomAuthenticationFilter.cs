using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using System.Web.Http;


/*This filter can be applied to specific controllers or action methods using the [CustomAuthenticationFilter] attribute, 
 * 
and it will be executed during the request processing pipeline to handle authentication and challenges.*/

namespace Cafe_Management_System
{
    public class CustomAuthenticationFilter : AuthorizeAttribute, IAuthenticationFilter
    {
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;
            if (authorization == null || authorization.Scheme != "Bearer" || string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult();
                return;
            }
            context.Principal = TokenManager.GetPrincipal(authorization.Parameter);
        }


        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var result = await context.Result.ExecuteAsync(cancellationToken);
            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                result.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Basic", "real=localhost"));
            }
            context.Result = new ResponseMessageResult(result);
        }
    }
    public class AuthenticationFailureResult : IHttpActionResult
    {
        public AuthenticationFailureResult() { }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            return Task.FromResult(responseMessage);
        }
    }
}