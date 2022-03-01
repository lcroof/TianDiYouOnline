using Functions;
using GameServerCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WebSocketController : ControllerBase
    {
        /// <summary>
        /// 连接WebSocket
        /// </summary>
        /// <returns></returns>
        [HttpPost,Route("PrevConnectWebsocket")]
        public async Task<string> PrevConnectWebsocket()
        {
            Player player = new Player();
            bool isRegsiter = await player.CheckPlayerRegsiter();
            if (!isRegsiter)
            {
                return "未注册";
            }
            if (Helper.HasOnline(player.PlayerId))
            {
                return "失败";
            }
            Helper.PrevConnectServer(player.PlayerId, "nil");
            //ImHelper.JoinChan(CurrentPlayer.Id, "ddz_chan");
            return "成功";
        }

    }
}
