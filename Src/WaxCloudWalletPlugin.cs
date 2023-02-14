using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
// ReSharper disable once RedundantUsingDirective
using AOT; // Do not remove!
using EosSharp.Core.Api.v1;
using Newtonsoft.Json;
using Action = EosSharp.Core.Api.v1.Action;
using System.IO;
using System.Net;
using System.Collections;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

#if UNITY_ANDROID || UNITY_IOS
using Universal.UniversalSDK;
#endif

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
    [JsonProperty("result")]
    public PushTransactionResponse Result;
}

public class WaxCloudWalletPlugin : MonoBehaviour
{
    private bool refocusWindow;

    #region WebGL
#if UNITY_WEBGL


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
            if (signEvent?.Result != null)
            {
                _instance.OnTransactionSigned.Invoke(signEvent);
            }
        }
    }

#endif
    #endregion

    #region Mobile
#if (UNITY_ANDROID || UNITY_IOS)

    private UniversalSDK _universalSdk;

    public void OpenCustomTabView(string url)
    {
        try
        {
            _universalSdk.OpenCustomTabView(url, result =>
            {
                result.Match(
                    value =>
                    {
                        Debug.Log(value);
                    },
                    error =>
                    {
                        Debug.LogError(error);
                    });
            });
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
#endif
    #endregion

    #region Desktop
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && !UNITY_WEBGL

    private IntPtr unityWindow;

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    const int ALT = 0xA4;
    const int EXTENDEDKEY = 0x1;
    const int KEYUP = 0x2;

    private void Update()
    {
        if (refocusWindow)
            StartCoroutine(RefocusWindow(0f));

        DispatchEventQueue();
    }

    private IEnumerator RefocusWindow(float waitSeconds)
    {
        // wait for new window to appear
        yield return new WaitWhile(() => unityWindow == GetActiveWindow());

        yield return new WaitForSeconds(waitSeconds);

        // Simulate alt press
        keybd_event((byte)ALT, 0x45, EXTENDEDKEY | 0, 0);

        // Simulate alt release
        keybd_event((byte)ALT, 0x45, EXTENDEDKEY | KEYUP, 0);

        SetForegroundWindow(unityWindow);

        refocusWindow = false;
    }
#endif
    #endregion

    public void InitializeWebGl(string rpcAddress, bool tryAutoLogin = false, string waxSigningURL = null, string waxAutoSigningURL = null)
    {
#if UNITY_WEBGL
        WCWSetOnLogin(DelegateOnLoginEvent);
        WCWSetOnSign(DelegateOnSignEvent);
        WCWSetOnError(DelegateOnErrorEvent);
        WCWInit(rpcAddress, tryAutoLogin, waxSigningURL, waxAutoSigningURL);
        _instance._isInitialized = true;
#endif
    }

    public void InitializeDesktop(uint localPort, string wcwSigningWebsiteUrl, bool hostLocalWebsite = true, string indexHtmlDataPath = null, string waxJsDataPath = null)
    {
#if UNITY_IOS || UNITY_ANDROID || UNTIY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
        try
        {
            if (wcwSigningWebsiteUrl.StartsWith("https"))
                throw new NotSupportedException("wcwSigningWebsiteUrl can't be SSL encrypted");

            if (hostLocalWebsite)
            {
                string data;
                if (indexHtmlDataPath == null)
                    data = File.ReadAllText(Application.dataPath + "/Packages/WcwUnityWebGl/Assets/index.html");
                else
                    data = File.ReadAllText(indexHtmlDataPath);
                indexHtml = Encoding.UTF8.GetBytes(data);

                if (waxJsDataPath == null)
                    data = File.ReadAllText(Application.dataPath + "/Packages/WcwUnityWebGl/Assets/waxjs.js");
                else
                    data = File.ReadAllText(waxJsDataPath);
                waxjs = Encoding.UTF8.GetBytes(data);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        _localUrl = $"http://127.0.0.1:{localPort}/";
        if (hostLocalWebsite == false)
            _remoteUrl = wcwSigningWebsiteUrl;
        else
            _remoteUrl = $"http://127.0.0.1:{localPort}/index.html";
#endif
    }

    public void InitializeMobile(uint localPort, string wcwSigningWebsiteUrl, bool hostLocalWebsite, string indexHtmlString = null, string waxJsString = null)
    {
#if UNITY_IOS || UNITY_ANDROID || UNTIY_STANDALONE || UNITY_STANDALONE_WIN
        try
        {
            if (wcwSigningWebsiteUrl.StartsWith("https"))
                throw new NotSupportedException("wcwSigningWebsiteUrl can't be SSL encrypted");

            if (hostLocalWebsite)
            {
                if (!string.IsNullOrEmpty(indexHtmlString) && !string.IsNullOrEmpty(waxJsString))
                {
                    indexHtml = Encoding.UTF8.GetBytes(indexHtmlString);
                    waxjs = Encoding.UTF8.GetBytes(waxJsString);
                }
                else
                    throw new NotSupportedException("Due to compression, on Android and iOS-Builds the index.html and wax.js must be provided as strings");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        _localUrl = $"http://127.0.0.1:{localPort}/";
        _remoteUrl = wcwSigningWebsiteUrl;
#endif
#if (UNITY_ANDROID || UNITY_IOS)
        _universalSdk = new GameObject(nameof(UniversalSDK)).AddComponent<UniversalSDK>();
#endif
    }

    #region Mobile
#if UNITY_IOS || UNITY_ANDROID
    
    public void StartBrowserCommunication(string url)
    {
        StartHttpListener();
        OpenCustomTabView(url);
    }
    
#endif
    #endregion

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && !UNITY_WEBGL
    public void StartBrowserCommunication(string url)
    {
        unityWindow = GetActiveWindow();

        StartHttpListener();
        Application.OpenURL(url);
    }
#endif

#if (UNITY_STANDALONE || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX) && !UNITY_STANDALONE_WIN
    public void StartBrowserCommunication(string url)
    {
        StartHttpListener();
        Application.OpenURL(url);
    }
#endif

    #region Desktop and Mobile
#if UNITY_IOS || UNITY_ANDROID || UNTIY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX

    public class WcwPreflightResponse
    {
        [JsonProperty("ok")]
        public bool Ok;
    }

    private string _remoteUrl;

    private string _localUrl;

    private bool _isLoggedIn;

    private string _account;


    public Action<WcwLoginEvent> OnLoggedIn;
    public Action<WcwSignEvent> OnTransactionSigned;
    public Action<WcwErrorEvent> OnError;

    public string Account => _account;

    private CancellationTokenSource tokenSource;
    private CancellationToken token;
    private byte[] indexHtml;
    private byte[] waxjs;

    public void Sign(Action[] actions)
    {
        if (!_isLoggedIn)
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

        StartBrowserCommunication(BuildUrl("sign", JsonConvert.SerializeObject(actions)));
    }

    public void Login()
    {
        StartBrowserCommunication(BuildUrl("login"));
    }

    private string BuildUrl(string loginOrSign, string json = null)
    {
        if (_remoteUrl.EndsWith("/"))
        {
            _remoteUrl = _remoteUrl.Substring(0, _remoteUrl.Length - 1);
        }
        if (json == null)
            json = "";
        return $"{_remoteUrl}#{loginOrSign}{json}";
    }

    public void StartHttpListener()
    {
        try
        {
            tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            token = tokenSource.Token;
            Task.Run(Listen);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async Task Listen()
    {
        try
        {
            using (var _listener = new HttpListener())
            {
                _listener.Prefixes.Add(_localUrl);
                _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                _listener.Start();
                var maxConcurrentRequests = 5;
                var requests = new HashSet<Task>();
                for (int i = 0; i < maxConcurrentRequests; i++)
                    requests.Add(_listener.GetContextAsync());

                while (!token.IsCancellationRequested)
                {
                    Task t = await Task.WhenAny(requests);
                    requests.Remove(t);

                    if (t is Task<HttpListenerContext>)
                    {
                        var context = (t as Task<HttpListenerContext>).Result;
                        requests.Add(HandleRequest(context));
                        requests.Add(_listener.GetContextAsync());
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        refocusWindow = true;
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            if (request.RawUrl.EndsWith("/index.html"))
            {
                response.Headers.Set("Content-Type", "text/html");

                response.ContentLength64 = indexHtml.Length;

                await response.OutputStream.WriteAsync(indexHtml, 0, indexHtml.Length);
                response.Close();
                return;
            }

            if (request.RawUrl.EndsWith("/waxjs.js"))
            {
                response.Headers.Set("Content-Type", "text/html");

                response.ContentLength64 = waxjs.Length;

                await response.OutputStream.WriteAsync(waxjs, 0, waxjs.Length);
                response.Close();
                return;
            }

            if (request.RawUrl.EndsWith("/favicon.ico"))
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
                return;
            }

            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");

            switch (request.HttpMethod)
            {
                case "GET":
                    if (request.Url.ToString().EndsWith("preflight"))
                    {
                        if (request.ContentType == "application/json")
                        {
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";

                            var responseBody =
                                Encoding.UTF8.GetBytes(
                                    JsonConvert.SerializeObject(new WcwPreflightResponse() { Ok = true }));

                            response.ContentLength64 = responseBody.Length;
                            await response.OutputStream.WriteAsync(responseBody, 0, responseBody.Length);
                            response.Close();
                            break;
                        }
                        else
                            throw new NotSupportedException($"ContentType {request.ContentType} is not supported");
                    }
                    else
                        throw new NotSupportedException($"path {request.Url.ToString()} is not supported");
                case "POST":
                    if (request.ContentType == "application/json")
                    {
                        string jsonBody;
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            jsonBody = await reader.ReadToEndAsync();
                        }

                        _eventList.Add(string.Copy(jsonBody));

                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.Close();

                        tokenSource.Cancel();
                    }
                    else
                        throw new NotSupportedException($"ContentType {request.ContentType} is not supported");
                    break;
                default:
                    throw new NotSupportedException($"HttpMethod {request.HttpMethod} is not supported");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private readonly List<string> _eventList = new List<string>();
    public void DispatchEventQueue()
    {
        var messageListCopy = new List<string>(_eventList);
        _eventList.Clear();

        foreach (var msg in messageListCopy)
        {
            var loginEvent = JsonConvert.DeserializeObject<WcwLoginEvent>(msg);
            if (!string.IsNullOrEmpty(loginEvent?.Account))
            {
                _account = loginEvent?.Account;
                if (loginEvent?.Account != null)
                    _isLoggedIn = true;

                OnLoggedIn?.Invoke(loginEvent);
                continue;
            }

            var errorEvent = JsonConvert.DeserializeObject<WcwErrorEvent>(msg);
            if (!string.IsNullOrEmpty(errorEvent?.Message))
            {
                OnError?.Invoke(errorEvent);
                continue;
            }

            var signEvent = JsonConvert.DeserializeObject<WcwSignEvent>(msg);
            if (signEvent?.Result != null)
            {
                OnTransactionSigned.Invoke(signEvent);
                continue;
            }
            throw new NotSupportedException($"Can't parse Json-Body {msg}");
        }
    }

#endif
    #endregion
}
