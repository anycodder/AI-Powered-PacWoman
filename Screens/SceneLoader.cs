using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickButton(string buttonName)
    {
        Debug.Log(buttonName);
        switch (buttonName)
        {
            case "Scene1":
                SceneManager.LoadScene("PacWoman");
                break;
            case "Scene2":
                SceneManager.LoadScene("PW2");
                break;
            case "Scene3":
                SceneManager.LoadScene("PW3");
                break;
            case "Scene4":
                SceneManager.LoadScene("PW4");
                break;
            default:
                Debug.LogError("Unknown button name: " + buttonName);
                break;
        }
    }
}
