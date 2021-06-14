﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ultraleap.TouchFree
{
    [DefaultExecutionOrder(-10)]
    public class TransparentWindow : MonoBehaviour
    {
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
        static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("Dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        const int GWL_STYLE = -16;
        const uint WS_POPUP = 0x80000000;
        const uint WS_VISIBLE = 0x10000000;
        const int HWND_TOPMOST = -1;

        const uint WS_EX_LAYERED = 0x00080000;
        const uint WS_EX_TRANSPARENT = 0x00000020;

        const int SWP_NOACTIVATE = 16;
        const int SWP_FRAMECHANGED = 32;
        const int SWP_SHOWWINDOW = 64;

        const int LWA_ALPHA = 2;

        public IntPtr hwnd;

        private Vector2 position;

        [HideInInspector] public bool clickThroughEnabled = false;

        void Start()
        {
#if !UNITY_EDITOR // You really don't want to enable this in the editor..
		hwnd = GetActiveWindow();
#endif
            clickThroughEnabled = false;
            EnableClickThrough();
        }

        public void DisableClickThrough()
        {
#if !UNITY_EDITOR
        if (!clickThroughEnabled)
        {    return;
        }
        clickThroughEnabled = false;
        SetConfigWindow(true);
#endif
        }

        public void EnableClickThrough()
        {
#if !UNITY_EDITOR
        if (clickThroughEnabled || ctiActive)
        {            return;
        }
        clickThroughEnabled = true;
        SetCursorWindow(true);
#endif
        }

        private void OnEnable()
        {
            ScreenManager.UIActivated += DisableClickThrough;
            ScreenManager.UIDeactivated += EnableClickThrough;

            CallToInteractController.OnCTIActive += CTIActivated;
            CallToInteractController.OnCTIInactive += CTIDeactivated;
        }

        private void OnDisable()
        {
            ScreenManager.UIActivated -= DisableClickThrough;
            ScreenManager.UIDeactivated -= EnableClickThrough;

            CallToInteractController.OnCTIActive -= CTIActivated;
            CallToInteractController.OnCTIInactive -= CTIDeactivated;
        }

        void SetCursorWindow(bool setResolution)
        {
            if (setResolution)
            {
                Screen.SetResolution(TouchFreeMain.CursorWindowSize, TouchFreeMain.CursorWindowSize, FullScreenMode.Windowed);
            }

            SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
            SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA);// Transparency=51=20%
            SetWindowPos(hwnd, HWND_TOPMOST, Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y),
                TouchFreeMain.CursorWindowSize, TouchFreeMain.CursorWindowSize, SWP_FRAMECHANGED | SWP_SHOWWINDOW | SWP_NOACTIVATE);

            var margins = new MARGINS() { cxLeftWidth = -1 };
            SetWindowLong(hwnd, -20, WS_EX_LAYERED | WS_EX_TRANSPARENT);
            SetWindowPos(hwnd, HWND_TOPMOST, Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y),
                TouchFreeMain.CursorWindowSize, TouchFreeMain.CursorWindowSize, SWP_FRAMECHANGED | SWP_SHOWWINDOW | SWP_NOACTIVATE);

            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        void SetConfigWindow(bool setResolution)
        {
            if (setResolution)
            {
                Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.Windowed);
            }

            SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
            SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA);
            SetWindowPos(hwnd, -2, 0, 0, Display.main.systemWidth, Display.main.systemHeight, SWP_FRAMECHANGED | SWP_SHOWWINDOW | SWP_NOACTIVATE);

            var margins = new MARGINS();

            // get the current -20 window and remove the layerd and transparent parts
            long style = GetWindowLong(hwnd, -20);
            style &= ~WS_EX_TRANSPARENT;
            style &= ~WS_EX_LAYERED;

            SetWindowLong(hwnd, -20, style);
            SetWindowPos(hwnd, -2, 0, 0, Display.main.systemWidth, Display.main.systemHeight, SWP_FRAMECHANGED | SWP_SHOWWINDOW | SWP_NOACTIVATE);

            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        void Update()
        {
#if !UNITY_EDITOR
		if (clickThroughEnabled)
		{
            int xPos = Mathf.RoundToInt(position.x);
            int yPos = Mathf.RoundToInt(position.y);
            if (xPos + TouchFreeMain.CursorWindowSize > 0 && xPos < Display.main.systemWidth && yPos + TouchFreeMain.CursorWindowSize > 0 && yPos < Display.main.systemHeight)
            {
                SetWindowPos(hwnd,
                    HWND_TOPMOST,
                    Mathf.RoundToInt(position.x),
                    Mathf.RoundToInt(position.y),
                    TouchFreeMain.CursorWindowSize,
                    TouchFreeMain.CursorWindowSize,
                    SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
            }
		}
#endif
        }

        public void SetPosition(Vector2 value)
        {
            position = value;

            position.x = position.x - (TouchFreeMain.CursorWindowSize / 2);
            position.y = Display.main.systemHeight - position.y - (TouchFreeMain.CursorWindowSize / 2);
        }


        bool ctiActive = false;
        void CTIActivated()
        {
            ctiActive = true;
            DisableClickThrough();
        }

        void CTIDeactivated()
        {
            ctiActive = false;
            if (ScreenManager.Instance != null && !ScreenManager.Instance.isActive)
            {
                EnableClickThrough();
            }
        }

        private void OnApplicationQuit()
        {
            SetConfigWindow(true);
        }
    }
}