using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
   public void NextScene()
   {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
   }
   public void Settings()
   {
      SceneManager.LoadScene("Settings");
   }
   public void QuitGame()
   {
      Application.Quit();
   }
   public void Restart()
   {
      SceneManager.LoadScene("Menu");
   }

   public void TeamSelectScene()
   {
      SceneManager.LoadScene("JoinTeam");
   }

   public void LoadScene(string scene)
   {
      SceneManager.LoadScene(scene);
   }
}
    