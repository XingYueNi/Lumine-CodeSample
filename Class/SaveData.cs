using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData
{
    public float playSeconds;      // 累计游玩时长（秒）
    public string saveTimeIso;     // 最近一次存档时间（DateTime "o" 格式，便于算相对时间）
    public int index;
    public byte[] screenshotData;//截图

    public Vector3 PlayerPosition;
    public Vector3 PlayerRotation;
    public float currentHealth;
    public float currentPosture;
    public bool isNew=true;//新创建的存档不需要从存档恢复玩家数据

}
