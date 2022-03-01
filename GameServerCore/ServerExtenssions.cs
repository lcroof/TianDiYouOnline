using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace GameServerCore
{
    public static class ServerExtenssions
    {
        static bool isUseWebSockets = false;
        public static IApplicationBuilder UseServer(this IApplicationBuilder app, ServerOptions options)
        {
            app.Map(options.PathMatch, appcur =>
            {
                var imserv = new Server(options);
                if (isUseWebSockets == false)
                {
                    isUseWebSockets = true;
                    appcur.UseWebSockets();
                }
                appcur.Use((ctx, next) =>
                    imserv.Acceptor(ctx, next));
            });
            return app;
        }
    }
}
