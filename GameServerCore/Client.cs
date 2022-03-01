using DataPublic;
using FreeRedis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GameServerCore
{
    
    public class Client
    {
        protected RedisClient redis;
        protected string[] servers;
        protected string redisPrefix;
        protected string pathMatch;

        /// <summary>
        /// 推送消息的事件，可审查推向哪个Server节点
        /// </summary>
        public EventHandler<SendEventArgs> OnSend;

        public IEnumerable<Guid> GetClientListByOnline()
        {
            return null;
        }

        public string PrevConnectServer(Guid clientId, string clientMetaData)
        {
            string server = SelectServer(clientId);
            string token = $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");
            redis.Set($"{redisPrefix}Token{token}", JsonConvert.SerializeObject((clientId, clientMetaData)), 30 * 60);
            return $"ws://{server}{pathMatch}?token={token}";
        }

        public Client(ClientOptions options)
        {
            if (options.Redis.IsNullOrEmpty())
            {
                throw new ArgumentException("ClientOptions.Redis 参数不能为空");
            }
            if (!options.Servers.Any())
            {
                throw new ArgumentException("ClientOptions.Servers 参数不能为空");
            }
            redis = options.Redis;
            servers = options.Servers;
            redisPrefix = $"wsim{options.PathMatch.Replace('/', '_')}";
            pathMatch = options.PathMatch ?? "/ws";
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public bool HasOnline(Guid clientId)
        {
            return redis.HGet<int>($"{redisPrefix}Online", clientId.ToString()) > 0;
        }

        /// <summary>
        /// 上线
        /// </summary>
        /// <param name="clientId"></param>
        public void Online(Guid clientId)
        {
            redis.HSet<int>($"{redisPrefix}Online", clientId.ToString(), 1);
        }

        /// <summary>
        /// 下线
        /// </summary>
        /// <param name="clientId"></param>
        public void Offline(Guid clientId)
        {
            redis.HDel($"{redisPrefix}Online", clientId.ToString());
        }

        /// <summary>
        /// 客户端定时从Redis获取RSA公钥进行密文加密，30分钟后过期重新生成
        /// </summary>
        /// <returns></returns>
        public string GetRSAPublicKey()
        {
            if (redis.Ttl("RSAPublicKey") > 0)
            {                
                return redis.Get("RSAPublicKey");
            }
            else
            {
                CryptoDecrypt.Initialize();
                string publicKey = CryptoDecrypt.RSAEncryptRSAPublicKey();
                redis.Set("RSAPublicKey", publicKey, 30 * 60);
                return publicKey;
            }
        }

        /// <summary>
        /// 负载分区规则：取clientId后四位字符，转成10进制数字0-65535，求模
        /// </summary>
        /// <param name="clientId">客户端id</param>
        /// <returns></returns>
        protected string SelectServer(Guid clientId)
        {
            var servers_idx = int.Parse(clientId.ToString("N").Substring(28), NumberStyles.HexNumber) % servers.Length;
            if (servers_idx >= servers.Length) servers_idx = 0;
            return servers[servers_idx];
        }

        /// <summary>
        /// 向指定的多个客户端id发送消息
        /// </summary>
        /// <param name="senderClientId">发送者的客户端id</param>
        /// <param name="receiveClientId">接收者的客户端id</param>
        /// <param name="message">消息</param>
        /// <param name="receipt">是否回执</param>
        public void SendMessage(Guid senderClientId, IEnumerable<Guid> receiveClientId, object message, bool receipt = false)
        {
            receiveClientId = receiveClientId.Distinct().ToArray();
            Dictionary<string, SendEventArgs> redata = new Dictionary<string, SendEventArgs>();

            foreach (var uid in receiveClientId)
            {
                string server = SelectServer(uid);
                if (redata.ContainsKey(server) == false) redata.Add(server, new SendEventArgs(server, senderClientId, message, receipt));
                redata[server].ReceiveClientId.Add(uid);
            }
            var messageJson = JsonConvert.SerializeObject(message);
            foreach (var sendArgs in redata.Values)
            {
                OnSend?.Invoke(this, sendArgs);
                redis.Publish($"{redisPrefix}Server{sendArgs.Server}",
                    JsonConvert.SerializeObject((senderClientId, sendArgs.ReceiveClientId, messageJson, sendArgs.Receipt)));
            }
        }
    }
}
