namespace Star {

  using SFML.Window;

  public static class MouseButtons {
    public enum MB {
      LEFT,
      RIGHT
    }
  }

  public class KeyButton {

    public enum BUTTONCODE {
      CONFIRM,
      CANCEL,
      UP,
      DOWN,
      LEFT,
      RIGHT,
      ALPHANUMERIC
    };

    public BUTTONCODE buttoncode;
    public char alphanumeric = (char)0;

    
    public KeyButton(BUTTONCODE buttoncode) {
      this.buttoncode = buttoncode;
    }

    public KeyButton(char alphanumeric) {
      this.buttoncode = BUTTONCODE.ALPHANUMERIC;
      this.alphanumeric = alphanumeric;
    }

    public override int GetHashCode() => (int)buttoncode * 1000 + (int)alphanumeric;

    public override bool Equals(object? obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      }
      KeyButton other = (KeyButton) obj;
      return (this.buttoncode == other.buttoncode && this.alphanumeric == other.alphanumeric);
    }
  }

  public class KeyButtonEvent {

    public enum EVENTTYPE {
      KEYDOWN,
      KEYUP
    }

    private KeyButton kb;
    public EVENTTYPE eventType;

    public KeyButton.BUTTONCODE ButtonCode => kb.buttoncode;
    public char AlphaNumeric => kb.alphanumeric;

    public KeyButton KB => kb;

    public KeyButtonEvent(KeyButton kb, EVENTTYPE eventType) {
      this.kb = kb;
      this.eventType = eventType;
    }

    public override string ToString() => $"{ButtonCode}, {AlphaNumeric}, {eventType}";

  }

  public class MouseClickEvent {
    
    public MouseButtons.MB mouseButton;
    public bool consumed = false;

    public MouseClickEvent(MouseButtons.MB mouseButton) {
      this.mouseButton = mouseButton;
    }

  }



  static class Input {

    //Mouse
    static public XY GetMouseCursorXY(Window window) {
      var pos = SFML.Window.Mouse.GetPosition(window);
      return new XY(pos.X, pos.Y);
    }

    static bool leftMouseButtonPressedThisFrame = false;
    static bool leftMouseButtonPressedLastFrame = false;

    static public List<MouseClickEvent> GetMouseClickEvents() {
      List<MouseClickEvent> events = new List<MouseClickEvent>();
      if (leftMouseButtonPressedThisFrame && !leftMouseButtonPressedLastFrame) {
        events.Add(new MouseClickEvent(MouseButtons.MB.LEFT));
      }
      return events;
    }


    static private void GetMouseInput(bool isWindowFocused) {
      leftMouseButtonPressedLastFrame = leftMouseButtonPressedThisFrame;
      if (isWindowFocused) {
        leftMouseButtonPressedThisFrame = SFML.Window.Mouse.IsButtonPressed(Mouse.Button.Left);
      } else {
        leftMouseButtonPressedThisFrame = false;
      }
    }



    //Internal to this class. Holds keys we poll every frame.
    static HashSet<KeyButton> activeThisFrame = new HashSet<KeyButton>();
    static HashSet<KeyButton> activeLastFrame = new HashSet<KeyButton>();

    //Internal to this class. Holds keys we don't. For now, alphanumerics go in here.
    static HashSet<KeyButton> nonPolledKeysPressed = new HashSet<KeyButton>();
    static HashSet<KeyButton> nonPolledKeysDown = new HashSet<KeyButton>();
    static HashSet<KeyButton> nonPolledKeysReleased = new HashSet<KeyButton>();

    //This is what gets exposed outside of this class.
    static List<KeyButtonEvent> keyButtonEvents = new List<KeyButtonEvent>();

    static public List<KeyButtonEvent> GetKeyButtonEvents() {
      return keyButtonEvents;
    }

    private static void GetIndividualButtons() {
      if (Keyboard.IsKeyPressed(Keyboard.Key.Enter)) {
        activeThisFrame.Add(new KeyButton(KeyButton.BUTTONCODE.CONFIRM));
      }
      if (Keyboard.IsKeyPressed(Keyboard.Key.Escape)) {
        activeThisFrame.Add(new KeyButton(KeyButton.BUTTONCODE.CANCEL));
      }
      if (Keyboard.IsKeyPressed(Keyboard.Key.Down)  || Keyboard.IsKeyPressed(Keyboard.Key.Numpad2)) {
        activeThisFrame.Add(new KeyButton(KeyButton.BUTTONCODE.DOWN));
      }
      if (Keyboard.IsKeyPressed(Keyboard.Key.Up)    || Keyboard.IsKeyPressed(Keyboard.Key.Numpad8)) {
        activeThisFrame.Add(new KeyButton(KeyButton.BUTTONCODE.UP));
      }
      if (Keyboard.IsKeyPressed(Keyboard.Key.Left)  || Keyboard.IsKeyPressed(Keyboard.Key.Numpad4)) {
        activeThisFrame.Add(new KeyButton(KeyButton.BUTTONCODE.LEFT));
      }
      if (Keyboard.IsKeyPressed(Keyboard.Key.Right) || Keyboard.IsKeyPressed(Keyboard.Key.Numpad6)) {
        activeThisFrame.Add(new KeyButton(KeyButton.BUTTONCODE.RIGHT)); 
      }
    }

    public static void GetInput(bool isWindowFocused) {
      GetKeyboardInput(isWindowFocused);
      GetMouseInput(isWindowFocused);
    }

    private static void GetKeyboardInput(bool isWindowFocused) {

      keyButtonEvents.Clear();

      Util.Swap(ref activeThisFrame, ref activeLastFrame);
      activeThisFrame.Clear();

      //If the window is focused, then it's safe to grab input.
      //If the window is NOT focused, treat all of the non-polled keys as if they were released.
      if (isWindowFocused)  { GetIndividualButtons(); }
      else                  { nonPolledKeysReleased.UnionWith(nonPolledKeysDown); nonPolledKeysDown.Clear(); }

      //Find each key-down this frame!
      foreach(var key in activeThisFrame) {
        if (!activeLastFrame.Contains(key)) {
          keyButtonEvents.Add(new KeyButtonEvent(key, KeyButtonEvent.EVENTTYPE.KEYDOWN));
        }
      }
      //Find each key-up this frame!
      foreach(var key in activeLastFrame) {
        if (!activeThisFrame.Contains(key)) {
          keyButtonEvents.Add(new KeyButtonEvent(key, KeyButtonEvent.EVENTTYPE.KEYUP));
        }
      }

      //Find each key-down for the non-polled alphanumerics.
      foreach(var key in nonPolledKeysPressed) {
        keyButtonEvents.Add(new KeyButtonEvent(key, KeyButtonEvent.EVENTTYPE.KEYDOWN));
        nonPolledKeysDown.Add(key);
        Log.Write("Generated key down event.");
      }
      //Find each key-up for the non-polled alphanumerics.
      foreach(var key in nonPolledKeysReleased) {
        keyButtonEvents.Add(new KeyButtonEvent(key, KeyButtonEvent.EVENTTYPE.KEYUP));
        nonPolledKeysDown.Remove(key);
        Log.Write("Generated key up event.");
      }

      nonPolledKeysPressed.Clear();
      nonPolledKeysReleased.Clear();

    }

    public static void HandleKeyPressEvent(object? sender, SFML.Window.KeyEventArgs eventArgs) {
      HandleKeyboardEvents(eventArgs, KeyButtonEvent.EVENTTYPE.KEYDOWN);
    }

    public static void HandleKeyReleaseEvent(object? sender, SFML.Window.KeyEventArgs eventArgs) {
      HandleKeyboardEvents(eventArgs, KeyButtonEvent.EVENTTYPE.KEYUP);
    }

    //This looks at keypressed/keyreleased events coming from the window.
    //I use this for individual keypresses. I use this to generate alphanumeric key events.
    private static void HandleKeyboardEvents(SFML.Window.KeyEventArgs eventArgs, KeyButtonEvent.EVENTTYPE eventType) {
      Keyboard.Key sfmlKeyCode = eventArgs.Code;
      if (!alphaNumericConversions.ContainsKey(sfmlKeyCode)) { return; }
      char alphaNumericChar = alphaNumericConversions[sfmlKeyCode];

      if (eventType == KeyButtonEvent.EVENTTYPE.KEYDOWN) {
        nonPolledKeysPressed.Add(new KeyButton(alphaNumericChar));
        Log.Write($"Key press: {alphaNumericChar}");
      }
      if (eventType == KeyButtonEvent.EVENTTYPE.KEYUP) {
        nonPolledKeysReleased.Add(new KeyButton(alphaNumericChar));
        Log.Write($"Key release: {alphaNumericChar}");
      }

    }


    //this is kinda dumb ngl
    static Dictionary<Keyboard.Key, char> alphaNumericConversions = new Dictionary<Keyboard.Key, char> {
      { Keyboard.Key.A, 'a' },
      { Keyboard.Key.B, 'b' },
      { Keyboard.Key.C, 'c' },
      { Keyboard.Key.D, 'd' },
      { Keyboard.Key.E, 'e' },
      { Keyboard.Key.F, 'f' },
      { Keyboard.Key.G, 'g' },
      { Keyboard.Key.H, 'h' },
      { Keyboard.Key.I, 'i' },
      { Keyboard.Key.J, 'j' },
      { Keyboard.Key.K, 'k' },
      { Keyboard.Key.L, 'l' },
      { Keyboard.Key.M, 'm' },
      { Keyboard.Key.N, 'n' },
      { Keyboard.Key.O, 'o' },
      { Keyboard.Key.P, 'p' },
      { Keyboard.Key.Q, 'q' },
      { Keyboard.Key.R, 'r' },
      { Keyboard.Key.S, 's' },
      { Keyboard.Key.T, 't' },
      { Keyboard.Key.U, 'u' },
      { Keyboard.Key.V, 'v' },
      { Keyboard.Key.W, 'w' },
      { Keyboard.Key.X, 'x' },
      { Keyboard.Key.Y, 'y' },
      { Keyboard.Key.Z, 'z' },
      { Keyboard.Key.Num0,    '0' },
      { Keyboard.Key.Numpad0, '0' },
      { Keyboard.Key.Num1,    '1' },
      { Keyboard.Key.Numpad1, '1' },
      { Keyboard.Key.Num2,    '2' },
      { Keyboard.Key.Numpad2, '2' },
      { Keyboard.Key.Num3,    '3' },
      { Keyboard.Key.Numpad3, '3' },
      { Keyboard.Key.Num4,    '4' },
      { Keyboard.Key.Numpad4, '4' },
      { Keyboard.Key.Num5,    '5' },
      { Keyboard.Key.Numpad5, '5' },
      { Keyboard.Key.Num6,    '6' },
      { Keyboard.Key.Numpad6, '6' },
      { Keyboard.Key.Num7,    '7' },
      { Keyboard.Key.Numpad7, '7' },
      { Keyboard.Key.Num8,    '8' },
      { Keyboard.Key.Numpad8, '8' },
      { Keyboard.Key.Num9,    '9' },
      { Keyboard.Key.Numpad9, '9' },
    };

 


  }

}