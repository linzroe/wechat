using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Wechat.Task.App.MessageListener;
using Wechat.Task.App.ServiceRunner;

namespace Wechat.Task.App
{
    class Program
    {
        static void Main(string[] args)
        {

            HostFactory.Run(x =>
            {
                x.RunAsLocalSystem();
                x.SetDescription("执行消息队列服务，消费同步消息上传oss文件的服务");
                x.SetDisplayName("执行消息队列服务，消费同步消息上传oss文件的服务");
                x.SetServiceName("UploadOssService");
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
