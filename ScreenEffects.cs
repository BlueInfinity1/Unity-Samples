using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//This class will contain different camera-related effects, such as screen shakes, fade outs, glitch effects, etc.
public class ScreenEffects : MonoBehaviour
{
    [SerializeField] GameObject screenFadeVeil;
    Image veilImage; 

    void Start()
    {        
        screenFadeVeil.SetActive(false);
        veilImage = screenFadeVeil.GetComponent<Image>();
    }

    //Initiate a screen fade in or out with the given fade speed
    public void InitiateScreenFade(bool fadeOut = true, float fadeSpeed = 10.0f)
    {        
        StartCoroutine(FadeScreen(fadeOut, fadeSpeed));
    }

    IEnumerator FadeScreen(bool fadeOut, float fadeSpeed)
    {
        screenFadeVeil.SetActive(true);
        
        Color veilColor = veilImage.color;
        veilColor = new Color(veilColor.r, veilColor.g, veilColor.b, fadeOut ? 0 : 1);

        int fadeMultiplier = fadeOut ? 1 : -1;
        while (true)
        {
            veilColor = new Color(veilColor.r, veilColor.g, veilColor.b, veilColor.a + fadeMultiplier * fadeSpeed * Time.deltaTime);
            veilImage.color = veilColor;

            if ((fadeOut && veilColor.a >= 1) || (!fadeOut && veilColor.a <= 0))
                break;

            yield return null;
        }
    }
}
