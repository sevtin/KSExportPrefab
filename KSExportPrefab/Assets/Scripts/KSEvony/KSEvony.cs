using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EvonyDebug
{
    public static bool enableLog = false;

    public static void Log(object message) { }
    public static void Log(object message, UnityEngine.Object context) { }
    public static void LogError(object message) { }
    public static void LogError(object message, UnityEngine.Object context) { }
    public static void LogException(Exception exception) { }
    public static void LogException(Exception exception, UnityEngine.Object context) { }
    public static void LogWarning(object message) { }
    public static void LogWarning(object message, UnityEngine.Object context) { }
}

public class KSSpriteInfo
{
    public Rect rect;
    public Vector4 border;
    public Vector2 pivod;
    public Vector4 padding; //{x = left, y = bottom, z = right, w = top}

    public System.WeakReference spriteRef;
}

public static class KSExtension
{
    public static KSSpriteInfo GetSpriteInfo(this Sprite sprite)
    {
        return new KSSpriteInfo();
    }
}
