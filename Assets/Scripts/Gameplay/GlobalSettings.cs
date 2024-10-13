using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] private Color highlightedColor;
    
    public Color HighlightedColor => highlightedColor;
    
    public static GlobalSettings Instance;

    private void Awake()
    {
        Instance = this;
    }
}
