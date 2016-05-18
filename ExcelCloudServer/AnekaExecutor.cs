using System;
using System.Diagnostics;
using Aneka.Tasks;

namespace ExcelCloudServer
{
    [Serializable]
    public class AnekaExecutor : ITask
    {
        /// <summary>
        /// Gets, sets the task ID for each execution
        /// </summary>
        public int taskID { get; set; }

        private String task, args;
        public int result;

        public AnekaExecutor(String task, String args) { this.task = task; this.args = args; }

        public void Execute()
        {
            try
            {
                String outputfilename = String.Empty;
                String cmd = "cmd";

                Process userTask = new Process();
                userTask.StartInfo.FileName = cmd;
                userTask.StartInfo.Arguments = args;
                userTask.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                userTask.StartInfo.UseShellExecute = false;
                userTask.StartInfo.CreateNoWindow = true;
                userTask.StartInfo.RedirectStandardError = true;
                userTask.StartInfo.RedirectStandardOutput = true;


                userTask.StartInfo.RedirectStandardError = true;
                userTask.StartInfo.RedirectStandardOutput = true;

                // attach event handlers
                // for output handling
                userTask.OutputDataReceived +=
                       new DataReceivedEventHandler(userTask_OutputDataReceived);
                userTask.ErrorDataReceived +=
                       new DataReceivedEventHandler(userTask_ErrorDataReceived);

                userTask.Start();
                userTask.BeginErrorReadLine();
                userTask.BeginOutputReadLine();
                while (!userTask.HasExited)
                {
                    // since we are getting notified async.
                    userTask.WaitForExit(1000);
                }
                if (userTask.HasExited)
                {
                    if (userTask.ExitCode != 0)
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw ex;
            }
        }

        static void userTask_OutputDataReceived(object SendingProcess, DataReceivedEventArgs outLine)
        {
            Debug.WriteLine("Output is:" + outLine);
        }

        static void userTask_ErrorDataReceived(object SendingProcess, DataReceivedEventArgs outLine)
        {
            Debug.WriteLine("Output is:" + outLine);
        }
    }
}
