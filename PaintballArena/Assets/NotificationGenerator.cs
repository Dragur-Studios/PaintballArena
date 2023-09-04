using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationGenerator : MonoBehaviour
{
    [SerializeField] GameObject popupPrefab;

    public void GeneratePopup(string message, Color color = default(Color))
    {
        var go = Instantiate(popupPrefab);
        go.transform.SetParent(transform, false);

        var tmpText = go.GetComponent<TMP_Text>();
        tmpText.text = message;

        if (color.Compare(default(Color)))
            color = Color.white;

        tmpText.color = color;

    }
}
