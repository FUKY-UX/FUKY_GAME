using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookingMechanism : MonoBehaviour
{
    public CookingMechanism instance;
    public Pot _pot;
    public FoodItemBase _meet;
    private void Awake()
    {
        instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
