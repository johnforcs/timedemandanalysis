using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeDemandAnalysis
{
    
    public class CriticalSection
    {
        int executeTime;
        int criticalSectionId;
        
        public CriticalSection(int csId, int csExecuteTime)
        {
            criticalSectionId = csId;
            executeTime = csExecuteTime;
        }
        public int getCriticalSectionId() {return criticalSectionId;}
        public int getExecutionTime() {return executeTime;}

        private void setCriticalSectionId(int csId)
        {
            criticalSectionId = csId;
        }
        private void setExecutionTime(int csExecuteTime)
        {
            executeTime = csExecuteTime;
        }
    }


}
