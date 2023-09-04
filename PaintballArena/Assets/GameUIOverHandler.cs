using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameUIOverHandler : iGameStateListener
{
    [SerializeField] GameObject gradients = null;

    [SerializeField] Image backgroundImage = null;
    [SerializeField] Button tryAgainButton = null;
    [SerializeField] Button exitToMenuButton = null;
    [SerializeField] TMP_Text gameOverText = null;

    float fadeTimeS = 2.0f;
    float animationSpeed = 2.0f;

    [SerializeField] AnimationCurve splatBounceCurve;

    protected override void Start()
    {
        base.Start();
        
        tryAgainButton.interactable = false;
        exitToMenuButton.interactable = false;

        var images = GetComponentsInChildren<Image>(true).ToList();
        var texts = GetComponentsInChildren<TMP_Text>(true).ToList();

        texts.ForEach(txt => { var color = txt.color; color.a = 0; txt.color = color; });
        images.ForEach(i => { var color = i.color; color.a = 0; i.color = color; });
    }

    public override void HandleGameOver()
    {
        StartCoroutine(Animate());
    }

    public override void HandleGamePaused()
    {

    }


    IEnumerator Animate()
    {


        // FADE THE BACKGROUND
        for (float t = 0; t < fadeTimeS; t += Time.deltaTime * animationSpeed)
        {
            var color = backgroundImage.color;
            color.a = t / fadeTimeS;
            var images = gradients.GetComponentsInChildren<Image>(true).ToList();
            
            images.ForEach(i => { var color = i.color; color.a = t / fadeTimeS; i.color = color; });

            backgroundImage.color = color;

            yield return null;
        }

        // SHOW THE GAME OVER TEXT 
        for (float t = 0; t < fadeTimeS; t += Time.deltaTime * animationSpeed)
        {
            var text_color = gameOverText.color;
            text_color.a = t / fadeTimeS;
            gameOverText.color = text_color;

            yield return null;
        }

        // SHOW THE FIRST BUTTON
        for (float t = 0; t < fadeTimeS; t += Time.deltaTime * animationSpeed)
        {
            var bg_color = tryAgainButton.image.color;
            bg_color.a = t / fadeTimeS;
            tryAgainButton.image.color = bg_color;

            var tmpText = tryAgainButton.GetComponentInChildren<TMP_Text>();
            var text_color = tmpText.color;
            text_color.a = t / fadeTimeS;
            tmpText.color = text_color;

            yield return null;
        }

        // SHOW THE SECOND BUTTON
        for (float t = 0; t < fadeTimeS; t += Time.deltaTime * animationSpeed)
        {
            var bg_color = exitToMenuButton.image.color;
            bg_color.a = t / fadeTimeS;
            exitToMenuButton.image.color = bg_color;

            var tmpText = exitToMenuButton.GetComponentInChildren<TMP_Text>();
            var text_color = tmpText.color;
            text_color.a = t / fadeTimeS;
            tmpText.color = text_color;

            yield return null;
        }

        tryAgainButton.interactable = true;
        exitToMenuButton.interactable = true;

    }

    public override void HandleGameUnpaused()
    {
      
    }
}
