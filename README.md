# Cards Against Humanity Online
Cards Against Humanity Online is a web-based online multiplayer version of the card game 'Cards Against Humanity' which you can play together with your friends. It can be hosted on any computer supporting the .NET Framework 4.0 or higher and played on every modern device capable of browsing the web.

The development of this app started due to me not liking how [Pretend You're Xyzzy](pretendyoure.xyz) forces all players to stay connected and does not really allow long-term game sessions with long breaks.

### Some technical notes
Cards Against Humaniy Online is based around a webserver developed in C# and all of the communication is done via http requests. All the webpages are generated server-side and do not require anything else besides what is sent with the initial request. This means that the whole application uses neither fonts, nor external style sheets & javascript files, images or even gigantic JS frameworks. Furthermore, every request over a certain size is automatically gzipped but nothing is ever minified.

The result is that an active game takes ~2KB for the initial load and then about **500KB per hour** of gameplay.

### Setup
The amount of work necessary for setting up the server is minimal: Compile the project in release mode (or download the pre-compiled binaries from the release-section), start the server-executable with admin rights (needed to be able to listen on the game port properly) and you're done - your own Cards Against Humanity Online-server is up and running.

The server runs on port `31815` by default and can be accessed in the following ways:

* Access from host PC: Use [http://localhost:31815/](http://localhost:31815/) from your local pc to access the server. This is also the only way to access the server when run in Debug-Mode.
* Access from local network (LAN): Use your local ip address to access the server from your local LAN network (you might have to open the port in your computer's firewall), e.g. [http://192.168.1.152:31815/](http://192.168.1.152:31815/).
* Access from the internet: use your public ip address (use a tool such as [WhatIsMyIP](http://www.whatismypublicip.com/) to find it out) to play with your friends from all over the internet (you will probably have to open the port used for the server in your router's firewall). If you do not want to find out the correct ip address every time, you can use a service such as [No-IP](https://www.noip.com/) to get a static url which redirects to your server dynamically.

### Legal Notice
This web game is based off the card game [Cards Against Humanity](https://www.cardsagainsthumanity.com/) and [JSON Against Humanity](http://www.crhallberg.com/cah/json) which are available for free under the [Creative Commons BY-NC-SA 2.0 license](https://creativecommons.org/licenses/by-nc-sa/2.0/). I am neither associated with the Cards Against Humanity LLC, creators of Cards Against Humanity, nor Chris Hallberg, the creator of JSON Against Humanity, in any way. If you should have concerns regarding the correctness of my usage of their property, please send me an email (the address is available through my profile).
