using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Wechat.Task.SyncMessage.App.ServiceRunner;
namespace Wechat.Task.SyncMessage.App
{
    class Program
    {
        static void Main(string[] args)
        {
          
            HostFactory.Run(x =>
            {
                x.RunAsLocalSystem();
                x.SetDescription("执行消息队列服务，同步微信消息");
                x.SetDisplayName("执行消息队列服务，同步微信消息");
                x.SetServiceName("SyncWechatMessage");
                Util.Log.Logger.GetLog<Program>().Info("服务启动成功");
                x.Service<MessageTopshelf>(s =>
                {
                    s.ConstructUsing(name => new MessageTopshelf());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
            });




            Console.ReadLine();
        }
    }
}
