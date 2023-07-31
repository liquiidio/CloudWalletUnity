using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace WaxCloudWalletUnity.Examples.Ui
{
    public class WaxCloudWalletLoginPanel : ScreenBase
    {
        /**
         * Child-Controls
         */

        private Button _loginButton;


        /**
         * Fields, Properties
         */
        [SerializeField] internal UiToolkitExample UiToolkitExample;

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
                    Debug.LogError(e);
                    throw;
                }
            };
        }
        #endregion

    }
}