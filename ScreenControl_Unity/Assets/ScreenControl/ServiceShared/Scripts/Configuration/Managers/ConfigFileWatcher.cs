﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Ultraleap.ScreenControl.Core
{
    public class ConfigFileWatcher : MonoBehaviour
    {
        private FileSystemWatcher interactionWatcher;
        private FileSystemWatcher physicalWatcher;

        bool fileChanged = false;
        bool fileDeleted = false;

        private void Start()
        {
            interactionWatcher = new FileSystemWatcher();
            interactionWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            interactionWatcher.NotifyFilter = NotifyFilters.LastWrite;
            interactionWatcher.NotifyFilter = NotifyFilters.LastAccess;
            interactionWatcher.Filter = InteractionConfigFile.ConfigFileNameS;
            interactionWatcher.Changed += new FileSystemEventHandler(FileUpdated);
            interactionWatcher.Deleted += new FileSystemEventHandler(FileDeleted);
            interactionWatcher.IncludeSubdirectories = true;
            interactionWatcher.EnableRaisingEvents = true;


            physicalWatcher = new FileSystemWatcher();
            physicalWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
            physicalWatcher.NotifyFilter = NotifyFilters.LastWrite;
            physicalWatcher.NotifyFilter = NotifyFilters.LastAccess;
            physicalWatcher.Filter = PhysicalConfigFile.ConfigFileNameS;
            physicalWatcher.Changed += new FileSystemEventHandler(FileUpdated);
            physicalWatcher.Deleted += new FileSystemEventHandler(FileDeleted);
            physicalWatcher.IncludeSubdirectories = true;
            physicalWatcher.EnableRaisingEvents = true;
        }

        private void Update()
        {
            if(fileDeleted)
            {
                ConfigFileUtils.CheckForConfigDirectoryChange();
                interactionWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                physicalWatcher.Path = ConfigFileUtils.ConfigFileDirectory;
                fileChanged = true;
            }

            if (fileChanged)
            {
                fileChanged = false;
                ConfigManager.LoadConfigsFromFiles();
                ConfigManager.InteractionConfig.ConfigWasUpdated();
                ConfigManager.PhysicalConfig.ConfigWasUpdated();
            }
        }

        private void FileUpdated(object source, FileSystemEventArgs e)
        {
            // save that it changed, this is on a thread so needs the reaction to be thread safe
            fileChanged = true;
        }

        private void FileDeleted(object source, FileSystemEventArgs e)
        {
            // save that it changed, this is on a thread so needs the reaction to be thread safe
            fileDeleted = true;
        }
    }
}