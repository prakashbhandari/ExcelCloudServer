using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ExcelCloudServer
{
    [Serializable]
    public class CustomTask
    {
        /// <summary>
        /// Gets, sets the task ID for each execution
        /// </summary>
        public int taskID;

        private string task, args;
        public string result;

        public CustomTask(String task, String args) { this.task = task; this.args = args; }

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
                    string response = JsonConvert.SerializeObject(this);
                    AsyncConnection.Send(response);
                    AsyncConnection.sendDone.WaitOne();
                    Debug.WriteLine(response);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
