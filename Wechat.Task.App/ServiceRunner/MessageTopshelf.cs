using org.apache.rocketmq.client.consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.App.MessageListener;
using Wechat.Util.Mq;

namespace Wechat.Task.App.ServiceRunner
{
    public class MessageTopshelf
    {
        DefaultMQPushConsumer consumer = null;

        public MessageTopshelf()
        {
            consumer = RocketMqHelper.CreateDefaultMQPushConsumer<UploadOssMessageListener>(MqConst.UploadOssCusomerGroup);

        }

        public void Start()
        {
            consumer.subscribe(MqConst.UploadOssTopic, "*");
            Util.Log.Logger.GetLog<UploadOssMessageListener>().Info("上传oss文件服务启动成功");

        }

        public void Stop()
        {
            consumer.shutdown();
        }
    }
}
