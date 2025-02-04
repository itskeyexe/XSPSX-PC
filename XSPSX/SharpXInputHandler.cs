using SharpDX.XInput;
using System;
using System.Threading.Tasks;

namespace XSPSX
{
    public class SharpXInputHandler
    {
        private Controller controller;
        private State previousState;
        private State currentState;

        public bool IsConnected { get; private set; }

        private MainMenu mainMenu;

        public SharpXInputHandler(MainMenu mainMenu)
        {
            this.mainMenu = mainMenu; // Pass the MainMenu instance
            controller = new Controller(UserIndex.One);
            IsConnected = controller.IsConnected;

            if (IsConnected)
            {
                Console.WriteLine("Controller connected: Xbox Controller (via DS4Windows)");
                previousState = controller.GetState();

                // Delay the notification to ensure the window is fully loaded
                Task.Delay(2000).ContinueWith(_ =>
                {
                    mainMenu.Dispatcher.Invoke(() =>
                    {
                        mainMenu.ShowNotification(
                            "itskeyexe",
                            "XSPSX - Controller Connected!",
                            "Resources/Icons/controller.png"
                        );
                    });
                });
            }
            else
            {
                Console.WriteLine("No controller detected.");
            }
        }




        private bool IsButtonJustPressed(GamepadButtonFlags button, GamepadButtonFlags currentButtons, GamepadButtonFlags previousButtons)
        {
            // Check if the button is newly pressed (wasn't pressed in the previous state)
            return (currentButtons & button) != 0 && (previousButtons & button) == 0;
        }

        public void Update()
        {
            if (!IsConnected)
            {
                if (controller.IsConnected)
                {
                    IsConnected = true;
                    Console.WriteLine("Controller reconnected.");
                    previousState = controller.GetState();
                }
                else
                {
                    return; // Still disconnected
                }
            }

            // Get the current state
            try
            {
                currentState = controller.GetState();
                var currentButtons = currentState.Gamepad.Buttons;
                var previousButtons = previousState.Gamepad.Buttons;

                // D-Pad Debounced States
                DPadUp = IsButtonJustPressed(GamepadButtonFlags.DPadUp, currentButtons, previousButtons);
                DPadDown = IsButtonJustPressed(GamepadButtonFlags.DPadDown, currentButtons, previousButtons);
                DPadLeft = IsButtonJustPressed(GamepadButtonFlags.DPadLeft, currentButtons, previousButtons);
                DPadRight = IsButtonJustPressed(GamepadButtonFlags.DPadRight, currentButtons, previousButtons);

                // Buttons Debounced States
                ButtonA = IsButtonJustPressed(GamepadButtonFlags.A, currentButtons, previousButtons);
                ButtonB = IsButtonJustPressed(GamepadButtonFlags.B, currentButtons, previousButtons);

                previousState = currentState; // Update the previous state
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating controller state: {ex.Message}");
                IsConnected = false;
            }
        }

        public bool DPadUp { get; private set; }
        public bool DPadDown { get; private set; }
        public bool DPadLeft { get; private set; }
        public bool DPadRight { get; private set; }
        public bool ButtonA { get; private set; }
        public bool ButtonB { get; private set; }

        public bool IsDPadUpPressed() => DPadUp;
        public bool IsDPadDownPressed() => DPadDown;
        public bool IsDPadLeftPressed() => DPadLeft;
        public bool IsDPadRightPressed() => DPadRight;
        public bool IsButtonAPressed() => ButtonA;
        public bool IsButtonBPressed() => ButtonB;

        public void Dispose()
        {
            Console.WriteLine("Controller resources released.");
        }
    }
}
