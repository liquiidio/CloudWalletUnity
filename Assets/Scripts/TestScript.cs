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

    private WCWWebGl WcwWebGl = new WCWWebGl();

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
        //WcwWebGl.Initialize();
        WcwWebGl.OnSigned += WcwWebGlOnOnSigned;
        WcwWebGl.OnLoggedIn += WcwWebGlOnOnLoggedIn;
        WcwWebGl.OnError += WcwWebGlOnOnError;

        _initButton = Root.Q<Button>("init-button");

        _loginButton = Root.Q<Button>("login-button");

        _signButton = Root.Q<Button>("sign-button");


        _initButton.clickable.clicked += () =>
        {
            WcwWebGl.Initialize("https://wax.greymass.com");
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

    private void WcwWebGlOnOnError(WCWWebGl.ErrorEvent obj)
    {
        Debug.Log($"OnError {obj.Message}");
    }

    private void WcwWebGlOnOnSigned(string obj)
    {
        Debug.Log($"OnSigned {obj}");
    }

    private void WcwWebGlOnOnLoggedIn(WCWWebGl.LoginEvent obj)
    {
        Debug.Log($"OnLoggedIn {obj.Account}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Login()
    {
        WcwWebGl.Login();
    }

    public void Sign()
    {
        WcwWebGl.Sign(new Action[]
        {
            new Action()
            {
                account = "eosio.token",
                name = "transfer",
                data = new Dictionary<string, object>()
                {
                    {"from", WCWWebGl.Instance.Account},
                    {"to", "test1.liq"},
                    {"quantity", "0.00010000"},
                    {"memo", "just a test"}
                }
            }
        });
    }

}
