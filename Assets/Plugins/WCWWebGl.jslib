mergeInto(LibraryManager.library, {
    $waxCloudWalletWebglState: {
        wax : null,
        OnLogin: null,
        OnSigned: null,
        OnError: null,
        Debug: true
    },

    WCWInit: function(rpcAddress){
        waxCloudWalletWebglState.wax = new waxjs.WaxJS({
            rpcEndpoint: rpcAddress
        });
    },

    WCWLogin: async function () {
        if(waxCloudWalletWebglState.Debug){
            window.alert("Login called");        
        }

        try {
            const userAccount = await wax.login();
            waxCloudWalletWebglState.OnLogin(JSON.stringify({ account: userAccount }));
        } catch(e) {
            waxCloudWalletWebglState.OnError(JSON.stringify({ message: e.message }));
        }
    },

    WCWSign: async function (actionDataJsonString) {
        if(waxCloudWalletWebglState.Debug){
            window.alert("Sign called");        
            window.alert(str);
        }
        if(!wax.api) {
            waxCloudWalletWebglState.OnError(JSON.stringify({ message: "Login First!" }));
        }

        const actionDataJson = JSON.parse(actionDataJsonString);


        try {
            const result = await wax.api.transact({
                actions: actionDataJson
            }, 
            {
                blocksBehind: 3,
                expireSeconds: 30
            });
            waxCloudWalletWebglState.OnSign(JSON.stringify({ message: JSON.stringify(result) }));
        } catch(e) {
            waxCloudWalletWebglState.OnError(JSON.stringify({ message: e.message }));
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

});