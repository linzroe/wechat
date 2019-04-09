 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Protocol;

namespace Wechat.Task.SyncMessage.App.Models
{
    /// <summary>
    /// 推送消息
    /// </summary>
    public class SyncMessageResult
    {
        public string WxId { get; set; }

        public InitResponse Data { get; set; }
    }
}
