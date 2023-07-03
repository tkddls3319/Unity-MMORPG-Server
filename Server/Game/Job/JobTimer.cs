using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Server.Game
{
    struct JobTimerElement : IComparable<JobTimerElement>
    {
        public int exeTick;
        public IJob job;

        public int CompareTo(JobTimerElement other)
        {
            return other.exeTick - exeTick;
        }
    }

    public class JobTimer
    {
        PriorityQueue<JobTimerElement> _pq= new PriorityQueue<JobTimerElement>();
        object _lock = new object();    

        public void Push(IJob job, int tickAfter =0)
        {
            JobTimerElement jobElement;
            jobElement.exeTick = Environment.TickCount + tickAfter;
            jobElement.job = job;

            lock (_lock)
            {
                _pq.Push(jobElement);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = Environment.TickCount;

                JobTimerElement jobElement;

                lock(_lock )
                {
                    if (_pq.Count == 0)
                        break;

                    jobElement = _pq.Peek();
                    if (jobElement.exeTick > now)
                        break;

                    _pq.Pop();
                }
                jobElement.job.Excute();
            }
        }
    }
}
