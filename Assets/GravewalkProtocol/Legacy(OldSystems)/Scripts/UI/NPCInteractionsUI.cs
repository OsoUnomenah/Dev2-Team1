using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogueNode
{
    public string npcName;
    public string dialogue;

    public List<DialogueResponse> responses;
}

[Serializable]
public class DialogueResponse
{
    public string responseText;

    public DialogueNode nextNode;

    public Action onSelected;
}

public class NPCInteractionsUI : MonoBehaviour
{
    public static NPCInteractionsUI instance;

    [SerializeField] TMP_Text npcName;
    [SerializeField] TMP_Text npcDialogue;
    [SerializeField] Transform[] responsePos;
    [SerializeField] Button responseButtonPrefab;

    private List<Transform> currButtons;
    private DialogueNode currNode;
    public float textSpeed;
    private int index;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    void Start()
    {
        npcDialogue.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartDialogue(DialogueNode dialogue)
    {
        if (responsePos == null)
        {
            return;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        gameManager.instance.playerInputHandler.rotateAction.Disable();

        gameObject.SetActive(true);
        currNode = dialogue;
        npcName.text = dialogue.npcName;

        StartCoroutine(TypeLine());

        if (currButtons.Count > 0 && currButtons != null)
        {
            foreach (Transform pos in currButtons)
            {
                Destroy(pos.gameObject);
            }
            currButtons.Clear();
        }


        index = 0;
        foreach (DialogueResponse response in currNode.responses)
        {
            Button button = Instantiate(responseButtonPrefab, responsePos[index]);
            currButtons.Add(button.transform);

            button.GetComponentInChildren<TMP_Text>().text = response.responseText;
            button.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Italic;

            button.onClick.AddListener(() =>
            {
                ChooseResponse(response);
            });
            index++;
        }
    }

    private void ChooseResponse(DialogueResponse response)
    {
        response.onSelected?.Invoke();
        npcDialogue.text = string.Empty;
        if (response.nextNode != null)
        {
            StartDialogue(response.nextNode);
        }
        else
        {
            CloseDialogue();
        }
    }

    public void CloseDialogue()
    {
        gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        gameManager.instance.playerInputHandler.rotateAction.Enable();
    }

    IEnumerator TypeLine()
    {
        foreach (char c in currNode.dialogue.ToCharArray())
        {
            npcDialogue.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    public void Leave()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        gameManager.instance.playerInputHandler.rotateAction.Enable();
        gameObject.SetActive(false);
    }
}


