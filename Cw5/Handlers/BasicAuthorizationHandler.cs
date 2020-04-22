using Cw5.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Cw5.Handlers
{
    public class BasicAuthorizationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {

         IStudentDbService service;
        public BasicAuthorizationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IStudentDbService service
            ) : base(options, logger, encoder, clock)
        {
            this.service = service;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(" Authorization "))
                return AuthenticateResult.Fail(" Wymagany nagłówek Authorization! ");

            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialsBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialsBytes).Split(":");
            //credentials[0]- login ; credentials[1]- pass;
            if (credentials.Length != 2)
                return AuthenticateResult.Fail("Złe dane");
            if (!service.CheckCredential(credentials[0], credentials[1]))
                return AuthenticateResult.Fail("Zły login lub hasło");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,"1"),
                new Claim(ClaimTypes.Name,"StudentName"),
                new Claim(ClaimTypes.Role,"admin"),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name); 
            return AuthenticateResult.Success(null);
        }

    }
}
