using ColossalFramework.UI;
using UnityEngine;
using System;

namespace GameDayTimer
{
    /// <summary>
    /// panel to display timings
    /// </summary>
    public class GameDayTimerPanel : UIPanel
    {
        // default panel position
        public const float DefaultPanelPositionX = 50f;
        public const float DefaultPanelPositionY = 50f;

        // UI components on the panel
        private UILabel CurrentDayLabel;
        private UILabel PreviousDayLabel;
        private UILabel FramesPerSecLabel;
        private UILabel CurrentDayValue;
        private UILabel PreviousDayValue;
        private UILabel FramesPerSecValue;

        // flags for dragging the panel
        private bool MouseDown;
        private bool Dragging;

        // variables for doing game day timing (GDT)
        private bool GDTFirstDayChanged;                // whether or not the first day of timing changed, used prevent timing a partial first day
        private int GDTPreviousDay;                     // previous game day, used to detect game day change
        private double GDTTotalTimeInDay;               // accumulcated total real time spent in a game day
        private string GDTPreviousTotalTimeInDayText;   // the previous text value of TotalTimeInDay, used to detect change in time to be displayed
        private int GDTPreviousSimSpeed;                // previous simulation speed, used to detect change in simulation speed
        private double GDTPreviousRealTime;             // previous real time, used to compute time delta from previous frame

        // variables for doing frames per second (FPS) timing
        private double FPSPreviousRealTime;             // previous real time, used to compute when one second has elapsed
        private int FPSFrameCount;                      // count of frames

        // define text colors
        Color32 ColorRed = new Color32(255, 0, 0, 255);
        Color32 ColorOrange = new Color32(255, 136, 0, 255);
        Color32 ColorYellow = new Color32(255, 255, 0, 255);
        Color32 ColorWhite = new Color32(255, 255, 255, 255);

