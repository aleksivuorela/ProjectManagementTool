﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

public class Database
{
    static atlasEntities ctx = new atlasEntities();
    public Database(){}
    #region PROJECTS
    /// <summary>
    /// Gets project object from DB with the given project ID.
    /// </summary>
    public static project GetProjectFromDb(int id)
    {
        try
        {
            using (var db = new atlasEntities())
            {
                var ret = from p in db.projects
                          where p.id == id
                          select p;
                var project = ret.FirstOrDefault();
                if (project != null)
                    return project;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return null;
    }

    /// <summary>
    /// Gets all projects for given user from DB.
    /// </summary>
    public static List<project> GetAllProjectsForUser(string username)
    {
        try
        {
            using (var db = new atlasEntities())
            {
                var ret = from u in db.users
                          where u.username == username
                          select u;
                var user = ret.FirstOrDefault();
                if (user != null)
                {
                    return user.projects.ToList();
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        return null;
    }
    #endregion PROJECTS

    #region GANTT
    public static IEnumerable<task> GetChildren(int inputID)
    {
        // haetaan vanhempi
        var query = ctx.tasks.Where(x => x.id == inputID).ToList();
        // loopissa haetaan queryn seuraava rivi ja etsitään kaikki sen lapset, jotka lisätään unionilla queryn loppuun. 
        // queryn koko kasvaa loopin aikana kunnes kaikki lapset on haettu. 
        for (int i = 0; i < query.Count(); i++)
        {
            int currentID = query.ElementAt(i).id;
            var children = ctx.tasks.Where(x => x.task_id == currentID).ToList();
            query = query.Union(children).ToList();
        }

        return query;
    }

    public static IEnumerable<int> GetChildrenIds(int inputID)
    {
        using (var db = new atlasEntities())
        {
            // haetaan vanhempi
            var query = db.tasks.Where(x => x.id == inputID).Select(z => z.id).ToList();

        // loopissa haetaan queryn seuraava rivi ja etsitään kaikki sen lapset, jotka lisätään unionilla queryn loppuun. 
        // queryn koko kasvaa loopin aikana kunnes kaikki lapset on haettu. 
        for (int i = 0; i < query.Count(); i++)
        {
            int currentID = query.ElementAt(i);
            var children = db.tasks.Where(x => x.task_id == currentID).Select(z => z.id).ToList();
            query = query.Union(children).ToList();
        }

        return query;
        }
    }

    public static List<Task> GetProjectWorkingHours(int projectID)
    {
        using (var db = new atlasEntities())
        {
            List<Task> TopTasks = new List<Task>();
            Task tempTask;
            IEnumerable<int> tasks;
            int tempHours;

            // haetaan ensimmäisen polven taskit, joilla task_id == null
            var majorTasksQuery = from c in db.tasks
                                  where c.project_id == projectID && c.task_id == null
                                  select c;

            // iteroidaan löydettyjen taskien läpi
            foreach (var item in majorTasksQuery)
            {
                // luo tilapäisolio taskin tiedoilla
                tempTask = new Task(item.id, item.name);
                // hae taskin kaikki lapset listaan
                tasks = GetChildrenIds(item.id);
                // hae työtunnit jokaisesta listan taskista tilapäisolion tietoihin
                tempHours = GetWorkingHours(tasks);
                // varmistetaan että työtunteja on olemassa
                if (tempHours > 0)
                {
                    tempTask.Duration = tempHours;
                    // tallennetaan tilapäisolio pysyvään listaan
                    TopTasks.Add(tempTask);
                }
            }
            // palauta lista

            return TopTasks;
        }
       
    }

    public static List<Task> GetProjectWorkingHoursForUser(int projectID, int userID)
    {
        try
        {
            using (var db = new atlasEntities())
            {
                List<Task> TopTasks = new List<Task>();
                Task tempTask;
                IEnumerable<int> tasks;
                int tempHours;

                // haetaan ensimmäisen polven taskit, joilla task_id == null
                var majorTasksQuery = from c in db.tasks
                                      where c.project_id == projectID && c.task_id == null
                                      select c;

                // iteroidaan löydettyjen taskien läpi
                foreach (var item in majorTasksQuery)
                {
                    // luo tilapäisolio taskin tiedoilla
                    tempTask = new Task(item.id, item.name);
                    // hae taskin kaikki lapset listaan
                    tasks = GetChildrenIds(item.id);
                    // hae työtunnit jokaisesta listan taskista tilapäisolion tietoihin
                    tempHours = GetWorkingHours(tasks, userID);
                    // varmistetaan että työtunteja on olemassa
                    if (tempHours > 0)
                    {
                        tempTask.Duration = tempHours;
                        // tallennetaan tilapäisolio pysyvään listaan
                        TopTasks.Add(tempTask);
                    }
                }
                return TopTasks;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    // hakee taskien id-listan perusteella yhteistuntimäärän taskeista
    protected static int GetWorkingHours(IEnumerable<int> tasks)
    {
        using (var db = new atlasEntities())
        {
            try
            {
                var query = (from dtask in db.donetasks
                             join t in tasks on dtask.task_id equals t
                             where dtask.task_id == t
                             select dtask.worktime);

                if (query.Count() > 0)
                {
                    return query.Sum();
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    // hakee taskien id-listan ja userID:n perusteella yhteistuntimäärän taskeista tietylle käyttäjälle
    protected static int GetWorkingHours(IEnumerable<int> tasks, int userID)
    {
        using (var db = new atlasEntities())
        {
            try
            {
                var query = (from dtask in db.donetasks
                             join t in tasks on dtask.task_id equals t
                             where dtask.task_id == t && dtask.user_id == userID
                             select dtask.worktime);

                if (query.Count() > 0)
                {
                    return query.Sum();
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }


    public static List<Task> GetTasks(int projectID)
    {
        // hae kaikki projektiin liittyvien donetaskien ja taskien olennainen data
        var donetasks = (from dtask in ctx.donetasks
                         join t in (from c in ctx.tasks where c.project_id == projectID select c) on dtask.task_id equals t.id
                         where dtask.task_id == t.id
                         select new { TaskID = t.id, dTaskID = dtask.id, Date = dtask.date, Worker = dtask.user_id, WorkTime = dtask.worktime, Name = t.name, Parent = t.task_id });

        Task tempTask;

        List<Task> Tasks = new List<Task>();

        // luo datasta uusia Task-olioita. 
        // Huom, Task != task
        // Task on oma luokkansa joka sisältää molempien taulujen dataa 
        int i = 1;
        foreach (var dtask in donetasks)
        {
            tempTask = new Task(dtask.TaskID, dtask.Name + " - " + dtask.Worker, dtask.Date.Value.Day + "-" + dtask.Date.Value.Month + "-" + dtask.Date.Value.Year, dtask.WorkTime, dtask.Parent, i);
            Tasks.Add(tempTask);
            i++;
        }

        // asetetaan Taskeille parentit ganttia varten
        foreach (Task t in Tasks)
        {
            if (t.Parent != null)
            {
                t.GanttParentId = (from d in Tasks where d.ID == t.Parent select d.GanttId).FirstOrDefault();
            }
        }

        return Tasks;
    }
    #endregion
}