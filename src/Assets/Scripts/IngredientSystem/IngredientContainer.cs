using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(IngredientSpawner))]
public class IngredientContainer : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnPositions;
    [SerializeField] private int maximumCapacity = 3;
    [SerializeField] private int currentAmount;

    [SerializeField] private bool isDebugging;

    [Header("Only if bark container")]
    [SerializeField] private bool needsEmpty;
    [SerializeField] private GameObject emptyIngredient;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject barkDirt;

    private IngredientSpawner _spawner;

    private void Awake()
    {
        _spawner = GetComponent<IngredientSpawner>();
        EmptyGrabbable.HoverBarkContainer += HandleBarkHover;

        if (spawnPositions.Count > 0)
        {
            maximumCapacity = spawnPositions.Count;
        }

        if (needsEmpty)
        {
            Refill();
            return;
        }

        for(int i = 0; i < spawnPositions.Count; i++)
        {
            RefillSlot();
        }

        
    }

    private void HandleBarkHover(bool isHovered)
    {
        if (!needsEmpty) return;
        animator.SetBool("IsOpened", isHovered);
    }

    private void Start()
    {
        if(needsEmpty) ResetEmptyIngredient(); // Only bark
    }

    public bool Refill()
    {
        if(currentAmount == maximumCapacity)
        {
            if(isDebugging) print("There was already max in bark");
            return false;
        }
        currentAmount = maximumCapacity;
        barkDirt.SetActive(true);
        return true;
    }

    private void RefillSlot()
    {
        Ingredient noobIngredient = _spawner.SpawnIngredient();
        foreach(GameObject spawnPosition in spawnPositions)
        {
            if(spawnPosition.transform.childCount <= 0)
            {
                noobIngredient.gameObject.transform.SetParent(spawnPosition.transform, false);
                noobIngredient.transform.position = spawnPosition.transform.position;
                currentAmount++;
                return;
            }
        }       
    }

    public bool RefillSingle()
    {
        if (isDebugging) print("Current amount: " + currentAmount + " Max capacity: " + maximumCapacity);
        if(currentAmount < maximumCapacity)
        {
            RefillSlot();
            return true;
        }
        else
        {
            if (isDebugging) print("You have refilled max!");
            return false;
        }
    }

    public Ingredient TakeIngredient()
    {
        if (currentAmount <= 0)
        {
            if (isDebugging) print("There are no more ingredients");
            barkDirt.SetActive(false);
            return null;
        }
        currentAmount--;

        Ingredient noobIngredient = _spawner.SpawnIngredient();
        if (isDebugging) print("Returning new ingredient");
        return noobIngredient;
    }

    public void ResetEmptyIngredient()
    {
        Instantiate(emptyIngredient, transform);
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Ingredient")) // If they have the same parent, then the ingredient should be unparented because it is taken away
        {
            currentAmount--;
            if(currentAmount <= 0) currentAmount = 0; // WORKAROUND Kinda Fixes bug of some other ingredients leave the box and mess up the counter so there is endless refill
            if (isDebugging) print("An ingredient left  " + collider + " Current amount: " + currentAmount);
        }
    }

}
