using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using EosSharp.Core.Api.v1;
using Newtonsoft.Json;
using Action = EosSharp.Core.Api.v1.Action;

public class WcwErrorEvent
{
    [JsonProperty("message")]
    public string Message;
}

public class WcwLoginEvent
{
    [JsonProperty("account")]
    public string Account;
}

public class WcwSignEvent
{
    [JsonProperty("trx")]
    public string Trx;
}

public class WaxCloudWalletPlugin : MonoBehaviour
{
    private static WaxCloudWalletPlugin _instance;

    public bool IsInitialized => _instance._isInitialized;
    public bool IsLoggedIn => _instance._isLoggedIn;
    public string Account => _instance._account;

    private bool _isInitialized = false;
    private bool _isLoggedIn = false;
    private string _account;

    private Action<WcwLoginEvent> _onLoggedIn;
    public Action<WcwLoginEvent> OnLoggedIn
    {
        get => _instance._onLoggedIn;
        set => _instance._onLoggedIn = value;
    }

    private Action<WcwSignEvent> _onTransactionSigned;
    public Action<WcwSignEvent> OnTransactionSigned
    {
        get => _instance._onTransactionSigned;
        set => _instance._onTransactionSigned = value;
    }

    private Action<WcwErrorEvent> _onError;
    public Action<WcwErrorEvent> OnError
    {
        get => _instance._onError;
        set => _instance._onError = value;
    }

    public delegate void OnLoginCallback(System.IntPtr onLoginPtr);

    public delegate void OnSignCallback(System.IntPtr onSignPtr);

    public delegate void OnErrorCallback(System.IntPtr onErrorPtr);

    [DllImport("__Internal")]
    private static extern void WCWInit(string rpcAddress, bool tryAutoLogin, string waxSigningURL, string waxAutoSigningURL);

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

        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }

    }

    void Update()
    {
        DispatchEventQueue();
    }

    void Start()
    {
    }

    public void Sign(Action[] actions)
    {
        if (!_instance._isInitialized)
        {
            Debug.Log("Not initialized");
            return;
        }

        if (!_instance._isLoggedIn)
        {
            Debug.Log("Not Logged in");
            return;
        }

        foreach (var action in actions)
        {
            action.authorization = new List<PermissionLevel>()
            {
                new PermissionLevel()
                {
                    actor = _account,
                    permission = "active"
                }
            };
        }

        // TODO, [JsonIgnore] hex_data in EosSharp.Core.Action
        WCWSign(JsonConvert.SerializeObject(actions));
    }

    public void Login()
    {
        WCWLogin();
    }

    public void Initialize(string rpcAddress, bool tryAutoLogin = false, string waxSigningURL = null, string waxAutoSigningURL = null)
    {
        WCWSetOnLogin(DelegateOnLoginEvent);
        WCWSetOnSign(DelegateOnSignEvent);
        WCWSetOnError(DelegateOnErrorEvent);
        WCWInit(rpcAddress, tryAutoLogin, waxSigningURL, waxAutoSigningURL);
        _instance._isInitialized = true;
    }

    [MonoPInvokeCallback(typeof(OnLoginCallback))]
    public static void DelegateOnLoginEvent(System.IntPtr onLoginPtr)
    {
        Debug.Log("DelegateOnLoginEvent called");

        var msg = Marshal.PtrToStringAuto(onLoginPtr);
        if (msg?.Length == 0 || msg == null)
            throw new ApplicationException("LoginCallback Message is null");
        
        _instance._eventList.Add(string.Copy(msg));
    }

    [MonoPInvokeCallback(typeof(OnSignCallback))]
    public static void DelegateOnSignEvent(System.IntPtr onSignPtr)
    {
        Debug.Log("DelegateOnSignEvent called");

        var msg = Marshal.PtrToStringAuto(onSignPtr);
        if (msg?.Length == 0 || msg == null)
            throw new ApplicationException("SignCallback Message is null");
        
        _instance._eventList.Add(string.Copy(msg));
    }

    [MonoPInvokeCallback(typeof(OnErrorCallback))]
    public static void DelegateOnErrorEvent(System.IntPtr onErrorPtr)
    {
        Debug.Log("DelegateOnErrorEvent called");

        var msg = Marshal.PtrToStringAuto(onErrorPtr);
        if (msg?.Length == 0 || msg == null)
            throw new ApplicationException("SignCallback Message is null");

        _instance._eventList.Add(string.Copy(msg));
    }

    private readonly List<string> _eventList = new List<string>();
    public void DispatchEventQueue()
    {
        var messageListCopy = new List<string>(_instance._eventList);
        _instance._eventList.Clear();

        foreach (var msg in messageListCopy)
        {
            var loginEvent = JsonConvert.DeserializeObject<WcwLoginEvent>(msg);
            if (!string.IsNullOrEmpty(loginEvent?.Account))
            {
                _instance._account = loginEvent?.Account;
                if (loginEvent?.Account != null)
                    _instance._isLoggedIn = true;
                _instance.OnLoggedIn?.Invoke(loginEvent);
                continue;
            }

            var errorEvent = JsonConvert.DeserializeObject<WcwErrorEvent>(msg);
            if (!string.IsNullOrEmpty(errorEvent?.Message))
            {
                _instance.OnError?.Invoke(errorEvent);
                continue;
            }

            var signEvent = JsonConvert.DeserializeObject<WcwSignEvent>(msg);
            if (!string.IsNullOrEmpty(signEvent?.Trx))
            {
                _instance.OnTransactionSigned.Invoke(signEvent);
            }
        }
    }
}
