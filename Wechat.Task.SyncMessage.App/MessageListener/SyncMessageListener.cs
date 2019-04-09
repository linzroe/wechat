using java.util;
using org.apache.rocketmq.client.consumer.listener;
using org.apache.rocketmq.client.producer;
using org.apache.rocketmq.common.message;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Wechat.Protocol;
using Wechat.Task.SyncMessage.App.Models;
using Wechat.Util.Cache;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
using Wechat.Util.Mq;

namespace Wechat.Task.SyncMessage.App.MessageListener
{
    public class SyncMessageListener : MessageListenerConcurrently
    {
        private WechatHelper wechatHelper = new WechatHelper();
        private RedisCache cache = RedisCache.CreateInstance();
        private DefaultMQProducer producer = RocketMqHelper.CreateDefaultMQProducer(MqConst.SyncMessageProducerGroup);
        private DefaultMQProducer userproducer = RocketMqHelper.CreateDefaultMQProducer(MqConst.UserSyncMessageProducerGroup);

        private DefaultMQProducer offlineproducer = RocketMqHelper.CreateDefaultMQProducer(MqConst.UserOfflineStatusProducerGroup);

        private int maxCount = 5;
        private static ConcurrentDictionary<string, int> Dic = new ConcurrentDictionary<string, int>();
        public ConsumeConcurrentlyStatus consumeMessage(List list, ConsumeConcurrentlyContext ccc)
        {
            Iterator iterator = list.iterator();
            while (iterator.hasNext())
            {
                string wxId = null;
                byte[] buffer = null;
                try
                {
                    var messageClientExt = iterator.next() as org.apache.rocketmq.common.message.MessageClientExt;
                    buffer = messageClientExt.getBody();
                    wxId = Encoding.UTF8.GetString(buffer);

                    var result = wechatHelper.SyncInit(wxId);

                    if (result.ModUserInfos?.Count > 0 || result.AddMsgs?.Count > 0 || result.DelContacts?.Count > 0 || result.AddMsgs?.Count > 0)
                    {
                        SyncMessageResult syncMessageResult = new SyncMessageResult();
                        syncMessageResult.WxId = wxId;
                        syncMessageResult.Data = result;
                        var resultJson = syncMessageResult.ToJson();
                        var dataBuffer = Encoding.UTF8.GetBytes(resultJson);
                        Message message = new Message(MqConst.SyncMessageTopic, dataBuffer);
                        var sendResult = producer.SendMessage(message);
                    }
                    OfflineStatus offlineStatus = new OfflineStatus()
                    {
                        WxId = wxId,
                        Status = 0
                    };
                    offlineproducer.SendMessage(new Message(MqConst.UserOfflineStatusTopic, Encoding.UTF8.GetBytes(offlineStatus.ToJson())));

                    var userMessage = new Message(MqConst.UserSyncMessageTopic, buffer);
                    userMessage.setDelayTimeLevel(2);           //2表示5秒
                    userproducer.SendMessage(userMessage);
                    Util.Log.Logger.GetLog<SyncMessageListener>().Info(wxId);
                }
                catch (ExpiredException ex)
                {
                    if (!string.IsNullOrEmpty(wxId))
                    {
                        OfflineStatus offlineStatus = new OfflineStatus()
                        {
                            WxId = wxId,
                            Status = 2
                        };
                        offlineproducer.SendMessage(new Message(MqConst.UserOfflineStatusTopic, Encoding.UTF8.GetBytes(offlineStatus.ToJson())));
                    }
                    if (Dic.ContainsKey(wxId))
                    {
                        if (Dic[wxId] < maxCount)
                        {
                            Dic[wxId]++;
                            userproducer.SendMessage(new Message(MqConst.UserSyncMessageTopic, buffer));
                        }
                        else
                        {
                            Dic[wxId] = 0;
                        }
                    }
                    else
                    {
                        Dic.TryAdd(wxId, 1);
                        userproducer.SendMessage(new Message(MqConst.UserSyncMessageTopic, buffer));
                    }

                    Util.Log.Logger.GetLog<SyncMessageListener>().Error($"{wxId}---重试次数:{Dic[wxId]}\r\n", ex);


                }
                catch (Exception ex)
                {
                    userproducer.SendMessage(new Message(MqConst.UserSyncMessageTopic, buffer));
                    Util.Log.Logger.GetLog<SyncMessageListener>().Error(wxId, ex);
                }
            }
            return ConsumeConcurrentlyStatus.CONSUME_SUCCESS;
        }
    }

    public class OfflineStatus
    {
        /// <summary>
        /// 微信ID
        /// </summary>
        public string WxId { get; set; }

        /// <summary>
        /// 0 在线 2：下线
        /// </summary>
        public int Status { get; set; }
    }
}

