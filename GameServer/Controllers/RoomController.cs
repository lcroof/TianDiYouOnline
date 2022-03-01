using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPublic;
using System.Text.Json;
using Newtonsoft.Json;
using FreeRedis;
using GameServerCore;

namespace GameServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        static RedisClient redis => Startup.Redis;

        [HttpGet, Route("TcpClientStart")]
        public string TcpClientStart()
        {            
            return CryptoDecrypt.RSADecrypt(CryptoDecrypt.RSAEncrypt("Adfasdffasdfasdfasdfasdfasdfasdfadfasdfasdfasdfsdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfas", true), true);
        }

        [HttpPost, Route("TcpClientSend")]
        public string TcpClientSend([FromBody] dynamic data)
        {
            return CryptoDecrypt.AESDecrypt(CryptoDecrypt.AESEncrypt("Adfasdffasdfasdfasdfasdfasdfasdfadfasdfasdfasdfsdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasfddfasdfasdfasasdfasdfasdfasdfasdfasdfasdfas", true), true);
        }
    }
}
