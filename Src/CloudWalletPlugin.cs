using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_WEBGL
using AOT;
#endif
using EosSharp.Core.Api.v1;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;
using Action = EosSharp.Core.Api.v1.Action;

namespace Assets.Packages.CloudWalletUnity.Src
{
#if UNITY_ANDROID || UNITY_IOS
using Universal.UniversalSDK;
#endif

    public class ActionConfigWrapper
    {
        [JsonProperty("actions")]
        public Action[] Actions;

        [JsonProperty("config")]
        public SignTransactionConfig Config;
    }

    public class SignTransactionConfig
    {
        /** If the transaction should also be broadcast */
        [JsonProperty("broadcast")] 
        public bool Broadcast;

        /** Number of blocks behind */
        [JsonProperty("blocksBehind")] 
        public uint BlocksBehind;

        /** Number of seconds before expiration */
        [JsonProperty("expireSeconds")] 
        public uint ExpireSeconds;
    }

    [Preserve]
    public class CloudWalletErrorEvent
    {
        [JsonProperty("message")]
        public string Message;

        public CloudWalletErrorEvent()
        {

        }
    }

    [Preserve]
    public class CloudWalletLoginEvent
    {
        [JsonProperty("account")]
        public string Account;

        public CloudWalletLoginEvent()
        {

        }
    }

    [Preserve]
    public class CloudWalletSignEvent
    {
        [JsonProperty("sign_result")]
        public PushTransactionResponse Result;

        public CloudWalletSignEvent()
        {

        }
    }

    [Preserve]
    public class CloudWalletLogoutEvent
    {
        [JsonProperty("logout_result")]
        public string LogoutResult;

        public CloudWalletLogoutEvent()
        {

        }
    }

    [Preserve]
    public class CloudWalletInitEvent
    {
        [JsonProperty("init_result")]
        public string InitResult;

        public CloudWalletInitEvent()
        {

        }
    }

    [Preserve]
    public class CloudWalletCreateInfoResult
    {
        [JsonProperty("contract")]
        public string Contract;
        [JsonProperty("message")]
        public string Message;
        [JsonProperty("amount")]
        public string Amount;
        [JsonProperty("memo")]
        public string Memo;

        public CloudWalletCreateInfoResult()
        {

        }
    }

    [Preserve]
    public class CloudWalletCreateInfoEvent
    {
        [JsonProperty("create_info_result")]
        public CloudWalletCreateInfoResult Result;

        public CloudWalletCreateInfoEvent()
        {

        }
    }


    public class CloudWalletPlugin : MonoBehaviour
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

        private static CloudWalletPlugin _instance;

        public bool IsInitialized => _instance._isInitialized;
        public bool IsLoggedIn => _instance._isLoggedIn;
        public string Account => _instance._account;

        private bool _isInitialized = false;
        private bool _isLoggedIn = false;
        private string _account;

        #region WebGL
#if UNITY_WEBGL

    private Action<CloudWalletLoginEvent> _onLoggedIn;
    public Action<CloudWalletLoginEvent> OnLoggedIn
    {
        get => _instance._onLoggedIn;
        set => _instance._onLoggedIn = value;
    }

    private Action<CloudWalletSignEvent> _onTransactionSigned;
    public Action<CloudWalletSignEvent> OnTransactionSigned
    {
        get => _instance._onTransactionSigned;
        set => _instance._onTransactionSigned = value;
    }

    private Action<CloudWalletLogoutEvent> _onLogout;
    public Action<CloudWalletLogoutEvent> OnLogout
    {
        get => _instance._onLogout;
        set => _instance._onLogout = value;
    }

    private Action<CloudWalletErrorEvent> _onError;
    public Action<CloudWalletErrorEvent> OnError
    {
        get => _instance._onError;
        set => _instance._onError = value;
    }

    private Action<CloudWalletCreateInfoEvent> _onInfoCreated;
    public Action<CloudWalletCreateInfoEvent> OnInfoCreated
    {
        get => _instance._onInfoCreated;
        set => _instance._onInfoCreated = value;
    }

