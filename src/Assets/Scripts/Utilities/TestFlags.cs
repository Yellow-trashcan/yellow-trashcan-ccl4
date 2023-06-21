using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CurrentRoom { Garden, Brewing, Entrance }
public enum InteractionEvents { GrabIngredient, ReleaseIngredient, PutIngredientPot, RefilledLiquid, RefilledSolid, CreateCorrectPotion, CreateIncorrectPotion, GrabPotion, ReleasePotion, DeliverCorrectPotion, DeliverIncorrectPotion, ThrowPotionGarbage, TravelledGarden, TravelledBrewing, TravelledEntrance, CreatePotion, GrabFlask, CustomerArrives, LevelStarted, LevelEnded, PauseGame, ResumeGame, RevealIngredients, FinishedTutorial, RefilledHerb, RefilledBark, RefilledMushroom }

public class TestFlags : MonoBehaviour
{
}
