using ICities;

namespace GameDayTimer
{
    public class GameDayTimer : IUserMod
    {
        public string Name => "Game Day Timer";
        public string Description => "Display real time duration of one game day and frames per second (FPS)";

        // the panel to display the timings, gets created in GameDayTimerLoading
        public static GameDayTimerPanel Panel;
        
        public void OnSettingsUI(UIHelperBase helper)
        {
            // create a new group heading
            UIHelperBase group = helper.AddGroup("Game Day Timer Settings");

            // add a check box to show/hide the timings
            GameDayTimerConfiguration config = Configuration<GameDayTimerConfiguration>.Load();
            group.AddCheckbox("Show timings", config.PanelIsVisible, (bool isChecked) =>
                {
                    // save the visibility in the config file
                    GameDayTimerConfiguration.SavePanelIsVisible(isChecked);

                    // if there is a panel, show or hide it
                    if (Panel != null)
                        Panel.isVisible = isChecked;
                });

            // add a button to reset the panel position
            group.AddButton("Reset Position", () =>
                {
                    // save the default position in the config file
                    GameDayTimerConfiguration.SavePanelPosition(GameDayTimerPanel.DefaultPanelPositionX, GameDayTimerPanel.DefaultPanelPositionY);

                    // if there is a panel, move it to the default position
                    if (Panel != null)
                        Panel.MovePanelToPosition(GameDayTimerPanel.DefaultPanelPositionX, GameDayTimerPanel.DefaultPanelPositionY);
                });
        }

        //public static void ShowMessage(string message)
        //{
        //    DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, message);
        //    DebugOutputPanel.Show();
        //}
    }
}