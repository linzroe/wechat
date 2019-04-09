using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wechat.Util.TaskServer
{
    public class QuartzHelper
    {
        private static IScheduler scheduler = null;
        public static async Task CreateScheduler()
        {

            //从工厂中获取一个调度器实例化
            scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            //开启调度器
            await scheduler.Start();     
        }

        /// <summary>
        /// 注册作业
        /// </summary>
        /// <typeparam name="TJob"></typeparam>
        /// <param name="cronExpression">/5 * * ? * *</param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static async Task RegisterJob<TJob>(string cronExpression, string group) where TJob : IJob
        {
        
            // 创建一个作业
            IJobDetail job = JobBuilder.Create<TJob>().WithIdentity(typeof(TJob).Name, group).Build();


            ITrigger trigger = TriggerBuilder.Create()
             .WithIdentity(typeof(TJob).Name, group)
             .StartAt(DateTimeOffset.Now.AddSeconds(30))
             .WithCronSchedule(cronExpression).Build();

            //把作业，触发器加入调度器。
            await scheduler.ScheduleJob(job, trigger);    
        }
    }
}
