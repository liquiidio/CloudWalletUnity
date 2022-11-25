var WcwUnityWebGlPlugin =  {
    $waxCloudWalletWebglState: {
        wax : null,
        OnLogin: null,
        OnSign: null,
        OnError: null,
        Debug: true
    },

    WCWInit: function(rpcAddress, tryAutoLogin, waxSigningURL, waxAutoSigningURL ){
        if(waxCloudWalletWebglState.Debug){
            console.log("init called");
        }

        try {
            waxCloudWalletWebglState.wax = new waxjs.WaxJS({
                rpcEndpoint: UTF8ToString(rpcAddress),
                tryAutoLogin: tryAutoLogin != null ? tryAutoLogin : null,
                waxSigningURL: waxSigningURL != null ? UTF8ToString(waxSigningURL) : null,
                waxAutoSigningURL: waxAutoSigningURL != null = UTF8ToString(waxAutoSigningURL) : null
            });
            if(waxCloudWalletWebglState.Debug){
                console.log("wax Initialized!");
            }
        } catch(e) {
            if(waxCloudWalletWebglState.Debug){
                console.log(e.message);
            }

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
            console.log("Login called");        
        }

        var msg = "";
        var error = false;

        try {
            const userAccount = await waxCloudWalletWebglState.wax.login();
            msg = JSON.stringify({ account: userAccount });
        } catch(e) {
            if(waxCloudWalletWebglState.Debug){
                console.log(e.message);
            }
            error = true;
            msg = JSON.stringify({ message: e.message });
        }

        var length = lengthBytesUTF8(msg) + 1;
		var buffer = _malloc(length);
		stringToUTF8(msg, buffer, length);

		try {
            if(error)
	            Module.dynCall_vi(waxCloudWalletWebglState.OnError, buffer);
            else
                Module.dynCall_vi(waxCloudWalletWebglState.OnLogin, buffer);
		} finally {
			_free(buffer);
        }
    },

    WCWSign: async function (actionDataJsonString) {
        if(waxCloudWalletWebglState.Debug){
            console.log("Sign called");        
            console.log(UTF8ToString(actionDataJsonString));
        }

        var msg = "";
        var error = false;

        if(!waxCloudWalletWebglState.wax.api) {
            msg = JSON.stringify({ message: "Login First!" });
            error = true;
        }

        if(!error){
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
				console.log(msg);
			    var length = lengthBytesUTF8(msg) + 1;
			    var buffer = _malloc(length);
			    stringToUTF8(msg, buffer, length);

			    try {
				    Module.dynCall_vi(waxCloudWalletWebglState.OnSign, buffer);
			    } finally {
				    _free(buffer);
			    }
                return;
            } catch(e) {
                if(waxCloudWalletWebglState.Debug){
                    console.log(e.message);
                }
                error = true;
                msg = JSON.stringify({ message: e.message });
            }
        }

        var length = lengthBytesUTF8(msg) + 1;
		var buffer = _malloc(length);
		stringToUTF8(msg, buffer, length);

		try {
            if(error)
	            Module.dynCall_vi(waxCloudWalletWebglState.OnError, buffer);
            else
                Module.dynCall_vi(waxCloudWalletWebglState.OnLogin, buffer);
		} finally {
			_free(buffer);
        }
    },

    WCWSetOnLogin: function (callback) {
        if(waxCloudWalletWebglState.Debug){
            console.log("WaxSetOnLogin called");        
        }
        waxCloudWalletWebglState.OnLogin = callback;
    },

    WCWSetOnSign: function (callback) {
        if(waxCloudWalletWebglState.Debug){
            console.log("WaxSetOnSign called");        
        }
        waxCloudWalletWebglState.OnSign = callback;
    },

    WCWSetOnError: function (callback) {
        if(waxCloudWalletWebglState.Debug){
            console.log("WaxSetOnError called");        
        }
        waxCloudWalletWebglState.OnError = callback;
    },

};

autoAddDeps(WcwUnityWebGlPlugin, '$waxCloudWalletWebglState');
mergeInto(LibraryManager.library, WcwUnityWebGlPlugin);
