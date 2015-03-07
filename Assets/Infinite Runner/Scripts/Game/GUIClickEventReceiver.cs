using UnityEngine;
using System.Collections;

namespace InfiniteRunner.Game
{
    /*
     * The user pressed a button, perform some action
     */
    public enum ClickType
    {
        StartGame, Stats, Store, EndGame, Restart, MainMenu, MainMenuRestart, Pause, Resume, ToggleTutorial, Missions,
        StorePurchase, StoreNext, StorePrevious, StoreTogglePowerUps, MainMenuFromStore, EndGameFromStore, Facebook, Twitter, Revive, EndGameFromRevive
    }
    public class GUIClickEventReceiver : MonoBehaviour
    {
        public ClickType clickType;

        public void OnClick()
        {
            if (!Application.isPlaying || !GUIManager.instance.CanClick()) {
                return;
            }

            bool playSoundEffect = true;
            switch (clickType) {
                case ClickType.StartGame:
                    GameManager.instance.StartGame(false);
                    break;
                case ClickType.Store:
                    GameManager.instance.ShowStore(true);
                    GUIManager.instance.ShowGUI(GUIState.Store);
                    break;
                case ClickType.Stats:
                    GUIManager.instance.ShowGUI(GUIState.Stats);
                    break;
                case ClickType.EndGame:
                    GUIManager.instance.ShowGUI(GUIState.EndGame);
                    break;
                case ClickType.Restart:
                    GameManager.instance.RestartGame(true);
                    break;
                case ClickType.MainMenu:
                    GameManager.instance.BackToMainMenu(false);
                    break;
                case ClickType.MainMenuRestart:
                    GameManager.instance.BackToMainMenu(true);
                    break;
                case ClickType.Pause:
                    GameManager.instance.PauseGame(true);
                    playSoundEffect = false;
                    break;
                case ClickType.Resume:
                    GameManager.instance.PauseGame(false);
                    break;
                case ClickType.ToggleTutorial:
                    GameManager.instance.ToggleTutorial();
                    break;
                case ClickType.Missions:
                    GUIManager.instance.ShowGUI(GUIState.Missions);
                    break;
                case ClickType.StoreNext:
                    GUIManager.instance.RotateStoreItem(true);
                    break;
                case ClickType.StorePrevious:
                    GUIManager.instance.RotateStoreItem(false);
                    break;
                case ClickType.StorePurchase:
                    GUIManager.instance.PurchaseStoreItem();
                    break;
                case ClickType.StoreTogglePowerUps:
                    GUIManager.instance.TogglePowerUpVisiblity();
                    break;
                case ClickType.MainMenuFromStore:
                    GameManager.instance.ShowStore(false);
                    GUIManager.instance.RemoveStoreItemPreview();
                    GameManager.instance.BackToMainMenu(false);
                    break;
                case ClickType.EndGameFromStore:
                    GameManager.instance.ShowStore(false);
                    GUIManager.instance.RemoveStoreItemPreview();
                    GUIManager.instance.ShowGUI(GUIState.EndGame);
                    break;
                case ClickType.Facebook:
                    SocialManager.instance.OpenFacebook();
                    break;
                case ClickType.Twitter:
                    SocialManager.instance.OpenTwitter();
                    break;
                case ClickType.Revive:
                    GameManager.instance.TryRevive();
                    break;
                case ClickType.EndGameFromRevive:
                    DataManager.instance.GameOver();
                    GUIManager.instance.ShowGUI(GUIState.EndGame);
                    break;
            }

            if (playSoundEffect)
                AudioManager.instance.PlaySoundEffect(SoundEffects.GUITapSoundEffect);
        }
    }
}