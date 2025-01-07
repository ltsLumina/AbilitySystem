using System;
using System.Collections;
using Lumina.Essentials.Modules;
using UnityEngine;
using static Lumina.Essentials.Modules.Helpers;

public class GolemClaymore : Item
{
    void Start()
    {
        Action();
    }

    public override void Action()
    {
        Debug.Log("Golem Claymore invoked.");

        while (true)
        {
            StartCoroutine(Wait());
            break;
        }

        return;
        IEnumerator Wait()
        {
            Player player = Find<Player>();
            
            Debug.Log("Golem Claymore effect applied.");
            yield return new WaitForSeconds(20);
        }
    }
}
