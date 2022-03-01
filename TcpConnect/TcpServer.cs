using DataPublic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpConnection
{
    public class TcpServer
    {
        private Socket socket;
        private List<TcpClient> clientList = new List<TcpClient>();

		public List<TcpClient> ClientList
        {
			get
            {
				return this.clientList;
            }
        }

		public TcpServer(string ip, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            socket.Listen(1000);
			socket.BeginAccept(StartAcceptCallback, this.socket);
		}

        public void RemoveClient(TcpClient client)
        {
            this.clientList.Remove(client);
        }

		private void StartAcceptCallback(IAsyncResult AsyncResult)
		{
			try
			{
				bool isExist = false;
				Socket clientSocket = this.socket.EndAccept(AsyncResult);
                TcpClient client = new TcpClient(clientSocket, this);
                this.clientList.Add(client);
    //            foreach (TcpClient clientRow in this.clientList)
    //            {
                    
				//}
				this.socket.BeginAccept(StartAcceptCallback, this.socket);
			}
			catch (Exception ex)
			{
				Console.WriteLine("无法接收连接：" + ex.Message);
			}
		}

		public void OnRequest(int requestCode, int actionCode, string data, TcpClient client)
		{
            //if (requestCode == (int)RequestCode.Room)
            //{
            //    Rooms room = new Rooms();
            //    string result = room.OnRequest(requestCode, actionCode, data, client, this);
            //}
            //else if (requestCode == RequestCode.User)
            //{
            //	UserController userController = new UserController();
            //	string result = userController.OnRequest(requestCode, actionCode, data, client, this);
            //	if (result != "")
            //	{
            //		SendResponse(requestCode, actionCode, result, client);
            //	}
            //}
            //else if (requestCode == RequestCode.Result)
            //{
            //	ResultController resultController = new ResultController();
            //	string result = resultController.OnRequest(requestCode, actionCode, data, client, this);
            //	if (result != "")
            //	{
            //		SendResponse(requestCode, actionCode, result, client);
            //	}
            //}
            //else if (requestCode == RequestCode.Room)
            //{
            //	RoomController roomController = new RoomController();
            //	string result = roomController.OnRequest(requestCode, actionCode, data, client, this);
            //	if (result != "")
            //	{
            //		SendResponse(requestCode, actionCode, result, client);
            //	}
            //}
        }
    }
}
