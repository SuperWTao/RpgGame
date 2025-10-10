using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScene : MonoBehaviour
{
    private void InitPanel()
    {
        PanelManager.Init();
    }

    private void Awake()
    {
        InitPanel();
    }

    private void OnDestroy()
    {
        PanelManager.Clear();
    }
}
