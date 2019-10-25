namespace GameDayTimer
{
    /// <summary>
    /// define global (i.e. for this mod but not game specific) configuration properties
    /// </summary>
    /// <remarks>convention for the config file name seems to be the mod name + "Config.xml"</remarks>
    [ConfigurationFileName("GameDayTimerConfig.xml")]
    public class GameDayTimerConfiguration
    {
        // it is important to set default config values in case there is no config file

        // panel properties
        public bool PanelIsVisible = true;
        public float PanelPositionX = GameDayTimerPanel.DefaultPanelPositionX;
        public float PanelPositionY = GameDayTimerPanel.DefaultPanelPositionY;

        /// <summary>
        /// Save the specified panel visibility to the global config file
        /// </summary>
        /// <param name="isVisible"></param>
        public static void SavePanelIsVisible(bool isVisible)
        {
            GameDayTimerConfiguration config = Configuration<GameDayTimerConfiguration>.Load();
            config.PanelIsVisible = isVisible;
            Configuration<GameDayTimerConfiguration>.Save();
        }

        /// <summary>
        /// Save the specified panel position to the global config file
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