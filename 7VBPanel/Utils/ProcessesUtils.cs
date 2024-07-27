﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _7VBPanel.Utils
{
    public static class ProcessesUtils
    {

        public static Process[] GetProcessesByParentPID(int parentPID, string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return Array.FindAll(processes, process => IsChildProcess(process, parentPID));
        }

        public static bool IsChildProcess(Process process, int parentPID)
        {
            try
            {
                while (process != null)
                {
                    if (process.Id == parentPID)
                    {
                        return true;
                    }
                    process = GetParentProcess(process);
                }
            }
            catch (Exception)
            {
                // Handle exception
            }

            return false;
        }
        public static HashSet<string> GetLoadedDlls(int pid)
        {
            var dlls = new HashSet<string>();
            try
            {
                Process process = Process.GetProcessById(pid);
                foreach (ProcessModule module in process.Modules)
                {
                    if (module.ModuleName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        dlls.Add(module.ModuleName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing process: {ex.Message}");
            }
            return dlls;
        }

        public static void WaitForLoadCS2(int CS2Pid)
        {
            var knownDlls = GetLoadedDlls(CS2Pid);
            foreach (var dll in knownDlls)
            {
                if (dll.Equals("ddraw.dll", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"{CS2Pid} - {dll}");
                    return;
                }
            }
            while (true)
            {
                Thread.Sleep(200);
                var currentDlls = GetLoadedDlls(CS2Pid);
                var newDlls = currentDlls.Except(knownDlls).ToList();

                if (newDlls.Count > 0)
                {
                    foreach (var dll in newDlls)
                    {
                        if (dll.Equals("propsys.dll", StringComparison.OrdinalIgnoreCase))
                        {
                            Thread.Sleep(3000);
                            return;
                        }
                    }
                    knownDlls = currentDlls.ToHashSet();
                }
            }
        }
        public static Process GetParentProcess(Process process)
        {
            try
            {
                using (ManagementObject mo = new ManagementObject($"win32_process.handle='{process.Id}'"))
                {
                    mo.Get();
                    int parentPID = Convert.ToInt32(mo["ParentProcessId"]);
                    return Process.GetProcessById(parentPID);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void KillAllSteamAndCS()
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                try
                {
                    if (process.ProcessName.ToLower().Contains("steam"))
                    {
                        process.Kill();
                    }
                    if (process.ProcessName.ToLower().Contains("cs2"))
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
