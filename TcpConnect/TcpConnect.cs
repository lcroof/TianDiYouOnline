using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpConnection

{
    public class TcpConnect
    {
        private Socket socket;
        private TcpConnect instance;
        private decimal userID = 0;

        public TcpConnect Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TcpConnect();
                }
                return instance;
            }
        }

        public decimal UserID
        {
            get
            {
                return userID;
            }
            set
            {
                userID = value;
            }
        }

        public TcpConnect()
        {

        }

        public string Connect(string ip, int port, string data)
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                this.socket.Connect(ip, port);
                this.socket.Send(Encoding.ASCII.GetBytes(data), SocketFlags.None);
                return "连接成功";
            }
            catch
            {
                return "连接失败";
            }
        }
    }
}
