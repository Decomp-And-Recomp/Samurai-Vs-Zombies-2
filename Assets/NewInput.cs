using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewInput : MonoBehaviour
{
    public static bool pause
    {
        get
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            return Input.GetButtonDown("Pause");
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }

    public static bool upgradeLeadership
    {
        get
        {
            return Input.GetButtonDown("Upgrade Leadership");
        }
    }

    public static bool SpawnAlly(int index)
    {
        return Input.GetButtonDown("Spawn Ally " + (index + 1));
    }

    public static bool UseAbility(int index, int length)
    {
        return Input.GetButtonDown("Ability " + (length - index));
    }

    public static bool UseConsumable(int index)
    {
        return Input.GetButtonDown("Consumable " + (index + 1));
    }
}