        /// <summary>
        /// Start is called after the panel is created in Loading
        /// set up and populate the panel
        /// </summary>
        public override void Start()
        {
            // do base processing
            base.Start();

            try
            {
                // set panel properties
                name = "GameDayTimerPanel";
                width = 140;
                height = 68;
                backgroundSprite = "MenuPanel2";
                canFocus = true;
                opacity = 0.75f;

                // move panel to initial position according to the config
                GameDayTimerConfiguration config = Configuration<GameDayTimerConfiguration>.Load();
                MovePanelToPosition(config.PanelPositionX, config.PanelPositionY);

                // set up mouse event handlers for dragging the panel
                eventMouseDown += GameDayTimerPanel_eventMouseDown;
                eventMouseMove += GameDayTimerPanel_eventMouseMove;
                eventMouseUp += GameDayTimerPanel_eventMouseUp;

                // set position and size for labels
                const float LabelLeft = 8f;     // all labels have same left position, but value labels are right justified
                const float LabelTop = 4f;      // top of topmost labels
                float LabelWidth = width - 2 * LabelLeft;
                float LabelHeight = (height - 2 * LabelTop) / 3f;

                // create label components to show text
                CurrentDayLabel = AddUIComponent<UILabel>();
                CurrentDayLabel.name = "GameDayTimerCurrentDayLabel";
                CurrentDayLabel.text = "Curr Day";
                CurrentDayLabel.relativePosition = new Vector3(LabelLeft, LabelTop);
                CurrentDayLabel.textAlignment = UIHorizontalAlignment.Left;
                CurrentDayLabel.autoSize = false;
                CurrentDayLabel.width = LabelWidth;
                CurrentDayLabel.height = LabelHeight;

                PreviousDayLabel = AddUIComponent<UILabel>();
                PreviousDayLabel.name = "GameDayTimerPreviousDayLabel";
                PreviousDayLabel.text = "Prev Day";
                PreviousDayLabel.relativePosition = new Vector3(LabelLeft, LabelTop + LabelHeight);
                PreviousDayLabel.textAlignment = UIHorizontalAlignment.Left;
                PreviousDayLabel.autoSize = false;
                PreviousDayLabel.width = LabelWidth;
                PreviousDayLabel.height = LabelHeight;

                FramesPerSecLabel = AddUIComponent<UILabel>();
                FramesPerSecLabel.name = "GameDayTimerFramesPerSecLabel";
                FramesPerSecLabel.text = "FPS";
                FramesPerSecLabel.relativePosition = new Vector3(LabelLeft, LabelTop + 2 * LabelHeight);
                FramesPerSecLabel.textAlignment = UIHorizontalAlignment.Left;
                FramesPerSecLabel.autoSize = false;
                FramesPerSecLabel.width = LabelWidth;
                FramesPerSecLabel.height = LabelHeight;

                // create label components to show timing values
                CurrentDayValue = AddUIComponent<UILabel>();
                CurrentDayValue.name = "GameDayTimerCurrentDayValue";
                CurrentDayValue.text = "";
                CurrentDayValue.relativePosition = CurrentDayLabel.relativePosition;
                CurrentDayValue.textAlignment = UIHorizontalAlignment.Right;
                CurrentDayValue.autoSize = false;
                CurrentDayValue.width = LabelWidth;
                CurrentDayValue.height = LabelHeight;

                PreviousDayValue = AddUIComponent<UILabel>();
                PreviousDayValue.name = "GameDayTimerPreviousDayValue";
                PreviousDayValue.text = "";
                PreviousDayValue.relativePosition = PreviousDayLabel.relativePosition;
                PreviousDayValue.textAlignment = UIHorizontalAlignment.Right;
                PreviousDayValue.autoSize = false;
                PreviousDayValue.width = LabelWidth;
                PreviousDayValue.height = LabelHeight;

                FramesPerSecValue = AddUIComponent<UILabel>();
                FramesPerSecValue.name = "GameDayTimerFramesPerSecValue";
                FramesPerSecValue.text = "";
                FramesPerSecValue.relativePosition = FramesPerSecLabel.relativePosition;
                FramesPerSecValue.textAlignment = UIHorizontalAlignment.Right;
                FramesPerSecValue.autoSize = false;
                FramesPerSecValue.width = LabelWidth;
                FramesPerSecValue.height = LabelHeight;

                // initialize timing things
                GDTTotalTimeInDay = 0;
                GDTPreviousTotalTimeInDayText = "xx";
                GDTPreviousDay = SimulationManager.instance.m_currentGameTime.Day;
                GDTPreviousSimSpeed = SimulationManager.instance.SelectedSimulationSpeed;
                GDTPreviousRealTime = GetCurrentRealTime();
                FPSPreviousRealTime = GetCurrentRealTime();

                // wait for a day change
                WaitForDayChange();

                // show or hide the panel according to the config
                isVisible = config.PanelIsVisible;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Update is called every frame
        /// all the timing calcs are performed here
        /// </summary>
        public override void Update()
        {
            // do base processing
            base.Update();

            try
            {
                // get current real time (used often)
                double CurrentRealTime = GetCurrentRealTime();

                // do game day timing only if not paused
                if (!SimulationManager.instance.SimulationPaused && !SimulationManager.instance.ForcedSimulationPaused)
                {
                    // check for simulation speed change
                    int SimSpeed = SimulationManager.instance.SelectedSimulationSpeed;
                    if (SimSpeed != GDTPreviousSimSpeed)
                    {
                        // sim speed changed, wait for day to change before starting game day timings again
                        GDTPreviousSimSpeed = SimSpeed;
                        WaitForDayChange();
                    }
                    else
                    {
                        // accumulate time delta
                        GDTTotalTimeInDay += CurrentRealTime - GDTPreviousRealTime;

                        // if first day has changed, then show current day running total
                        if (GDTFirstDayChanged)
                        {
                            // show current day total only when it changes to resolution of 0.1 seconds
                            string GDTTotalTimeInDayText = GDTTotalTimeInDay.ToString("0.0");
                            if (GDTTotalTimeInDayText != GDTPreviousTotalTimeInDayText)
                            {
                                CurrentDayValue.text = GDTTotalTimeInDayText;
                                GDTPreviousTotalTimeInDayText = GDTTotalTimeInDayText;

                                // set color based on sim speed and elapsed time
                                Color32 color = GetGameDayTimeColor(SimSpeed, GDTTotalTimeInDay);
                                CurrentDayLabel.textColor = color;
                                CurrentDayValue.textColor = color;
                            }
                        }

                        // check for day change
                        int CurrentGameDay = SimulationManager.instance.m_currentGameTime.Day;
                        if (CurrentGameDay != GDTPreviousDay)
                        {
                            // day has changed, display current running total as previous day value
                            // display only on second and subsequent day changes 
                            if (GDTFirstDayChanged)
                            {
                                PreviousDayValue.text = GDTTotalTimeInDay.ToString("0.0");

                                // set color based on sim speed and elapsed time
                                Color32 color = GetGameDayTimeColor(SimSpeed, GDTTotalTimeInDay);
                                PreviousDayLabel.textColor = color;
                                PreviousDayValue.textColor = color;
                            }

                            // reset for next day
                            GDTFirstDayChanged = true;
                            GDTPreviousDay = CurrentGameDay;
                            GDTTotalTimeInDay = 0;
                            GDTPreviousTotalTimeInDayText = "xx";
                        }
                    }
                }

                // always save current real time, even if simulation is paused, 
                // so that next time delta is computed correctly when simulation resumes
                GDTPreviousRealTime = CurrentRealTime;

                // count frames
                FPSFrameCount++;

                // check if one second has elapsed
                double ElapsedTime = CurrentRealTime - FPSPreviousRealTime;
                if (ElapsedTime >= 1d)
                {
                    // display FPS
                    double FramesPerSecond = FPSFrameCount / ElapsedTime;
                    FramesPerSecValue.text = FramesPerSecond.ToString("0");

                    // change color of FPS display
                    Color32 color;
                    if (FramesPerSecond <= 10) color = ColorRed;
                    else if (FramesPerSecond <= 20) color = ColorOrange;
                    else if (FramesPerSecond <= 30) color = ColorYellow;
                    else color = ColorWhite;
                    FramesPerSecLabel.textColor = color;
                    FramesPerSecValue.textColor = color;

                    // reset for next second
                    FPSPreviousRealTime = CurrentRealTime;
                    FPSFrameCount = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                WaitForDayChange();
            }

        }

        /// <summary>
        /// Get the color associated with the sim speed and elapsed time
        /// </summary>
        /// <param name="SimSpeed"></param>
        /// <param name="ElapsedTime"></param>
        /// <returns></returns>
        private Color32 GetGameDayTimeColor(int SimSpeed, double ElapsedTime)
        {
            // define standard elapsed time based on the speed
            double StandardElapsedTime;
            if (SimSpeed == 1) StandardElapsedTime = 10.0;
            else if (SimSpeed == 2) StandardElapsedTime = 5.0;
            else StandardElapsedTime = 2.5;

            // compute the multiple of elapsed time to standard time
            double TimeMultiple = ElapsedTime / StandardElapsedTime;

            // set color based on the multiple
            Color32 color;
            if (TimeMultiple >= 4) color = ColorRed;
            else if (TimeMultiple >= 3) color = ColorOrange;
            else if (TimeMultiple >= 2) color = ColorYellow;
            else color = ColorWhite;

            return color;
        }

        private void GameDayTimerPanel_eventMouseDown(UIComponent component, UIMouseEventParameter eventParam)
        {
            // remember that user did mouse down
            MouseDown = true;
        }

        private void GameDayTimerPanel_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            // if moving while the mouse button is down, then start dragging and make panel follow mouse
            if (MouseDown)
            {
                Dragging = true;
                MovePanelToPosition(relativePosition.x + eventParam.moveDelta.x, relativePosition.y - eventParam.moveDelta.y);
                // don't save position to config file while dragging
            }
        }

        private void GameDayTimerPanel_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam)
        {
            // if dragging, then drag is done, save final panel position
            if (Dragging)
            {
                GameDayTimerConfiguration.SavePanelPosition(relativePosition.x, relativePosition.y);
            }

            // reset dragging flags
            MouseDown = false;
            Dragging = false;
        }

        /// <summary>
        /// Move panel to specified position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MovePanelToPosition(float x, float y)
        {
            relativePosition = new Vector3(x,y);
        }

        /// <summary>
        /// Wait for a day to change
        /// </summary>
        private void WaitForDayChange()
        {
            // first game day has not changed
            GDTFirstDayChanged = false;

            // clear some values
            CurrentDayLabel.textColor = ColorWhite;
            CurrentDayValue.text = "";
            PreviousDayLabel.textColor = ColorWhite;
            PreviousDayValue.text = "";
        }

        /// <summary>
        /// get the current real time in seconds and fractions of a second
        /// </summary>
        /// <returns></returns>
        private double GetCurrentRealTime()
        {
            // cannot use the simulation real timer because it counts up to 60 and then resets to 0
            return DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond;
        }
    }
}
