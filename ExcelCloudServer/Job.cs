using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace ExcelCloudServer
{
    class Job
    {
        // Job attributes
        public List<string> inputDatas = new List<string>();
        public IDictionary<string, string> tasks = new Dictionary<string, string>();
        public string jobExecution = String.Empty;
        public int numTasks;
        public int numParams;

        string args = String.Empty;

        // Server attributes
        public bool usingAneka;
        public IDictionary<string, string> serverDetails = new Dictionary<string, string>();

        private static int failed;
        private static int completed;
        private static int total;

        public void SubmitJob()
        {
            int taskId = 1;
            foreach (KeyValuePair<string, string> taskFile in tasks)
            {
                for (int i = 0; i < numTasks; i++)
                {
                    args = String.Empty;
                    for (int j = 0; j < numParams; j++)
                    {
                        args += (jobExecution.Equals("Row based")) ? inputDatas[(i * numParams) + j] + " " : inputDatas[(j * numTasks) + i] + " ";
                    }

                    Debug.WriteLine("Running task:" + taskFile.Value);
                    TaskExecutor taskExecutor = new TaskExecutor(taskFile.Value, args);
                    taskExecutor.taskID = taskId;
                    taskExecutor.Execute();
                    taskId++;
                }
            }
            AsyncConnection.Send("EOF");
            AsyncConnection.sendDone.WaitOne();
        }
    }
}
