using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Name : MonoBehaviour
{
    public TextMeshProUGUI keyBoardEnter;
    
    public static string NameText;
   public const float TimeLimit = 30F;
    // timer variable
    private float _timer = 0F;
    // Start is called before the first frame update
    void Start()
    {
        NameText = keyBoardEnter.ToString();
    }
    void Update()
    {
        this._timer += Time.deltaTime;
        // check if it's time to switch scenes
        if (this._timer >= TimeLimit)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
