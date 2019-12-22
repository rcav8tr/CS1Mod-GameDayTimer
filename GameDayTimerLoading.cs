using ColossalFramework.UI;
using ICities;
using UnityEngine;
using System;

namespace GameDayTimer
{
    /// <summary>
    /// handle game loading and unloading
    /// </summary>
    /// <remarks>A new instance of GameDayTimerLoading is NOT created when loading a game from the Pause Menu.</remarks>
    public class GameDayTimerLoading : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            // do base processing
            base.OnLevelLoaded(mode);

            try
            {
                // check for new or loaded game
                if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame || mode == LoadMode.NewGameFromScenario)
                {
                    // destroy the panel if a previous instance exists
                    if (GameDayTimer.Panel != null)
                    {
                        UnityEngine.Object.Destroy(GameDayTimer.Panel);
                    }

                    // create a new GameDayTimerPanel which will trigger the panel's Start event
                    UIView v = UIView.GetAView();
                    GameDayTimer.Panel = (GameDayTimerPanel)v.AddUIComponent(typeof(GameDayTimerPanel));
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public override void OnLevelUnloading()
        {
            // do base processing
            base.OnLevelUnloading();

            try
            {
                // destroy the panel
                // must do this explicitly because loading a saved game from the Pause Menu
                // does not destroy the panel implicitly like returning to the Main Menu to load a saved game
                if (GameDayTimer.Panel != null)
                {
                    UnityEngine.Object.Destroy(GameDayTimer.Panel);
                    GameDayTimer.Panel = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}