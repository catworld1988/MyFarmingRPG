using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonobehaviour<UIManager>
{
    private bool _pauseMenuOn = false;
    [SerializeField] private UIInventoryBar uiInventoryBar = null;
    [SerializeField] private PauseMenuInventoryManagement pauseMenuInventoryManagement = null;
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private GameObject[] menuTabs = null;
    [SerializeField] private Button[] menuButtons = null;


    public bool PauseMenuOn { get =>_pauseMenuOn; set => _pauseMenuOn = value; }

    protected override void Awake()
    {
        base.Awake();

        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        PauseMenu();
    }

    private void PauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseMenuOn)
            {
                DisablePauseMenu();
            }
            else
            {
                EnablePauseMenu();
            }
        }


    }


    private void EnablePauseMenu()
    {
        //销毁当前拖拽的物品
        uiInventoryBar.DestroyCurrentlyDraggedItem();

        //取消选中当前的物品
        uiInventoryBar.ClearCurrentlySelectedItem();

        PauseMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;
        //时间缩放 0暂停
        Time.timeScale = 0;
        pauseMenu.SetActive(true);

        //强制垃圾收集。
        System.GC.Collect();

        //高亮选择的按钮
        HightlightBUttonForSelectedTab();

    }



    private void DisablePauseMenu()
    {
        //销毁任何当前拖拽的物品
        pauseMenuInventoryManagement.DestroyCurrentlyDraggedItems();

        PauseMenuOn = false;
        Player.Instance.PlayerInputIsDisabled = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }


    private void HightlightBUttonForSelectedTab()
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (menuTabs[i].activeSelf)
            {
                SetButtonColourToActive(menuButtons[i]);
            }
            else
            {
                SetButtonColourToInactive(menuButtons[i]);
            }
        }
    }

    private void SetButtonColourToActive(Button button)
    {
        //临时存储
        ColorBlock colors = button.colors;

        colors.normalColor = colors.pressedColor;

        button.colors = colors;
    }

    private void SetButtonColourToInactive(Button button)
    {
        ColorBlock colors = button.colors;

        colors.normalColor = colors.disabledColor;

        button.colors = colors;
    }

    public void SwitchPauseMenuTab(int tabNum)
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (i!=tabNum)
            {
                menuTabs[i].SetActive(false);
            }
            else
            {
                menuTabs[i].SetActive(true);
            }
        }
        HightlightBUttonForSelectedTab();
    }
}
