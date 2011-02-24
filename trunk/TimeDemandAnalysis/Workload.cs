using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TimeDemandAnalysis
{
    class Workload
    {
        SortedList<int, TaskType> tasks;
        int hyperPeriod;
        int maxPeriod;
        int minPeriod;
        string workLoadName;

        public Workload(string name = "My Workload")
        {
            tasks = new SortedList<int, TaskType>();
            tasks.Clear();
            hyperPeriod = 0;
            maxPeriod = 0;
            minPeriod = 0;
            workLoadName = name;
        }

        public void addTask(TaskType t)
        {
            tasks.Add(t.getPeriod(), t);
        }

        public List<TaskType> getTasks()
        {
            List<TaskType> taskList = new List<TaskType>();
            taskList.AddRange(tasks.Values);
            return taskList;
        }
        
        public int getHyperPeriod()
        {
            return hyperPeriod;
        }
        private void setHyperPeriod(int hPeriod)
        {
            hyperPeriod = hPeriod;
        }
        internal void performTimeDemandAnalysis()
        {
            Console.WriteLine("Performing Time Demand Analysis");
            int i = 1; //task i
            double[] wi = new double[getHyperPeriod()];  //total demand Wi(t)
            InitDoubleArray(wi);
            int t = 1; //time
            int k = 1; //lower priority task k
            int maxBlockingTime = 0;

            //Start with Task 1 (has the smallest period - that is highest priority) 
            for (i = 1; i <= tasks.Count(); i++)
            {
                TaskType iTask = (TaskType)tasks.Values.ElementAt(i-1);
                maxBlockingTime = 0;        
                 for (t = 1; t <= getHyperPeriod(); t++)
                 {
                     //add the execution time of the current task
                     wi[t - 1] = (double)iTask.getExecution();
                     
                     //Start looping through tasks.
                     //Start with 1 and go to the task just below
                     for (k = 1; k < i;  k++)
                     {
                         TaskType kTask = (TaskType)tasks.Values.ElementAt(k-1);
                         
                         //making sure that we are looping through higher priority tat
                         if (iTask.getPeriod() > kTask.getPeriod())
                         {
                             wi[t - 1] += Math.Ceiling((double)t / kTask.getPeriod()) * kTask.getExecution();
                         }
                     }
                     //Find the blocking time for the lower priority tasks
                     foreach (TaskType kT in tasks.Values)
                     {
                         if (iTask.getPeriod() == kT.getPeriod())
                             continue;
                         if (kT.getMutualExclusion() == MutualExclusionType.MaskingInterrupts && kT.getMaxBlockingTime() > maxBlockingTime)
                             maxBlockingTime = kT.getMaxBlockingTime();
                         else if (kT.getMutualExclusion() == MutualExclusionType.RealTimeSemaphore && iTask.isSemaphoreShared(kT) && iTask.getSharedSemaphoreBlockingTime(kT) > maxBlockingTime)
                             maxBlockingTime = iTask.getSharedSemaphoreBlockingTime(kT);


                     }
                     //Add the blocking time to the demand value
                     wi[t - 1] += maxBlockingTime;

                     if (t > iTask.getDeadline())
                         break;
                     if (t >= wi[t-1])
                     {
                         Console.WriteLine("\tFEASIBLE - Task {0}: \ttime={1}   \twi[t]={2} \tb={3}", i, t, wi[t - 1], maxBlockingTime);
                         iTask.setFeasible(true);
                         break;
                     }
                     
                }
                if (!iTask.isFeasible())
                    Console.WriteLine("\tNOT FEASIBLE - Task {0}: ", i);
                         
                         
                    

            }


            return;
        } 
        internal void analyzePeriods()
        {
            hyperPeriod = 1;
            List<int> taskPeriods = new List<int>();
            Console.WriteLine("\n\nWorkload Analysis: {0}", workLoadName);
            
            foreach (TaskType t in tasks.Values)
            {
                Console.WriteLine("\t(p={0}ms, e={1}ms, D={2}ms, Critical Section={3}ms, {4})", 
                                t.getPeriod(), t.getExecution(), t.getDeadline(), t.getMaxBlockingTime(), t.getMutualExclusionString());

                setHyperPeriod (getHyperPeriod() * t.getPeriod());
                taskPeriods.Add(t.getPeriod());
            }
            minPeriod = taskPeriods.Min();
            maxPeriod = taskPeriods.Max();
            Console.WriteLine("MaxPeriod= {0}, MinPeriod= {1}  ", maxPeriod, minPeriod);
            findLeastCommonMultiple();
            
            

        }
        internal void findLeastCommonMultiple()
        {
            for (int i = maxPeriod; i <= getHyperPeriod(); i += maxPeriod)
            {

                int c = 0;
                for (int j = 0; j < getTasks().Count(); j++)
                {
                    if (i % getTasks().ElementAt(j).getPeriod() == 0)
                        c += 1;
                }
                if (c == getTasks().Count())
                {
                    setHyperPeriod(i);
                    Console.WriteLine("The LCM/Hyperperiod of the periods: {0} ", hyperPeriod);

                    break;
                }
            }
        }
        static void InitDoubleArray(double[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = 0;
        } 
    }
}
