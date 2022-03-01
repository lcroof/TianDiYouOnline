using DataPublic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerCore
{
    /// <summary>
    /// 核心连接服务端
    /// 继承客户端方法，与客户端进行消息同步和分发
    /// </summary>
    public class Server : Client
    {
        /// <summary>
        /// 客户端连接的服务器
        /// </summary>
        protected string server { get; set; }

        /// <summary>
        /// 字节流大小
        /// </summary>
        const int BufferSize = 4096;

        /// <summary>
        /// 线程安全的客户端实例
        /// </summary>
        ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, ServerClient>> clients = 
            new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, ServerClient>>();

        /// <summary>
        /// 服务器端订阅
        /// </summary>
        /// <param name="options"></param>
        public Server(ServerOptions options) : base(options)
        {
            server = options.Server;
            redis.Subscribe($"{redisPrefix}Server{server}", RedisSubScribleMessage);
        }        

        /// <summary>
        /// 信息接收
        /// 线程安全的接收
        /// </summary>
        /// <param name="context">网络请求内容</param>
        /// <param name="next"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal async Task Acceptor(HttpContext context, Func<Task> next)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                return;
            }

            string token = context.Request.Query["token"];
            if (token.IsNullOrEmpty())
            {
                return;
            }
            var token_value = redis.Get($"{redisPrefix}Token{token}");
            if (token_value.IsNullOrEmpty())
            {
                throw new Exception("授权错误：用户需通过 Helper.PrevConnectServer 获得包含 token 的连接");
            }

            //从Redis中获取的Token进行Json化
            var data = JsonConvert.DeserializeObject<(Guid clientId, string clientMetaData)>(token_value);

            //接收WebSocket异步
            var socket = await context.WebSockets.AcceptWebSocketAsync();

            var cli = new ServerClient(socket, data.clientId);
            var newid = Guid.NewGuid();

            var wslist = clients.GetOrAdd(data.clientId, cliid => new ConcurrentDictionary<Guid, ServerClient>());
            wslist.TryAdd(newid, cli);
            using (var pipe = redis.StartPipe())
            {
                pipe.HIncrBy($"{redisPrefix}Online", data.clientId.ToString(), 1);
                pipe.Publish($"evt_{redisPrefix}Online", token_value);
                pipe.EndPipe();
            }

            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);
            try
            {
                while (socket.State == WebSocketState.Open && clients.ContainsKey(data.clientId))
                {
                    var incoming = await socket.ReceiveAsync(seg, CancellationToken.None);
                    var outgoing = new ArraySegment<byte>(buffer, 0, incoming.Count);
                }
                socket.Abort();
            }
            catch
            {
            }
            wslist.TryRemove(newid, out var oldcli);
            if (wslist.Any() == false) clients.TryRemove(data.clientId, out var oldwslist);
            redis.Eval($"if redis.call('HINCRBY', KEYS[1], '{data.clientId}', '-1') <= 0 then redis.call('HDEL', KEYS[1], '{data.clientId}') end return 1", new[] { $"{redisPrefix}Online" });
            //LeaveChan(data.clientId, GetChanListByClientId(data.clientId));
            redis.Publish($"evt_{redisPrefix}Offline", token_value);
        }

        /// <summary>
        /// Redis消息订阅
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="msg"></param>
        private void RedisSubScribleMessage(string serverName, object msg)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<(Guid senderClientId, Guid[] receiveClientId, string content, bool receipt)>(msg as string);
                Console.WriteLine($"收到消息：{data.content}" + (data.receipt ? "【需回执】" : ""));

                var outgoing = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data.content));
                foreach (var clientId in data.receiveClientId)
                {
                    if (clients.TryGetValue(clientId, out var wslist) == false)
                    {
                        //Console.WriteLine($"websocket{clientId} 离线了，{data.content}" + (data.receipt ? "【需回执】" : ""));
                        if (data.senderClientId != Guid.Empty && clientId != data.senderClientId && data.receipt)
                            SendMessage(clientId, new[] { data.senderClientId }, new
                            {
                                data.content,
                                receipt = "用户不在线"
                            });
                        continue;
                    }

                    ServerClient[] sockarray = wslist.Values.ToArray();

                    //如果接收消息人是发送者，并且接收者只有1个以下，则不发送
                    //只有接收者为多端时，才转发消息通知其他端
                    if (clientId == data.senderClientId && sockarray.Length <= 1) continue;

                    foreach (var sh in sockarray)
                        sh.socket.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);

                    if (data.senderClientId != Guid.Empty && clientId != data.senderClientId && data.receipt)
                        SendMessage(clientId, new[] { data.senderClientId }, new
                        {
                            data.content,
                            receipt = "发送成功"
                        });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"订阅方法出错了：{ex.Message}");
            }
        }

    }

    
}
