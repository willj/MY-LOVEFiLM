using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Linq;
using Microsoft.Phone.Info;
using System.IO.IsolatedStorage;

namespace WPLovefilm.Helpers
{
    public class BackgroundAgentHelper
    {
        string agentName = "NewTitleNotifications";
        PeriodicTask newTitlesTask;

        public BackgroundAgentHelper()
        {
            newTitlesTask = ScheduledActionService.Find(agentName) as PeriodicTask;
        }

        public void CheckAgentStatus()
        {
            // Is this a 256MB Device that doesn't allow BG Agents?
            // Move to mainviewmodel so it can hide the settings button
            /*
            if (!DeviceCanUseAgents())
            {
                return;
            }
             */

            // Should the agent be running?
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("NewTitleNotifications"))
            {
                MessageBoxResult r = MessageBox.Show("Would you like to be notified when new films & games are shipped?", "", MessageBoxButton.OKCancel);

                if (r == MessageBoxResult.OK)
                {
                    if (EnableAgent())
                    {
                        IsolatedStorageSettings.ApplicationSettings["NewTitleNotifications"] = true;
                    }
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["NewTitleNotifications"] = false;
                }
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            else
            {
                ResetTile();

                IsolatedStorageSettings.ApplicationSettings["TileBackDataSet"] = false;
                IsolatedStorageSettings.ApplicationSettings.Save();

                if ((bool)IsolatedStorageSettings.ApplicationSettings["NewTitleNotifications"])
                {
                    //Refresh the agent
                    EnableAgent();
                }
            }
        }

        public bool EnableAgent()
        {
            // If the task already exists and the IsEnabled property is false
            // background agents have been disabled by the user
            if (newTitlesTask != null && !newTitlesTask.IsEnabled)
            {
                MessageBoxResult r = MessageBox.Show("Notifications have been switched off, check background tasks in your phone Settings. Would you like notifications to try and start again next time you start MY LOVEFiLM MOBILE?", "", MessageBoxButton.OKCancel);

                if (r == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            // if (testTask != null && testTask.IsEnabled)
            // Don't test for enabled, because if it's been disabled but allowed to restart it returns as disabled so we can never enable again.
            if (newTitlesTask != null)
            {
                DisableAgent();
            }

            newTitlesTask = new PeriodicTask(agentName);
            newTitlesTask.Description = "Shows a notification when new films & games are sent.";
            newTitlesTask.ExpirationTime = DateTime.Now.AddDays(14);

            try
            {
                ScheduledActionService.Add(newTitlesTask);
            }
            catch (Exception)
            {
                MessageBox.Show("Oops, notifications couldn't be turned on. Check background tasks in your phone Settings to see if this one is disabled or if you're running too many.");
                return false;
            }

            //Probably worked... :)
            return true;
        }

        public void DisableAgent()
        {
            try
            {
                ScheduledActionService.Remove(agentName);
            }
            catch (Exception)
            {
                //Nothing to do here...
            }
        }

        public bool AgentStatus()
        {
            if (newTitlesTask == null || !newTitlesTask.IsEnabled)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void ResetTile()
        {
            ShellTile tile = ShellTile.ActiveTiles.First();

            if (tile != null)
            {
                StandardTileData tileData = new StandardTileData
                {
                    Count = 0,
                    BackContent = ""
                };

                tile.Update(tileData);
            }
        }

        public bool DeviceCanUseAgents()
        {
            try
            {
                long ninetyMb = 94371840;
                long workingSet = (long)DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit");

                if (workingSet < ninetyMb)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // The device has not received the 7.1.1 OS update, which means the device is a 512-MB device.
                return true;
            }
        }

        public void TestAgent()
        {
            ScheduledActionService.LaunchForTest(agentName, TimeSpan.FromSeconds(5));
        }

    }
}
