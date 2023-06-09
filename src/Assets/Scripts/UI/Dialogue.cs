using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public struct TextBlock
{
    [SerializeField, TextArea] public string text;
    [SerializeField] public UnityEvent events;
    [SerializeField] public List<ActionToFulfill> actionsToFulfill;
    [SerializeField] public bool needsClickToContinue;
    [SerializeField] public List<FVXHandler> elementsToVFX;
}

[Serializable]
public class ActionToFulfill
{
    [SerializeField] public InteractionEvents interactionEvent;
    [SerializeField] public bool hasBeenFulfilled;
}

[Serializable]
public class ElementsToVFX
{
    public List<MeshRenderer> meshRenderer;
}

public class Dialogue : MonoBehaviour
{
    [SerializeField] private BookoFacade bookoFacade;
    [SerializeField] private List<TextBlock> textBlocks;
    [SerializeField] private float characterDelay;

    [SerializeField]
    [Tooltip("This is the pressing A action")]
    InputActionReference _pressAAction;

    [SerializeField]
    [Tooltip("This is the pressing Y (emergency exit) action")]
    InputActionReference _pressYAction;

    [SerializeField] private int dialogueToStart = 0;
    [SerializeField] private bool isDebugging;

    private bool _writing = true;
    private bool _canAdvance = false;
    private int _currentDialogue = 0;

    public static event Action<CurrentRoom> AskToActivateDoor;
    public static event Action AskToSpawnCustomer;
    public static event Action<InteractionEvents> InteractionRaised;

    private InputAction pressA;
    private InputAction pressY;
    private void Awake()
    {
        pressA = GetInputAction(_pressAAction);
        pressA.canceled += PressedA;

        pressY = GetInputAction(_pressYAction);
        pressY.canceled += PressedY;

        InteractionsHandler.InteractionRaised += HandleFlags;
    }

    public void ActivateATip()
    {
        bookoFacade.DeactivateTips();
        bookoFacade.ATip.SetActive(true);
    }

    public void ActivateBTip()
    {
        bookoFacade.DeactivateTips();
        bookoFacade.BTip.SetActive(true);
    }

    public void ActivateTriggerTip()
    {
        bookoFacade.DeactivateTips();
        bookoFacade.TriggerTip.SetActive(true);
    }

    public void ActivateGrabTip()
    {
        bookoFacade.DeactivateTips();
        bookoFacade.GrabTip.SetActive(true);
    }

    private void OnDisable()
    {
        if (isDebugging) print("The dialogue was disabled");
        InteractionsHandler.InteractionRaised -= HandleFlags;
        pressA.canceled -= PressedA;
        pressY.canceled -= PressedY;
    }

    void Start()
    {
        _currentDialogue = dialogueToStart;
        bookoFacade.ContinueButton.SetActive(false);
        StartCoroutine(NextTextblock());
        EnableContinue();
    }

    public void CloseDialogue()
    {
        if(isDebugging) print("Closed dialogue");
        InteractionRaised?.Invoke(InteractionEvents.FinishedTutorial);
        gameObject.SetActive(false);
    }

