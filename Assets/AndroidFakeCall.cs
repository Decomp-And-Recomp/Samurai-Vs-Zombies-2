using System;
using UnityEngine;
using UniDebug = UnityEngine.Debug;

#if !UNITY_ANDROID || UNITY_EDITOR
public class AndroidJavaObject : IDisposable
{
    readonly string name;

    public AndroidJavaObject(string str, params object[] args)
    {
        name = str;
    }

    public void Call(string str, params object[] args)
    {
        UniDebug.Log("Call from: \"" + name + "\" to: " + str);
    }

    public T Call<T>(string str, params object[] args)
    {
        UniDebug.Log("RETURNING Call from: \"" + name + "\" to: " + str);

        return default(T);
    }

    public void CallStatic(string str, params object[] args)
    {
        UniDebug.Log("CallStatic from: \"" + name + "\" to: " + str);
    }

    public T CallStatic<T>(string str, params object[] args)
    {
        UniDebug.Log("RETURNING CallStatic from: \"" + name + "\" to: " + str);
        return default(T);
    }

    public T Get<T>(string str)
    {
        UniDebug.Log("RETURNING Get from: \"" + name + "\" in: " + str);

        return default(T);
    }

    public T GetStatic<T>(object str)
    {
        UniDebug.Log("RETURNING GetStatic from: \"" + name);

        return default(T);
    }

    public IntPtr GetRawObject()
    {
        return IntPtr.Zero;
    }

    public IntPtr GetRawClass()
    {
        return IntPtr.Zero;
    }

    public void Dispose()
    {

    }
}

public class AndroidJavaClass : IDisposable
{
    readonly string name;

    public AndroidJavaClass(string str, params object[] args)
    {
        name = str;
    }

    public void Call(string str, params object[] args)
    {
        UniDebug.Log("Call from: \"" + name + "\" to: " + str);
    }

    public T Call<T>(string str, params object[] args)
    {
        UniDebug.Log("RETURNING Call from: \"" + name + "\" to: " + str);

        return default(T);
    }

    public void CallStatic(string str, params object[] args)
    {
        UniDebug.Log("CallStatic from: \"" + name + "\" to: " + str);
    }

    public T CallStatic<T>(string str, params object[] args)
    {
        UniDebug.Log("RETURNING CallStatic from: \"" + name + "\" to: " + str);

        if (typeof(T) == typeof(AndroidJavaObject))
        {
            object instance = new AndroidJavaObject(str, args);
            return (T)instance;
        }

        if (typeof(T) == typeof(string))
        {
            object instance = string.Empty;
            return (T)instance;
        }

        return default(T);
    }

    public T Get<T>(string str)
    {
        UniDebug.Log("RETURNING Get from: \"" + name + "\" in: " + str);

        return default(T);
    }

    public T GetStatic<T>(object str)
    {
        UniDebug.Log("RETURNING GetStatic from: \"" + name);

        return default(T);
    }

    public AndroidJavaClass SetStatic(params object[] param)
    {
        return new AndroidJavaClass("Idk");
    }

    public T SetStatic<T>(params object[] param)
    {
        return default(T);
    }

    public IntPtr GetRawClass()
    {
        return IntPtr.Zero;
    }

    public void Dispose()
    {

    }
}

public static class AndroidJNI
{ 
    public static IntPtr GetMethodID(IntPtr clazz, string name, string sig)
    {
        return IntPtr.Zero;
    }

    public static void CallVoidMethod(IntPtr obj, IntPtr methodID, jvalue[] args)
    {

    }

    public static void CallObjectMethod(IntPtr obj, IntPtr methodID, jvalue[] args)
    {

    }

    public static int AttachCurrentThread()
    {
        return 0; // non 0 meant an error happened, according to some scripts
    }

    public static void ExceptionClear()
    {

    }

    public static void ExceptionDescribe()
    {

    }

    public static IntPtr ExceptionOccurred()
    {
        return IntPtr.Zero;
    }

    public static bool IsSameObject(IntPtr obj1, IntPtr obj2)
    {
        return obj1 == obj2;
    }
}

public class AndroidJavaProxy
{
    //
    // Сводка:
    //     Java interface implemented by the proxy.
    public readonly AndroidJavaClass javaInterface;

    internal AndroidJavaObject proxyObject;

    public AndroidJavaProxy(string javaInterface) : this(new AndroidJavaClass(javaInterface)) { }

    public AndroidJavaProxy(AndroidJavaClass javaInterface)
    {
        this.javaInterface = javaInterface;
    }

    public virtual AndroidJavaObject Invoke(string methodName, object[] args)
    {
        return new AndroidJavaObject(methodName, args);
    }

    public virtual AndroidJavaObject Invoke(string methodName, AndroidJavaObject[] javaArgs)
    {
        return Invoke(methodName, javaArgs);
    }

    public virtual bool equals(AndroidJavaObject obj)
    {
        return false;
        /*IntPtr obj2 = obj?.GetRawObject() ?? IntPtr.Zero;
        return AndroidJNI.IsSameObject(GetProxy().GetRawObject(), obj2);*/
    }

    public virtual int hashCode()
    {
        return 0;
        /*jvalue[] array = new jvalue[1];
        array[0].l = GetProxy().GetRawObject();
        return AndroidJNISafe.CallStaticIntMethod(s_JavaLangSystemClass, s_HashCodeMethodID, array);*/
    }

    public virtual string toString()
    {
        return ToString() + " <c# proxy java object>";
    }
}
#endif