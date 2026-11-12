using UnityEngine;

namespace DigitalLove.Game.Placement
{
    // One headset-worn check for the placement views. The editor counts as worn so the authored
    // layout can run; on device the headset must report the user present.
    internal static class HeadsetPresence
    {
        public static bool IsWorn() =>
            Application.isEditor || OVRManager.instance == null || OVRManager.instance.isUserPresent;
    }
}
