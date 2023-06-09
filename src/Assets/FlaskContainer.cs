using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlaskContainer : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnPositions;
    [SerializeField] private int maximumCapacity = 2;
    [SerializeField] private int currentCapacity;
    [SerializeField] private GameObject flaskPrefab;

    private void Awake()
    {
        InteractionsHandler.InteractionRaised += HandleRaisedInteractions;
        if (spawnPositions.Count > 0)
        {
            maximumCapacity = spawnPositions.Count;
        }

        for (int i = 0; i < spawnPositions.Count; i++)
        {
            RefillSlot();
        }

        Refill();
    }

    private void OnDisable()
    {
        InteractionsHandler.InteractionRaised -= HandleRaisedInteractions;
    }

    private void HandleRaisedInteractions(InteractionEvents raisedEvent)
    {
        if(raisedEvent == InteractionEvents.ThrowPotionGarbage || raisedEvent == InteractionEvents.DeliverCorrectPotion || raisedEvent == InteractionEvents.DeliverIncorrectPotion)
        {
            RefillSlot();
        }
    }

    private void RefillSlot()
    {
        GameObject noobFlask = Instantiate(flaskPrefab);
        Flask flask = noobFlask.GetComponent<Flask>();

        foreach (GameObject spawnPosition in spawnPositions)
        {
            if (spawnPosition.transform.childCount <= 0)
            {
                noobFlask.transform.SetParent(spawnPosition.transform, false);
                noobFlask.transform.position = spawnPosition.transform.position;

                return;
            }
        }
    }

    public void RefillSingle()
    {
        if (currentCapacity != maximumCapacity)
        {
            RefillSlot();
            currentCapacity--;
        }
    }
    public void Refill()
    {
        currentCapacity = maximumCapacity;
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Ingredient"))
        {
            currentCapacity--;
        }
    }
}
