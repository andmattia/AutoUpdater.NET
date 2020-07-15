using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InnoSetupWrap
{
    public partial class frmMain : Form
    {
        private BackgroundWorker _backgroundWorker;
        readonly StringBuilder _logBuilder = new StringBuilder();

        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            _logBuilder.AppendLine(DateTime.Now.ToString("F"));
            _logBuilder.AppendLine();
            _logBuilder.AppendLine("InnoWrapper started with following command line arguments.");

            string[] args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                _logBuilder.AppendLine($"[{index}] {arg}");
            }

            _logBuilder.AppendLine();

            if (args.Length >= 4)
            {
                string executablePath = args[3];

                // Extract all the files.
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += (o, eventArgs) =>
                {
                    foreach (var process in Process.GetProcesses())
                    {
                        try
                        { 
                            _logBuilder.AppendLine($"process-> {process.MainModule.FileName}");
                            if (process.MainModule.FileName.Equals(executablePath))
                            {
                                _logBuilder.AppendLine("Waiting for application process to exit...");

                                _backgroundWorker.ReportProgress(0, "Waiting for application to exit...");
                                process.WaitForExit();
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                        }
                    }

                    _logBuilder.AppendLine("BackgroundWorker started successfully.");

                    var path = args[1];
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = $"{path}",
                        Arguments = $"/SILENT",
                        UseShellExecute = true
                    };
                    
                    Process pExecute = new Process()
                    {

                        StartInfo = processStartInfo
                    };
                    
                    //pExecute.EnableRaisingEvents = true;
                    // Eventhandler wich fires when exited
                    //pExecute.Exited += new EventHandler((s,e)=> myProcess_Exited(s,e,executablePath));
                    
                    
                    // Starts the process
                    _logBuilder.AppendLine("Start Update");
                    pExecute.Start();
                    pExecute.WaitForExit();
                    _logBuilder.AppendLine("Finish Update");
                };

                //_backgroundWorker.ProgressChanged += (o, eventArgs) =>
                //{
                //    progressBar.Value = eventArgs.ProgressPercentage;
                //    textBoxInformation.Text = eventArgs.UserState.ToString();
                //    textBoxInformation.SelectionStart = textBoxInformation.Text.Length;
                //    textBoxInformation.SelectionLength = 0;
                //};

                _backgroundWorker.RunWorkerCompleted += (o, eventArgs) =>
                {
                    try
                    {
                        if (eventArgs.Error != null)
                        {
                            throw eventArgs.Error;
                        }

                        if (!eventArgs.Cancelled)
                        {
                            //textBoxInformation.Text = @"Finished";
                            try
                            {
                                ProcessStartInfo processStartInfo = new ProcessStartInfo(executablePath);
                                if (args.Length > 4)
                                {
                                    processStartInfo.Arguments = args[4];
                                }

                                Process.Start(processStartInfo);

                                _logBuilder.AppendLine("Successfully launched the updated application.");
                            }
                            catch (Win32Exception exception)
                            {
                                if (exception.NativeErrorCode != 1223)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        _logBuilder.AppendLine();
                        _logBuilder.AppendLine(exception.ToString());

                        MessageBox.Show(exception.Message, exception.GetType().ToString(),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        _logBuilder.AppendLine();
                        Application.Exit();
                    }
                };

                _backgroundWorker.RunWorkerAsync();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _backgroundWorker?.CancelAsync();

            _logBuilder.AppendLine();
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InnoLog.log"),
                _logBuilder.ToString());
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
