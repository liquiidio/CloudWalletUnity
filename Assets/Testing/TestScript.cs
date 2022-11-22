using System.Collections;
using System.Collections.Generic;
using EosSharp.Core.Api.v1;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private WCWWebGl WcwWebGl = new WCWWebGl();
    // Start is called before the first frame update
    void Start()
    {
        WcwWebGl.Initialize();
        WcwWebGl.OnSigned += WcwWebGlOnOnSigned;
        WcwWebGl.OnLoggedIn += WcwWebGlOnOnLoggedIn;
        WcwWebGl.OnError += WcwWebGlOnOnError;
    }

    private void WcwWebGlOnOnError(string obj)
    {
        Debug.Log($"OnError {obj}");
    }

    private void WcwWebGlOnOnSigned(string obj)
    {
        Debug.Log($"OnSigned {obj}");
    }

    private void WcwWebGlOnOnLoggedIn(string obj)
    {
        Debug.Log($"OnLoggedIn {obj}");
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
                account = "",
                authorization = new List<PermissionLevel>()
                {
                    new PermissionLevel()
                    {
                        actor = "", permission = ""
                    }
                },
                name = "",
                data = new Dictionary<string, object>()
                {

                },
            }
        });
    }

}
