using System;
using System.Threading.Tasks;
using Assets.Packages.WcwUnity.Src;
using WaxCloudWalletUnity.Examples.Ui;
using Newtonsoft.Json;
using UnityEngine;

    public class UiToolkitExample : MonoBehaviour
    {
        private WaxCloudWalletPlugin _waxCloudWalletPlugin;
        [SerializeField] internal WaxCloudWalletLoginPanel _waxCloudWalletLoginPanel;
        [SerializeField] internal WaxCloudWalletMainPanel _waxCloudWalletMainPanel;
        [SerializeField] internal WcwSuccessPanel _wcwSuccessPanel;
        [SerializeField] internal MessageBox _messageBox;
        public string Account { get; private set; }

        public string IndexHtmlString;
        public string WaxJsString;

        public void Start()
        {
            _waxCloudWalletPlugin = new GameObject(nameof(WaxCloudWalletPlugin)).AddComponent<WaxCloudWalletPlugin>();

            _waxCloudWalletPlugin.OnInit += (initEvent) =>
            {
                Debug.Log("WaxJs Initialized");
            };

            _waxCloudWalletPlugin.OnLoggedIn += (loginEvent) =>
            {
                Account = loginEvent.Account;
                Debug.Log($"{loginEvent.Account} Logged In");

                //show a successful login panel here for 15 sec
                _wcwSuccessPanel.Rebind(true);
                _wcwSuccessPanel.Show();

                //show the main panel here after a successful login
                _waxCloudWalletLoginPanel.Hide();
                _waxCloudWalletMainPanel.Rebind(Account, _waxCloudWalletPlugin);
                _waxCloudWalletMainPanel.Show();
            };

            _waxCloudWalletPlugin.OnError += (errorEvent) =>
            {
                _messageBox.Rebind(errorEvent.Message);
                _messageBox.Show();
            };
            
            _waxCloudWalletPlugin.OnInfoCreated += (infoCreatedEvent) =>
            {
                _messageBox.Rebind(JsonConvert.SerializeObject(infoCreatedEvent.Result));
                _messageBox.Show();
            };

            _waxCloudWalletPlugin.OnLogout += (logoutEvent) =>
            {
                Debug.Log($"LogoutResult: {logoutEvent.LogoutResult}");
            };

            _waxCloudWalletPlugin.OnTransactionSigned += (signEvent) =>
            {
                _messageBox.Rebind($"Transaction with ID {signEvent.Result.transaction_id} signed");
                _messageBox.Show();
                Debug.Log($"Transaction signed: {JsonConvert.SerializeObject(signEvent.Result)}");

                //show a successful Transaction signed panel here for 15 sec
                _wcwSuccessPanel.Rebind(false);
                _wcwSuccessPanel.Show();
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

        // ask the user to sign the transaction and then broadcast to chain
        public void BidName(EosSharp.Core.Api.v1.Action action)
        {
            _waxCloudWalletPlugin.Sign(new[] { action });
        }
}