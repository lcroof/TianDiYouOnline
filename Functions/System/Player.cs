using DataPublic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class Player
    {
        private decimal id;
        private string playerId;
        private string playerName;
        private bool isExpired;

        public Guid PlayerId
        {
            get
            {
                return Guid.Parse(playerId);
            }
        }


        public string PlayerName
        {
            get
            {
                return this.playerName;
            }
        }

        public bool IsExpired
        {
            get
            {
                return this.isExpired;
            }
        }

        public Player()
        {
            this.playerId = string.Empty;
            this.playerName = string.Empty;
        }


        public void PlayerLogin(string userName, string password)
        {
            //检查Redis是否存在，如果存在直接删除
            //检查
        }

        public Task<bool> CheckPlayerRegsiter()
        {
            if (this.playerId.IsNullOrEmpty())
            {
                this.playerId = "lcroof";
            }
            return Task.FromResult(true);
        }

        public async Task<bool> CheckPlayerInDesk()
        {
            bool isRegsiter = await this.CheckPlayerRegsiter();
            if (!isRegsiter)
            {
                return false;
            }
            return true;
        }
    }
}
