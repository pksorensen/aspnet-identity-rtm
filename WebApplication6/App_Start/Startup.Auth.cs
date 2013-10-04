using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using WebApplication6.Providers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Microsoft.Owin;

namespace WebApplication6
{
    public class CampusDaysUser : IdentityUser
    {
        public CampusDaysUser() { }
        public CampusDaysUser(string userName)
            : base(userName)
        {

        }

        [StringLength(100)]
        public string Email { get; set; }
        [Column(TypeName = "Date")]
        public DateTime? BirthDate { get; set; }
        [StringLength(1)]
        public string Gender { get; set; }
    }


    public class CampusDaysDbContext : IdentityDbContext<CampusDaysUser>
    {
        public CampusDaysDbContext() : base("DefaultConnection") { }


    }


    public partial class Startup
    {
        public const string ExternalCookieAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie;
        public const string ExternalOAuthAuthenticationType = "ExternalToken";

        static Startup()
        {
            PublicClientId = "self";

            // IdentityManagerFactory = new IdentityManagerFactory(IdentityConfig.Settings, () => new CampusDaysIdentityStore(new CampusDaysDbContext()));
            IdentityManagerFactory = new UserManagerFactory<CampusDaysUser>(new UserStoreFactory<CampusDaysUser>());

            CookieOptions = new CookieAuthenticationOptions();

            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp=true,
                TokenEndpointPath = new Microsoft.Owin.PathString("/Token"),
                AuthorizeEndpointPath = new Microsoft.Owin.PathString("/api/Account/ExternalLogin"),
                Provider = new ApplicationOAuthProvider<CampusDaysUser>(PublicClientId, IdentityManagerFactory, CookieOptions)
            };
        }

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static CookieAuthenticationOptions CookieOptions { get; private set; }

        public static UserManagerFactory<CampusDaysUser> IdentityManagerFactory { get; set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use cookies to authenticate users
            app.UseCookieAuthentication(CookieOptions);

            // Enable the application to use a cookie to store temporary information about a user logging in with a third party login provider
            app.UseExternalSignInCookie();


            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);


            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //   clientId: "",
            //   clientSecret: "");


            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            app.UseGoogleAuthentication();
        }
    }
}
