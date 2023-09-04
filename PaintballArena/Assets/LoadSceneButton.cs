using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadSceneButton : MonoBehaviour
{
    Button button;
    public string levelName = string.Empty;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => { SceneLoader.LoadScene(levelName); Debug.Log("Click"); });
    }


}
