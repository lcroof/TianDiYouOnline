using FreeRedis;
using Functions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    
    /// <summary>
    /// 登录请求数据传送处理对象
    /// </summary>
    public class LoginRequestDTO
    {
        [Required]
        [JsonProperty("username")]
        public string Username { get; set; }


        [Required]
        [JsonProperty("password")]
        public string Password { get; set; }
    }

    /// <summary>
    /// 认证请求服务接口
    /// </summary>
    public interface IAuthenticateService
    {
        bool IsAuthenticated(LoginRequestDTO request, out string token);
        bool IsAuthenticated(string token, out string newToken);
    }

    /// <summary>
    /// Token认证请求服务
    /// </summary>
    public class TokenAuthenticationService : IAuthenticateService
    {
        private readonly IUserService _userService;
        private readonly TokenManagement _tokenManagement;
        static RedisClient redis => Startup.Redis;

        /// <summary>
        /// Token认证请求服务方法
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="tokenManagement"></param>
        public TokenAuthenticationService(IUserService userService, IOptions<TokenManagement> tokenManagement)
        {
            _userService = userService;
            _tokenManagement = tokenManagement.Value;
        }

        /// <summary>
        /// 判断是否已认证，返回Token
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <param name="token">Token</param>
        /// <returns></returns>
        public bool IsAuthenticated(LoginRequestDTO request, out string token)
        {
            token = string.Empty;
            if (!_userService.IsValid(request))
            {
                return false;
            }

            //进行数据库验证
            Player player = new Player();
            player.PlayerLogin(request.Username, request.Password);

            //确认无误后发放Token
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, player.PlayerName)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(_tokenManagement.Issuer, _tokenManagement.Audience, claims,
                expires: DateTime.Now.AddMinutes(_tokenManagement.AccessExpiration),
                signingCredentials: credentials);
            token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            redis.HSet("ClientLoginToken", token, player.PlayerName);
            return true;
        }

        /// <summary>
        /// 换Token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="newToken"></param>
        /// <returns></returns>
        public bool IsAuthenticated(string token, out string newToken)
        {
            newToken = string.Empty;

            string userName = redis.HGet("ClientLoginToken", token);
            if (string.IsNullOrEmpty(userName))
            {
                return false;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(_tokenManagement.Issuer, _tokenManagement.Audience, claims,
                expires: DateTime.Now.AddMinutes(_tokenManagement.AccessExpiration),
                signingCredentials: credentials);
            newToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            redis.HSet("ClientLoginToken", token, userName);
            return true;
        }
    }
}
