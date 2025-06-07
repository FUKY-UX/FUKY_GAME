using UnityEngine;
using System;



public class DefaultItem : GrabInteractedItemOrigin
{
    private void Awake()
    {
        base.InitItemStateAndPhy();
    }
    private void Start()
    {
        base.registerAduioList();
    }
}
