
using System.Collections.Generic;

using SuperNewRoles.Buttons;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral
{
    class WaveCannonJackal
    {
        public static void ResetCoolDowns()
        {
            HudManagerStartPatch.JackalKillButton.MaxTimer = CustomOptions.WaveCannonJackalKillCoolDown.GetFloat();
            HudManagerStartPatch.JackalKillButton.Timer = HudManagerStartPatch.JackalKillButton.MaxTimer;
        }
    }
}
