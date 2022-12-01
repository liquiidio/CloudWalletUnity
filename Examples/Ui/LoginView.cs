using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Packages.WaxCloudWalletUnity.Examples.Ui
{
    public class LoginView : ScreenBase
    {
        /*
         * Child-Controls
         */

        private Button _loginButton;


        /*
         * Fields, Properties
         */
        [SerializeField] internal UiToolkitExample UiToolkitExample;
        [SerializeField] internal MainView MainView;


        void Start()
        {
            _loginButton = Root.Q<Button>("login-button");

            BindButtons();
            Show();
        }

        #region Button Binding
        private void BindButtons()
        {
            _loginButton.clickable.clicked += () =>
            {
                try
                {
                    UiToolkitExample.Login();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    throw;
                }
            };
        }
        #endregion

    }
}