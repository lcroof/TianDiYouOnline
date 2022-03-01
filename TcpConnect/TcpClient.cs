using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpConnection
{
    public class TcpClient
    {
        TcpServer tcpServer;
        Socket socket;
		decimal userid;
        byte[] _buffer = new byte[1024 * 1024];

		public Socket Socket
        {
			get
            {
				return this.socket;
            }
        }

		public decimal UserID
        {
			get
            {
				return this.userid;
            }
        }

        public TcpClient(Socket socket, TcpServer server)
        {
            this.socket = socket;
            this.tcpServer = server;
			this.Start();
        }

        private void Start()
        {
            this.socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, StartReceiveCallback, this.socket);
        }

		private void StartReceiveCallback(IAsyncResult AsyncResult)
		{
			try
			{
				int length = this.socket.EndReceive(AsyncResult);
				if (length == 0)
				{
					Close();
				}
				else if (length > 0)
				{
					string str = Encoding.UTF8.GetString(_buffer, 0, length);
					var strs = str.Split('|');
					int requestCode = int.Parse(strs[0]);
					int actionCode = int.Parse(strs[1]);
					string data = strs[2];
					this.tcpServer.OnRequest(requestCode, actionCode, data, this);
				}
				Start();
			}
			catch (Exception ex)
			{
				Console.WriteLine("无法接收消息：" + ex.Message);
				Close();
			}
		}

		public void Close()
		{
			try
			{
				if (this.socket != null)
				{
					this.socket.Close();
				}
				this.tcpServer.RemoveClient(this);
			}
			catch (Exception ex)
			{
				Console.WriteLine("无法关闭连接：" + ex.Message);
			}
		}

		/// <summary>
		/// 客户端给服务端发送信息
		/// </summary>
		/// <param name="data"></param>
		public void Send(int requestCode, int actionCode, string data)
		{
			try
			{
				byte[] buffer = Encoding.UTF8.GetBytes(requestCode + "|" + actionCode + "|" + data);
				this.socket.Send(buffer);
			}
			catch (Exception ex)
			{
				Console.WriteLine("无法接收连接：" + ex.Message);
			}
		}
	}
}
