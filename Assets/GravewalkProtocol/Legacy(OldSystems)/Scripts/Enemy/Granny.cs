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
        
        DialogueNode jerk = new DialogueNode()
        {
            npcName = "Granny",
            dialogue = "That's all you got to say? Be nicer next time jerk and maybe I'll let you buy something."
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
                nextNode = null,
                onSelected = gameManager.instance.shop.ToggleShop
            },

            new DialogueResponse()
            {
                responseText = "No",
                nextNode = null,
                onSelected = gameManager.instance.stateUnpause
            }
        };

        jerk.responses = new List<DialogueResponse>()
        {
            new DialogueResponse()
            {
                responseText = "Jeez, okay then...",
                nextNode = null,
                onSelected = gameManager.instance.stateUnpause
            },

           
        };

        dance.responses = new List<DialogueResponse>()
        {
            new DialogueResponse()
            {
                responseText = "Wow that's great!",
                nextNode = openShop
            },

            new DialogueResponse()
            {
                responseText = "Okay...",
                nextNode = jerk
            }
        };
    }

    public void Interact()
    {
        if (greeting == null)
            return;
        gameManager.instance.statePause();
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
