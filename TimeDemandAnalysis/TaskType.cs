using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeDemandAnalysis
{

    public enum MutualExclusionType
    {
        NoExclusion = -1,
        MaskingInterrupts = 0,
        RealTimeSemaphore = 1
    }
    public class TaskType
    {
        
        int period;
        int deadline;
        int executionTime;
        bool feasible;
        List <CriticalSection> criticalSection;
        MutualExclusionType mutualExclusion;
             

        public TaskType(int p, int e)
        {
            setPeriod(p);
            setDeadline(p);
            setExecution(e);
            setFeasible(false);
            criticalSection = new List<CriticalSection>();
            criticalSection.Clear();
            mutualExclusion = MutualExclusionType.NoExclusion;
        }
        public TaskType(int p, int e, int d)
        {
            setPeriod(p);
            setDeadline(d);
            setExecution(e); 
            setFeasible(false);
            criticalSection = new List<CriticalSection>();
            criticalSection.Clear();
            mutualExclusion = MutualExclusionType.NoExclusion;
        }
        public int getPeriod() { return period; }
        public int getExecution() { return executionTime; }
        public int getDeadline() { return deadline; }
        public bool isFeasible() { return feasible; }
        public String getMutualExclusionString() { return mutualExclusion.ToString(); }
        public MutualExclusionType getMutualExclusion() { return mutualExclusion; }
        public List<CriticalSection> getCriticalSection() { return criticalSection; }
        public int getMaxBlockingTime()
        {
            int bTime = (criticalSection.Count() > 0 ? criticalSection.Max(x => x.getExecutionTime()) : 0);
            return bTime;
        }
        public bool isSemaphoreShared(TaskType kTask)
        {
            if (mutualExclusion == MutualExclusionType.RealTimeSemaphore)
            {   
                foreach (CriticalSection c in kTask.getCriticalSection())
                {
                    CriticalSection matchingCS = getCriticalSection().Find(x => x.getCriticalSectionId() == c.getCriticalSectionId());
                    if (matchingCS != null)
                         return true;
                }
            }
            return false;
        }
        public int getSharedSemaphoreBlockingTime(TaskType kTask)
        {
            int blockTime = 0;
            foreach (CriticalSection c in getCriticalSection())
            {
                CriticalSection matchingCS = kTask.getCriticalSection().Find(x => x.getCriticalSectionId() == c.getCriticalSectionId());
                if (matchingCS != null && matchingCS.getExecutionTime() > blockTime)
                    blockTime = matchingCS.getExecutionTime();
            }
            return blockTime;
        }
        
        private void setPeriod(int p)
        {
            period = p;
            return;
        }
        private void setExecution(int e)
        {
            executionTime = e;
            return;
        }
        private void setDeadline(int d)
        {
            deadline = d;
            return;
        }
        public void setFeasible(bool flag)
        {
            feasible = flag;
            return;
        }
        internal void addCriticalSection(CriticalSection cs, MutualExclusionType me)
        {
            criticalSection.Add(cs);
            mutualExclusion = me;
        }

    }
}
