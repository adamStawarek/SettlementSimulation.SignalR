using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using SettlementSimulation.Server;
using System.Web.Http;

[assembly: OwinStartup(typeof(Startup))]
namespace SettlementSimulation.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { id = RouteParameter.Optional }
            );
            app.UseCors(CorsOptions.AllowAll);
            app.Map("/signalr", map =>
            {
                map.RunSignalR();
            });
            app.UseWebApi(config);
        }
    }
}
