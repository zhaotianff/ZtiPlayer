using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using ZtiPlayer.Models;

namespace ZtiPlayer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// OnStartup
        /// </summary>
        /// <remarks>
        /// *****************************************************************************
        /// *startup args
        ///     Arguement          Description
        /// *   -path              open designated path
        /// *   -playlist          import video list,separated by semicolon 
        /// *   -width             set player width
        /// *   -height            set player height
        /// *   -silient           make player silent
        /// *   -volume            set player volume
        /// *   -help              show help info
        /// *****************************************************************************
        /// </remarks>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            CheckSingleInstance();
            base.OnStartup(e);       
            ParseArgs(e.Args);                   
        }


        private void ParseArgs(string[] args)
        {
            StartupArgs startArgs = new StartupArgs();
            if(args.Length == 0)
            {
                startArgs.ArgsCount = 0;
            }
            else
            {
                startArgs.ArgsCount = args.Length / 2;
                Dictionary<string, string> argsDic = new Dictionary<string, string>();
                for (int i = 0; i < args.Length; i+=2)
                {
                    if (argsDic.ContainsKey(args[i]))
                        continue;
                    argsDic.Add(args[i], args[i + 1]);
                }
                try
                {
                    startArgs.Height = Convert.ToInt32(argsDic[startArgs.Height.GetType().Name]);  //TODO
                    startArgs.Width = Convert.ToInt32(argsDic[startArgs.Width.GetType().Name]);
                    startArgs.IsSilent = Convert.ToBoolean(argsDic[startArgs.IsSilent.GetType().Name]);
                    startArgs.Path = argsDic[startArgs.Path.GetType().Name];
                    startArgs.PlayList = new string[] { };//TODO
                    startArgs.Volume = Convert.ToInt32(argsDic[startArgs.Volume.GetType().Name]);                
                }
                catch(Exception ex)
                {
                    //TODO
                }
            }

            StartApplication(startArgs);
        }

        private void StartApplication(StartupArgs argsObj)
        {
            MainWindow main;         
            if (argsObj.ArgsCount == 0)
            {
                main = new ZtiPlayer.MainWindow();
            }
            else
            {
                main = new ZtiPlayer.MainWindow(argsObj);
            }
            main.Show();
        }

        private void StartReceiveMessageThread()
        {

        }

        private void ReceiveMessage()
        {

        }

        private void CheckSingleInstance()
        {
            var currentprocess = Process.GetCurrentProcess();
            var processArray = Process.GetProcesses();
            var result = processArray.Count(x => x.ProcessName == currentprocess.ProcessName);
            if(result > 1)
            {
                Application.Current.Shutdown(0);
            }
        }
    }
}
