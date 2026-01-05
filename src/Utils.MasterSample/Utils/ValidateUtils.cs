using System.Runtime.CompilerServices;
using AndanteTribe.Utils.MasterSample.Units;

namespace AndanteTribe.Utils.MasterSample.Utils;

public static class ValidateUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ValidateBasicStatus(BasicStatus status)
    {
        // HP(-21億~21億)
        if (status.HP is < -2100000000 or > 2100000000)
        {
            return false;
        }

        // 筋力(0~9999)
        if (status.Strength is < 0 or > 9999)
        {
            return false;
        }

        // 耐久力(0~9999)
        if (status.Vitality is < 0 or > 9999)
        {
            return false;
        }

        // 敏捷(50~300)
        if (status.Agility is < 50 or > 300)
        {
            return false;
        }

        // 幸運(0~100)
        if (status.Luck is < 0 or > 100)
        {
            return false;
        }

        // 意志力(0~300)
        if (status.WillPower is < 0 or > 300)
        {
            return false;
        }

        // 絆ゲージ(0~300)
        if (status.LinkGauge is < 0 or > 300)
        {
            return false;
        }

        return true;
    }
}