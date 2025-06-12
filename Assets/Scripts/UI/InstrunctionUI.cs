using UnityEngine;
using UnityEngine.UI;
using GameCore;         // 引用 SceneLoader

public class InstructionUI : MonoBehaviour
{
    [Header("Sprites in order")]
    [SerializeField] Sprite[] pages;        // 拖 2–3 张

    [Header("Scene refs")]
    [SerializeField] Image   pageImage;     // Image_Page
    [SerializeField] Button  btnPrev;
    [SerializeField] Button  btnNext;
    [SerializeField] Button  btnBack;

    int index;     // 当前页

    void Start()
    {
        Debug.Log($"Instruction Start, pages = {pages.Length}");

        if (pages.Length == 0) { Debug.LogError("No pages set"); return; }

        btnPrev.onClick.AddListener(Prev);
        btnNext.onClick.AddListener(Next);
        btnBack.onClick.AddListener(() => SceneLoader.Load("MainMenu"));

        Refresh();
    }

    /*------------------ Page Nav ------------------*/
    void Prev() { if (index > 0) { index--; Refresh(); } }
    void Next() { if (index < pages.Length - 1) { index++; Refresh(); } }

    void Refresh()
    {

        Debug.Log("Set sprite to " + pages[index].name);
        pageImage.sprite = pages[index];

        // 边界时禁按钮
        btnPrev.interactable = index > 0;
        btnNext.interactable = index < pages.Length - 1;
    }
}
