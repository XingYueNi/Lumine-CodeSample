using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TaskManager;

public  class  TipShow : MonoBehaviour
{
    private TaskManager taskManager;
    private List<GameTask> tasks=new List<GameTask>();
    private GameObject[] Tip;
    private GameTask currentTask;//��ǰ����
    private TMPro.TMP_Text title;
    private TMPro.TMP_Text description;
    bool flag_1 = false, flag_2 = false, flag_3 = false, flag_4 = false;
    private bool uiself=false;//UI�Ƿ����
    private void Awake()
    {
        taskManager = GetComponent<TaskManager>();
        taskManager.AddTask("��������","ʹ��WASD�����ƶ�");
        taskManager.AddTask("��������","ʹ�����������һ�ι���");
        taskManager.AddTask("��������","ʹ���Ҽ���");
        taskManager.AddTask("С������","��ǰ�ƶ�������̨ǰ��");
        taskManager.AddTask("С������","���ܵ�ǰ����");
        GetTask();
    }
    private void Update()
    {
        if(uiself==false)
        {
            uiself = true;
            StartCoroutine(UiReload());
        }
        if (currentTask.Id == 0)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                flag_1 = true;
            } 
            if (Input.GetKeyDown(KeyCode.A))
            {
                flag_2 = true;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                flag_3 = true;
            } 
            if (Input.GetKeyDown(KeyCode.D))
            {
                flag_4 = true;
            }
            if (flag_1 && flag_2 && flag_3 && flag_4)
            {
                PostTask();
            }
        }
        if (currentTask.Id == 1)
        {

        }
        if (currentTask.Id == 2)
        {
            if (Input.GetMouseButtonDown(1))
            {
                PostTask();
            }
        }
    }
    public void GetTask()
    {
        
        currentTask = taskManager.GetTasks();
        if (currentTask == null) return;
        title.text = currentTask.Title;
        description.text = currentTask.Description;
        taskManager.Display();
        if (currentTask.Id > 0)
        {
            Tip[currentTask.Id-1].SetActive(false);
        }
        if(currentTask.Id<4)
        Tip[currentTask.Id].SetActive(true);

    }
    public void PostTask()
    {
        taskManager.CompleteTask(currentTask.Id);//�ύ������ȡ������
        GetTask();
    }
    IEnumerator UiReload()
    {
        if (GameObject.Find("PlayerUI"))
        {
            //����ҵ�
            GameObject _gameObject = GameObject.Find("PlayerUI").transform.Find("Task").gameObject;
            title = _gameObject.transform.Find("Title").gameObject.GetComponent<TMPro.TMP_Text>();
            description=_gameObject.transform.Find("description").gameObject.GetComponent<TMPro.TMP_Text>();
            yield break;
        }
        else
            yield return new WaitForSeconds(2f);//�������������
        yield break;
    }
}
