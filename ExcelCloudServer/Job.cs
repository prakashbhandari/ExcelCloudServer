//Title        :  Job.cs
//Package      :  ExcelCloudServer
//Project      :  ExcelCloud
//Description  :  Provides functionality to create tasks, submit tasks and send response
//Created on   :  June 5, 2016
//Author	   :  Prakash Bhandari

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Aneka;
using Aneka.Tasks;
using Aneka.Entity;
using Aneka.Security;
using Newtonsoft.Json;
using System.IO;

namespace ExcelCloudServer
{
    /// <summary>
    /// Class Job: Holds the JOb attributes prepares
    /// the job and submit the job to work units.
    /// Once execution completed the output is send
    /// back to the addin
    /// </summary>
    class Job
    {
        // Job attributes
        /// <summary>
        /// List of all the values in the user selected inputRange
        /// </summary>
        public List<string> inputDatas = new List<string>();
        /// <summary>
        /// List of all the executable task files
        /// </summary>
        public List<string> taskFiles = new List<string>();
        /// <summary>
        /// Job Execution type: required at server to prepare job arguments
        /// </summary>
        public string jobExecution = String.Empty;
        /// <summary>
        /// Number of tasks based on Job execution
        /// </summary>
        public int numTasks;
        /// <summary>
        /// Number of params based on Job execution
        /// </summary>
        public int numParams;
        public string args = String.Empty;

        // Server attributes
        public bool usingAneka;
        /// <summary>
        /// List of all the information host, port, library of server
        /// </summary>
        public IDictionary<string, string> serverDetails = new Dictionary<string, string>();

        // Aneka attributes
        /// <summary>
        /// List of all the attributes to connect to aneka master
        /// </summary>
        public IDictionary<string, string> anekaDetails = new Dictionary<string, string>();

        /// <summary>
        /// set semaphore signal Aneka app has finished
        /// </summary>
        private static AutoResetEvent semaphore = null;
        /// <summary>
        /// create a null instance of Aneka application
        /// </summary>
        private static AnekaApplication<AnekaTask, TaskManager> app = null;

        /// <summary>
        /// Number of failed work units
        /// </summary>
        private static int failed;
        /// <summary>
        /// Number of completed work units
        /// </summary>
        private static int completed;
        /// <summary>
        /// Number of total work units
        /// </summary>
        private static int total;

