﻿using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Configuration
{
    [Serializable]
    public class PhysicalConfigInternal
    {
        public float ScreenHeightMm = 330f;
        public Vector3 LeapPositionRelativeToScreenBottomMm = new Vector3(0f, -120f, -250f);
        public Vector3 LeapRotationD = Vector3.Zero;
        public float ScreenRotationD = 0f;

        public int ScreenWidthPX = 0;
        public int ScreenHeightPX = 0;

        public PhysicalConfig ForApi()
        {
            return new PhysicalConfig()
            {
                LeapPositionRelativeToScreenBottomM = LeapPositionRelativeToScreenBottomMm / 1000f,
                LeapRotationD = LeapRotationD,
                ScreenHeightM = ScreenHeightMm / 1000f,
                ScreenHeightPX = ScreenHeightPX,
                ScreenRotationD = ScreenRotationD,
                ScreenWidthPX = ScreenWidthPX
            };
        }

        public PhysicalConfigInternal()
        {
            this.ScreenHeightMm = 330f;
            this.LeapPositionRelativeToScreenBottomMm = new Vector3(0f, -120f, -250f);
            this.LeapRotationD = Vector3.Zero;
            this.ScreenRotationD = 0f;

            this.ScreenWidthPX = 0;
            this.ScreenHeightPX = 0;
        }

        public PhysicalConfigInternal(PhysicalConfig fromFile)
        {
            this.ScreenHeightMm = fromFile.ScreenHeightM * 1000f;
            this.LeapPositionRelativeToScreenBottomMm = fromFile.LeapPositionRelativeToScreenBottomM * 1000f;

            this.LeapRotationD = fromFile.LeapRotationD;
            this.ScreenRotationD = fromFile.ScreenRotationD;

            this.ScreenWidthPX = fromFile.ScreenWidthPX;
            this.ScreenHeightPX = fromFile.ScreenHeightPX;
        }
    }
}