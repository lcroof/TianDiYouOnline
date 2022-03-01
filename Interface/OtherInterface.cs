using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface
{
    /// <summary>
    /// 在BinaryClone時自動產生新ID所用
    /// </summary>
    public interface INewID
    {
        void NewID();
    }

    public interface IMainFormEnhance
    {
        /// <summary>
        /// 刷新菜單
        /// </summary>
        void RefreshCustomMenu();
    }

    public interface IMainForm
    {
        event EventHandler LogOut;
        event EventHandler Exit;

        void InitService();
        /// <summary>
        /// 關閉
        /// </summary>
        void Close();
        /// <summary>
        /// 設定狀態
        /// </summary>
        /// <param name="status"></param>
        void SetStatus(string status);
        /// <summary>
        /// 設定進度條
        /// </summary>
        void SetProgressBar(int min, int max, int value);
        /// <summary>
        /// 刷新菜單
        /// </summary>
        void RefreshMenu();
        /// <summary>
        /// 導航到...
        /// </summary>
        /// <param name="url"></param>
        void Navigate(string caption, string url);
        /// <summary>
        /// 顯示錯誤
        /// </summary>
        /// <param name="err"></param>
        void ShowException(Exception err);
    }

    public interface IMonitorMessenger
    {
        string GetSoundFilePath();
    }
}
