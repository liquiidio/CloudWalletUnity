

<div align="center">
 <img src="https://avatars.githubusercontent.com/u/82725791?s=200&v=4" align="center"
     alt="Liquiid logo" width="280" height="300">
</div>

# WAX Cloud Wallet (WCW) (!TODO ALL!)

A combination of local HttpListeners receiving OAuth-Callbacks from WCW-related web-adresses opened through the WebView-Plugin, gathering necessary initial information like OAuth-Tokens, followed by regular non-browser-based (no WebView needed) communication with the WCW-API/Server.


# Installation

**_Requires Unity 2019.1+ with .NET 4.x+ Runtime_**

This package can be included into your project by either:

 1. Installing the package via Unity's Package Manager (UPM) in the editor (recommended).
 2. Importing the .unitypackage which you can download here.
 3. Manually add the files in this repo.
 4. Installing it via NuGet.
---
### 1. Installing via Unity Package Manager (UPM).
In your Unity project:
 1. Open the Package Manager Window/Tab

    ![image](https://user-images.githubusercontent.com/74650011/208429048-37e2277c-3e10-4794-97e7-3ec87f55f8c9.png)

 2. Click on + icon and then click on "Add Package From Git URL"

    ![image](https://user-images.githubusercontent.com/74650011/208429298-76fe1101-95f3-4ab0-bbd5-f0a32a1cc652.png)

 3. Enter URL:  `https://github.com/endel/NativeWebSocket.git#upm`
    // (!TODO!) ADD CORRECT LINK AND RELEVANT SCREENSHOT
---
### 2. Importing the Unity Package.
Download the UnityPackage here <<-- (Hyper link this). Then in your Unity project:

 1. Open up the import a custom package window
    
    ![image](https://user-images.githubusercontent.com/74650011/208430044-caf91dd9-111e-4224-8441-95d116dbec3b.png)

 3. Navigate to where you downloaded the file and open it.
    
    ![image](https://user-images.githubusercontent.com/74650011/208430782-871b64c5-fa00-44bf-96c3-685743b77a63.png)

 4. Check all the relevant files needed (if this is a first time import, just select ALL) and click on import.
   (!TODO!)
   // ADD THE CORRECT SCREENSHOT FOR IMPORT WINDOW
   
   ![image](https://user-images.githubusercontent.com/74650011/208431004-953e07d1-325d-4e9a-a4e1-fc845de06fdd.png)

---
### 3. Install manually. (!TODO!)
Download this project there here <<-- (Hyper link this to the zip download). Then in your Unity project:

 1. Copy the sources from `NativeWebSocket/Assets/WebSocket` into your `Assets` directory.

---
### 4. Install via NuGet (!TODO!)
<img src="https://media.tenor.com/SLXlt36s35kAAAAC/scooby-doo-witch-doctor.gif" align="center"
     alt="Liquiid logo">

# Usage (!TODO!)

.NET and Unity3D-compatible (Desktop, Mobile, WebGL) ApiClient for the different APIs. 
Endpoints have its own set of parameters that you may build up and pass in to the relevant function.

### Examples (!TODO!)

 Based on the different endpoints
 
```csharp
    new AnchorLink(new LinkOptions()
                {
                    Transport = this.Transport,
                    // Uncomment this for and EOS session
                    //ChainId = "aca376f206b8fc25a6ed44dbdc66547c36c6c33e3a119ffbeaef943642f0e906",
                    //Rpc = "https://eos.greymass.com",
```
<br>
```csharp
    // WAX session
            ChainId = "1064487b3cd1a897ce03ae5b6a865651747e2e152090f99c1d19d44e01aea5a4",
            Rpc = "https://api.wax.liquidstudios.io",
            ZlibProvider = new NetZlibProvider(),
            Storage = new PlayerPrefsStorage()
        });
```
---
## Additional examples (!TODO!)
These are examples based on the specific plugin/package usage.
Achor link - Creating and signing different kinds of transactions.  

### An example (!TODO!)

AnchorLink

Token Transfer 
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

            //Debug.Log($"Session {_session.Identifier}");
            //Debug.Log($"Link: {_link.ChainId}");

            try
            {
                var transactResult = await _link.Transact(new TransactArgs() { Action = action });
                // OR (see next line)
                //var transactResult = await _session.Transact(new TransactArgs() { Action = action });
                Debug.Log($"Transaction broadcast! {transactResult.Processed}");

                waitCoroutine = StartCoroutine(SwitchPanels(Transport.currentPanel, CustomActionsPanel, 1.5f));

            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }
```
Link? (!TODO!)

- NFT Transfer - link
- Create Permission - link
- Get Balanaces - link



[build-badge]: https://github.com/mkosir/react-parallax-tilt/actions/workflows/build.yml/badge.svg
[build-url]: https://github.com/mkosir/react-parallax-tilt/actions/workflows/build.yml
[test-badge]: https://github.com/mkosir/react-parallax-tilt/actions/workflows/test.yml/badge.svg
[test-url]: https://github.com/mkosir/react-parallax-tilt/actions/workflows/test.yml
