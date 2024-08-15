This repository contains my mods for VRChat. This is not a community-official repo because this mod can be exploited in-game. Join the [VRChat Modding Group discord](https://discord.gg/rCqKSvR) for official mods and support!  
## Special Thanks
Special thanks to nitro. (https://github.com/nitrog0d) and the entire modding community in general for inspiring me to get interested on C# and helping me to learn and solve my mistakes :)

## ItemGrabber
Features:
 * Lets you grab any item of your choice from any part of the map;
 * Lets you grab _all_ the items of the map simultaneously;
   * Allows you to limit that range if necessary, to avoid instance lag.
 * Allows you to control if you want to take network ownership of the item, necessary if you want to grab items that you weren't the last one to touch;
   * This will also forcefully take the items out of people's hands. Be careful!
 * Patches the item on grab so you can manipulate it even if it's disabled by default.
   * Also allows you to manually patch one item, or even _all_ items by choice. This will also allow you to take items from people's hands if you want;
   * This can also be configured in the settings in order to auto-patch every Scene Load.

## Orbit
Features:
 * Allows you to orbit your friends with all the pickups; Ehe~
   * Orbit Speed, X Y Z tilts and Radius are configurable;
   * There are three different types of orbit: Circular, Cylindrical and Spherical.
 * Just like ItemGrabber, allows you to decide if you wanna patch the items or not;
 * This mod will return all items to their original position and rotation after orbit stop.

## Force Clone
Features:
 * Allows you to forcefully clone any public avatar;
   * The button will get a different colour when the avatar is public, the person has cloning off and their avatar is not the same as yours.
 * The button will still show the compatible platforms.

## Copy Asset
Features:
 * Allows you to copy a .vrca asset to a path of your choice;
   * The path can be set on mod configs. Default is user's desktop.

## Used libraries:
* [UIExpansionKit by knah](https://github.com/knah/VRCMods/tree/master/UIExpansionKit) for menus and buttons interface.

## Installation
Before installing:  
**Modding itself is against VRChat's ToS, so, according to the staff, it can lead to punishment, these mods included. Be careful while using these!**
**Oh, also, don't be an asshole by using this mods for malicious purposes and ruining everyone's fun :). I don't promote such behaviours, and I made this mods for my own personal learning, usage and to play around with my friends.**

You will need [MelonLoader](https://discord.gg/2Wn3N2P) (discord link, see \#how-to-install).
After that, drop the mods .dll files in the `Mods` folder in your game's directory.

## Building
To build these, use the required libraries (found in `<vrchat install dir>/MelonLoader/Managed`) after MelonLoader installation.
