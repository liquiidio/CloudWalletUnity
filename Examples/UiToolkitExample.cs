using System;
using System.Threading.Tasks;
using WaxCloudWalletUnity.Examples.Ui;
using Newtonsoft.Json;
using UnityEngine;

namespace WaxCloudWalletUnity.Examples
{
    public class UiToolkitExample : MonoBehaviour
    {
        // Assign UnityTransport through the Editor
        private WaxCloudWalletPlugin _waxCloudWalletPlugin;
        public WaxCloudWalletLoginPanel WaxCloudWalletLoginPanel;
        public WaxCloudWalletMainPanel WaxCloudWalletMainPanel;
        public WcwSuccessPanel WcwSuccessPanel;
        public string Account { get; private set; }

        public string indexHtmlString;
        public string waxJsString;

        public void Start()
        {
            _waxCloudWalletPlugin = new GameObject(nameof(WaxCloudWalletPlugin)).AddComponent<WaxCloudWalletPlugin>();

            _waxCloudWalletPlugin.OnLoggedIn += (loginEvent) =>
            {
                Account = loginEvent.Account;
                Debug.Log($"{loginEvent.Account} Logged In");

                //show a successful login panel here for 15 sec
                WcwSuccessPanel.Rebind(true);
                WcwSuccessPanel.Show();

                //show the main panel here after a successful login
                WaxCloudWalletLoginPanel.Hide();
                WaxCloudWalletMainPanel.Rebind(Account);
                WaxCloudWalletMainPanel.Show();
            };

            _waxCloudWalletPlugin.OnError += (errorEvent) =>
            {
                Debug.Log($"Error: {errorEvent.Message}");
            };

            _waxCloudWalletPlugin.OnTransactionSigned += (signEvent) =>
            {
                Debug.Log($"Transaction signed: {JsonConvert.SerializeObject(signEvent.Result)}");

                //show a successful Transaction signed panel here for 15 sec
                WcwSuccessPanel.Rebind(false);
                WcwSuccessPanel.Show();
            };

#if UNITY_WEBGL
            _waxCloudWalletPlugin.InitializeWebGl("https://wax.greymass.com");
#elif UNTIY_ANDROID || UNITY_IOS
            _waxCloudWalletPlugin.InitializeMobile(1234, "http://127.0.0.1:1234/index.html", true, indexHtmlString, waxJsString);
#else
            _waxCloudWalletPlugin.InitializeDesktop(1234, "http://127.0.0.1:1234/index.html");
#endif
        }

        public void Login()
        {
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
