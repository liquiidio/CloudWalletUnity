using System;
using System.Threading.Tasks;
using Assets.Packages.WaxCloudWalletUnity.Examples.Ui;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Packages.WaxCloudWalletUnity.Examples
{
    public class UiToolkitExample : MonoBehaviour
    {
        // Assign UnityTransport through the Editor
        private WaxCloudWalletPlugin _waxCloudWalletPlugin;
        public LoginView LoginView;
        public MainView MainView;
        public string Account { get; private set; }

        public void Start()
        {

        }

        public void Login()
        {
            _waxCloudWalletPlugin = new GameObject(nameof(WaxCloudWalletPlugin)).AddComponent<WaxCloudWalletPlugin>();

            _waxCloudWalletPlugin.OnLoggedIn += (loginEvent) =>
            {
                Account = loginEvent.Account;
                Debug.Log($"{loginEvent.Account} Logged In");
                MainView.Rebind(Account);
                LoginView.Hide();
                MainView.Show();
            };

            _waxCloudWalletPlugin.OnError += (errorEvent) =>
            {
                Debug.Log($"Error: {errorEvent.Message}");
            };

            _waxCloudWalletPlugin.OnTransactionSigned += (signEvent) =>
            {
                Debug.Log($"Transaction signed: {JsonConvert.SerializeObject(signEvent.Result)}");
            };

#if UNITY_WEBGL
            _waxCloudWalletPlugin.InitializeWebGl("https://wax.greymass.com");
#elif UNTIY_ANDROID || UNITY_IOS
            _waxCloudWalletPlugin.InitializeMobile(1234, "http://127.0.0.1:1234/index.html", true, indexHtmlString, waxJsString);
#else
            _waxCloudWalletPlugin.InitializeDesktop(1234, "http://127.0.0.1:1234/index.html");
#endif

            _waxCloudWalletPlugin.Login();
        }

        // transfer tokens using a session  
        public void Transfer(EosSharp.Core.Api.v1.Action action)
        {
            _waxCloudWalletPlugin.Sign(new[] { action });
        }

        // ask the user to sign the transaction and then broadcast to chain
        public void Vote(EosSharp.Core.Api.v1.Action action)
        {
            _waxCloudWalletPlugin.Sign(new[] { action });
        }

        // ask the user to sign the transaction and then broadcast to chain
        public void SellOrBuyRam(EosSharp.Core.Api.v1.Action action)
        {
            _waxCloudWalletPlugin.Sign(new[] { action });
        }
    }

}
