using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ExtrasMenuController : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject items;
    [SerializeField] private GameObject controls;
    [SerializeField] private GameObject credits;

    [Header("Default buttons")]
    [SerializeField] private GameObject itemsBack;
    [SerializeField] private GameObject controlsBack;
    [SerializeField] private GameObject creditsBack;
    [SerializeField] private GameObject itemsButton;
    [SerializeField] private GameObject controlsButton;
    [SerializeField] private GameObject creditsButton;

    public void ShowItems()
    {
        this.menu.SetActive(false);
        this.items.SetActive(true);

        this.SetButton(this.itemsBack);
    }

    public void ShowControls()
    {
        this.menu.SetActive(false);
        this.controls.SetActive(true);

        this.SetButton(this.controlsBack);
    }

    public void ShowCredits()
    {
        this.menu.SetActive(false);
        this.credits.SetActive(true);

        this.SetButton(this.creditsBack);
    }

    public void BackFromItems()
    {
        this.menu.SetActive(true);
        this.items.SetActive(false);

        this.SetButton(this.itemsButton);
    }

    public void BackFromControls()
    {
        this.menu.SetActive(true);
        this.controls.SetActive(false);

        this.SetButton(this.controlsButton);
    }

    public void BackFromCredits()
    {
        this.menu.SetActive(true);
        this.credits.SetActive(false);

        this.SetButton(this.creditsButton);
    }

    private void SetButton(GameObject button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }

    public void BackToMainMenu() => GameController.LoadScene(Scenes.Menu);

    public void Quit() => Application.Quit();
}
