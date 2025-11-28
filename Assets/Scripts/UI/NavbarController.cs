using UnityEngine;
using MenuScripts;
using Data;
using Core; 


public class NavbarController : MonoBehaviour
{
    public MenuUIController menuController;

    private void Start()
    {
        if (menuController == null)
        {
            Debug.LogError("O MenuController não foi atribuído no Inspector do NavbarController!");
        }
    }

    public void OnHomeButtonPressed()
    {
        menuController?.ShowHomePanel();
    }

    public void OnShopButtonPressed()
    {
        menuController?.ShowShopPanel();
    }

    public void OnSettingsButtonPressed()
    {
        menuController?.ShowSettingsPanel();
    }
}