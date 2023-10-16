// (C) Copyright 2023 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software
// in object code form for any purpose and without fee is hereby
// granted, provided that the above copyright notice appears in
// all copies and that both that copyright notice and the limited
// warranty and restricted rights notice below appear in all
// supporting documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK,
// INC. DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL
// BE UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is
// subject to restrictions set forth in FAR 52.227-19 (Commercial
// Computer Software - Restricted Rights) and DFAR 252.227-7013(c)
// (1)(ii)(Rights in Technical Data and Computer Software), as
// applicable.
//

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitExternalExeTestAddin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
        }

        private void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            LogTrace("Design Automation Ready event triggered ...");

            e.Succeeded = true;
            e.Succeeded = this.DoTask(e.DesignAutomationData);
        }

        private bool DoTask(DesignAutomationData data)
        {
            if (data == null)
                return false;

            Application app = data.RevitApp;
            if (app == null)
            {
                LogTrace("Error occured");
                LogTrace("Invalid Revit App");
                return false;
            }

            LogTrace("Prepare running `RunMeshOptimizer.exe`...");
            var asm = Assembly.GetExecutingAssembly();

            var exePath = Path.Combine(Path.GetDirectoryName(asm.Location), "RunMeshOptimizer.exe");
            var newExePath = Path.Combine(Directory.GetCurrentDirectory(), "RunMeshOptimizer.exe");

            LogTrace("- Current exe path is {0}", exePath);
            //LogTrace("- New exe path is {0}", newExePath);

            //try
            //{
            //    LogTrace("- Copying exe to new location...");
            //    File.Copy(exePath, newExePath, true);
            //    LogTrace("-- DONE");
            //}
            //catch (Exception ex)
            //{
            //    LogTrace("- Error occured");
            //    LogTrace("- Failed to copy exe to new location");
            //    LogTrace(ex.Message);

            //    if (ex.InnerException != null)
            //        LogTrace(ex.InnerException.Message);

            //    return false;
            //}

            LogTrace("- DONE");
            LogTrace("Start running `RunMeshOptimizer.exe`...");

            var outputFilename = "test.txt";
            using (var exeProcess = new Process())
            {
                //exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                exeProcess.StartInfo.CreateNoWindow = false;
                exeProcess.StartInfo.UseShellExecute = false;
                exeProcess.StartInfo.RedirectStandardOutput = true;
                exeProcess.StartInfo.RedirectStandardError = true;
                exeProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                exeProcess.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                //exeProcess.StartInfo.Arguments = $"\"{outputFilename}\"";
                exeProcess.StartInfo.FileName = exePath;//newExePath;

                LogTrace("- Current exe working dir: `{0}`", exeProcess.StartInfo.WorkingDirectory);
                //exeProcess.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                //LogTrace("- Change current exe working dir: `{0}`", exeProcess.StartInfo.WorkingDirectory);

                LogTrace("- Running `RunMeshOptimizer.exe`...");
                var runResult = exeProcess.Start();
                if (runResult == false)
                {
                    LogTrace("Error occured");
                    LogTrace("Failed to run `RunMeshOptimizer.exe`");
                    return false;
                }

                exeProcess.BeginOutputReadLine();
                exeProcess.BeginErrorReadLine();
                exeProcess.WaitForExit();

                if (exeProcess.ExitCode < 0)
                {
                    LogTrace("Error occured");
                    LogTrace("Failed to run `RunMeshOptimizer.exe`, which exit code is {0}", exeProcess.ExitCode);
                    return false;
                }

                if (!exeProcess.HasExited)
                {
                    LogTrace("Kill porcess of `RunMeshOptimizer.exe`");
                    exeProcess.Kill();
                }
            }

            LogTrace("- DONE");

            return true;
        }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)

            if (!string.IsNullOrWhiteSpace(outLine.Data))
                LogTrace(outLine.Data);
        }

        private void PrintError(Exception ex)
        {
            LogTrace("Error occurred");
            LogTrace(ex.Message);

            if (ex.InnerException != null)
                LogTrace(ex.InnerException.Message);
        }

        /// <summary>
        /// This will appear on the Design Automation output
        /// </summary>
        private static void LogTrace(string format, params object[] args)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine(string.Format(format, args));
#endif
            System.Console.WriteLine(format, args);
        }
    }
}
