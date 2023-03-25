namespace Star.Widget {
  //Going to start by making a vertical list. Will split out other classes as needed.
  public abstract class ContainerWidget : Widget {

    //Class that holds data on the locations of the children and the children themselves
    protected class ChildWidget {
      public int xlocal, ylocal;
      public Widget child;
      public bool spanning = false;

      public ChildWidget(Widget child, bool spanning) {
        this.child = child;
        this.spanning = spanning;
      }
    }

    protected List<ChildWidget> children = new List<ChildWidget>();

    protected int borderLeft = 1;
    protected int borderRight = 1;
    protected int borderTop = 1;
    protected int borderBot = 1;

    protected int spaceBetweenElements {get; private set;} = 0;

    protected Widget? spaceFillingWidget = null;
    protected bool spaceFillingWidgetGetsSpaceBetweenElements = false;

    public ContainerWidget(int minW, int minH, int maxW = MAX_W, int maxH = MAX_H) 
      : base(minW, minH, maxW, maxH) {
    }

    //Returns true if the selected widget gets space after it.
    //It will not get space after it in the following cases:
    //1) it's a spaceFillingWidget or 2) it's the last widget in children.
    protected bool HasSpaceAfterElement(Widget wid) {
      if (wid == null) return false;    //Maybe this should be false? Probably you shouldn't have null widgets!
      if (children.Count > 0 && children.Last().child == wid) return false;
      if (wid != spaceFillingWidget) return true;
      return spaceFillingWidgetGetsSpaceBetweenElements;
    }

    public void AddChild(Widget child, bool spanning = false) {
      children.Add(new ChildWidget(child, spanning));
      child.SetParent(this);
      Resize();
    }

    public void SetSpaceBetweenElements(int space) {
      space = Math.Max(0,space);
      if (space == spaceBetweenElements) return;
      spaceBetweenElements = space;
      Resize(true);
    }

    //This sets the assigned child to be a space-filler and then adds it to our set.
    public void AddSpaceFillingChild(Widget child, bool spanning = true) {
      spaceFillingWidget = child;
      AddChild(child, spanning);
    }

    public void SetSpaceFillingChildToHaveExtraSpaceAtEnd(bool hasExtraSpace = true) {
      if (hasExtraSpace == spaceFillingWidgetGetsSpaceBetweenElements) return;
      spaceFillingWidgetGetsSpaceBetweenElements = hasExtraSpace;
      Resize(true);
    }

    //I DON'T THINK THIS SHOULD BE HERE.
    //Should not change the size of any of the contents!
    //The childrens' bounds should already be called before calling this!
    /*protected override (int, int) CalculateSizeNeededForContents() {
      int width = 0;
      int height = 0;
      foreach (var childwidget in children) {
        var child = childwidget.child;
        var childSize = child.CalculateMySizeBeforeParentalMinimums();
        width = Math.Max(childSize.Item1, width);
        height += childSize.Item2;
      }
      width += borderLeft + borderRight;
      height += borderTop + borderBot;

      return (width,height);
    }*/

    protected override void ResizeMyChildren() {
      for (int i = 0; i < children.Count; ++i) {
        //Don't climb uphill here; just propagate the changes downhill.
        children[i].child.Resize(false);
      }
    }

    public override void PaintToWithStyler(ASCIIGrid grid, int xorigin, int yorigin, StarIntRect canvas, WidgetStyler? styler) {
      base.PaintToWithStyler(grid, xorigin, yorigin, canvas, styler);

      var internalContents = new StarIntRect(xorigin+borderLeft,
                                             yorigin+borderTop,
                                             W-borderLeft-borderRight,
                                             H-borderTop-borderBot);


      var intersection = StarIntRect.Intersection(internalContents, canvas);

      bool intersectionExists = intersection.Exists();
      
      for (int i = 0; i < children.Count && intersectionExists; ++i) {
        var childWidget = children[i];
        int x = childWidget.xlocal + xorigin;
        int y = childWidget.ylocal + yorigin;
        childWidget.child.PaintTo(grid, x, y, intersection);
      }

      PaintBorders(grid, xorigin, yorigin, canvas);
    }

    protected void PaintBorders(ASCIIGrid grid, int xorigin, int yorigin, StarIntRect canvas) {

      if (borderTop <= 0 && borderBot <= 0 && borderRight <= 0 && borderLeft <= 0) return;

      if (W <= 0 || H <= 0) return;

      int x0 = xorigin;
      int y0 = yorigin;
      int x1 = xorigin + W - 1;
      int y1 = yorigin + H - 1;

      

      List<(int,int,char)> pipeTiles = new List<(int,int,char)>();

      for (int y = y0 + 1; y <= y1-1; ++y) {
        if (borderLeft > 0)  pipeTiles.Add((x0,y, ASCIISheet.PIPENS));
        if (borderRight > 0) pipeTiles.Add((x1,y, ASCIISheet.PIPENS));
      }
      for (int x = x0 + 1; x <= x1-1; ++x) {
        if (borderTop > 0)   pipeTiles.Add((x,y0, ASCIISheet.PIPEEW));
        if (borderBot > 0)   pipeTiles.Add((x,y1, ASCIISheet.PIPEEW));
      }

      if (x0 < x1 || y0 < y1) {
        if (borderBot > 0 || borderRight > 0) pipeTiles.Add((x1,y1,ASCIISheet.PIPENW));
        if (borderTop > 0 || borderRight > 0) pipeTiles.Add((x1,y0,ASCIISheet.PIPESW));
        if (borderBot > 0 || borderLeft > 0)  pipeTiles.Add((x0,y1,ASCIISheet.PIPENE));
        if (borderTop > 0 || borderLeft > 0)  pipeTiles.Add((x0,y0,ASCIISheet.PIPESE));
      } else {
        pipeTiles.Add((x0,y0,ASCIISheet.SQUARE));
      }

      foreach (var tup in pipeTiles) {
        int x = tup.Item1;
        int y = tup.Item2;
        char tile = tup.Item3;
        if (canvas.ContainsPoint(x,y)) {
          grid.SetTile(x,y,tile);
        }
      }      
      
    }

    public void UpdateBorders(int top, int bot, int left, int right) {
      borderTop = top > -1 ? top : borderTop;
      borderBot = bot > -1 ? bot : borderBot;
      borderLeft = left > -1 ? left : borderLeft;
      borderRight = right > -1 ? right : borderRight;

      UpdateMyChildrensBounds();
      Resize();
    }

    protected override void UpdateMyChildrensBounds() {
      int maxW = Math.Min(wMaxIntrinsic, wMaxParent);
      int maxH = Math.Min(hMaxIntrinsic, hMaxParent);

      maxW -= borderLeft + borderRight;
      maxH -= borderTop + borderBot;

      for (int i = 0; i < children.Count; ++i) {
        var childContainer = children[i];
        var child = childContainer.child;

        var minBounds = childContainer.spanning ? GetSpanningChildMinimumBounds() : (0,0);

        int childMinW = minBounds.Item1;
        int childMinH = minBounds.Item2;

        child.UpdateMyBoundsFromParent(childMinW, childMinH, maxW, maxH);
      }
    }

    //If a child is assigned to span, this returns the minimum width or height that it has to have.
    protected abstract (int, int) GetSpanningChildMinimumBounds();

    public override void UpdateLogic() {
      for (int i = 0; i < children.Count; ++i) {
        children[i].child.UpdateLogic();
      }
    }

    protected override void PropagateMouseEventToChildren(int mouseXlocal, int mouseYlocal, MouseClickEvent mouseClickEvent) {
      //Bails out early if the event gets consumed
      for (int i = 0; i < children.Count && !mouseClickEvent.consumed; ++i) {
        int x = mouseXlocal - children[i].xlocal;
        int y = mouseYlocal - children[i].ylocal;
        children[i].child.PropagateMouseEvent(x,y,mouseClickEvent);
        //clickHandled = children[i].child.PropagateMouseEvents(x,y, clicked, clickHandled);
      }
    }

    protected override bool HandleMouseOverLogicForChildren(int mouseXlocal, int mouseYlocal, bool mouseOverConsumed) {
      for (int i = 0; i < children.Count; ++i) {
        int x = mouseXlocal - children[i].xlocal;
        int y = mouseYlocal - children[i].ylocal;
        bool handled = children[i].child.HandleMouseOverLogic(x, y, mouseOverConsumed);
        mouseOverConsumed = handled || mouseOverConsumed;
      }
      return mouseOverConsumed;
    }


  }
}