using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore
{
    public class ServerOptions : ClientOptions
    {
        /// <summary>
        /// 设置服务名称，它应该是 servers 内的一个
        /// </summary>
        public string Server { get; set; }
    }
}
