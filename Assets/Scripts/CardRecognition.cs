using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class CardRecognition : MonoBehaviour
{
    private ObserverBehaviour observer;  // Vuforia 观察者
    public string cardName; // 该卡牌的名称（可以在 Inspector 设置）

    void Start()
    {
        observer = GetComponent<ObserverBehaviour>();
        if (observer)
        {
            observer.OnTargetStatusChanged += OnStatusChanged;
        }
    }

    private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        if (status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED)
        {
            Debug.Log($"Card recognized:{cardName}, Location:{transform.position}");
        }
        else
        {
            Debug.Log($"Card {cardName} Losted");
        }
    }
}
