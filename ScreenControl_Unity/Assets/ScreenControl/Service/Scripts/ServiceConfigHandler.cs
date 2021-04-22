﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Service
{
    public class ServiceConfigHandler : MonoBehaviour
    {
        void Start()
        {
            ClientConnectionManager.Instance.LostAllConnections += OnLostAllConnections;
        }

        private void OnDestroy()
        {
            ClientConnectionManager.Instance.LostAllConnections -= OnLostAllConnections;
        }

        void OnLostAllConnections()
        {
            Core.ConfigManager.LoadConfigsFromFiles();
            Core.ConfigManager.InteractionConfig.ConfigWasUpdated();
            Core.ConfigManager.PhysicalConfig.ConfigWasUpdated();
            Core.ConfigManager.GlobalSettings.CreateVirtualScreen();
            Debug.Log("lost all");
        }
    }
}