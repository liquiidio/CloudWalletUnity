var WcwUnityWebGlPlugin =  {
    $waxCloudWalletWebglState: {
        wax : null,
        OnLogin: null,
        OnSigned: null,
        OnError: null,
        Debug: true
    },

    WCWInit: function(rpcAddress){
        if(waxCloudWalletWebglState.Debug){
            window.alert("init called");        
        }

        try {
            waxCloudWalletWebglState.wax = new waxjs.WaxJS({
                rpcEndpoint: UTF8ToString(rpcAddress)
            });
        } catch(e) {
            var msg = JSON.stringify({ message: e.message });
			var length = lengthBytesUTF8(msg) + 1;
			var buffer = _malloc(length);
			stringToUTF8(msg, buffer, length);

			try {
				Module.dynCall_vi(waxCloudWalletWebglState.OnError, buffer);
			} finally {
				_free(buffer);
			}
        }
    },

    WCWLogin: async function () {
        if(waxCloudWalletWebglState.Debug){
            window.alert("Login called");        
        }

        try {
            const userAccount = await waxCloudWalletWebglState.wax.login();
            window.alert(userAccount);        
            var msg = JSON.stringify({ account: userAccount });
			var length = lengthBytesUTF8(msg) + 1;
			var buffer = _malloc(length);
			stringToUTF8(msg, buffer, length);

			try {
                window.alert("Module call");        
				Module.dynCall_vi(waxCloudWalletWebglState.OnLogin, buffer);
			} finally {
				_free(buffer);
			}
        } catch(e) {

            window.alert(e.message);        

            var msg = JSON.stringify({ message: e.message });
			var length = lengthBytesUTF8(msg) + 1;
			var buffer = _malloc(length);
			stringToUTF8(msg, buffer, length);

			try {
				Module.dynCall_vi(waxCloudWalletWebglState.OnError, buffer);
			} finally {
				_free(buffer);
			}
        }
    },

    WCWSign: async function (actionDataJsonString) {
        if(waxCloudWalletWebglState.Debug){
            window.alert("Sign called");        
            window.alert(UTF8ToString(actionDataJsonString));
        }
        if(!waxCloudWalletWebglState.wax.api) {
            var msg = JSON.stringify({ message: "Login First!" });
			var length = lengthBytesUTF8(msg) + 1;
			var buffer = _malloc(length);
			stringToUTF8(msg, buffer, length);

			try {
				Module.dynCall_vi(waxCloudWalletWebglState.OnError, buffer);
			} finally {
				_free(buffer);
			}
        }

        const actionDataJson = JSON.parse(UTF8ToString(actionDataJsonString));


        try {
            const result = await waxCloudWalletWebglState.wax.api.transact({
                actions: actionDataJson
            }, 
            {
                blocksBehind: 3,
                expireSeconds: 30
            });

            var msg = JSON.stringify({ message: JSON.stringify(result) });
			var length = lengthBytesUTF8(msg) + 1;
			var buffer = _malloc(length);
			stringToUTF8(msg, buffer, length);

			try {
				Module.dynCall_vi(waxCloudWalletWebglState.OnSign, buffer);
			} finally {
				_free(buffer);
			}
        } catch(e) {
            var msg = JSON.stringify({ message: e.message });
			var length = lengthBytesUTF8(msg) + 1;
			var buffer = _malloc(length);
			stringToUTF8(msg, buffer, length);

			try {
				Module.dynCall_vi(waxCloudWalletWebglState.OnError, buffer);
			} finally {
				_free(buffer);
			}
        }
    },

    WCWSetOnLogin: function (callback) {
        if(waxCloudWalletWebglState.Debug){
            window.alert("WaxSetOnLogin called");        
        }
        waxCloudWalletWebglState.OnLogin = callback;
    },

    WCWSetOnSign: function (callback) {
        if(waxCloudWalletWebglState.Debug){
            window.alert("WaxSetOnSigned called");        
        }
        waxCloudWalletWebglState.OnSigned = callback;
    },

    WCWSetOnError: function (callback) {
        if(waxCloudWalletWebglState.Debug){
            window.alert("WaxSetOnError called");        
        }
        waxCloudWalletWebglState.OnError = callback;
    },

};

autoAddDeps(WcwUnityWebGlPlugin, '$waxCloudWalletWebglState');
mergeInto(LibraryManager.library, WcwUnityWebGlPlugin);
