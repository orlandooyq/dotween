﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2014/08/27 19:03
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php

using System;
using DG.Tweening.Core;
using DG.Tweening.Core.Easing;
using DG.Tweening.Plugins.Core;
using UnityEngine;

#pragma warning disable 1591
namespace DG.Tweening.Plugins
{
    public enum SpiralMode
    {
        /// <summary>The spiral motion will expand outwards for the whole the tween</summary>
        Expand,
        /// <summary>The spiral motion will expand outwards for half the tween and then will spiral back to the starting position</summary>
        ExpandThenContract
    }

    public struct SpiralOptions
    {
//        public Vector3 axis; // Axis around which the spiral will rotate (replaced by plugin's endValue)
        public float depth; // Additional depth motion (if 0 the spiral will move only along its local X/Y axes)
        public float frequency;
        public float speed; // 360* rotations x second
        public SpiralMode mode; // If TRUE the spiral will expand for half the time and then contract again
        public bool snapping;

        internal float unit;
        internal Quaternion directionQ;
    }

    /// <summary>
    /// Tweens a Vector3 along a spiral.
    /// EndValue represents the direction of the spiral
    /// </summary>
    public class SpiralPlugin : ABSTweenPlugin<Vector3, Vector3, SpiralOptions>
    {
        public static readonly Vector3 DefaultDirection = Vector3.forward;

        public static ABSTweenPlugin<Vector3, Vector3, SpiralOptions> Get()
        {
            return PluginsManager.GetCustomPlugin<SpiralPlugin, Vector3, Vector3, SpiralOptions>();
        }

        public override Vector3 ConvertT1toT2(TweenerCore<Vector3, Vector3, SpiralOptions> t, Vector3 value)
        {
            return value;
        }

        public override void SetRelativeEndValue(TweenerCore<Vector3, Vector3, SpiralOptions> t)
        {
            // This plugin is already relative
        }

        public override void SetChangeValue(TweenerCore<Vector3, Vector3, SpiralOptions> t)
        {
            // Change value is not used.
            // Instead use this method to set correct speed (so that 1 equals to 360* x second)
            // and to get directionQ
            t.plugOptions.speed *= 10 / t.plugOptions.frequency;
            t.plugOptions.directionQ = Quaternion.LookRotation(t.endValue, Vector3.up);
        }

        public override float GetSpeedBasedDuration(SpiralOptions options, float unitsXSecond, Vector3 changeValue)
        {
            return unitsXSecond; // this plugin has already a speed option, so ignore this method
        }

        public override Vector3 Evaluate(SpiralOptions options, Tween t, bool isRelative, DOGetter<Vector3> getter, float elapsed, Vector3 startValue, Vector3 changeValue, float duration)
        {
            // TODO Incremental
            if (t.loopType == LoopType.Incremental) startValue += changeValue * (t.isComplete ? t.completedLoops - 1 : t.completedLoops);

            float elapsedPerc = EaseManager.Evaluate(t, elapsed, 0, 1, duration, t.easeOvershootOrAmplitude, t.easePeriod);
            float pluginElapsed = elapsed * options.speed;
            options.unit = pluginElapsed;

            Vector3 spiral = new Vector3(
                options.unit * Mathf.Cos(pluginElapsed * options.frequency),
                options.unit * Mathf.Sin(pluginElapsed * options.frequency),
                options.depth * elapsedPerc
            );
            spiral = options.directionQ * spiral + startValue; // Orient to chosen direction
            if (options.snapping) {
                spiral.x = (float)Math.Round(spiral.x);
                spiral.y = (float)Math.Round(spiral.y);
                spiral.z = (float)Math.Round(spiral.z);
            }
            return spiral;
        }
    }
}