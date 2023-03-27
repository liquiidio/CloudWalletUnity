using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using EosSharp.Core.Api.v1;
using Newtonsoft.Json;
using UnityEngine;
using Action = EosSharp.Core.Api.v1.Action;

namespace Assets.Packages.WcwUnity.Src
{
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
        private bool _refocusWindow;

        //! Placeholder for an Account-Name, can be used if no Account-Name is specified
        private const string PlaceholderName = "............1";

        //! Placeholder for a Permission-Name, can be used if no Permission-Name is specified
        private const string PlaceholderPermission = "............2";


        //! Placeholder for a Authorization, can be used if no Authorization is specified
        public static readonly PermissionLevel PlaceholderAuth = new PermissionLevel()
        {
            actor = PlaceholderName,
            permission = PlaceholderPermission
        };

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
            Debug.LogError(e);;
        }
    }
#endif
        #endregion

        #region Desktop
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && !UNITY_WEBGL

        private IntPtr _unityWindow;

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const int Alt = 0xA4;
        private const int Extendedkey = 0x1;
        private const int Keyup = 0x2;

        private void Update()
        {
            if (_refocusWindow)
                StartCoroutine(RefocusWindow(0f));

            DispatchEventQueue();
        }

        private IEnumerator RefocusWindow(float waitSeconds)
        {
            // wait for new window to appear
            yield return new WaitWhile(() => _unityWindow == GetActiveWindow());

            yield return new WaitForSeconds(waitSeconds);

            // Simulate alt press
            keybd_event(Alt, 0x45, Extendedkey | 0, 0);

            // Simulate alt release
            keybd_event(Alt, 0x45, Extendedkey | Keyup, 0);

            SetForegroundWindow(_unityWindow);

            _refocusWindow = false;
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
                    var data = indexHtmlDataPath == null ? File.ReadAllText(Application.dataPath + "/Packages/WcwUnity/Assets/index.html") : File.ReadAllText(indexHtmlDataPath);
                    _indexHtmlBinary = Encoding.UTF8.GetBytes(data);

                    data = waxJsDataPath == null ? File.ReadAllText(Application.dataPath + "/Packages/WcwUnity/Assets/waxjs.js") : File.ReadAllText(waxJsDataPath);
                    _waxjsBinary = Encoding.UTF8.GetBytes(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);;
            }
            _localUrl = $"http://127.0.0.1:{localPort}/";
            _remoteUrl = hostLocalWebsite == false ? wcwSigningWebsiteUrl : $"http://127.0.0.1:{localPort}/index.html";
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
                        _indexHtmlBinary = Encoding.UTF8.GetBytes(indexHtmlString);
                        _waxjsBinary = Encoding.UTF8.GetBytes(waxJsString);
                    }
                    else
                        throw new NotSupportedException("Due to compression, on Android and iOS-Builds the index.html and wax.js must be provided as strings");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);;
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
        private void StartBrowserCommunication(string url)
        {
            _unityWindow = GetActiveWindow();

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

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private byte[] _indexHtmlBinary;
        private byte[] _waxjsBinary;

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
                    new()
                    {
                        actor = _account,
                        permission = "active" // permission is always active in Cloud Wallet
                    }
                };
                if (action.account == PlaceholderName)
                    action.account = _account;

                var dict = ToDictionary<object>(action.data);
                var dict2 = dict.ToDictionary(keyValuePair => keyValuePair.Key,
                    keyValuePair => keyValuePair.Value is string and PlaceholderName ? _account : keyValuePair.Value);

                var dataObj = JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(dict2));
                action.data = dataObj;
            }

            StartBrowserCommunication(BuildUrl("sign", JsonConvert.SerializeObject(actions)));
        }

        public static Dictionary<string, TValue> ToDictionary<TValue>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(json);
            return dictionary;
        }

        public void Login()
        {
            StartBrowserCommunication(BuildUrl("login"));
        }

        private string BuildUrl(string loginOrSign, string json = null)
        {
            if (_remoteUrl.EndsWith("/")) 
                _remoteUrl = _remoteUrl[..^1];
        
            json ??= "";
            return $"{_remoteUrl}#{loginOrSign}{json}";
        }

        private void StartHttpListener()
        {
            try
            {
                _tokenSource?.Cancel();
                Task.Delay(100, _token);
                _tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                _token = _tokenSource.Token;
                Task.Run(Listen, _token);
            }
            catch (Exception e)
            {
                Debug.LogError(e);;
            }
        }

        void OnApplicationQuit()
        {
            try
            {
                if (_listener == null) 
                    return;
                
                _listener.Stop();
                _listener.Close();
                _listener.Abort();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private HttpListener _listener;
        private async Task Listen()
        {
            try
            {
                var requests = new HashSet<Task>();
                if (_listener == null)
                {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add(_localUrl);
                    _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                    _listener.Start();
                    const int maxConcurrentRequests = 5;
                    for (var i = 0; i < maxConcurrentRequests; i++)
                        requests.Add(_listener.GetContextAsync());
                }

                while (!_token.IsCancellationRequested)
                {
                    if (requests.Any())
                    {
                        var requestAwaiterTask = await Task.WhenAny(requests).ConfigureAwait(false);
                        requests.Remove(requestAwaiterTask);

                        if (requestAwaiterTask is not Task<HttpListenerContext> task)
                            continue;

                        HttpListenerContext context = null;
                        try
                        {
                            context = task?.Result;
                        }
                        catch
                        {
                            Debug.Log("requestAwaiterTask is already disposed, you can ignore this Log");
                        }

                        if (context == null)
                            continue;

                        requests.Add(HandleRequest(context));
                        requests.Add(_listener.GetContextAsync());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(e.StackTrace);
            }
            _refocusWindow = true;
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

                    response.ContentLength64 = _indexHtmlBinary.Length;

                    await response.OutputStream.WriteAsync(_indexHtmlBinary, 0, _indexHtmlBinary.Length, _token).ConfigureAwait(false);
                    response.Close();
                    return;
                }

                if (request.RawUrl.EndsWith("/waxjs.js"))
                {
                    response.Headers.Set("Content-Type", "text/html");

                    response.ContentLength64 = _waxjsBinary.Length;

                    await response.OutputStream.WriteAsync(_waxjsBinary, 0, _waxjsBinary.Length, _token).ConfigureAwait(false);
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
                        if (!request.Url.ToString().EndsWith("preflight"))
                            throw new NotSupportedException($"path {request.Url} is not supported");
                        
                        if (request.ContentType != "application/json")
                            throw new NotSupportedException($"ContentType {request.ContentType} is not supported");
                            
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.ContentType = "application/json";

                        var responseBody =
                            Encoding.UTF8.GetBytes(
                                JsonConvert.SerializeObject(new WcwPreflightResponse() { Ok = true }));

                        response.ContentLength64 = responseBody.Length;
                        await response.OutputStream.WriteAsync(responseBody, 0, responseBody.Length, _token).ConfigureAwait(false);
                        response.Close();
                        break;

                    case "POST":
                        if (request.ContentType == "application/json")
                        {
                            string jsonBody;
                            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                            {
                                jsonBody = await reader.ReadToEndAsync().ConfigureAwait(false);
                            }

                            _eventList.Add(string.Copy(jsonBody));

                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.Close();
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
                Debug.LogError(e);;
            }
        }

        private readonly List<string> _eventList = new();

        private void DispatchEventQueue()
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
                if (signEvent?.Result == null) 
                    throw new NotSupportedException($"Can't parse Json-Body {msg}");
                
                OnTransactionSigned.Invoke(signEvent);
            }
        }

#endif
        #endregion
    }
}