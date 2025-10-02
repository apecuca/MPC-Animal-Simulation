using UnityEngine;

public class FSM : AIHead
{
    //

    override protected void Awake()
    {
        base.Awake();
    }

    // Tomada de decisões da FSM
    override protected AIAction DecideNewAction() 
    { 
        return AIAction.NONE; 
    }
}
