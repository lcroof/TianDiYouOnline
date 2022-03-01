using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameServerCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticateService _authService;
        public AuthenticationController(IAuthenticateService authService)
        {
            this._authService = authService;
        }

        /// <summary>
        /// 在通信过程中获取加密RSA公钥
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet, Route("RequestPublicKey")]
        public string RequestPublicKey()
        {
            return Helper.GetRSAPublicKey();
        }

        /// <summary>
        /// 换Token
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet, Route("RequestPublicKey")]
        public ActionResult ReLogin([FromBody] dynamic data)
        {
            string token;
            string oldToken = data.token;
            if (_authService.IsAuthenticated(oldToken, out token))
            {
                return Ok(token);
            }
            return BadRequest("Invalid Request");
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("RequestToken")]
        public ActionResult Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            string token;
            if (_authService.IsAuthenticated(request, out token))
            {
                return Ok(token);
            }

            return BadRequest("Invalid Request");
        }
    }
}