using UnityEngine;

public class StateA : AState
{
    public override void Update()
    {
        Debug.Log("State A Update");
    }
}

public class StateB : AState
{
    public override void Update()
    {
        Debug.Log("State B Update");
    }
}