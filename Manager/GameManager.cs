using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public  event Action<float> OnLoadProgress;      // 进度 0~1
    public  event Action OnLoadStarted;
    public  event Action OnLoadCompleted;

    public bool IsLoading { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }
    private void OnEnable()
    {
        SaveDataManager.OnSaveSuccess += ExitGame;
    }
    private void OnDisable()
    {
        SaveDataManager.OnSaveSuccess -= ExitGame;
    }
    //public void PauseGame();       // 暂停
    // public void ResumeGame();      // 恢复
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void LoadSceneAsync(string sceneName)
    {
        if (IsLoading)
        {
            Debug.LogWarning("正在加载场景中");
            return;
        }
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        IsLoading = true;
        OnLoadStarted?.Invoke();
        Debug.Log($"开始加载场景：{sceneName}");
        // 1. 异步加载场景（但不立即激活）
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;   // 阻止自动激活，等我们控制
        // 2. 加载进度回调
        while (!op.isDone)
        {
            // Unity 的 progress 到 0.9 就停了（剩下 0.1 是激活场景）
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            OnLoadProgress?.Invoke(progress);
            // 进度到 0.9 时，允许激活场景
            if (op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }
        Debug.Log($"场景加载完成：{sceneName}");
        IsLoading = false;
        OnLoadCompleted?.Invoke();
    }
    public void ExitGame()//退出游戏
    {
        Debug.Log("保存完成，退出游戏");
        Application.Quit();
    }
    public static IEnumerator DelayBool(Animator animator, string var)
    {
        animator.SetBool(var, true);
        yield return new WaitForSeconds(2f);
        animator.SetBool(var, false);
        yield break;
    }
    public static IEnumerator SetFloat(Animator animator, string x, float targer)
    {
        float smoothSpeed = 0.0375f;
        float currentNum = animator.GetFloat(x);
        while (true)
        {
            if (currentNum == targer)
                yield break;
            if (targer == -1)
            {
                currentNum -= smoothSpeed;
                if (currentNum <= -1)
                    currentNum = -1;
                animator.SetFloat(x, currentNum);
            }
            if (targer == 1)
            {
                currentNum += smoothSpeed;
                if (currentNum >= 1)
                    currentNum = 1;
                animator.SetFloat(x, currentNum);
            }

            yield return null;
        }
    }
    public static IEnumerator TimeScale()//顿帧
    {
        Time.timeScale = 0.4f;
        yield return new WaitForSeconds(0.01f);
        Time.timeScale = 1f;
        yield break;
    }
    public static IEnumerator ExecutionSlowMotion()//失衡慢动作
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
    }
    public static IEnumerator TimeLinePlay(PlayableDirector animation)
    {
        Debug.Log("播放处决动画");
        animation.Play();
        while (animation.state != PlayState.Playing)
        {
            yield return null;
        }
        Debug.Log("动画播放完成");
        yield break;
    }
    public static IEnumerator BlackScreenTransition(Image black,float timer)
    {
        yield return new WaitForSeconds(timer);
        float Alpha=0;
        while (true)
        {
            Alpha += 0.01f;
            black.color = new Color(0, 0, 0, Alpha);
            if (Alpha > 100) break;
        }
        yield return new WaitForSeconds(2f);
        while (true)
        {
            Alpha -= 0.01f;
            black.color = new Color(0, 0, 0, Alpha);
            if (Alpha <0) break;
        }
        yield break;
    }
  
}
    
