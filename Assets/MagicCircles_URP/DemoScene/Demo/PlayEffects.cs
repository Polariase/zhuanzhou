using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayEffects : MonoBehaviour
{
    public GameObject [] Effects;
    private int num;

    void Start()
    {
        UpdateEffects();
    }

  /*  void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            ShowNextEffect();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            ShowLastEffect();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ShowNowEffect();
        }
    }   
  */

    private void UpdateEffects()
    {
        if (num >= Effects.Length)
            num = 0;
        else if (num < 0)
            num = Effects.Length - 1;

        foreach (var effect in Effects)
            effect.SetActive(false);

    Effects[num].SetActive(true);
    }

    public void ShowNextEffect()
    {
        num++;
        UpdateEffects();
    }

    public void ShowLastEffect()
    {
        num--;
        UpdateEffects();
    }

    public void ShowNowEffect()
    {
        UpdateEffects();
    }

}
