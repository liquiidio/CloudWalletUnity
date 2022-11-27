using System.Collections;
using System.Collections.Generic;
using EosSharp.Core.Api.v1;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class WaxCloudWalletSampleScript : MonoBehaviour
{
    internal VisualElement Root;

    internal UIDocument Screen;

    private WaxCloudWalletPlugin _waxCloudWalletPlugin;

    private Button _initButton;
    private Button _loginButton;
    private Button _signButton;

    public string indexHtmlString;
    public string waxJsString;

    private void Awake()
    {
        Screen = GetComponent<UIDocument>();
        Root = Screen.rootVisualElement;
    }

    // Start is called before the first frame update
    void Start()
    {
        _waxCloudWalletPlugin = new GameObject(nameof(WaxCloudWalletPlugin)).AddComponent<WaxCloudWalletPlugin>();

        _waxCloudWalletPlugin.OnTransactionSigned += WCWOnTransactionSigned;
        _waxCloudWalletPlugin.OnLoggedIn += WCWOnLoggedIn;
        _waxCloudWalletPlugin.OnError += WCWOnError;

        _initButton = Root.Q<Button>("init-button");
        _loginButton = Root.Q<Button>("login-button");
        _signButton = Root.Q<Button>("sign-button");


        _initButton.clickable.clicked += () =>
        {
#if UNITY_WEBGL
            _waxCloudWalletPlugin.InitializeWebGl("https://wax.greymass.com");
#elif UNTIY_ANDROID || UNITY_IOS
            _waxCloudWalletPlugin.InitializeMobile(1234, "http://127.0.0.1:1234/index.html", true, indexHtmlString, waxJsString);
#else
            _waxCloudWalletPlugin.InitializeDesktop(1234, "http://127.0.0.1:1234/index.html");
#endif
        };

        _loginButton.clickable.clicked += () =>
        {
            Login();
        };

        _signButton.clickable.clicked += () =>
        {
            Sign();
        };
    }

    private void WCWOnError(WcwErrorEvent obj)
    {
        Debug.Log($"OnError {obj.Message}");
    }

    private void WCWOnTransactionSigned(WcwSignEvent obj)
    {
        Debug.Log($"OnSigned {JsonConvert.SerializeObject(obj.Result)}");
    }

    private void WCWOnLoggedIn(WcwLoginEvent obj)
    {
        Debug.Log($"OnLoggedIn {obj.Account}");
    }

    public void Login()
    {
        _waxCloudWalletPlugin.Login();
    }

    public void Sign()
    {
        Debug.Log(_waxCloudWalletPlugin.Account);

        _waxCloudWalletPlugin.Sign(new Action[]
        {
            new Action()
            {
                account = "eosio.token",
                name = "transfer",
                data = new Dictionary<string, object>()
                {
                    {"from", _waxCloudWalletPlugin.Account},
                    {"to", "test1.liq"},
                    {"quantity", "0.00010000 WAX"},
                    {"memo", "just a test"}
                }
            }
        });
    }

}
