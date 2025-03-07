using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    private void Awake()
    {
        HideDialogueBox();
    }

    public void ShowDialogueBox()
    {
        dialogueBox.SetActive(true);
    }

    public void HideDialogueBox()
    {
        dialogueBox.SetActive(false);
    }

    public void UpdateDialogue(string name, string portrait, string text)
    {
        ShowDialogueBox();

        nameText.text = name;
        dialogueText.text = text;

        // 캐릭터 초상화 업데이트
        Sprite portraitSprite = Resources.Load<Sprite>($"Portraits/{portrait}");
        if (portraitSprite != null)
        {
            portraitImage.sprite = portraitSprite;
        }
        else
        {
            Debug.LogWarning($"초상화 이미지 없음: {portrait}");
        }
    }

    // 버튼 클릭 시 다음 대화 진행
    public void OnNextButtonClick()
    {
        DialogueManager.Instance.ShowNextDialogue();
    }
}
