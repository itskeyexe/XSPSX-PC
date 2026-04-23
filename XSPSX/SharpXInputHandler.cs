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

        public SharpXInputHandler(MainMenu mainMenu = null) // Default to null for sub-windows
        {
            this.mainMenu = mainMenu;
            controller = new Controller(UserIndex.One);
            IsConnected = controller.IsConnected;

            if (IsConnected)
            {
                Console.WriteLine("Controller connected: Xbox Controller (via DS4Windows)");
                previousState = controller.GetState();

                // FIX: Only attempt notification if mainMenu is NOT null
                if (mainMenu != null)
                {
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
            }
            else
            {
                Console.WriteLine("No controller detected.");
            }
        }

        private bool IsButtonJustPressed(GamepadButtonFlags button, GamepadButtonFlags currentButtons, GamepadButtonFlags previousButtons)
        {
            return (currentButtons & button) != 0 && (previousButtons & button) == 0;
        }

        public void Update()
        {
            try
            {
                if (controller == null) return;

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
                        return;
                    }
                }

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

                previousState = currentState;
            }
            catch (SharpDX.SharpDXException)
            {
                // Specifically handle controller disconnection/lost device
                IsConnected = false;
            }
            catch (Exception) // Removed 'ex' variable to resolve the "unused variable" warning
            {
                // High-speed polling (60fps) can occasionally throw a SharpDXException 
                // if the controller is physically unplugged during a GetState() call. 
                // We catch it silently here to prevent a UI crash.
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
            controller = null;
        }
    }
}