    private IEnumerator NextTextblock()
    {
        AkSoundEngine.PostEvent("Play_BookoDialogue", gameObject);
        bookoFacade.BookoAnimator.SetBool("IsTalking", true);
        HandleVFXElements();
        bookoFacade.DeactivateTips();

        if (_currentDialogue < textBlocks.Count)
        {
            _writing = true;
            bookoFacade.DialogueText.text = "";
            char[] charArray = textBlocks[_currentDialogue].text.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                bookoFacade.DialogueText.text += charArray[i];
                yield return new WaitForSeconds(characterDelay);
            }

            textBlocks[_currentDialogue].events.Invoke();
            _writing = false;
        }
        if (textBlocks[_currentDialogue].needsClickToContinue)
        {
            bookoFacade.ContinueButton.SetActive(true);
            if (isDebugging) print("Setting in Next block");
        }
        bookoFacade.BookoAnimator.SetBool("IsTalking", false);
    }

    private void HandleVFXElements()
    {
        for (int i = 0; i < textBlocks[_currentDialogue].elementsToVFX.Count; i++) // Goes through all of the elements that should be highlighted
        {
            textBlocks[_currentDialogue].elementsToVFX[i].ActivateHighLight();
        }

        if (_currentDialogue <= 1) return;
        for (int i = 0; i < textBlocks[_currentDialogue - 1].elementsToVFX.Count; i++) // Goes through all of the elements that should be dehilighted (in case they werent already)
        {
            textBlocks[_currentDialogue - 1].elementsToVFX[i].DeactivateHighlight();
        }
    }

    public void ProceedDialogue()
    {
        if (!_writing)
        {
            _currentDialogue++;
            StartCoroutine(NextTextblock());
            DisableContinue();
            CheckIfFlagsFulfilled();
        }
    }

    private void HandleFlags(InteractionEvents interactionEvent)
    {
        if(isDebugging) print("An interaction was raised: " + interactionEvent + "CurrentRoom Block: " + _currentDialogue );
        for (int i = 0; i < textBlocks[_currentDialogue].actionsToFulfill.Count; i ++) // Iterate through all of the needed actions to ulfill from the current block
        {
            // continuing if the action has been fulfilled would let us have two of the same but would prevent being able to revert a done to a needs to be done (like messing up something and you need to redo it)
            if (textBlocks[_currentDialogue].actionsToFulfill[i].interactionEvent == interactionEvent) // Then it was the same one, so I can raise the flag
            {
                textBlocks[_currentDialogue].actionsToFulfill[i].hasBeenFulfilled = true;
                CheckIfFlagsFulfilled();
                return; // TODO: Make it better so that there could be two of grab ingredient
            }
        }
    }

    private void CheckIfFlagsFulfilled()
    {
        for (int i = 0; i < textBlocks[_currentDialogue].actionsToFulfill.Count; i++)
        {
            if (textBlocks[_currentDialogue].actionsToFulfill[i].hasBeenFulfilled == false) 
            {
                return; // If any are false, we co not get to enable continue
            }
        }

        EnableContinue();
    }

    private void EnableContinue()
    {
        _canAdvance = true;
        if(!textBlocks[_currentDialogue].needsClickToContinue) ProceedDialogue();
    }

    private void DisableContinue()
    {
        if (isDebugging) print("Setting inactive in disable");
        bookoFacade.ContinueButton.SetActive(false);
        _canAdvance = false;
    }

    public void CheckWhichNeedToBeFulfilled()
    {
        if (isDebugging) print("Dialogue check. Number: " + textBlocks[_currentDialogue].actionsToFulfill.Count + " dialogue: " + _currentDialogue);

        for(int i = 0; i < textBlocks[_currentDialogue].actionsToFulfill.Count; i++)
        {
            print(textBlocks[_currentDialogue].actionsToFulfill[i].interactionEvent);
        }
        
    }

    public void ActivateEntranceDoor()
    {
        if (isDebugging) print("Asking to activate entrance door");
        AskToActivateDoor?.Invoke(CurrentRoom.Entrance);
    }

    public void ActivateBrewingDoor()
    {
        if (isDebugging) print("Asking to activate brewing door");
        AskToActivateDoor?.Invoke(CurrentRoom.Brewing);
    }

    public void ActivateGardenDoor()
    {
        if (isDebugging) print("Asking to activate garden door");
        AskToActivateDoor?.Invoke(CurrentRoom.Garden);
    }
    private void PressedA(InputAction.CallbackContext context)
    {
        if (_canAdvance) ProceedDialogue();
    }

    private void PressedY(InputAction.CallbackContext context)
    {
        if (isDebugging) print("Pressed emergency button");
        ProceedDialogue();
    }

    public void SpawnTutorialCustomer()
    {
        if (isDebugging) print("Asked to spawn tutorial customer.");
        AskToSpawnCustomer?.Invoke();
    }

    static InputAction GetInputAction(InputActionReference actionReference)
    {
        return actionReference != null ? actionReference.action : null;
    }
}
