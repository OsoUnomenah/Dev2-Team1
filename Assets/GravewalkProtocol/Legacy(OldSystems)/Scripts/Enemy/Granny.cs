using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class Granny : MonoBehaviour, IInteract
{
    private DialogueNode greeting;

    private void Start()
    {
        greeting = new DialogueNode()
        {
            npcName = "Granny",
            dialogue = " Hey there sonny!"
        };

        DialogueNode openShop = new DialogueNode()
        {
            npcName = "Granny",
            dialogue = "Are you in the mood for some shopping?"
        };

        DialogueNode dance = new DialogueNode()
        {
            npcName = "Granny",
            dialogue = "Dancing keeps me young!"
        };


        greeting.responses = new List<DialogueResponse>()
        {
            new DialogueResponse()
            {
                responseText = "Hello ma'am!",
                nextNode = openShop
            },

            new DialogueResponse()
            {
                responseText = "Why are you dancing?",
                nextNode = dance
            }
        };

        openShop.responses = new List<DialogueResponse>()
        {
            new DialogueResponse()
            {
                responseText = "Yes",
                nextNode = null
            },

            new DialogueResponse()
            {
                responseText = "No",
                nextNode = null
            }
        };

        dance.responses = new List<DialogueResponse>()
        {
            new DialogueResponse()
            {
                responseText = "Wow that's great!",
                nextNode = null
            },

            new DialogueResponse()
            {
                responseText = "Ok...",
                nextNode = null
            }
        };
    }

    public void Interact()
    {
        NPCInteractionsUI.instance.StartDialogue(greeting);
    }

    

    public void OnHoverEnter()
    {
        gameManager.instance.interactText.enabled = true;
    }

    public void OnHoverExit()
    {
        gameManager.instance.interactText.enabled = false;
    }
}
