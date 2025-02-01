using UnityEngine.InputSystem;

public class InputWrapper {
    public InputDevice device;
    public InputType inputType;

    public InputWrapper(InputDevice device) {
        this.device = device;
        if (device is Gamepad) {
            inputType = InputType.GamePad;
        } else if (device is Keyboard) {
            inputType = InputType.Keyboard;
        }
    }
}

public enum InputType {
    GamePad,
    Keyboard
}