    private Action<CloudWalletInitEvent> _onInit;
    public Action<CloudWalletInitEvent> OnInit
    {
        get => _instance._onInit;
        set => _instance._onInit = value;
    }



    public delegate void OnLoginCallback(System.IntPtr onLoginPtr);

    public delegate void OnSignCallback(System.IntPtr onSignPtr);

    public delegate void OnErrorCallback(System.IntPtr onErrorPtr);

    public delegate void OnCreateInfoCallback(System.IntPtr onCreateInfoPtr);

    public delegate void OnLogoutCallback(System.IntPtr onLogoutPtr);

    public delegate void OnWaxProofCallback(System.IntPtr onWaxProofPtr);

    public delegate void OnUserAccountProofCallback(System.IntPtr onUserAccountProofPtr);

    [DllImport("__Internal")]
    private static extern void CloudWalletInit(
        string rpcAddress,          // string - The WAX public node API endpoint URL you wish to connect to. Required
        bool tryAutoLogin,          // bool - Always attempt to autologin when your dapp starts up. Default true
        string userAccount,         // string - User account to start up with. Optional
        string pubKeys,             // json-array - Public keys for the userAccount manually specified above. Optional.
        string apiSigner,           // json-object? - Custom signing logic. Note that free bandwidth will not be provided to custom signers. Default Optional
        string eosApiArgs,          // json-object? - Custom eosjs constructor arguments to use when instantiating eosjs. Optional
        bool freeBandwidth,         // bool - Request bandwidth management from WAX. Default true
        bool feeFallback,           // bool - Add wax fee action if user exhausted their own bandwidth, the free boost. Default true
        string verifyTx,            // ??? - Verification function that you can override to confirm that your transactions received from WAX are only modified with bandwidth management actions, 
                                    // and that your transaction is otherwise unaltered. The function signature is (userAccount: string,  originalTx: any, augmentedTx: any) => void. 
                                    // Where userAccount is the account being signed for, originalTx is the tx generated by your dapp, 
                                    // and augmentedTx is the potentially bandwidth-managed altered tx you will receive from WAX. The default verifier does this for you, 
                                    // and you should check this to be confident that the verifier is sufficiently rigorous. Optional
        string metricsUrl,          // string - used by WAXIO to gather metrics about failed transaction, times it takes to load a transaction. Default Optional
        bool returnTempAccounts     // bool - using this flag will return temporary accounts or accounts that have signed up for a cloud wallet 
                                    // but not paid the introduction fee to get a blockchain account created. When this is set to true, 
                                    // using the doLogin function will return blockchain account name that may not exist in the blockchain 
                                    // but it will also return an extra boolean flag called isTemp. If this flag is true it is a temporary account, it does not exist in the blockchain yet. 
                                    // If this constructor option is false then only accounts which have been activated and have a blockchain account will be returned.
                    );

    [DllImport("__Internal")]
    private static extern void CloudWalletLogin();

    [DllImport("__Internal")]
    private static extern void CloudWalletSign(string actionConfigJsonString);

    [DllImport("__Internal")]
    private static extern void CloudWalletCreateInfo();

    [DllImport("__Internal")]
    private static extern void CloudWalletLogout();

    [DllImport("__Internal")]
    private static extern void CloudWalletWaxProof(string nonce, bool verify = true);

    [DllImport("__Internal")]
    private static extern void CloudWalletUserAccountProof(string nonce, string description, bool verify = true);

    [DllImport("__Internal")]
    private static extern void CloudWalletSetOnLogin(OnLoginCallback onLoginCallback);

    [DllImport("__Internal")]
    private static extern void CloudWalletSetOnSign(OnSignCallback onSignCallback);

    [DllImport("__Internal")]
    private static extern void CloudWalletSetOnError(OnErrorCallback onErrorCallback);

    [DllImport("__Internal")]
    private static extern void CloudWalletSetOnCreateInfo(OnCreateInfoCallback onCreateInfoCallback);

    [DllImport("__Internal")]
    private static extern void CloudWalletSetOnLogout(OnLogoutCallback onLogoutCallback);

