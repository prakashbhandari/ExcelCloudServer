using System;
using System.Diagnostics;
using Aneka.Tasks;

namespace ExcelCloudServer
{
    [Serializable]
    class AnekaT : ITask
    {
        /// <summary>
        /// Gets, sets the task ID for each execution
        /// </summary>
        public int taskID;

        public String task, args;
        public string result;

        public AnekaT(String task, String args) { this.task = task; this.args = args; }

        public void Execute()
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = task,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    result = proc.StandardOutput.ReadLine();
                    Debug.WriteLine(result);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