        /// <summary>
        /// Prepare tasks and submit all the tasks to work units
        /// </summary>
        public void SubmitTasks()
        {
            if(this.usingAneka)
            {
                Aneka.Tasks.AnekaTask aTask = null;
                Configuration conf = null;
                try
                {
                    Logger.Start();
                    semaphore = new AutoResetEvent(false);
                    // Configure Aneka
                    conf = Configuration.GetConfiguration();
                    conf.UseFileTransfer = true;
                    conf.Workspace = ".";
                    conf.SingleSubmission = false;
                    conf.ResubmitMode = ResubmitMode.AUTO;
                    conf.PollingTime = 1000;
                    string anekaUrl = "tcp://" + anekaDetails["host"] + ":" + anekaDetails["port"] + "/Aneka";
                    conf.SchedulerUri = new Uri(anekaUrl, UriKind.Absolute);
                    conf.UserCredential = new UserCredentials(anekaDetails["username"], anekaDetails["password"]);

                    app = new AnekaApplication<AnekaTask, TaskManager>(conf);
                    app.WorkUnitFailed += new EventHandler<WorkUnitEventArgs<AnekaTask>>(app_workUnitFailed);
                    app.WorkUnitFinished += new EventHandler<WorkUnitEventArgs<AnekaTask>>(app_workUnitFinished);
                    app.ApplicationFinished += new EventHandler<ApplicationEventArgs>(app_applicationFinished);

                    // Initialise total, completed and failed work units
                    total = taskFiles.Count * numTasks;
                    completed = 0;
                    failed = 0;
                    
                    // Initialise taskId to 1.
                    int taskId = 1;

                    foreach (string taskFile in taskFiles)
                    {
                        string libraryDir = (serverDetails["libraryDir"].Trim().EndsWith(@"\")) ? serverDetails["libraryDir"].Trim(): serverDetails["libraryDir"].Trim() + @"\";
                        if (!File.Exists(libraryDir + taskFile))
                        {
                            AsyncConnection.Send("Error encountered - File not found in server");
                            AsyncConnection.sendDone.WaitOne();
                            break;
                        }
                        for (int i = 0; i < numTasks; i++)
                        {
                            args = String.Empty;
                            // prepare arguments to be submitted to executable task
                            for (int j = 0; j < numParams; j++)
                            {
                                args += (jobExecution.Equals("Row based")) ? inputDatas[(i * numParams) + j] + " " : inputDatas[(j * numTasks) + i] + " ";
                            }
                            
                            //Create instance of ITask 
                            AnekaT anekaExecutor = new AnekaT(libraryDir + taskFile, args);

                            anekaExecutor.taskID = taskId;
                            // Add the task to work unit
                            aTask = new AnekaTask(anekaExecutor);
                            // Execute the task
                            app.ExecuteWorkUnit(aTask);

                            // Increase taskId for each new task
                            taskId++;
                        }
                    }
                    Debug.WriteLine("Jobs Completed");
                }
                catch (NullReferenceException nre)
                {
                    Debug.WriteLine(nre.ToString());

                    AsyncConnection.Send("Error encountered - Check Aneka log files");
                    AsyncConnection.sendDone.WaitOne();
                }
                catch (Exception e)
                {
                    IOUtil.DumpErrorReport(e, "Excel Cloud Addin Errors");
                    Debug.WriteLine(e.ToString());

                    AsyncConnection.Send("Error encountered - Check Aneka log files");
                    AsyncConnection.sendDone.WaitOne();
                }
                finally
                {
                    Logger.Stop();
                }
            }
            else
            {
                // Similar to Aneka executa all the tasks 
                int taskId = 1;
                foreach (string taskFile in taskFiles)
                {
                    string libraryDir = (serverDetails["libraryDir"].Trim().EndsWith(@"\")) ? serverDetails["libraryDir"].Trim() : serverDetails["libraryDir"].Trim() + @"\";
                    if (!File.Exists(libraryDir + taskFile))
                    {
                        AsyncConnection.Send("Error encountered - File not found in server");
                        AsyncConnection.sendDone.WaitOne();
                        break;
                    }
                    for (int i = 0; i < numTasks; i++)
                    {
                        args = String.Empty;
                        for (int j = 0; j < numParams; j++)
                        {
                            args += (jobExecution.Equals("Row based")) ? inputDatas[(i * numParams) + j] + " " : inputDatas[(j * numTasks) + i] + " ";
                        }

                        Debug.WriteLine("Running task:" + taskFile);
                        CustomTask taskExecutor = new CustomTask(libraryDir + taskFile, args);
                        taskExecutor.taskID = taskId;
                        taskExecutor.Execute();
                        taskId++;
                    }
                }
                AsyncConnection.Send("EOF");
                AsyncConnection.sendDone.WaitOne();
            }
        }

        /// <summary>
        /// Aneka Application Completed. Send job completed message EOF.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void app_applicationFinished(object sender, ApplicationEventArgs e)
        {
            semaphore.Set();
            AsyncConnection.Send("EOF");
            AsyncConnection.sendDone.WaitOne();
        }
        
        /// <summary>
        /// Aneka application work unit finished. Send the 
        /// output received to the addin through socket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void app_workUnitFinished(object sender, WorkUnitEventArgs<Aneka.Tasks.AnekaTask> e)
        {
            Debug.WriteLine("WorkUnit Completed");
            completed = completed + 1;
            string response = JsonConvert.SerializeObject((AnekaT)e.WorkUnit.UserTask);
            AsyncConnection.Send(response);
            Debug.WriteLine("Sending: " + response);
            AsyncConnection.sendDone.WaitOne();
            if (completed == total)
            {
                app.StopExecution();
            }
        }

        /// <summary>
        /// Application work unit failed. Decrease the total task by 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void app_workUnitFailed(object sender, WorkUnitEventArgs<Aneka.Tasks.AnekaTask> e)
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
