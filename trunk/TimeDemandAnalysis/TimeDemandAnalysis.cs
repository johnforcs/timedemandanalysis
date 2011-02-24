
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace TimeDemandAnalysis
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Incorrect Usage.  Expecting 2 arguments:  inputFile outputFile");
                Console.WriteLine("Example Usage:");
                Console.WriteLine("C:\\Bin>TimeDemandAnalysis.exe c:\\temp\\input\\inputfile.txt c:\\temp\\outputfile\\outputfile.txt");
                return;
            }

            string inputFile = args[0];
            string outputFile = args[1];

            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(writer);
            

            List<Workload> wlList = parseFile(inputFile);
            foreach (Workload w in wlList)
            {
                w.analyzePeriods();
                w.performTimeDemandAnalysis();
            }

            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
            Console.WriteLine("Done");
        }
        
        
        static List<Workload> parseFile(string inputFile)
        {
            /**********************************************
             *  Workload: <workload name>
             *   (p=25ms, e=8ms, D=25ms, Critical Section=0ms, NoExclusion)
             *   (p=50ms, e=13ms, D=50ms, Critical Section=0ms, NoExclusion)
             *   (p=100ms, e=40ms, D=100ms, Critical Section=0ms, NoExclusion)
             */

            System.IO.TextReader tr = new StreamReader(inputFile);
            string myLine;
            TaskType aTask;
            Workload work = null;
            List<Workload> workList = new List<Workload>();
            int lineNumber = 1;
            while ((myLine = tr.ReadLine()) != null)
            {
                if (myLine.Contains("Workload"))
                {
                    /*************************************************************************
                     * input file can contain multiple workloads, so if it is already populated, 
                     * add the previous workload to the worklist and create a new worklaod.
                     **************************************************************************/
                    if (work != null)
                        workList.Add(work);

                    /** Create a new workload **/
                    string[] keyValue = myLine.Split(':');
                    work = new Workload(keyValue[1]);
                }
                else
                {   
                    aTask = parseLine(myLine);
                    if (work != null && aTask != null)
                        work.addTask(aTask);


                    else
                        Console.WriteLine("Error Parsing Input File at line: {0}", lineNumber);
                }
                lineNumber++;
            }
            tr.Close();

            if (work != null)
                workList.Add(work);
            return workList;
        }

        static TaskType parseLine(string aLine)
        {

            /**************************************************************************************************************************
             *    Workload: <workload name>
             *       (p=25ms, e=8ms, D=9ms, Critical Section=0ms, NoExclusion)
             *       (p=50ms, e=13ms, D=50ms, Critical Section2=2ms, Critical Section3=2ms, Critical Section6=10ms,RealTimeSemaphore)
             *       (p=100ms, e=40ms, D=100ms, Critical Section2=2ms, Critical Section3=4ms, Critical Section7=10ms,RealTimeSemaphore)
            ***************************************************************************************************************************/

            int p = 0, e = 0, d=0;
            string key= "";
            int val = 0;
            int optionalVal = 0;
            SortedList<int, int> csTime = new SortedList<int, int>();
            MutualExclusionType exclusionType = MutualExclusionType.NoExclusion;

            string[] keyValue = aLine.Split(',');
            for (int i = 0; i < keyValue.Length; i++)
            {
               parseKeyValue(keyValue[i], ref key, ref val, ref optionalVal);
               if (key.Equals("e")) e = val; 
               else if (key.Equals("p")) p = val; 
               else if (key.Equals("d")) d = val; 
               else if (key.Equals("critical section")) csTime.Add(optionalVal, val); 
               else if (key.Equals("noexclusion")) exclusionType = MutualExclusionType.NoExclusion; 
               else if (key.Equals("maskinginterrupts")) exclusionType = MutualExclusionType.MaskingInterrupts; 
               else if (key.Equals("realtimesemaphore")) exclusionType = MutualExclusionType.RealTimeSemaphore;  
               else Console.WriteLine("Error Parsing Input File at key/value: {0}/{1}", key, val);

            }
            TaskType T1 = new TaskType(p, e, (d > 0 ? d : p));
            foreach (KeyValuePair<int, int> kv in csTime)
            {
                CriticalSection cs = new CriticalSection(kv.Key,kv.Value);
                T1.addCriticalSection(cs, exclusionType);
            }


            
            return T1;
        }
        static void parseKeyValue(string keyValuePair, ref string k, ref int v, ref int o)
        {
            /**************************************************************************************************************************
             *    Workload: <workload name>
             *       (p=25ms, e=8ms, D=9ms, Critical Section=0ms, NoExclusion)
             *       (p=50ms, e=13ms, D=50ms, Critical Section2=2ms, Critical Section3=2ms, Critical Section6=10ms,RealTimeSemaphore)
             *       (p=100ms, e=40ms, D=100ms, Critical Section2=2ms, Critical Section3=4ms, Critical Section7=10ms,RealTimeSemaphore)
            ***************************************************************************************************************************/
            keyValuePair = keyValuePair.Replace('(', ' ');
            keyValuePair = keyValuePair.Replace(',', ' ');
            keyValuePair = keyValuePair.Replace(')', ' ');
            string[] paramValue = keyValuePair.Split('=');

            Regex criticalSectionChecker = new Regex("Critical Section[0-9]+");
           
            if (criticalSectionChecker.Match(paramValue[0]).Success)
            {
                //to determine the id of the critical section.
                o = (int)System.Convert.ToDouble(paramValue[0].Replace("Critical Section", "").Trim());
                k = "critical section";
            }
            else         
                k = paramValue[0].ToLower().Trim();

            if (paramValue.Length > 1)
            {
                Regex ms = new Regex("[0-9]+ms");
                Regex s = new Regex("[0-9]+s");
                if (ms.Match(paramValue[1]).Success)
                {
                    //support for units in ms.
                    v = (int)System.Convert.ToDouble(paramValue[1].Replace("ms", "").Trim());
                }
                else if (s.Match(paramValue[1]).Success)
                {
                    //support for units in seconds
                    paramValue[1].Replace("s", "");
                    v = (int)System.Convert.ToDouble(paramValue[1].Replace("s", "").Trim()) * 1000;
                }
                else
                {
                    Console.WriteLine("Error Parsing Input File.  Only ms and s support at this time.  Invalid: {0}", paramValue[1]);                   
                }
                
            }

           
            return;
        }
        

    }
}


