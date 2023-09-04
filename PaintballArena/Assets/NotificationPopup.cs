using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationPopup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();

        StartCoroutine(Animate());    
    }

    float animationSpeed = 1.0f;
    float animationTime = 2.0f;

    TMP_Text text;

    public AnimationCurve movementCurve;

    IEnumerator Animate()
    {
        for (float t = 0; t < animationTime; t += animationSpeed * Time.deltaTime)
        {
            var rectTransform = transform.GetComponent<RectTransform>();
            var position = rectTransform.anchoredPosition;
            position.y += movementCurve.Evaluate(t) * 40.0f * Time.deltaTime;
            rectTransform.anchoredPosition = position;

            var color = text.color;
            color.a = 1.0f - t;
            text.color = color;

            yield return null;
        }
    }


}
