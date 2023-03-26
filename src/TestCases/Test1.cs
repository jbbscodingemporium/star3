using Star;
using Star.Widget;

public class TestGUI : GUIEventManager {

  public static bool MouseOverWrite() {
    //Log.Write("here");
    return true;
  }

  CursorSet cursorSet = new CursorSet();

  WidgetEventHandler handler1;
  WidgetEventHandler handler2;

  public void AddHandler(WidgetEventHandler handler) {
    cursorSet.AddElement(handler);
  }

  public void AddButton1(WidgetEventHandler handler1) {
    this.handler1 = handler1;
    handler1.activationHandler = Response;
    handler1.clickHandler = handler1.ActivateOnClick;
    AddHandler(handler1);
  }

  public void AddButton2(WidgetEventHandler handler2, TextWidget wid) {
    this.handler2 = handler2;
    //handler2.activationHandler = Response;
    handler2.activationHandler = (WidgetEventHandler sender) => { wid.SetText("<leet>you pressed the button</leet>"); };
    handler2.clickHandler = handler2.ActivateOnClick;
    AddHandler(handler2);
  }

  public void Response(WidgetEventHandler sender) {
    if (sender == handler1) {
      Log.Write("Pressed button 1");
    }
    if (sender == handler2) {
      Log.Write("Pressed button 2");
    }
  }

  public bool ProcessEvent(KeyButtonEvent keyEvent) {
    if (keyEvent.eventType == KeyButtonEvent.EVENTTYPE.KEYUP) return false;
    switch (keyEvent.ButtonCode) {
      case KeyButton.BUTTONCODE.RIGHT:
      cursorSet.AdvanceCursor();
      Log.Write("Advanced!");
      return true;

      case KeyButton.BUTTONCODE.LEFT:
      cursorSet.RetreatCursor();
      Log.Write("Retreated!");
      return true;

      case KeyButton.BUTTONCODE.CONFIRM:
      var cursorTarget = cursorSet.GetCursorCurrentlyOver();
      Log.Write("Activation!");
      if (cursorTarget != null) cursorTarget.Activate();
      return true;

    }

    switch (keyEvent.AlphaNumeric) {
      case '1':
      handler1.Activate();
      return true;

      case '2':
      handler2.Activate();
      return true;
    }

    return false;
  }

}