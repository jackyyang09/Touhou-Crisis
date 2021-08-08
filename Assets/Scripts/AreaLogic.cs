using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AreaLogic : MonoBehaviour, IReloadable
{
    [System.Serializable]
    public class SubAreaProps
    {
        public Transform player1CoverTransform;
        public Transform player1FireTransform;
        public Transform player1MeleeTransform;

        public Transform player2CoverTransform;
        public Transform player2FireTransform;
        public Transform player2MeleeTransform;
    }

    [SerializeField] SubAreaProps[] subAreas = null;

    [SerializeField] short currentSubArea = 0;
    public short CurrentArea
    { 
        get
        {
            return currentSubArea;
        }
    }

    public Transform Player1CoverTransform { get { return subAreas[currentSubArea].player1CoverTransform; } }
    public Transform Player1FireTransform { get { return subAreas[currentSubArea].player1FireTransform; } }
    public Transform Player1MeleeTransform { get { return subAreas[currentSubArea].player1MeleeTransform; } }

    public Transform Player2CoverTransform { get { return subAreas[currentSubArea].player2CoverTransform; } }
    public Transform Player2FireTransform { get { return subAreas[currentSubArea].player2FireTransform; } }
    public Transform Player2MeleeTransform { get { return subAreas[currentSubArea].player2MeleeTransform; } }

    static AreaLogic instance;
    public static AreaLogic Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AreaLogic>();
            }
            return instance;
        }
    }

    public static System.Action OnEnterFirstArea;

    public void Reinitialize()
    {
        currentSubArea = -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public void ReportPlayerArrival()
    {
        currentSubArea++;
        if (currentSubArea == 0)
        {
            OnEnterFirstArea?.Invoke();
        }
    }

    private void Reset()
    {
        subAreas = new SubAreaProps[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform areaGameObject = transform.GetChild(i);
            var newSubArea = new SubAreaProps();
            newSubArea.player1CoverTransform = areaGameObject.GetChild(0).GetChild(0);
            newSubArea.player1FireTransform = areaGameObject.GetChild(0).GetChild(1);
            newSubArea.player2CoverTransform = areaGameObject.GetChild(1).GetChild(0);
            newSubArea.player2FireTransform = areaGameObject.GetChild(1).GetChild(1);
            subAreas[i] = newSubArea;
        }
    }
}
