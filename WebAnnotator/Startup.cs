using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebAnnotator.Startup))]
namespace WebAnnotator
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
