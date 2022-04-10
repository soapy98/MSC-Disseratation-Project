using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSet : MonoBehaviour
{
    private GameObject[] _canvas;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        _canvas = GameObject.FindGameObjectsWithTag("Menu");
        foreach (var va in _canvas)
        {
            va.GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