    [DllImport("__Internal")]
    private static extern void CloudWalletSetOnWaxProof(OnWaxProofCallback onWaxProofCallback);

    [DllImport("__Internal")]
    private static extern void CloudWalletSetOnUserAccountProof(OnUserAccountProofCallback onUserAccountProofCallback);

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

    public void Sign(Action[] actions, bool broadcast = true, uint blocksBehind = 3, uint expireSeconds = 60)
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
                new()
                {
                    actor = _account,
                    permission = "active" // permission is always active in Cloud Wallet
                }
            };

            if(action.data is IDictionary)
            {
                var placeholderDict1 = ToDictionary<object>(action.data);
                var placeholderDict2 = placeholderDict1.ToDictionary(keyValuePair => keyValuePair.Key,
                    keyValuePair => keyValuePair.Value is string and PlaceholderName ? _account : keyValuePair.Value);

                var dataObj = JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(placeholderDict2));
                action.data = dataObj;
            }
        }

        // TODO, [JsonIgnore] hex_data in EosSharp.Core.Action
        CloudWalletSign(JsonConvert.SerializeObject(new ActionConfigWrapper()
        {
            Actions = actions,
                Config = new SignTransactionConfig()
                {
                    Broadcast =  broadcast,
                    BlocksBehind = blocksBehind,
                    ExpireSeconds = expireSeconds
                }

        }));
    }

    public void Login()
    {
        CloudWalletLogin();
    }

    public void Logout()
    {
        CloudWalletLogout();
    }

    public void CreateInfo()
    {
        CloudWalletCreateInfo();
    }

    public void WaxProof(string nonce, bool verify = true)
    {
        CloudWalletWaxProof(nonce, verify);
    }

    public void UserAccountProof(string nonce, string description, bool verify = true)
    {
        CloudWalletUserAccountProof(nonce, description, verify);
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
            throw new ApplicationException("ErrorCallback Message is null");

        _instance._eventList.Add(string.Copy(msg));
    }    
    
    [MonoPInvokeCallback(typeof(OnCreateInfoCallback))]
    public static void DelegateOnCreateInfoEvent(System.IntPtr onCreateInfoPtr)
    {
        Debug.Log("DelegateOnCreateInfoEvent called");

        var msg = Marshal.PtrToStringAuto(onCreateInfoPtr);
        if (msg?.Length == 0 || msg == null)
            throw new ApplicationException("CreateInfoCallback Message is null");

        _instance._eventList.Add(string.Copy(msg));
    }

    [MonoPInvokeCallback(typeof(OnLogoutCallback))]
    public static void DelegateOnLogoutEvent(System.IntPtr onLogoutPtr)
    {
        Debug.Log("DelegateOnLogoutEvent called");

        var msg = Marshal.PtrToStringAuto(onLogoutPtr);
        if (msg?.Length == 0 || msg == null)
            throw new ApplicationException("LogoutCallback Message is null");

        _instance._eventList.Add(string.Copy(msg));
    }

    [MonoPInvokeCallback(typeof(OnWaxProofCallback))]
    public static void DelegateOnWaxProofEvent(System.IntPtr onWaxProofPtr)
    {
        Debug.Log("DelegateOnWaxProofEvent called");

        var msg = Marshal.PtrToStringAuto(onWaxProofPtr);
        if (msg?.Length == 0 || msg == null)
            throw new ApplicationException("WaxProofCallback Message is null");

        _instance._eventList.Add(string.Copy(msg));
    }

    [MonoPInvokeCallback(typeof(OnUserAccountProofCallback))]
    public static void DelegateOnUserAccountProofEvent(System.IntPtr onUserAccountProofPtr)
    {
        Debug.Log("DelegateOnUserAccountProofEvent called");

        var msg = Marshal.PtrToStringAuto(onUserAccountProofPtr);
        if (msg?.Length == 0 || msg == null)
            throw new ApplicationException("UserAccountProofCallback Message is null");

        _instance._eventList.Add(string.Copy(msg));
    }

    private readonly List<string> _eventList = new List<string>();
    public void DispatchEventQueue()
    {
        var messageListCopy = new List<string>(_instance._eventList);
        _instance._eventList.Clear();

        foreach (var msg in messageListCopy)
        {
            var loginEvent = JsonConvert.DeserializeObject<CloudWalletLoginEvent>(msg);
            if (!string.IsNullOrEmpty(loginEvent?.Account))
            {
                _instance._account = loginEvent?.Account;
                if (loginEvent?.Account != null)
                    _instance._isLoggedIn = true;
                _instance.OnLoggedIn?.Invoke(loginEvent);
                continue;
            }

            var errorEvent = JsonConvert.DeserializeObject<CloudWalletErrorEvent>(msg);
            if (!string.IsNullOrEmpty(errorEvent?.Message))
            {
                _instance.OnError?.Invoke(errorEvent);
                continue;
            }

            var signEvent = JsonConvert.DeserializeObject<CloudWalletSignEvent>(msg);
            if (signEvent?.Result != null)
            {
                _instance.OnTransactionSigned.Invoke(signEvent);
            }

            var createInfoEvent = JsonConvert.DeserializeObject<CloudWalletCreateInfoEvent>(msg);
            if (createInfoEvent?.Result != null)
            {
                _instance.OnInfoCreated.Invoke(createInfoEvent);
            }

            var logoutEvent = JsonConvert.DeserializeObject<CloudWalletLogoutEvent>(msg);
            if (!string.IsNullOrEmpty(logoutEvent?.LogoutResult))
            {
                _instance.OnLogout.Invoke(logoutEvent);
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
#elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        private void Update()
        {
            DispatchEventQueue();
        }
#endif
        #endregion

        /// <summary>Initialize the Cloud Wallet Plugin for WebGL, see https://github.com/worldwide-asset-exchange/waxjs for more information</summary>
        /// <param name="rpcAddress">The WAX public node API endpoint URL you wish to connect to. Required</param>
        /// <param name="tryAutoLogin">Always attempt to autologin when your dapp starts up. Default true</param>
        /// <param name="userAccount">User account to start up with. Optional</param>
        /// <param name="pubKeys">Public keys for the userAccount manually specified above. Optional.</param>
        /// <param name="apiSigner">Custom signing logic. Note that free bandwidth will not be provided to custom signers. Default Optional</param>
        /// <param name="eosApiArgs">Custom eosjs constructor arguments to use when instantiating eosjs. Optional</param>
        /// <param name="freeBandwidth">Request bandwidth management from WAX. Default true</param>
        /// <param name="feeFallback">Add wax fee action if user exhausted their own bandwidth, the free boost. Default true</param>
        /// <param name="verifyTx">Verification function that you can override to confirm that your transactions received from WAX are only modified with bandwidth management actions, and that your transaction is otherwise unaltered. The function signature is (userAccount: string,  originalTx: any, augmentedTx: any) => void. Where userAccount is the account being signed for, originalTx is the tx generated by your dapp, and augmentedTx is the potentially bandwidth-managed altered tx you will receive from WAX. The default verifier does this for you, and you should check this to be confident that the verifier is sufficiently rigorous. Optional</param>
        /// <param name="metricsUrl">used by WAXIO to gather metrics about failed transaction, times it takes to load a transaction. Default Optional</param>
        /// <param name="returnTempAccounts">using this flag will return temporary accounts or accounts that have signed up for a cloud wallet but not paid the introduction fee to get a blockchain account created. When this is set to true, using the doLogin function will return blockchain account name that may not exist in the blockchain but it will also return an extra boolean flag called isTemp. If this flag is true it is a temporary account, it does not exist in the blockchain yet. If this constructor option is false then only accounts which have been activated and have a blockchain account will be returned.</summary>
        public void InitializeWebGl(
            string rpcAddress,
            bool tryAutoLogin = true,
            string userAccount = null,
            string pubKeys = null,
            string apiSigner = null,
            string eosApiArgs = null,
            bool freeBandwidth = true,
            bool feeFallback = true,
            string verifyTx = null,
            string metricsUrl = null,
            bool returnTempAccounts = false
        )
        {
#if UNITY_WEBGL
        CloudWalletSetOnLogin(DelegateOnLoginEvent);
        CloudWalletSetOnSign(DelegateOnSignEvent);
        CloudWalletSetOnError(DelegateOnErrorEvent);
        CloudWalletSetOnCreateInfo(DelegateOnCreateInfoEvent);
        CloudWalletSetOnLogout(DelegateOnLogoutEvent);
        CloudWalletSetOnWaxProof(DelegateOnWaxProofEvent);
        CloudWalletSetOnUserAccountProof(DelegateOnUserAccountProofEvent);
        CloudWalletInit(rpcAddress, tryAutoLogin, userAccount, pubKeys, apiSigner, eosApiArgs, freeBandwidth, feeFallback, verifyTx, metricsUrl, returnTempAccounts);
        _instance._isInitialized = true;
#endif
        }

        public void InitializeDesktop(uint localPort, string CloudWalletSigningWebsiteUrl, bool hostLocalWebsite = true, string indexHtmlDataPath = null, string waxJsDataPath = null)
        {
#if UNITY_IOS || UNITY_ANDROID || UNTIY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
            try
            {
                if (CloudWalletSigningWebsiteUrl.StartsWith("https"))
                    throw new NotSupportedException("CloudWalletSigningWebsiteUrl can't be SSL encrypted");

                if (hostLocalWebsite)
                {
                    var data = indexHtmlDataPath != null ? File.ReadAllText(indexHtmlDataPath) : Resources.Load<TextAsset>("CloudWallet/index").text;
                    //var data = Resources.Load("/CloudWallet/index");
                    _indexHtmlBinary = Encoding.UTF8.GetBytes(data);

                    data = waxJsDataPath != null ? File.ReadAllText(waxJsDataPath) : Resources.Load<TextAsset>("CloudWallet/waxjs").text;
                    //data = Resources.Load("/CloudWallet/waxjs");
                    _waxjsBinary = Encoding.UTF8.GetBytes(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            _localUrl = $"http://127.0.0.1:{localPort}/";
            _remoteUrl = hostLocalWebsite == false ? CloudWalletSigningWebsiteUrl : $"http://127.0.0.1:{localPort}/index.html";
#endif
        }

        public void InitializeMobile(uint localPort, string CloudWalletSigningWebsiteUrl, bool hostLocalWebsite, string indexHtmlString = null, string waxJsString = null)
        {
#if UNITY_IOS || UNITY_ANDROID || UNTIY_STANDALONE || UNITY_STANDALONE_WIN
            try
            {
                if (CloudWalletSigningWebsiteUrl.StartsWith("https"))
                    throw new NotSupportedException("CloudWalletSigningWebsiteUrl can't be SSL encrypted");

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
                Debug.LogError(e);
            }
            _localUrl = $"http://127.0.0.1:{localPort}/";
            _remoteUrl = CloudWalletSigningWebsiteUrl;
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

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)  && (!UNITY_WEBGL && !UNITY_IOS && !UNITY_ANDROID)
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

        public class CloudWalletPreflightResponse
        {
            [JsonProperty("ok")]
            public bool Ok;
        }

        private string _remoteUrl;

        private string _localUrl;

        public Action<CloudWalletLoginEvent> OnLoggedIn;
        public Action<CloudWalletSignEvent> OnTransactionSigned;
        public Action<CloudWalletErrorEvent> OnError;
        public Action<CloudWalletCreateInfoEvent> OnInfoCreated;
        public Action<CloudWalletLogoutEvent> OnLogout;
        public Action<CloudWalletInitEvent> OnInit;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private byte[] _indexHtmlBinary;
        private byte[] _waxjsBinary;

        public void Sign(Action[] actions, bool broadcast = true, uint blocksBehind = 3, uint expireSeconds = 60)
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

                if (action.data is IDictionary)
                {
                    var placeholderDict1 = ToDictionary<object>(action.data);
                    var placeholderDict2 = placeholderDict1.ToDictionary(keyValuePair => keyValuePair.Key,
                        keyValuePair => keyValuePair.Value is string and PlaceholderName ? _account : keyValuePair.Value);

                    var dataObj = JsonConvert.DeserializeObject<object>(JsonConvert.SerializeObject(placeholderDict2));
                    action.data = dataObj;
                }
            }

            StartBrowserCommunication(BuildUrl("sign", JsonConvert.SerializeObject(new ActionConfigWrapper()
            {
                Actions = actions,
                Config = new SignTransactionConfig()
                {
                    Broadcast =  broadcast,
                    BlocksBehind = blocksBehind,
                    ExpireSeconds = expireSeconds
                }
            })));
        }

        public void Login()
        {
            StartBrowserCommunication(BuildUrl("login"));
        }

        public void Logout()
        {
            StartBrowserCommunication(BuildUrl("logout"));
        }

        public void CreateInfo()
        {
            StartBrowserCommunication(BuildUrl("create_info"));
        }

        private string BuildUrl(string hashpath, string json = null)
        {
            if (_remoteUrl.EndsWith("/"))
                _remoteUrl = _remoteUrl[..^1];

            json ??= "";
            return Uri.EscapeUriString($"{_remoteUrl}#{hashpath}{json}");
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
                Debug.LogError(e);
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
                            _tokenSource?.Cancel();
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
                                JsonConvert.SerializeObject(new CloudWalletPreflightResponse() { Ok = true }));

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
                Debug.LogError(e);
            }
        }

        private readonly List<string> _eventList = new();

        private void DispatchEventQueue()
        {
            var messageListCopy = new List<string>(_eventList);
            _eventList.Clear();

            foreach (var msg in messageListCopy)
            {
                var loginEvent = JsonConvert.DeserializeObject<CloudWalletLoginEvent>(msg);
                if (!string.IsNullOrEmpty(loginEvent?.Account))
                {
                    _account = loginEvent?.Account;
                    if (loginEvent?.Account != null)
                        _isLoggedIn = true;

                    OnLoggedIn?.Invoke(loginEvent);
                    continue;
                }

                var errorEvent = JsonConvert.DeserializeObject<CloudWalletErrorEvent>(msg);
                if (!string.IsNullOrEmpty(errorEvent?.Message))
                {
                    OnError?.Invoke(errorEvent);
                    continue;
                }

                var createInfoEvent = JsonConvert.DeserializeObject<CloudWalletCreateInfoEvent>(msg);
                if (createInfoEvent != null && createInfoEvent.Result != null &&
                    !string.IsNullOrEmpty(createInfoEvent.Result.Amount) &&
                    !string.IsNullOrEmpty(createInfoEvent.Result.Contract) &&
                    !string.IsNullOrEmpty(createInfoEvent.Result.Memo) &&
                    !string.IsNullOrEmpty(createInfoEvent.Result.Message))
                {
                    OnInfoCreated?.Invoke(createInfoEvent);
                    continue;
                }

                var logoutEvent = JsonConvert.DeserializeObject<CloudWalletLogoutEvent>(msg);
                if (logoutEvent != null && !string.IsNullOrEmpty(logoutEvent.LogoutResult))
                {
                    _isLoggedIn = false;
                    _account = null;

                    OnLogout?.Invoke(logoutEvent);
                    continue;
                }

                var initEvent = JsonConvert.DeserializeObject<CloudWalletInitEvent>(msg);
                if (initEvent != null && !string.IsNullOrEmpty(initEvent.InitResult))
                {
                    OnInit?.Invoke(initEvent);
                    continue;
                }

                var signEvent = JsonConvert.DeserializeObject<CloudWalletSignEvent>(msg);
                if (signEvent?.Result == null)
                    throw new NotSupportedException($"Can't parse Json-Body {msg}");

                OnTransactionSigned.Invoke(signEvent);
            }
        }

#endif
        private static Dictionary<string, TValue> ToDictionary<TValue>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(json);
            return dictionary;
        }

        #endregion
    }
}
