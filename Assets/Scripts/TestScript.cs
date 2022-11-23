using System.Collections;
using System.Collections.Generic;
using EosSharp.Core.Api.v1;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TestScript : MonoBehaviour
{
    internal VisualElement Root;

    internal UIDocument Screen;

    private WaxCloudWalletPlugin _waxCloudWalletPlugin;

    private Button _initButton;
    private Button _loginButton;
    private Button _signButton;

    private void Awake()
    {
        Screen = GetComponent<UIDocument>();
        Root = Screen.rootVisualElement;
    }

    // Start is called before the first frame update
    void Start()
    {
        _waxCloudWalletPlugin = new GameObject(nameof(WaxCloudWalletPlugin)).AddComponent<WaxCloudWalletPlugin>();

        //WcwWebGl.Initialize();
        _waxCloudWalletPlugin.OnTransactionSigned += WCWOnTransactionSigned;
        _waxCloudWalletPlugin.OnLoggedIn += WCWOnLoggedIn;
        _waxCloudWalletPlugin.OnError += WCWOnError;

        _initButton = Root.Q<Button>("init-button");
        _loginButton = Root.Q<Button>("login-button");
        _signButton = Root.Q<Button>("sign-button");


        _initButton.clickable.clicked += () =>
        {
            _waxCloudWalletPlugin.Initialize("https://wax.greymass.com");
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
        Debug.Log($"OnSigned {obj.Trx}");
    }

    private void WCWOnLoggedIn(WcwLoginEvent obj)
    {
        Debug.Log($"OnLoggedIn {obj.Account}");
    }

    // Update is called once per frame
    void Update()
    {
        
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
