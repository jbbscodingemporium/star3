namespace Star.Widget {

  public class LogicCursorSet {

    GUILogic? cursorOver = null;

    List<GUILogic> elements = new List<GUILogic>();

    public void AddElement(GUILogic element) {
      if (element.BelongsToALogicCursorSet()) {
        throw new StarExcept("Error: You cannot AddElement to a LogicCursorSet when that element already belongs to one!");
      }
      elements.Add(element);
      element.SetLogicCursorSet(this);
    }

    public bool IsCursorOver(GUILogic logic) => cursorOver == logic;

    public void AdvanceToNextElement() {
      MoveAlongList(1);
    }

    public void RetreatToPriorElement() {
      MoveAlongList(-1);
    }

    private void MoveAlongList(int delta) {
      if (elements.Count == 0) {
        cursorOver = null;
        return;
      }
      int index = GetIndexOfCursorOver();
      index += delta;
      int count = elements.Count;
      //index = Math.Min(index, elements.Count-1);
      index = ((index % count) + count) % count;
      cursorOver = elements[index];
    }

    //If it's been removed, then it sets cursorOver = null.
    //Returns -1 if it can't be found.
    private int GetIndexOfCursorOver() {
      if (cursorOver == null) return -1;
      for (int i = 0; i < elements.Count; ++i) {
        if (cursorOver == elements[i]) {
          return i;
        }
      }
      cursorOver = null;      //We didn't find it.
      return -1;
    }



    //Event handling for selecing elements from a list
    public bool HandleButtonPressAtCursor(GUILogic sender) {
      Log.Write("called");
      if (cursorOver == null)                         return false;
      if (cursorOver.forwardedButtonPressed == null)  return false;
      return cursorOver.forwardedButtonPressed.Invoke(cursorOver);
    }


    
  }


}