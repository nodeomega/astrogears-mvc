using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AstroGearsMVC.Startup))]
namespace AstroGearsMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
