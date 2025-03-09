using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    private StringBuilder stringBuilder = new StringBuilder();

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

        if(nameText.text != name)
        {
            nameText.text = name;
            dialogueText.text = text;
        }
        else
            dialogueText.text = ConnectText(dialogueText.text, text);

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

    private string ConnectText(string prevText, string nextText)
    {
        stringBuilder.Clear();
        stringBuilder.Append(prevText);
        stringBuilder.Append('\n');
        stringBuilder.Append(nextText);
        return stringBuilder.ToString();
    }
}
