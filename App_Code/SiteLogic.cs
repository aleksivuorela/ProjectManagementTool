﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SiteLogic
/// </summary>
/// 
namespace Atlas
{
    public class SiteLogic
    {
        Atlas.Database db;

        public SiteLogic()
        {
            db = new Database();
        }

        public string GetTasksInJson(int projectID)
        {
            string tasksJson = "{data:[";
            List<Task> tasks = db.GetTasks(projectID);
            Task tempTask;
            for (int i = 0; i < tasks.Count; i++)
            {
                //tasksJson += JsonConvert.SerializeObject(tasks.ElementAt(i));
                tempTask = tasks.ElementAt(i);
                tasksJson += "{id:" + tempTask.GanttId + @", text:""" + tempTask.Text + @""", start_date:""" + tempTask.StartDate + @""", duration:" + tempTask.Duration;

                if (tempTask.Parent != null)
                {
                    tasksJson += ", parent:" + tempTask.GanttParentId + "}";
                }
                else
                {
                    tasksJson += "}";
                }
                if (i != tasks.Count - 1)
                {
                    tasksJson += ",";
                }
                else
                {
                    tasksJson += "]}";
                }
            }

            return tasksJson;
        }
    }
}