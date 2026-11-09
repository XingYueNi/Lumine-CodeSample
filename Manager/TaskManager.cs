using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public class GameTask
    {
        public static int id = 0;
        public string Title { get; private set; }//任务名
        public string Description { get; private set; }//任务描述
        public bool IsFlag { get; private set; }//完成状态
        public int Id{ get; private set; }
        public GameTask(string title, string description)
        {
            Title = title;
            Description = description;
            IsFlag = false;
            Id = id++;
        }
        public void CompleteTask()//完成任务
        {
            IsFlag = true;
        }
    }
    private List<GameTask> tasks;
    public TaskManager()
    {
        tasks = new List<GameTask>();
    }
    public void AddTask(string title,string description)
    {

        tasks.Add(new GameTask(title,description) );
    }

    public void CompleteTask(int index)
    {
        if (index >= 0 && index < tasks.Count)
        {
            tasks[index].CompleteTask();
        }
    }

    public GameTask GetTasks()
    {
        if (tasks == null)
        {
            Debug.Log("任务列表未空或者未配置");
            return null;
        }
        foreach(GameTask var in tasks)
        {
            if (var.IsFlag == false)
            {
                return var;
            }
        }
        return new GameTask("全部任务已完成","请自由探索");
    }

    public List<GameTask> GetIncompleteTasks()
    {
        return tasks.FindAll(task => !task.IsFlag);
    }
    public void Display()
    {
        foreach(GameTask var in tasks)
        {
            if (var.IsFlag)
            {
                Debug.Log("已完成的任务:" +"任务Id:"+var.Id+"任务名"+ var.Title +"任务描述" +var.Description +"");
            }
            else
            {
                Debug.Log("任务Id:" + var.Id + "任务名" + var.Title + "任务描述" + var.Description + "");
            }
        }
    }
}
