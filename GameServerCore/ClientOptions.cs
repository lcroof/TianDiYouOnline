using FreeRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore
{
    public class ClientOptions
    {
        /// <summary>
        /// CSRedis 对象，用于存储数据和发送消息
        /// </summary>
        public RedisClient Redis { get; set; }
        /// <summary>
        /// 负载的服务端
        /// </summary>
        public string[] Servers { get; set; }
        /// <summary>
        /// websocket请求的路径，默认值：/ws
        /// </summary>
        public string PathMatch { get; set; } = "/ws";
    }
}
