using System;
using Unity.Cinemachine;
using UnityEngine;

public class DaniTechCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineBrain CinemachineBrain_Main;
    [SerializeField] private CinemachineBrainEvents CinemachinBrainEvents_Main;

    public static DaniTechCameraManager Inst { get; set; }

    private Camera _mainCamera;

    private void Awake()
    {
        Inst = this;

        _mainCamera = Camera.main;

        if (CinemachineBrain_Main == null)
        {
            var gObj = GameObject.Find("Main Camera");
            if (gObj != null)
            {
                var brain = gObj.GetComponent<CinemachineBrain>();
                if (brain != null)
                {
                    CinemachineBrain_Main = brain;
                }

                var brainEvents = gObj.GetComponent<CinemachineBrainEvents>();
                if (brainEvents != null)
                {
                    CinemachinBrainEvents_Main = brainEvents;
                }
            }
        }
    }

    public Camera GetMainCamera()
    {
        return _mainCamera;
    }

    public CinemachineBrain GetMainCinemachineBrain()
    {
        return CinemachineBrain_Main;
    }

    public void BindMainCameraUpdatedEvent(Action<CinemachineBrain> callbackEvent)
    {
        CinemachinBrainEvents_Main.BrainUpdatedEvent.AddListener(callbackEvent.Invoke);
    }

    public void UnBindMainCameraUpdatedEvent(Action<CinemachineBrain> callbackEvent)
    {
        CinemachinBrainEvents_Main.BrainUpdatedEvent.RemoveListener(callbackEvent.Invoke);
    }




}
