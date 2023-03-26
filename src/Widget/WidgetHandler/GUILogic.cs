/*namespace Star.Widget {

  public enum GUISTATE {
    NONE,
  }

  //This is just a test class.
  public class MenuReceiverGUILogic : GUILogic {

    public Widget? testWidget;

    private List<GUILogic> logicHeld = new List<GUILogic>();

    public LogicCursorSet lcs;

    public MenuReceiverGUILogic(LogicCursorSet lcs) {
      this.lcs = lcs;
      SetOnDownKeyHandler(KeyButton.BUTTONCODE.RIGHT, AdvanceLCS);
      SetOnDownKeyHandler(KeyButton.BUTTONCODE.LEFT, GoBackLCS);
      SetOnDownKeyHandler(KeyButton.BUTTONCODE.CONFIRM, lcs.HandleButtonPressAtCursor);
    }

    private bool AdvanceLCS(GUILogic caller) {
      lcs.AdvanceToNextElement();
      Log.Write("Advanced!");
      return true;
    }

    private bool GoBackLCS(GUILogic caller) {
      lcs.RetreatToPriorElement();
      Log.Write("Retreated!");
      return true;
    }

    public bool HandleButtonPress(GUILogic caller) {
      int id = -1;
      for (int i = 0; i < logicHeld.Count; ++i) {
        if (caller == logicHeld[i]) { id = i; }
      }
      Log.Write($"Button {id} was pressed.");

      if (id == 0 && testWidget != null) {
        testWidget.UpdateIntrinsicBounds(-1,0,25,-1);
      }
      if (id == 1 && testWidget != null) {
        testWidget.UpdateIntrinsicBounds(-1,0,33,-1);
      }
      return true;
    }

    public bool HandleClickedOn(MouseClickEvent click, GUILogic caller) {
      //click.consumed = true;
      HandleButtonPress(caller);
      return true;
    }

    public SimpleButtonGUILogic WireUpNewButton(Widget widget) {
      SimpleButtonGUILogic button = new SimpleButtonGUILogic();
      button.onClick = HandleClickedOn;
      button.forwardedButtonPressed = HandleButtonPress;
      logicHeld.Add(button);
      children.Add(button);
      widget.SetGUILogic(button);
      return button;
    }

  }

  //This is (obviously) also just a test class.
  public class SimpleButtonGUILogic : GUILogic {

  }

  public class GUILogic {

    protected GUILogic? parent = null;
    protected List<GUILogic> children = new List<GUILogic>();

    //The cursor set that this guiLogic element is a part of.
    protected LogicCursorSet? myLogicCursorSet = null;

    //Delegate types for handling button/key presses and mouse events
    //Returns true if the event has been processed.
    public delegate bool ButtonPressHandler(GUILogic sender);

    //Todo: consider making this a bool and having the return value be if the click was consumed.
    //Maybe do this instead of tracking whether the clickEvent was consumed within object itself.
    public delegate bool ClickHandler(MouseClickEvent clickEvent, GUILogic sender);

    public delegate bool OnMouseOver();
    public delegate void OnMouseNoLongerOver();

    //
    //Delegate types for logic-to-logic events
    //
    public delegate void LogicConfirmEvent(GUILogic caller);

    //
    //The actual delegates for the button and mouse events
    //
    //protected Dictionary<KeyButton.BUTTONCODE, ButtonPressHandler> onDownKeyHandlers = new Dictionary<KeyButton.BUTTONCODE, ButtonPressHandler>();
    protected Dictionary<KeyButton, ButtonPressHandler> onDownKeyHandlers = new Dictionary<KeyButton, ButtonPressHandler>();

    //This gets invoked by the cursor set when the cursor is over this element and the owner of the cursorset forwards an event to it.
    public ButtonPressHandler? forwardedButtonPressed = null;

    public ClickHandler? onClick;    

    //This gets called when the mouse is over this GUILogic.
    //The return value is true if the cursor was consumed by the system, i.e. don't propagate to lower branches in the tree.
    protected OnMouseOver? onMouseOver;

    protected OnMouseNoLongerOver? onMouseNoLongerOver;

    //
    //The actual delegates for the logic-to-logic events
    //
    protected LogicConfirmEvent? onLogicConfirmEvent = null;



    protected long framesMouseOver = 0;

    public void SetOnDownKeyHandler(KeyButton.BUTTONCODE buttonCode, ButtonPressHandler handler) {
      onDownKeyHandlers[new KeyButton(buttonCode)] = handler;
    }

    public void SetOnDownKeyHandler(char alphanumeric, ButtonPressHandler handler) {
      onDownKeyHandlers[new KeyButton(alphanumeric)] = handler;
    }

    //High priority goes first.
    public int Priority {get; private set;}

    public void SetPriority(int priority) { this.Priority = priority; }

    public void AddChild(GUILogic child) {
      children.Add(child);
      child.SetParent(this);
      SortChildren();
    }

    public void SetParent(GUILogic theParent) {
      if (parent != null && theParent != null && parent != theParent) {
        throw new StarExcept("Error: this GUILogic already has a parent!");
      }
      parent = theParent;
    }

    public void RemoveChild(GUILogic childToRemove) {
      Log.Write($"Size of children before removal: {children.Count}");
      children.RemoveAll((a) => a == childToRemove);
      Log.Write($"Size of children after removal: {children.Count}");
    }

    //Sorts from high priority to lowest priority.
    public void SortChildren() {
      children.Sort(delegate(GUILogic L1, GUILogic L2) { return -L1.Priority.CompareTo(L2.Priority); });
      String debug = "Sorted priorities: ";
      for (int i = 0; i < children.Count; ++i) {
        debug += children[i].Priority.ToString() + " ";
      }
      Log.Write(debug);
    }

    //Virtual think() function?

    //Returns true if the event has been handled.
    //Returns false if it has not
    public bool ProcessEvent(KeyButtonEvent kbe) {
      bool processed = false;

      for (int i = 0; i < children.Count && !processed; ++i) {
        processed = children[i].ProcessEvent(kbe);
      }

      //THIS IS REALLY DUMB. It will need to be fixed eventually.
      if (kbe.eventType == KeyButtonEvent.EVENTTYPE.KEYUP) return false;

      KeyButton keybutton = kbe.KB;
      if (onDownKeyHandlers.ContainsKey(keybutton)) {
        processed = onDownKeyHandlers[keybutton].Invoke(this);
      }

      return processed;
    }

    public void HandleMouseClickEvent(MouseClickEvent clickEvent) {
      if (onClick == null) return;
      bool consumed = onClick.Invoke(clickEvent, this);
      if (consumed) clickEvent.consumed = true;
    }

    //Returns true if we consumed the event.
    public bool HandleMouseOver(bool mouseOverMe) {
      if (!mouseOverMe) {
        if (framesMouseOver > 0) {        //i.e. if the mouse was over me for at least one frame, until this frame.
          framesMouseOver = 0;
          onMouseNoLongerOver?.Invoke();
        }
        return false;
      }
      //By getting here, we know that mouseOverMe == true;
      framesMouseOver++;
      if (onMouseOver == null) return false;
      return onMouseOver.Invoke();
    }


    //
    // LogicCursorSet functions
    //

    public bool BelongsToALogicCursorSet() => myLogicCursorSet != null;

    public void SetLogicCursorSet(LogicCursorSet logicCursorSet) {
      myLogicCursorSet = logicCursorSet;
    }

    public bool IsLogicCursorOverMe() {
      return myLogicCursorSet != null && myLogicCursorSet.IsCursorOver(this);
    }






  }
  
}*/