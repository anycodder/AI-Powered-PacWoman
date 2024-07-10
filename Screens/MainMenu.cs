using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameManager gameManager;  
    public Functions func {get; private set; }
    
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        func  = FindObjectOfType<Functions>();
    }

    /*private void StopAllCoroutinesInGame()
    {
        func.StopCoroutine("MoveCoroutine");
    }*/
    
    public void OnClickButtonMinimax()
    {
        //gameManager.of=false;
        func.buttonName = "ButtonMinimax";
        gameManager.HideButtons();
    }

    public void OnClickButtonAlpha_Beta()
    {
        //gameManager.of=false;
        func.buttonName = "ButtonAlpha_Beta";
        gameManager.HideButtons();
    }

    public void OnClickButtonExpectimax()
    {
       // gameManager.of=false;
        func.buttonName = "ButtonExpectimax";
        gameManager.HideButtons();
    }

    public void OnClickButtonUser()
    {
        func.buttonName = "User";
        gameManager.HideButtons();
    }

    public void ReturnToMainMenu()
    {
        //StopAllCoroutinesInGame();
        SceneManager.LoadScene("MainMenu");
    }
    public void NewGame()
    {
        SceneManager.LoadScene("NewGame");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void GoToLogin()
    {
        SceneManager.LoadScene("Login");
    }
    public void Restart()
    {
        gameManager.score = 0; // Ensure score is reset to 0
        gameManager.restart=true;
        gameManager.StartCoroutine(gameManager.StartGameAfterAudio());
        func.buttonName="";
        func.UpdateRotation();
    }
    public void GoToLevel1()
    {
      //  StopAllCoroutinesInGame();
        SceneManager.LoadScene("PacWoman 1");
    }
    public void GoToLevel2()
    {
      //  StopAllCoroutinesInGame();
        SceneManager.LoadScene("PW2");
    }
    public void GoToLevel3()
    {
      //  StopAllCoroutinesInGame();
        SceneManager.LoadScene("PW3");
    }
    public void GoToLevel4()
    {
       // StopAllCoroutinesInGame();
        SceneManager.LoadScene("PW4");
    }
}
