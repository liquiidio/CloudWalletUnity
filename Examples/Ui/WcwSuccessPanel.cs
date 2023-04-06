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
    public class WcwSuccessPanel : ScreenBase
    {
        /**
         * Child-Controls
         */
        private Label _subTitleLabel;
        private Button _closeViewButton;

        private void Start()
        {
            _subTitleLabel = Root.Q<Label>("anchor-link-subtitle-label");
            _closeViewButton = Root.Q<Button>("close-view-button");

            _closeViewButton.clickable.clicked += Hide;
        }

        #region Rebind
        /// <summary>
        /// Rebind and display appropriate message if the user is signing in or performing a transaction
        /// </summary>
        /// <param name="loginRequest"></param>
        internal void Rebind(bool loginRequest)
        {
            if (loginRequest) _subTitleLabel.text = "Login completed.";
            else _subTitleLabel.text = "Transaction signed";

            StartCoroutine(SetTimeout());
        }

        #endregion

        #region other

        /// <summary>
        /// Hide this screen after 15 sec has reached the counterDuration
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetTimeout()
        {
            // Get the current time
            var startTime = Time.time;

            // Run the coroutine for 15 seconds
            while (Time.time < startTime + 15f)
            {
                // Yield every frame
                yield return null;
            }

            // Coroutine has finished running
            this.Hide();
        }
        #endregion
    }
}
