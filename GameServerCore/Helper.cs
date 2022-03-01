using DataPublic;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerCore
{
    /// <summary>
    /// 静态类，用于实现Core类方法代理
    /// </summary>
    public static class Helper
    {
        private static Client client;

        public static Client Client
        {
            get { return client; }
        }

        public static void Initialize(ClientOptions options)
        {
            client = new Client(options);
        }

        //回发消息
        public static void SendMessage(Guid senderClientId, IEnumerable<Guid> receiveClientId, object message, bool receipt = false)
        {
            client.SendMessage(senderClientId, receiveClientId, message, receipt);
        }

        //返回是否在线
        public static bool HasOnline(Guid clientId)
        {
            return client.HasOnline(clientId);
        }

        public static void ClientOnline(Guid clientId)
        {
            client.Online(clientId);
        }

        //对连接WS的所有Client进行消息广播
        public static void BoardcastMessage()
        {

        }

        //对连接WS的所有Client进行对局状态广播
        public static void BoardcastGamePlay()
        {

        }

        /// <summary>
        /// 连接前的负载、授权，返回 ws 目标地址，使用该地址连接 websocket 服务端
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientMetaData"></param>
        /// <returns></returns>
        public static string PrevConnectServer(Guid clientId, string clientMetaData)
        {
            return client.PrevConnectServer(clientId, clientMetaData);
        }


        /// <summary>
        /// 获取在线用户列表
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Guid> GetClientListByOnline()
        {
            return client.GetClientListByOnline();
        }

        //显示观战人数
        public static decimal ShowObserveMemberCount()
        {
            return 0;
        }

        /// <summary>
        /// 客户端获取RSA公钥
        /// </summary>
        public static string GetRSAPublicKey()
        {
            return client.GetRSAPublicKey();
        }
    }
}
