using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;

namespace WebApplication6.Providers
{
    public class ApplicationOAuthProvider<T> : OAuthAuthorizationServerProvider where T : IdentityUser
    {
        private readonly string _publicClientId;
        private readonly UserManagerFactory<T> _identityManagerFactory;
        private readonly CookieAuthenticationOptions _cookieOptions;

        public ApplicationOAuthProvider(string publicClientId, UserManagerFactory<T> identityManagerFactory,
            CookieAuthenticationOptions cookieOptions)
        {
            
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            if (identityManagerFactory == null)
            {
                throw new ArgumentNullException("_identityManagerFactory");
            }

            if (cookieOptions == null)
            {
                throw new ArgumentNullException("cookieOptions");
            }

            _publicClientId = publicClientId;
            _identityManagerFactory = identityManagerFactory;
            _cookieOptions = cookieOptions;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (var identityManager = _identityManagerFactory.Create())
            {
                var user = await identityManager.FindAsync(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }

                string userId = user.Id; // await identityManager.Logins.GetUserIdForLocalLoginAsync(context.UserName);
                
             //   IEnumerable<Claim> claims = await identityManager.GetClaimsAsync(userId);
                ClaimsIdentity oAuthIdentity = await identityManager.CreateIdentityAsync(user, context.Options.AuthenticationType);  
                ClaimsIdentity cookiesIdentity = await identityManager.CreateIdentityAsync(user, _cookieOptions.AuthenticationType);

                AuthenticationProperties properties = CreatePropertiesAsync(user);
                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
                context.Validated(ticket);
                context.Request.Context.Authentication.SignIn(cookiesIdentity);
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                if (RequestExtensions.IsUrlLocalToHost(null, context.RedirectUri))
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        //public static ClaimsIdentity CreateIdentity(IdentityManager identityManager, IEnumerable<Claim> claims,
        //    string authenticationType)
        //{
        //    if (identityManager == null)
        //    {
        //        throw new ArgumentNullException("identityManager");
        //    }

        //    if (claims == null)
        //    {
        //        throw new ArgumentNullException("claims");
        //    }

        //    IdentityAuthenticationOptions options = identityManager.Settings.GetAuthenticationOptions();
        //    return new ClaimsIdentity(claims, authenticationType, options.UserNameClaimType, options.RoleClaimType);
        //}

        public static AuthenticationProperties CreatePropertiesAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

          
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", user.UserName }
            };
            return new AuthenticationProperties(data);
        }

        //public static Task<IList<Claim>> GetClaimsAsync(UserManager<T> identityManager, string userId)
        //{
        //    if (identityManager == null)
        //    {
        //        throw new ArgumentNullException("identityManager");
        //    }
        //    identityManager.GetClaimsAsync()
        //    AuthenticationManager authenticationManager = new AuthenticationManager(
        //        identityManager.Settings.GetAuthenticationOptions(), identityManager);

        //    return authenticationManager.GetUserIdentityClaimsAsync(userId, new Claim[0], CancellationToken.None);
        //}
    }
}