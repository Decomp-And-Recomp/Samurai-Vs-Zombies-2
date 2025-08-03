using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewInput : MonoBehaviour
{
    public static bool moveLeft
    {
        get
        {
            return Input.GetButton("Move Left");
        }
    }

    public static bool moveRight
    {
        get
        {
            return Input.GetButton("Move Right");
        }
    }

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
        switch (index)
        {
            case 0: return Input.GetButtonDown("Spawn Ally 1");
            case 1: return Input.GetButtonDown("Spawn Ally 2");
            case 2: return Input.GetButtonDown("Spawn Ally 3");
            case 3: return Input.GetButtonDown("Spawn Ally 4");
            case 4: return Input.GetButtonDown("Spawn Ally 5");
        }
        return false;
    }

    public static bool UseAbility(int index)
    {
        switch (index)
        {
            case 0: return Input.GetButtonDown("Ability 1");
            case 1: return Input.GetButtonDown("Ability 2");
            case 2: return Input.GetButtonDown("Ability 3");
            case 3: return Input.GetButtonDown("Ability 4");
        }
        return false;
    }
}
