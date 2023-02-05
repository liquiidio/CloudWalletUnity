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

        private void Start()
        {
            _subTitleLabel = Root.Q<Label>("anchor-link-subtitle-label");

            //OnStart();
        }

        #region Rebind
        /// <summary>
        /// Rebind and display appropriate message if the user is signing in or performing a transaction
        /// </summary>
        /// <param name="request"></param>
        internal void Rebind(/*SigningRequest request*/)
        {
            //if (request.IsIdentity())
            //{
            //    _subTitleLabel.text = "Login completed.";
            //}
            //else _subTitleLabel.text = "Transaction signed";

            StartCoroutine(SetTimeout());
        }

        #endregion

        #region other

        /// <summary>
        /// Hide this screen after set time has reached the counterDuration
        /// </summary>
        /// <param name="counterDuration"></param>
        /// <returns></returns>
        private IEnumerator SetTimeout(float counterDuration = 0.5f)
        {
            float _newCounter = 0;
            while (_newCounter < counterDuration * 2)
            {
                _newCounter += Time.deltaTime;
                yield return null;
            }
            this.Hide();
        }
        #endregion
    }
}
