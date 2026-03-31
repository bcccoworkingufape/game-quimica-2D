using UnityEngine;
using UnityEngine.UI;


public class NavbarController : MonoBehaviour
{

    [Header("Buttons")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button shopButton;

    [Header("Home Images")]
    [SerializeField] private Image homeDisabled;
    [SerializeField] private Image homeEnabled;

    [Header("Settings Images")]
    [SerializeField] private Image settingsDisabled;
    [SerializeField] private Image settingsEnabled;

    [Header("Shop Images")]
    [SerializeField] private Image shopDisabled;
    [SerializeField] private Image shopEnabled;

    [Header("Pages")]
    [SerializeField] private GameObject homePage;
    [SerializeField] private GameObject settingsPage;
    [SerializeField] private GameObject shopPage;

    // Main functions
    public void OnClickHome()
    {
        DisableButton(homeButton);
        EnableButton(settingsButton);
        EnableButton(shopButton);

        ShowHomePage();

        EnableImage(homeEnabled);

        EnableImage(settingsDisabled);
        EnableImage(shopDisabled);

        DisableImage(homeDisabled);
        DisableImage(settingsEnabled);
        DisableImage(shopEnabled);
    }

    public void OnClickSettings()
    {
        DisableButton(settingsButton);
        EnableButton(homeButton);
        EnableButton(shopButton);

        ShowSettingsPage();

        EnableImage(settingsEnabled);
        DisableImage(settingsDisabled);

        EnableImage(homeDisabled);
        DisableImage(homeEnabled);

        EnableImage(shopDisabled);
        DisableImage(shopEnabled);
    }

    public void OnClickShop()
    {
        DisableButton(shopButton);
        EnableButton(homeButton);
        EnableButton(settingsButton);

        ShowShopPage();

        EnableImage(shopEnabled);
        EnableImage(homeDisabled);
        EnableImage(settingsDisabled);

        DisableImage(homeEnabled);
        DisableImage(shopDisabled);
        DisableImage(settingsEnabled);
    }


    // Page Management
    private void ShowHomePage()
    {
        EnableGameObject(homePage);

        DisableGameObject(settingsPage);
        DisableGameObject(shopPage);

    }

    private void ShowSettingsPage()
    {
        EnableGameObject(settingsPage);

        DisableGameObject(homePage);
        DisableGameObject(shopPage);
    }

    private void ShowShopPage()
    {
        EnableGameObject(shopPage);

        DisableGameObject(homePage);
        DisableGameObject(settingsPage);
    }

    // GameObject Helpers
    private void EnableGameObject(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void DisableGameObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    // Button Helpers
    private void DisableButton(Button button)
    {
        button.interactable = false;
    }

    private void EnableButton(Button button)
    {
        button.interactable = true;
    }

    // Image Helpers
    private void EnableImage(Image img)
    {
        img.enabled = true;
    }

    private void DisableImage(Image img)
    {
        img.enabled = false;
    }

}