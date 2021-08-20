using System;
using MelonLoader;

namespace PMod.Loader
{
    // Straight skidded it from ReModCE https://github.com/RequiDev/ReModCE/blob/master/ReModCE.Loader/ReLogger.cs
    public static class PLogger
    {
        public static void Msg(string txt) => MelonLogger.Msg("[PMod] " + txt);
        public static void Msg(string txt, params object[] args) => MelonLogger.Msg("[PMod] " + txt, args);
        public static void Msg(object obj) => MelonLogger.Msg(obj);
        public static void Msg(ConsoleColor txtcolor, string txt) => MelonLogger.Msg(txtcolor, "[PMod] " + txt);
        public static void Msg(ConsoleColor txtcolor, string txt, params object[] args) => MelonLogger.Msg(txtcolor, "[PMod] " + txt, args);
        public static void Msg(ConsoleColor txtcolor, object obj) => MelonLogger.Msg(txtcolor, obj);

        public static void Warning(string txt) => MelonLogger.Warning("[PMod] " + txt);
        public static void Warning(string txt, params object[] args) => MelonLogger.Warning("[PMod] " + txt, args);
        public static void Warning(object obj) => MelonLogger.Warning(obj);

        public static void Error(string txt) => MelonLogger.Error("[PMod] " + txt);
        public static void Error(string txt, params object[] args) => MelonLogger.Error("[PMod] " + txt, args);
        public static void Error(object obj) => MelonLogger.Error(obj);
    }
}