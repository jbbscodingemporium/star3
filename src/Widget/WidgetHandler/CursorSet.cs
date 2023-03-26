namespace Star.Widget {
  
  public class CursorSet {

    WidgetEventHandler? cursorCurrentlyOver = null;

    List<WidgetEventHandler> elements = new List<WidgetEventHandler>();

    public void AddElement(WidgetEventHandler element) {
      if (elements.Contains(element)) {
        throw new StarExcept("Error: Tried to add a WidgetEventHandler to a cursor set that already had it!");
      }
      elements.Add(element);
      element.UpdateMyCursorSetReference(this);
    }

    public void RemoveElement(WidgetEventHandler element) {
      elements.Remove(element);
    }

    public bool IsCursorOver(WidgetEventHandler element) => element == cursorCurrentlyOver;

    public WidgetEventHandler? GetCursorCurrentlyOver() => cursorCurrentlyOver;

    public void AdvanceCursor() { MoveAlongList(1); }

    public void RetreatCursor() { MoveAlongList(-1); }

    private void MoveAlongList(int delta) {
      if (elements.Count == 0) {
        cursorCurrentlyOver = null;
        return;
      }
      int index = GetIndexOfCursorOver();
      index += delta;
      int count = elements.Count;
      index = ((index % count) + count) % count;
      cursorCurrentlyOver = elements[index];
    }

    private int GetIndexOfCursorOver() {
      if (cursorCurrentlyOver == null) return -1;
      for (int i = 0; i < elements.Count; ++i) {
        if (cursorCurrentlyOver == elements[i]) {
          return i;
        }
      }
      cursorCurrentlyOver = null;      //We didn't find it.
      return -1;
    }

  }

}