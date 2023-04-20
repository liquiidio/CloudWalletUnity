

<div align="center">
 <img src="https://avatars.githubusercontent.com/u/82725791?s=200&v=4" align="center"
     alt="Liquiid logo" width="280" height="300">
</div>

# Cloud Wallet Plugin
---

A native integration of the Wax Cloud Wallet compatible with all major Build Targets (WebGL, Windows, Mac, Linux, Android, iOS) without relying on WebViews.

# Installation

**_Requires Unity 2019.1+ with .NET 4.x+ Runtime_**

This package can be included into your project by either:

 1. Installing the package via Unity's Package Manager (UPM) in the editor (recommended).
 2. Importing the .unitypackage which you can download [here](https://github.com/liquiidio/WcwUnityWebGl/releases/latest/download/wcwunity.unitypackage).
 3. Manually add the files in this repo.
 4. Installing it via NuGet.
---
### 1. Installing via Unity Package Manager (UPM).
In your Unity project:
 1. Open the Package Manager Window/Tab

    ![image](https://user-images.githubusercontent.com/74650011/208429048-37e2277c-3e10-4794-97e7-3ec87f55f8c9.png)

 2. Click on + icon and then click on "Add Package From Git URL"

    ![image](https://user-images.githubusercontent.com/74650011/208429298-76fe1101-95f3-4ab0-bbd5-f0a32a1cc652.png)

 3. Enter URL: `https://github.com/liquiidio/CloudWalletUnity.git#upm`
   
---
### 2. Importing the Unity Package.
Download the [UnityPackage here](https://github.com/liquiidio/CloudWalletUnity/releases/latest/download/wcwunity.unitypackage).

Then in your Unity project:

 1. Open up the import a custom package window
    
    ![image](https://user-images.githubusercontent.com/74650011/208430044-caf91dd9-111e-4224-8441-95d116dbec3b.png)

 2. Navigate to where you downloaded the file and open it.
    
      ![image](https://user-images.githubusercontent.com/86061433/217523340-9b9ec00f-8e03-40dd-9647-52796371fedc.jpg)

 3. Check all the relevant files needed (if this is a first time import, just select ALL) and click on import.
   
     ![image](https://user-images.githubusercontent.com/86061433/217523464-e02b73fa-be34-4ac0-a406-fc4fd310d14c.jpg)

---
### 3. Install manually. 
Download this [project here](https://github.com/liquiidio/CloudWalletUnity/releases).

Then in your Unity project, copy the sources from `CloudWalletUnity` into your Unity `Assets` directory.

---

# Usage 
## Examples

### Quick Start

1. Create a new script inheriting from MonoBehaviour
2. Add a member of type WaxCloudWalletPlugin as well as a string to store the name of the user that is logged in.
3. In the Start-method, instantiate/initialize the CloudWalletPlugin.
4. Assign the EventHandlers/Callbacks allowing the CloudWalletPlugin to notify your Script about events and related Data 
5. Initialize the CloudWalletPlugin. This will start the communication with the Browser and create the binding between your local script and the wax-js running in the Browser.

```csharp
private CloudWalletPlugin _cloudWalletPlugin;
public string Account { get; private set; }

public void Start()
{
	// Instantiate the WaxCloudWalletPlugin
	_cloudWalletPlugin = new GameObject(nameof(CloudWalletPlugin)).AddComponent<CloudWalletPlugin>();

	// Assign Event-Handlers/Callbacks
	_cloudWalletPlugin.OnLoggedIn += (loginEvent) =>
	{
		Account = loginEvent.Account;
		Debug.Log($"{loginEvent.Account} Logged In");
	};

	_cloudWalletPlugin.OnError += (errorEvent) =>
	{
		Debug.Log($"Error: {errorEvent.Message}");
	};

	_cloudWalletPlugin.OnTransactionSigned += (signEvent) =>
	{
		Debug.Log($"Transaction signed: {JsonConvert.SerializeObject(signEvent.Result)}");
	};
	
	// Inititalize the WebGl binding while passign the RPC-Endpoint of your Choice
	_cloudWalletPlugin.InitializeWebGl("https://wax.greymass.com");
}
```

### Login


1. Logging in to the Wax Cloud Wallet Plugin is as simple as calling the Login-Method on [the previously](https://liquiidio.gitbook.io/unity-plugin-suite/v/cloudwalletwunity/examples/example_a) initialized WaxCloudWalletPlugin-instance.
2. Once the Login-Method is called, the user will be prompted with the standard Wax Cloud Wallet Login prompt and will be requested to follow the typical Login/Authentication-Scheme.

```csharp
public void Login()
{
	_cloudWalletPlugin.Login();
}
```

### Token Transfer

1. The following example shows how a Token Transfer Action can be created and passed to the Sign-Method of [the previously](https://liquiidio.gitbook.io/unity-plugin-suite/v/cloudwalletunity/examples/example_a) initialized WaxCloudWalletPlugin-Object.

```csharp
   // transfer tokens using a session
      private async Task Transfer(string frmAcc, string toAcc, string qnty, string memo)
      {
          var action = new EosSharp.Core.Api.v1.Action()
          {
              account = "eosio.token",
              name = "transfer",
              authorization = new List<PermissionLevel>() { _session.Auth },
              data = new Dictionary<string, object>
              {
                  {"from", frmAcc},
                  {"to", toAcc},
                  {"quantity", qnty},
                  {"memo", memo}
              }
          };
		
	  // Sign 
	 _cloudWalletPlugin.Sign(new[] { action });
	}
```
### Transact

1. Transacting/Signing Transactions with the Wax Cloud Wallet Plugin is as simple as calling the Sign-Method on [the previously](https://liquiidio.gitbook.io/unity-plugin-suite/v/cloudwalletunity/examples/example_a) initialized WaxCloudWalletPlugin-instance while passing a EosSharp Action Object.
2. To be able to perform a transaction, a user needs to [login](https://liquiidio.gitbook.io/unity-plugin-suite/v/cloudwalletunity/examples/example_b) first. Once a user has been logged in, the Plugin will automatically use the the logged in user to sign transactions.
3. Once the Sign-Method is called (while a user has previously logged in and a valid Action Object has been passed) the user will automatically be prompted with the typical Wax Cloud Authentication and Signing-Scheme.
4. If "Auto-Signing" is enabled, transactions will be signed automatically.
5. Immediately a Transaction-Signing is successful, the OnTransactionSigned-Handler will be called (see the [Quick-Start](https://liquiidio.gitbook.io/unity-plugin-suite/v/cloudwalletunity/examples/example_a) example)
6. If an error occurs, the OnError-Handler will be called (see the [Quick-Start](https://liquiidio.gitbook.io/unity-plugin-suite/v/cloudwalletunity/examples/example_a) example)

```csharp
public void Transact(EosSharp.Core.Api.v1.Action action)
{
	_cloudWalletPlugin.Sign(new[] { action });
}
```
