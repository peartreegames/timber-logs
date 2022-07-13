using System.Runtime.CompilerServices;
using UnityEngine;

namespace PeartreeGames.TimberLogs
{
    public static class TimberExtension
    {
        public static void Timber(this MonoBehaviour m, string msg, [CallerMemberName] string memberName = "")
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var go = m.gameObject;
            if (!TimberManager.Timbers.TryGetValue(go, out var timber))
            {
                timber = new TimberMessages(go);
                TimberManager.Timbers.Add(go, timber);
            }
            timber.Add($"[{m.GetType().Name}.{memberName}] {msg}");
#endif
        }
    }
}