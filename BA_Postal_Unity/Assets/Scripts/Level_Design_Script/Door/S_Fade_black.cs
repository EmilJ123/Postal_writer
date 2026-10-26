using UnityEngine;
using System.Collections;
public class S_Fade_black : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 2f;

    public IEnumerator FadeOut()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
}