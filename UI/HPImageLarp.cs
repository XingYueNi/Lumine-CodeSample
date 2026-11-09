using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPImageLarp : MonoBehaviour
{
    public Image _image;//缓冲条在下面
    public Image _larpImage;
    public float lerpSpeed;
    public float delay = 0.2f;//延迟时间
    protected  int n;
   public void OnHpChange(float newHealth)
    {
        // Debug.Log("血量更新");
        if (!_image.gameObject.activeSelf || !_larpImage.gameObject.activeSelf) return;
       StartCoroutine(ImageLerp(newHealth));
    }
   public IEnumerator ImageLerp(float target)
    {
        if (_image.fillAmount == 0 && target < 0) yield break;
        yield return new WaitForSeconds(delay);
        n = 8;
        _image.fillAmount = target / 100;
       // Debug.Log("目标值为"+target+"协程开始");
        lerpSpeed = _larpImage.fillAmount - _image.fillAmount;
        while (--n>0)
        {
            if (lerpSpeed>0)
                _larpImage.fillAmount -= lerpSpeed/=2;
            yield return null;
        }
        _larpImage.fillAmount = _image.fillAmount;
      //  Debug.Log("协程结束");
        yield break;
    }
}
