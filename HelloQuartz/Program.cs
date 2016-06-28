using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace HelloQuartz
{
    public class HelloJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0} Hello Job is executing on thread with managed id: {1}",
                DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("My value is : {0}", context.MergedJobDataMap.GetString("myParam"));
            Console.WriteLine("My value is : {0}", context.MergedJobDataMap.GetString("myParam1"));
            Console.ResetColor();
        }
    }

    class Program
    {
        private static void GetAllJobs(IScheduler scheduler)
        {
            IList<string> jobGroups = scheduler.GetJobGroupNames();
            // IList<string> triggerGroups = scheduler.GetTriggerGroupNames();

            foreach (string group in jobGroups)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys)
                {
                    var detail = scheduler.GetJobDetail(jobKey);
                    var triggers = scheduler.GetTriggersOfJob(jobKey);
                    foreach (ITrigger trigger in triggers)
                    {
                        Console.WriteLine(group);
                        Console.WriteLine(jobKey.Name);
                        Console.WriteLine(detail.Description);
                        Console.WriteLine(trigger.Key.Name);
                        Console.WriteLine(trigger.Key.Group);
                        Console.WriteLine(trigger.GetType().Name);
                        Console.WriteLine(scheduler.GetTriggerState(trigger.Key));
                        DateTimeOffset? nextFireTime = trigger.GetNextFireTimeUtc();
                        if (nextFireTime.HasValue)
                        {
                            Console.WriteLine(nextFireTime.Value.LocalDateTime.ToString());
                        }

                        DateTimeOffset? previousFireTime = trigger.GetPreviousFireTimeUtc();
                        if (previousFireTime.HasValue)
                        {
                            Console.WriteLine(previousFireTime.Value.LocalDateTime.ToString());
                        }
                    }
                }
            }
        }

        private static void Main(string[] args)
        {
            var scheduler = GetScheduler();

            Char key = 'i';
            do
            {
                if (key == 'j')
                    GetJobs(scheduler);
                if (key == 'x')
                {
                    break;
                }
                if (key == 'p')
                    scheduler.PauseAll();
                if (key == 'r')
                    scheduler.ResumeAll();
                if (key == 'u')
                {
                    scheduler.Shutdown();
                    scheduler = null;
                }
                if (key == 'a')
                {
                    scheduler = GetScheduler();
                }
                if (key == 'd')
                {
                    scheduler.DeleteJob(new JobKey("myJob" + (i-1), "myGroup" + (i-1)));
                    --i;
                }
                if (key == 'o')
                {
                    AddJob(scheduler);
                }

                Console.WriteLine();
                Console.WriteLine("Press x to exit");
                Console.WriteLine("Press d to delete job");
                Console.WriteLine("Press j to list jobs");
                Console.WriteLine("Press o to add job");
                Console.WriteLine("Press p to pause");
                Console.WriteLine("Press r (after p) to resume");
                Console.WriteLine("Press u to shutdown");
                Console.WriteLine("Press a (after u) to start");
                key = Console.ReadKey().KeyChar;
            } while (key != 'x');
            Console.WriteLine("done");

            // before exiting, you must set the scheduler to null. the program wont exit if you dont!

            if (scheduler != null)
                scheduler.Shutdown();
            scheduler = null;
        }

        public static List<IJobDetail> GetJobs(IScheduler scheduler)
        {
            List<IJobDetail> jobs = new List<IJobDetail>();

            var jobskeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (JobKey jobKey in jobskeys)
            {
                jobs.Add(scheduler.GetJobDetail(jobKey));
            }
            Console.WriteLine();
            jobs.ForEach(x => Console.WriteLine("Job Detail: {0}-{1}", x.Key.Name, x.Key.Group));
            return jobs;
        }

        private static IScheduler GetScheduler()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = factory.GetScheduler();
            scheduler.Start();

            AddJob(scheduler);

            return scheduler;
        }

        private static int i = 0;
        private static void AddJob(IScheduler scheduler)
        {
            IJobDetail jobInstance = JobBuilder.Create<HelloJob>()
                .WithIdentity("myJob" + (i), "myGroup" + (i))
                .UsingJobData("myParam1", "Henku1")
                .UsingJobData("xxx", "param1")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger" + (i), "myGroup" + (i))
                .StartNow()
                .WithSimpleSchedule(x =>
                    x.WithIntervalInSeconds(1)
                        .RepeatForever()
                        .WithMisfireHandlingInstructionIgnoreMisfires())
                .UsingJobData("myParam", "Henku")
                .Build();

            scheduler.ScheduleJob(jobInstance, trigger);
            ++i;
        }
    }
}

