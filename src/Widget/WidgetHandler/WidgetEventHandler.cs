namespace Star.Widget {

  //Holds the callbacks for a given widget
  public class WidgetEventHandler {
    
    public WidgetEventHandler() {}

    //Todo: Add CursorSet reference.
    private CursorSet? cursorSetIBelongTo = null;

    //Delegate type for when this GUI is a button that gets activated.
    public delegate void ActivationHandler(WidgetEventHandler sender); 

    //Delegate type for handling when clicks happen.
    public delegate void ClickHandler(MouseClickEvent clickEvent, WidgetEventHandler sender);

    //Delegate type for mouseover
    //Returns true if the mouseOver has been consumed, i.e. don't propagate OnMouseOver down the widget chain
    public delegate bool OnMouseOver();
    public delegate void OnMouseNoLongerOver();

    //The actual delegates

    //When this is a button and it gets activated
    public ActivationHandler? activationHandler = null;

    //When this gets clicked on.
    public ClickHandler? clickHandler = null;

    //When the mouse goes over this.
    public OnMouseOver? onMouseOver = null;

    //When the mouse is no longer over this.
    public OnMouseNoLongerOver? onMouseNoLongerOver = null;

    long framesMouseOver = 0;

    public void DisconnectEverything() {
      RemoveMeFromCursorSet();
      //TODO: FILL THIS OUT.
    }

    public void Activate() {
      activationHandler?.Invoke(this);
    }

    
    public void UpdateMyCursorSetReference(CursorSet newCursorSet) {
      if (newCursorSet == cursorSetIBelongTo) return;
      RemoveMeFromCursorSet();
      cursorSetIBelongTo = newCursorSet;
    }

    public void RemoveMeFromCursorSet() {
      cursorSetIBelongTo?.RemoveElement(this);
      cursorSetIBelongTo = null;
    }

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

    public void HandleMouseClickEvent(MouseClickEvent mouseClickEvent) {
      if (clickHandler == null) return;
      clickHandler.Invoke(mouseClickEvent, this);
    }

    public bool IsCursorSetOverMe() {
      if (cursorSetIBelongTo == null) return false;
      return cursorSetIBelongTo.IsCursorOver(this);
    }

    //Hook onClick to this so that it Activates() when you click on it.
    public void ActivateOnClick(MouseClickEvent clickEvent, WidgetEventHandler sender) {
      Activate();
      clickEvent.consumed = true;
    }












  }

}