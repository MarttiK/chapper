using System;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Linq;

[assembly: OwinStartup(typeof(Chapper.Startup))]
namespace Chapper
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Use Simple Dependency Injectiont to inject the storage client
            GlobalHost.DependencyResolver.Register(
                typeof(ChappHub),
                () => new ChappHub(new StorageClient.StorageClient("chapperstorage", "chapplogs")));

            app.MapSignalR();
        }
    }
}
