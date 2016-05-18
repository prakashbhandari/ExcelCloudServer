using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using Aneka;
using Aneka.Security;
using Aneka.Threading;
using Aneka.Tasks;
using Aneka.Entity;
using System.Diagnostics;

namespace ExcelCloudServer
{
    class Job
    {
        // Job details
        public List<String> inputDatas = new List<string>();
        public List<String> tasks = new List<string>();
        public String inputType;
        public String jobExecution;

        // Task details
        public int numRows;
        public int numColumns;

        // Aneka details and attributes
        public Boolean usingAneka;
        public IDictionary<String, String> anekaDetails = new Dictionary<String, String>();
        static AutoResetEvent semaphore = null;
        static AnekaApplication<AnekaTask, TaskManager> app = null;
        static Configuration conf;
        private static int failed = 0;
        private static int completed = 0;
        private static int total;
        int numTasks;
        int numParams;
        String args = String.Empty;

        public void PrepareBagOfTasks(String request)
        {
            JsonConvert.DeserializeObject<Job>(request);

            if (usingAneka)
            {
                configureAneka();
            }
        }

        public void configureAneka()
        {
            try
            {
                semaphore = new AutoResetEvent(false);
                Configuration conf = Configuration.GetConfiguration();
                conf.SingleSubmission = false;
                conf.ResubmitMode = ResubmitMode.AUTO;
                conf.PollingTime = 1000;
                String anekaUrl = "tcp://" + anekaDetails["master"] + ":" + anekaDetails["port"] + "/Aneka";
                conf.SchedulerUri = new Uri(anekaUrl, UriKind.Absolute);
                conf.UserCredential = new UserCredentials(anekaDetails["username"], anekaDetails["password"]);
            }
            catch (Exception e)
            {

            }
        }

        public void SubmitTasks()
        {
            if (this.usingAneka)
            {
                try
                {
                    AnekaTask aTask = null;
                    Logger.Start();
                    app = new AnekaApplication<AnekaTask, TaskManager>(conf);
                    app.WorkUnitFailed += new EventHandler<WorkUnitEventArgs<AnekaTask>>(app_workUnitFailed);
                    app.WorkUnitFinished += new EventHandler<WorkUnitEventArgs<AnekaTask>>(app_workUnitFinished);
                    app.ApplicationFinished += new EventHandler<ApplicationEventArgs>(app_applicationFinished);

                    numTasks = (jobExecution.Equals("Row based")) ? numRows : numColumns;
                    numParams = (jobExecution.Equals("Row based")) ? numColumns : numRows;

                    foreach (String task in tasks)
                    {
                        for (int i = 0; i < numTasks; i++)
                        {
                            for (int j = 0; j < numParams; j++)
                            {
                                args += inputDatas[(i * numParams) + j] + " ";
                            }
                            AnekaExecutor anekaExecutor = new AnekaExecutor(task, args);
                            // TaskID must start from 1 not 0.
                            anekaExecutor.taskID = i + 1;
                            aTask = new AnekaTask(anekaExecutor);
                            app.ExecuteWorkUnit(aTask);
                        }
                        semaphore.WaitOne();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
                finally
                {
                    Logger.Stop();
                }
            }
        }

        private static void app_applicationFinished(object sender, ApplicationEventArgs e)
        {
            semaphore.Set();
        }

        private static void app_workUnitAborted(object sender, WorkUnitEventArgs<AnekaTask> e)
        {
            Debug.WriteLine("WorkUnit Aborted");
        }

        private void app_workUnitFinished(object sender, WorkUnitEventArgs<AnekaTask> e)
        {
            string taskOutput = e.WorkUnit.UserTask.ToString();
            Debug.WriteLine("WorkUnit Finished with result: {0}", taskOutput);
            completed = completed + 1;
            if (completed == total)
            {
                app.StopExecution();
            }
        }

        private static void app_workUnitFailed(object sender, WorkUnitEventArgs<AnekaTask> e)
        {
            Debug.WriteLine("WorkUnit Failed");
            total = total - 1;
            if (completed == total)
            {
                app.StopExecution();
            }
            failed = failed + 1;
        }
    }
}
