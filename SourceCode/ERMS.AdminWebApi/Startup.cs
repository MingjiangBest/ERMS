using System;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.ServiceLocation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(ERMS.AdminWebApi.Startup))]

namespace ERMS.AdminWebApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var paras = new ServiceParas(ConnectStringProvider.DbType, ConnectStringProvider.CentralServerConnectString);
            var _onlineService = new OnlineUserService(paras);
            GlobalHost.DependencyResolver.Register(typeof(CacheHub), () => new CacheHub(_onlineService));


            // Branch the pipeline here for requests that start with "/signalr"
            app.Map("/signalr", map =>
            {
                // Setup the CORS middleware to run before SignalR.
                // By default this will allow all origins. You can 
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting line below.
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    //EnableJSONP = true
                };
                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch already runs under the "/signalr"
                // path.
                map.RunSignalR(hubConfiguration);
            });

            ConfigureAuthentication(app);
        }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuthentication(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                CookieName = "Atms.WebApiToken",
                SlidingExpiration = true
            });

            app.Use(async (context, next) =>
            {
                IOwinRequest req = context.Request;
                IOwinResponse res = context.Response;
                // for auth2 token requests
                if (req.Path.StartsWithSegments(new PathString("/Token")))
                {
                    // if there is an origin header
                    var origin = req.Headers.Get("Origin");
                    if (!string.IsNullOrEmpty(origin))
                    {
                        // allow the cross-site request
                        res.Headers.Set("Access-Control-Allow-Origin", origin);
                    }

                    // if this is pre-flight request
                    if (req.Method == "OPTIONS")
                    {
                        // respond immediately with allowed request methods and headers
                        res.StatusCode = 200;
                        res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Methods", "GET", "POST");
                        res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Headers", "authorization", "content-type");
                        // no further processing
                        return;
                    }
                }
                // continue executing pipeline
                await next();
            });

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = ServiceLocator.Current.GetInstance<SimpleAuthorizationServerProvider>(),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                AllowInsecureHttp = true,
            });
        }
    }
}
