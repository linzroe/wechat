using org.apache.rocketmq.client.consumer;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wechat.Task.SyncMessage.App.Jobs;
using Wechat.Task.SyncMessage.App.MessageListener;
using Wechat.Util.Cache;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.SyncMessage.App.ServiceRunner
{
    public class MessageTopshelf
    {
        DefaultMQPushConsumer consumer = null;

        public MessageTopshelf()
        {
            consumer = RocketMqHelper.CreateDefaultMQPushConsumer<SyncMessageListener>(MqConst.UserSyncMessageCusomerGroup);

        }

        public void Start()
        {

            #region 初始化同步用户信息
            //RedisCache cache = RedisCache.CreateInstance();
            //var customerInfoCaches = cache.HashGetAll<CustomerInfoCache>(ConstCacheKey.GetWxIdKey());
            //var producer = RocketMqHelper.CreateDefaultMQProducer(MqConst.UserSyncMessageProducerGroup);
            //foreach (var item in customerInfoCaches)
            //{
            //    Util.Log.Logger.GetLog<SyncMessageListener>().Info($"初始化微信Id：{item.WxId}");
            //    producer.SendMessage(new org.apache.rocketmq.common.message.Message(MqConst.UserSyncMessageTopic, Encoding.UTF8.GetBytes(item.WxId)));
            //}
            #endregion
            consumer.subscribe(MqConst.UserSyncMessageTopic, "*");

            Util.Log.Logger.GetLog<SyncMessageListener>().Info("启动同步微信消息服务成功");

        }

        public void Stop()
        {
            consumer.shutdown();
        }
    }
}
