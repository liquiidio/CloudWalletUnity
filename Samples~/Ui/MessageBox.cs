using System;
using System.Collections;
using System.Collections.Generic;
using AnchorLinkSharp;
using AnchorLinkTransportSharp;
using EosioSigningRequest;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace WaxCloudWalletUnity.Examples.Ui
{
    public class MessageBox : ScreenBase
    {
        /**
         * Child-Controls
         */
        private Label _text;
        private Button _closeViewButton;

        private void Start()
        {
            _text = Root.Q<Label>("text");
            _closeViewButton = Root.Q<Button>("close-view-button");

            _closeViewButton.clickable.clicked += Hide;
        }

        #region Rebind
        /// <summary>
        /// Rebind and display appropriate message if the user is signing in or performing a transaction
        /// </summary>
        /// <param name="loginRequest"></param>
        internal void Rebind(string text)
        {
            _text.text = text;
        }

        #endregion
    }
}
