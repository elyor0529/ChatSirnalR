using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Demo.ChatSirnalR.Startup))]
namespace Demo.ChatSirnalR
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}
