using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using Newtonsoft.Json;
using Action = EosSharp.Core.Api.v1.Action;

public class WCWWebGl : MonoBehaviour
{
    public static WCWWebGl Instance { get; private set; }

    public class ErrorMessage
    {
        [JsonProperty("message")]
        public string Message;
    }

    public event Action<string> OnLoggedIn;

    public event Action<string> OnSigned;

    public event Action<string> OnError;

    public delegate void OnLoginCallback(System.IntPtr onLoginPtr);

    public delegate void OnSignCallback(System.IntPtr onSignPtr);

    public delegate void OnErrorCallback(System.IntPtr onErrorPtr);

    [DllImport("__Internal")]
    private static extern void WCWInit(string rpcAddress);

    [DllImport("__Internal")]
    private static extern void WCWLogin();

    [DllImport("__Internal")]
    private static extern void WCWSign(string actionDataJsonString);

    [DllImport("__Internal")]
    private static extern void WCWSetOnLogin(OnLoginCallback onLoginCallback);

    [DllImport("__Internal")]
    private static extern void WCWSetOnSign(OnSignCallback onSignCallback);

    [DllImport("__Internal")]
    private static extern void WCWSetOnError(OnErrorCallback onErrorCallback);

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
    }

    public void Sign(Action[] actions)
    {
        if (!isInitialized)
        {
            Debug.Log("Not initialized");
            return;
        }

        if (!isLoggedIn)
        {
            Debug.Log("Not Logged in");
            return;
        }
        WCWSign(JsonConvert.SerializeObject(actions));
    }

    public void Login()
    {
        WCWLogin();
    }

    private static bool isInitialized = false;
    private static bool isLoggedIn = false;

    public void Initialize(string rpcAddress)
    {
        WCWSetOnLogin(DelegateOnLoginEvent);
        WCWSetOnSign(DelegateOnSignEvent);
        WCWSetOnError(DelegateOnErrorEvent);
        WCWInit(rpcAddress);
        isInitialized = true;
    }

    [MonoPInvokeCallback(typeof(OnLoginCallback))]
    public static void DelegateOnLoginEvent(System.IntPtr onLoginPtr)
    {
        Debug.Log("DelegateOnLoginEvent called");
        //var msg = new byte[msgSize];
        //Marshal.Copy(msgPtr, msg, 0, msgSize);
        var msg = Marshal.PtrToStringAuto(onLoginPtr);
        Debug.Log(msg);

        if (msg?.Length == 0)
            throw new ApplicationException("LoginCallback Message is null");

        //var message = Encoding.UTF8.GetString(msg);
        isLoggedIn = true;
        Instance.OnLoggedIn?.Invoke(msg);
    }

    [MonoPInvokeCallback(typeof(OnSignCallback))]
    public static void DelegateOnSignEvent(System.IntPtr onSignPtr)
    {
        Debug.Log("DelegateOnSignEvent called");

        //var msg = new byte[msgSize];
        //Marshal.Copy(msgPtr, msg, 0, msgSize);
        var msg = Marshal.PtrToStringAuto(onSignPtr);
        Debug.Log(msg);

        if (msg?.Length == 0)
            throw new ApplicationException("SignCallback Message is null");

        //var message = Encoding.UTF8.GetString(msg);
        Instance.OnSigned?.Invoke(msg);
    }

    [MonoPInvokeCallback(typeof(OnErrorCallback))]
    public static void DelegateOnErrorEvent(System.IntPtr onErrorPtr)
    {
        Debug.Log("DelegateOnErrorEvent called");

        //var msg = new byte[msgSize];
        //Marshal.Copy(msgPtr, msg, 0, msgSize);

        var msg = Marshal.PtrToStringAuto(onErrorPtr);
        Debug.Log(msg);

        if (msg?.Length == 0)
            throw new ApplicationException("SignCallback Message is null");

        //var message = Encoding.UTF8.GetString(msg);
        Instance.OnError?.Invoke(msg);
    }
}
