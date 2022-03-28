using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void MenuButtonAction();

    [Tooltip("The default color of button")]
    [SerializeField] private Color defaultColor;
    [Tooltip("The color of selected button")]
    [SerializeField] private Color selectedColor;
    [Tooltip("The color of hovered button")]
    [SerializeField] private Color highlighterColor;
    [SerializeField] private UnityEvent onActivate;

    private bool mouseOver = false;
    private Image image;
    private MenuNavigation instance;

    public event MenuButtonAction ActivateEvent = delegate { };
    public event MenuButtonAction SelectEvent = delegate { };

    private void Awake()
    {
        TryGetComponent(out image);
        //Try get menu instance reference 
        transform.parent.TryGetComponent(out instance);
    }

    private void Start()
    {
        image.color = defaultColor;
    }

    private void Update()
    {
        if(mouseOver == true && Input.GetButtonDown("Fire1") == true )
        {
            //if selected button for menu is this this button
            if (instance.SelectedButton == this)
            {
                Activate();
            }
            else
            {
                Select();
            }
        }
    }

    /// <summary>
    /// Use this method to invoke the selection event for the button.
    /// </summary>
    public void Select()
    {
        SelectEvent.Invoke();
    }

    /// <summary>
    /// Use this method to invoke the activation event for the button.
    /// </summary>
    public void Activate()
    {
        ActivateEvent.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
        if(instance.SelectedButton != this)
        {
            image.color = highlighterColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver=false;
        if (image.color == highlighterColor && this != instance.SelectedButton)
        {
            image.color = defaultColor;
        }
    }
    private void OnActivate()
    {
        onActivate.Invoke();
    }
    private void OnSelect()
    {
        if(instance.SelectedButton != null)
        {
            instance.SelectedButton.image.color = instance.SelectedButton.defaultColor;
        }
        instance.SelectedButton = this;
        image.color = selectedColor;
    }
    private void OnEnable()
    {
        ActivateEvent += OnActivate;
        SelectEvent += OnSelect;
    }
    private void OnDisable()
    {
        ActivateEvent -= OnActivate;
        SelectEvent -= OnSelect;

    }
}
