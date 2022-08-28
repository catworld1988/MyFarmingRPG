using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ObscuringItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void FadeOut()
    {
        StartCoroutine(FadeInRoutine());
    }

    public void FadeIn()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeInRoutine() //渐入  透明-不透明
    {
        float currentAlpha = spriteRenderer.color.a;
        float distance = 1f - currentAlpha;    //1-0.6


        while (1f - currentAlpha > 0.01f)
        {
            currentAlpha = currentAlpha + distance / Settings.fadeInSeconds * Time.deltaTime;
            spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }

        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    private IEnumerator FadeOutRoutine() //渐出  不透明-透明
    {
        float currentAlpha = spriteRenderer.color.a;
        float distance = currentAlpha - Settings.targetAlpha;


        while (currentAlpha - Settings.targetAlpha > 0.01f)
        {
            currentAlpha = currentAlpha - distance / Settings.fadeOutSeconds * Time.deltaTime;
            spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }

        spriteRenderer.color = new Color(1f, 1f, 1f, Settings.targetAlpha);
    }
}