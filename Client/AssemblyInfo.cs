using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;

[assembly: AssemblyTitle(Main.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(null)]
[assembly: AssemblyProduct(Main.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + Main.BuildInfo.Author)]
[assembly: AssemblyTrademark(null)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion(Main.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Main.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(Main.ModMain), Main.BuildInfo.Name, Main.BuildInfo.Version, Main.BuildInfo.Author)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(System.ConsoleColor.Magenta)]