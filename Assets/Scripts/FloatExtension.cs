using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatExtensions
{
    public static float Fallout(this float origin, float target, float damp)
    {
        return Mathf.Lerp(origin, target, 1 - Mathf.Exp(-damp * Time.deltaTime));
    }

    public static float FalloutUnscaled(this float origin, float target, float damp)
    {
        return Mathf.Lerp(origin, target, 1 - Mathf.Exp(-damp * Time.unscaledDeltaTime));
    }
}

public static class Vector3Extensions
{
    public static Vector3 Fallout(this Vector3 origin, Vector3 target, float damp)
    {
        return Vector3.Lerp(origin, target, 1 - Mathf.Exp(-damp * Time.deltaTime));
    }

    public static Vector3 FalloutUnscaled(this Vector3 origin, Vector3 target, float damp)
    {
        return Vector3.Lerp(origin, target, 1 - Mathf.Exp(-damp * Time.unscaledDeltaTime));
    }
}

public static class QuaternionExtensions
{
    public static Quaternion Fallout(this Quaternion origin, Quaternion target, float damp)
    {
        return Quaternion.Slerp(origin, target, 1 - Mathf.Exp(-damp * Time.deltaTime));
    }

    public static Quaternion FalloutUnscaled(this Quaternion origin, Quaternion target, float damp)
    {
        return Quaternion.Slerp(origin, target, 1 - Mathf.Exp(-damp * Time.unscaledDeltaTime));
    }
}