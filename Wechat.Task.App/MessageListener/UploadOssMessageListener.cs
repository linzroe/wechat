using java.util;
using org.apache.rocketmq.client.consumer.listener;
using System;
using System.Text;
using Wechat.Protocol;
using Wechat.Util;
using Wechat.Util.Cache;
using Wechat.Util.Extensions;
using Wechat.Util.FileStore;
namespace Wechat.Task.App.MessageListener
{
    /// <summary>
    /// 上传oss
    /// </summary>
    public class UploadOssMessageListener : MessageListenerConcurrently
    {

        public ConsumeConcurrentlyStatus consumeMessage(java.util.List list, ConsumeConcurrentlyContext ccc)
        {
            Iterator iterator = list.iterator();
            while (iterator.hasNext())
            {
                try
                {
                    var messageClientExt = iterator.next() as org.apache.rocketmq.common.message.MessageClientExt;

                    var content = Encoding.UTF8.GetString(messageClientExt.getBody());

                    UploadFileObj uploadFileObj = content.ToObj<UploadFileObj>();
         
                    string objName = null;
                    string mchId = RedisCache.CreateInstance().Get(ConstCacheKey.GetMchIdKey(uploadFileObj.WxId));
                    if (string.IsNullOrEmpty(mchId))
                    {
                        throw new Exception("未初始化商户Ip");
                    }
                    WechatHelper wechatHelper = new WechatHelper();
                    //图片
                    if (uploadFileObj.MsgType == 3)
                    {
                        byte[] buffer = wechatHelper.GetMsgBigImg(uploadFileObj.LongDataLength, uploadFileObj.MsgId, uploadFileObj.WxId, uploadFileObj.ToWxId, 0, (int)uploadFileObj.LongDataLength);
                        if (buffer != null)
                        {
                            objName = FileStorageHelper.GetObjectName(mchId);
                            FileStorageHelper.Upload(buffer, $"{objName}{uploadFileObj.MsgId}.png");
                        }
                    }
                    //语音
                    else if (uploadFileObj.MsgType == 34)
                    {

                        if (uploadFileObj.Buffer != null)
                        {
                            objName = FileStorageHelper.GetObjectName(mchId);
                            FileStorageHelper.Upload(uploadFileObj.Buffer, $"{objName}{uploadFileObj.MsgId}.silk");
                        }


                    }
                    //视频
                    else if (uploadFileObj.MsgType == 43)
                    {
                        byte[] buffer = wechatHelper.GetVideo(uploadFileObj.WxId, uploadFileObj.ToWxId, uploadFileObj.MsgId, uploadFileObj.LongDataLength, 0, (int)uploadFileObj.LongDataLength);

                        if (buffer != null)
                        {
                            objName = FileStorageHelper.GetObjectName(mchId);
                            FileStorageHelper.Upload(buffer, $"{objName}{uploadFileObj.MsgId}.mp4");
                        }


                    }
                    //Util.Log.Logger.GetLog<UploadOssMessageListener>().Info($"消费Id:{messageClientExt.getMsgId()}--数据：{content}");
                }
                catch (Exception ex)
                {
                    Util.Log.Logger.GetLog<UploadOssMessageListener>().Error(ex);
                    return ConsumeConcurrentlyStatus.RECONSUME_LATER;
                }

            }
            return ConsumeConcurrentlyStatus.CONSUME_SUCCESS;
        }
    }
}
