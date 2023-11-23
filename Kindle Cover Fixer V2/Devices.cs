﻿using MediaDevices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Kindle_Cover_Fixer_V2
{
    public partial class MainWindow
    {
        private void CheckKindle()
        {
            System.Timers.Timer aTimer = new(2000);
            aTimer.Elapsed += OnTimedEvent!;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            CheckKindleType();
        }
        // Check the kindle type that you have connected
        private void CheckKindleType()
        {
            var devices = MediaDevice.GetDevices();
            DriveInfo[] drives = DriveInfo.GetDrives();
            bool otherKindle = false;
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {
                    if (drive.VolumeLabel.Contains("Kindle"))
                    {
                        otherKindle = true;
                    }
                }
            }
            if (devices.Any())
            {
                if (devices.First().FriendlyName == "Kindle Scribe")
                {
                    Dispatcher.Invoke(() => {
                        connectedDevice.Content = "Kindle Scribe";
                    });
                    LogState("Connected Kindle Scribe", "CONNT", "KINDLE");
                }
                else if (otherKindle)
                {
                    Dispatcher.Invoke(() => {
                        connectedDevice.Content = "Kindle (Other)";
                    });
                    LogState("Connected Kindle (Other)", "CONNT", "KINDLE");
                }
                else
                {
                    Dispatcher.Invoke(() => {
                        connectedDevice.Content = "Device not connected";
                    });
                    LogState("Device disconnected", "DSCON", "DISCON");
                }
            }
            else
            {
                Dispatcher.Invoke(() => {
                    connectedDevice.Content = "Device not connected";
                });
                LogState("Device disconnected", "DSCON", "DISCON");
            }
        }
        // Log device status
        private void LogState(string description, string newState, string youAre)
        {
            Trace.WriteLine("STATE: " + StateDevice);
            if (StateDevice == "DSCON" && youAre == "KINDLE")
            {
                LogLine("DEVICE", description);
                StateDevice = newState;

            }
            else if (StateDevice == "CONNT" && youAre == "DSCON")
            {
                LogLine("DEVICE", description);
                StateDevice = newState;
            }
        }
    }
}
