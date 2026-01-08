# qb.WebServices
Components for web services connections

## CONTENT

**ApiSettings**

Provides a base class for configuring API settings, including versioning, entry point URLs, token management, and maintenance state

**JsonWebRequest\<T>**

Abstract class for a singleton scriptable object to manage a web request with json format as data format.
With T as type of request data response.

**JsonWebRequest_Cached\<T>**

Abstract Json web request class extension with cache management with refresh time to minimize web request calls.

**JsonWebRequest_CacheManager**

Monobehaviour component to manage caching for JSON web requests and ensures cache persistence across the application's lifetime.

#

**SteamApiSettings**

Steam api scriptable object settings for steam api request [Create/qb/Network/SteamAPI/SteamApiSettings]

**SteamStoreApiSettings**

Steam store api scriptable object for steam store api request [Create/qb/Network/SteamAPI/SteamStoreApiSettings]


**GetPlayerSummaries_SteamRequest**

**GetPlayerAchievemments_SteamRequest**

**GetOwnedGames_SteamRequest**

**GetFriendList_SteamRequest**

**GetAppsDetails_SteamRequest**

**GetShemasForGame_SteamRequest**



## HOW TO INSTALL

Use the Unity package manager and the Install package from git url option.

- Install at first time,if you haven't already done so previously, the package <mark>[unity-package-manager-utilities](https://github.com/sandolkakos/unity-package-manager-utilities.git)</mark> from the following url: 
  [GitHub - sandolkakos/unity-package-manager-utilities: That package contains a utility that makes it possible to resolve Git Dependencies inside custom packages installed in your Unity project via UPM - Unity Package Manager.](https://github.com/sandolkakos/unity-package-manager-utilities.git)

- Next, install the package from the current package git URL. 
  
  All other dependencies of the package should be installed automatically.

## Dependencies

- https://github.com/quanty-bandit/qb.Pattern.git
- https://github.com/quanty-bandit/qb.Datas.git
- https://github.com/quanty-bandit/qb.Utility.git
- https://github.com/quanty-bandit/qb.BuildInfo.git
- [GitHub - codewriter-packages/Tri-Inspector: Free inspector attributes for Unity [Custom Editor, Custom Inspector, Inspector Attributes, Attribute Extensions]](https://github.com/codewriter-packages/Tri-Inspector.git)
