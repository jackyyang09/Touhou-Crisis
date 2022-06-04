using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RailShooterInputModule : BaseInputModule
{
    [SerializeField] RailShooterLogic shooterLogic;

    GameObject selectedObject = null;

    PointerEventData pointerData = null;

    EventSystem es;

    protected override void Awake()
    {
        base.Awake();

        es = FindObjectOfType<EventSystem>();
        es.UpdateModules();

        pointerData = new PointerEventData(es);
    }

    protected override void OnEnable()
    {
        shooterLogic.OnShoot += OnShoot;
    }

    protected override void OnDisable()
    {
        shooterLogic.OnShoot -= OnShoot;
    }

    private void OnShoot(Ray arg1, Vector2 arg2)
    {
        ProcessPress(pointerData);
    }

    public override void Process()
    {
        // Reset data, set camera
        pointerData.Reset();
        pointerData.position = Input.mousePosition;

        // Raycast
        es.RaycastAll(pointerData, m_RaycastResultCache);
        pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        selectedObject = pointerData.pointerCurrentRaycast.gameObject;

        m_RaycastResultCache.Clear();

        HandlePointerExitAndEnter(pointerData, selectedObject);
    }

    public PointerEventData GetData()
    {
        return pointerData;
    }

    void ProcessPress(PointerEventData data)
    {
        // Set raycast
        data.pointerPressRaycast = data.pointerCurrentRaycast;

        // Check for object hit, get the down handler, call
        GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(selectedObject, data, ExecuteEvents.pointerDownHandler);

        // If no down handler, try and get click handler
        if (newPointerPress == null)
        {
            newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(selectedObject);
        }

        // Set data
        data.pressPosition = data.position;
        data.pointerPress = newPointerPress;
        data.rawPointerPress = selectedObject;

        ProcessRelease(pointerData);
    }

    private void ProcessRelease(PointerEventData data)
    {
        // Execute pointer up
        ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

        // Check click handler
        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(selectedObject);

        // Check if actual
        if (data.pointerPress == pointerUpHandler)
        {
            ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);
        }

        // Clear selected gameObject
        es.SetSelectedGameObject(null);

        // Reset data
        data.pressPosition = Vector2.zero;
        data.pointerPress = null;
        data.rawPointerPress = null;
    }
}
