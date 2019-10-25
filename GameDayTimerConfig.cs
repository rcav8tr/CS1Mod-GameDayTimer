namespace GameDayTimer
{
    [ConfigurationFileName("GameDayTimerConfig.xml")]
    public class GameDayTimerConfiguration
    {
        // it is important to set default config values in case there is no config file

        // panel visibility
        public bool PanelIsVisible = true;

        // panel position
        public float PanelPositionX = GameDayTimerPanel.DefaultPanelPositionX;
        public float PanelPositionY = GameDayTimerPanel.DefaultPanelPositionY;

        /// <summary>
        /// Save the specified panel visibility to the config file
        /// </summary>
        /// <param name="isVisible"></param>
        public static void SavePanelIsVisible(bool isVisible)
        {
            GameDayTimerConfiguration config = Configuration<GameDayTimerConfiguration>.Load();
            config.PanelIsVisible = isVisible;
            Configuration<GameDayTimerConfiguration>.Save();
        }

        /// <summary>
        /// Save the specified panel position to the config file
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SavePanelPosition(float x, float y)
        {
            GameDayTimerConfiguration config = Configuration<GameDayTimerConfiguration>.Load();
            config.PanelPositionX = x;
            config.PanelPositionY = y;
            Configuration<GameDayTimerConfiguration>.Save();
        }
    }
}