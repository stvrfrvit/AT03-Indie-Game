using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteraction : MonoBehaviour, IInteractable
{
    #region boolean definition
    private bool exampleBool;

    public bool ExampleBool
    {
        get { return exampleBool; }
    }
    #endregion

    public delegate void InteractionDelegate();

    public event InteractionDelegate interactionEvent = delegate { };

    private void OnEnable()
    {
        interactionEvent = new InteractionDelegate(TestMethod);
        interactionEvent += TestTwo;
    }

    private void OnDisable()
    {
        interactionEvent -= TestMethod;
        interactionEvent -= TestTwo;
    }

    public void Activate() 
    {
        interactionEvent.Invoke();
    }

    public void TestMethod()
    {
        Debug.Log("delegate exec");
    }

    private void TestTwo()
    {
        Debug.Log("delegate 2 exec");
    }
}
