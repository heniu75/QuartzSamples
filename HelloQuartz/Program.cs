using System;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;

namespace HelloQuartz
{
    public class HelloJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0} Hello Job is executing on thread with managed id: {1}", 
                DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId);
            //Console.ForegroundColor = ConsoleColor.Gray;
            //try
            //{
            //    var contextJson = JsonConvert.SerializeObject(context, Formatting.Indented);
            //    Console.WriteLine(contextJson);
            //}
            //catch (Exception exc)
            //{
            //    Console.WriteLine(exc);
            //}
            Console.ResetColor();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = factory.GetScheduler();
            scheduler.Start();

            IJobDetail jobInstance = JobBuilder.Create<HelloJob>()
                .WithIdentity("myJob", "group1")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x =>
                    x.WithIntervalInSeconds(10)
                        .RepeatForever())
                .Build();

            scheduler.ScheduleJob(jobInstance, trigger);
            Char key = 'i';
            do
            {
                if (key == 'x')
                    break;
                if (key == 'p')
                    scheduler.PauseAll();
                if (key == 'r')
                    scheduler.ResumeAll();

                Console.WriteLine();
                Console.WriteLine("Press x to exit");
                Console.WriteLine("Press p to pause");
                Console.WriteLine("Press r to resume");
                key = Console.ReadKey().KeyChar;
            } while (key != 'x');

        }
    }
}

