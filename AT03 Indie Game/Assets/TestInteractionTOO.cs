using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractionTOO : MonoBehaviour, IInteractable
{
    public TestInteraction interaction;

    private void Start()
    {
        interaction.interactionEvent += TestThree; 
    }

    public void Activate()
    {
        Debug.Log("box +: " + interaction.ExampleBool);
        interaction.Activate();
    }

    private void TestThree()
    {
        Debug.Log("delegate 3 exec");
    }
}
