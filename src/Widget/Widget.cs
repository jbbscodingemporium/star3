namespace Star.Widget {

  public class Widget {

    //Maximum width, height of a widget.
    protected const int MAX_W = 400;
    protected const int MAX_H = 300;

    public int W { get; private set; }
    public int H { get; private set; }

    //Maximum and minimum values of the size
    protected int wMinIntrinsic {get; private set;} = 1;
    protected int hMinIntrinsic {get; private set;} = 1;
    protected int wMaxIntrinsic {get; private set;} = MAX_W;
    protected int hMaxIntrinsic {get; private set;} = MAX_H;

    //Maximum and minimum values from its parent
    protected int wMinParent {get; private set;} = 0;
    protected int hMinParent {get; private set;} = 0;
    protected int wMaxParent {get; private set;} = MAX_W;
    protected int hMaxParent {get; private set;} = MAX_H;

    Widget? parent = null;

    //Temporary values for testing.
    public char testChar = ' ';
    //public SFML.Graphics.Color testfg;
    //public SFML.Graphics.Color? testbg;

    public WidgetStyler? baseStyler = null;
    public WidgetStyler? logicHighlightStyler = null;

    //The guilogic bound to this widget
    protected GUILogic? guiLogic = null;

    public Widget(int minW, int minH, int maxW = MAX_W, int maxH = MAX_H) {
      wMinIntrinsic = minW;
      hMinIntrinsic = minH;
      wMaxIntrinsic = maxW;
      hMaxIntrinsic = maxH;

      Resize();
    }

    public void SetParent(Widget parent) {
      this.parent = parent;
    }

    //Set to null to clear the GUILogic
    public void SetGUILogic(GUILogic? logic) {
      if (logic != null && guiLogic != null && guiLogic != logic) {
        throw new StarExcept("Error: tried to assign a guiLogic to a widget that already had one!");
      }
      guiLogic = logic;
    }

    

    //Override this to write specific code for the widget implementation,
    //e.g. how much space it needs to paint on a string of text.
    //Returns a (w,h) tuple.
    //Should never actually change the size of the widget!
    protected virtual (int,int) CalculateSizeNeededForContents() {
      return (0,0);
    }

    //The minimum desired size is the size needed for the contents || the intrinsic minimums.
    //Returns (w,h) tuple.
    private (int,int) CalculateMinimumDesiredSize() {
      var neededSize = CalculateSizeNeededForContents();
      int widthDesired = neededSize.Item1;
      int heightDesired = neededSize.Item2;

      widthDesired = Math.Max(widthDesired, wMinIntrinsic);
      heightDesired = Math.Max(heightDesired, hMinIntrinsic);
      return (widthDesired,heightDesired);
    }

    //Limits wValue and hValue to the limits specified by our limits.
    private void BoundToMaximumSize(ref int wValue, ref int hValue) {
      //Bound this to the maximum, either internally or imposed by a parent.
      wValue = Math.Min(wValue, wMaxIntrinsic);
      wValue = Math.Min(wValue, wMaxParent);
      hValue = Math.Min(hValue, hMaxIntrinsic);
      hValue = Math.Min(hValue, hMaxParent);
    }

    //Returns a (W,H) tuple that includes the new correct size
    //Does not actually change the size! And should not do so!
    //The new size first gets the minimum desired size, then factors in parent minimums, and then maximum limits.
    public (int, int) CalculateMySize() {
      //var contentSize = CalculateSizeNeededForContents();
      var desiredSize = CalculateMySizeBeforeParentalMinimums();
      int newW = desiredSize.Item1;
      int newH = desiredSize.Item2;

      //Update the values so that they're as large as the parent minimums
      newW = Math.Max(newW, wMinParent);
      newH = Math.Max(newH, hMinParent);

      //Because we possibly increased newW and newH, we need to bound them to their maximums again.
      //Bound this to the maximum, either internally or imposed by a parent.
      //Maximum bounds take precedence over minimum requirements, so this occurs at the end.
      BoundToMaximumSize(ref newW, ref newH);

      return (newW, newH);
    }

    public (int, int) CalculateMySizeBeforeParentalMinimums() {
      //var contentSize = CalculateSizeNeededForContents();
      var desiredSize = CalculateMinimumDesiredSize();
      int newW = desiredSize.Item1;
      int newH = desiredSize.Item2;

      //Bound this to the maximum, either internally or imposed by a parent.
      //Maximum bounds take precedence over minimum requirements, so this occurs at the end.
      BoundToMaximumSize(ref newW, ref newH);

      return (newW, newH);
    }

    ///Pass a -1 to any argument to keep its value the same.
    public void UpdateIntrinsicBounds(int minW, int minH, int maxW, int maxH) {
      wMinIntrinsic = minW < 0 ? wMinIntrinsic : minW;
      hMinIntrinsic = minH < 0 ? hMinIntrinsic : minH;
      wMaxIntrinsic = maxW < 0 ? wMaxIntrinsic : maxW;
      hMaxIntrinsic = maxH < 0 ? hMaxIntrinsic : maxH;

      UpdateMyChildrensBounds();

      Resize();
    }

    protected virtual void UpdateMyChildrensBounds() {}

    public void UpdateMyBoundsFromParent(int? minw, int? minh, int? maxw, int? maxh) {

      if (minw == null) {minw = wMinParent;}
      if (minh == null) {minh = hMinParent;}
      if (maxw == null) {maxw = wMaxParent;}
      if (maxh == null) {maxh = hMaxParent;}

      int oldWMinParent = wMinParent;
      int oldHMinParent = hMinParent;
      int oldWMaxParent = wMaxParent;
      int oldHMaxParent = hMaxParent;

      wMinParent = Math.Max(0,(int)minw);
      hMinParent = Math.Max(0,(int)minh);
      wMaxParent = Math.Max(0,(int)maxw);
      hMaxParent = Math.Max(0,(int)maxh);

      bool changed = oldWMinParent != wMinParent ||
                     oldHMinParent != hMinParent ||
                     oldWMaxParent != wMaxParent ||
                     oldHMaxParent != hMaxParent; 

      if (changed) { UpdateMyChildrensBounds(); }
    }

    //Resize function
    public void Resize(bool climbUphill = true) {
      (int, int) newCalcSize = CalculateMySize();
      //bool actuallyChanged = W != newCalcSize.Item1 || H != newCalcSize.Item2;

      W = newCalcSize.Item1;
      H = newCalcSize.Item2;

      //TODO: Add special span code here. Tell your children that their new bounds have changed.
      //Maybe it doesn't go here.

      //Alert children.
      UpdateMyChildrensBounds();
      ResizeMyChildren();

      //Locate the "stuff" in this widget.
      PrePositionMyContentsAndResizeSpaceFillingWidget();
      PositionMyContents();

      //Pass this up the ladder, if you're walking up the ladder.
      if (climbUphill && parent != null) {
        parent.Resize(true);
      }
    }

    protected virtual void ResizeMyChildren() {}

    //This gets called by container objects that have a space-filling widget.
    //It figures out how much space the existing widgets use up (based on current recorded sizes),
    //uses this to figure out how much extra space there is in the container,
    //And will give extra space to the space-filling widget if there is one.
    protected virtual void PrePositionMyContentsAndResizeSpaceFillingWidget() {}

    //This function should be called after setting a new size and resizing the widget's children, if any.
    //It positions the widget's internal contents such that they are correctely spaced.
    protected virtual void PositionMyContents() {}

    //public void PaintToGrid(ASCIIGrid asciiGrid, int x0, int y0) => PaintTo(asciiGrid, x0, y0, 0, 0, (int)asciiGrid.W-1, (int)asciiGrid.H-1);
    public void PaintToGrid(ASCIIGrid asciiGrid, int x0, int y0) => PaintTo(asciiGrid, x0, y0, new StarIntRect(0,0,asciiGrid.W,asciiGrid.H));

    //When you pass in a canvas (an area that you are able to draw on), it gives you an area that intersects the canvas and the contents of the widget.
    //Uses xorigin and yorigin to figure out where the widget is located relative to 0,0 on the grid that it's drawing on.
    protected StarIntRect GetContentsCanvasIntersection(int xorigin, int yorigin, StarIntRect canvas) {
      StarIntRect contentsBox = new StarIntRect(xorigin, yorigin, W, H);
      StarIntRect intersection = StarIntRect.Intersection(contentsBox, canvas);
      return intersection;
    }

    /*protected SFML.Graphics.Color GetBaseFG() {
      //return styler != null && ? styler.fg : WidgetStyler.defaultFG;
      return styler?.fg ?? WidgetStyler.defaultFG;
    }

    protected SFML.Graphics.Color? GetBaseBG() {
      return styler?.bg ?? WidgetStyler.defaultBG;
    }*/

    protected SFML.Graphics.Color GetFG(WidgetStyler? overrideStyler) {
      return overrideStyler?.fg ?? WidgetStyler.defaultFG;
    }

    protected SFML.Graphics.Color? GetBG(WidgetStyler? overrideStyler) {
      return overrideStyler?.bg ?? WidgetStyler.defaultBG;
    }


    public void PaintTo(ASCIIGrid asciiGrid, int xorigin, int yorigin, StarIntRect canvas ) {
      var styler = SelectStyler();

      PaintToWithStyler(asciiGrid, xorigin, yorigin, canvas, styler);
    }

    protected WidgetStyler? SelectStyler() {
      WidgetStyler? styler = baseStyler;

      if (guiLogic != null && guiLogic.IsLogicCursorOverMe() && logicHighlightStyler != null) {
        styler = (styler == null) ? logicHighlightStyler : styler.Compound(logicHighlightStyler);
      }
      
      return styler;
    }

    //Only paint to squares covered by canvas
    public virtual void PaintToWithStyler(ASCIIGrid asciiGrid, int xorigin, int yorigin, StarIntRect canvas, WidgetStyler? styler) {
      StarIntRect intersection = GetContentsCanvasIntersection(xorigin, yorigin, canvas);
      if (!intersection.Exists()) { return; }

      //var fg = GetBaseFG();
      //var bg = GetBaseBG();

      var fg = GetFG(styler);
      var bg = GetBG(styler);

      int x1 = intersection.x1;
      int y1 = intersection.y1;
      for (int y = intersection.y0; y <= y1; ++y) {
        for (int x = intersection.x0; x <= x1; ++x) {
          asciiGrid.SetFGColor(x,y,fg);
          asciiGrid.SetBGColor(x,y,bg);
          asciiGrid.SetTile(x,y,testChar);
        }
      }
    }

    public virtual void UpdateLogic() {}

    public bool IsPointInBounds(int localX, int localY) {
      return (localX >= 0 && localY >= 0 && localX < W && localY < H);
    }

    protected virtual void PropagateMouseEventToChildren(int mouseXlocal, int mouseYlocal, MouseClickEvent mouseClickEvent) { return; }

    public void PropagateMouseEvent(int mouseXlocal, int mouseYlocal, MouseClickEvent mouseClickEvent) {

      PropagateMouseEventToChildren(mouseXlocal, mouseYlocal, mouseClickEvent);

      if (mouseClickEvent.consumed) return;

      if (mouseClickEvent.mouseButton == MouseButtons.MB.LEFT &&
          guiLogic != null && IsPointInBounds(mouseXlocal, mouseYlocal)) {
        //mouseClickEvent.consumed = guiLogic.MouseClickedOnMe();
        guiLogic.HandleMouseClickEvent(mouseClickEvent);
      }
    }

    //Returns true if the mouseOver has been consumed (handled).
    public virtual bool HandleMouseOverLogic(int mouseXlocal, int mouseYlocal, bool mouseOverConsumed = false) {

      mouseOverConsumed = HandleMouseOverLogicForChildren(mouseXlocal, mouseYlocal, mouseOverConsumed);

      if (guiLogic == null) { return mouseOverConsumed; }
      bool mouseOverMe = IsPointInBounds(mouseXlocal, mouseYlocal);
      bool mouseConsideredOver = mouseOverMe && !mouseOverConsumed;
      return guiLogic.HandleMouseOver(mouseConsideredOver);
    }

    protected virtual bool HandleMouseOverLogicForChildren(int mouseXlocal, int mouseYlocal, bool mouseOverConsumed) {
      return mouseOverConsumed;
    }

    /*protected virtual bool PropagateMouseEventsToChildren(int mouseXlocal, int mouseYlocal, bool clicked, bool clickHandled) { return false; }

    public bool PropagateMouseEvents(int mouseXlocal, int mouseYlocal, bool clicked, bool clickHandled = false) {

      clickHandled = PropagateMouseEventsToChildren(mouseXlocal, mouseYlocal, clicked, clickHandled);

      if (clickHandled) return clickHandled;

      if (clicked && guiLogic != null && IsPointInBounds(mouseXlocal, mouseYlocal)) {
        clickHandled = guiLogic.MouseClickedOnMe();
      }

      return clickHandled;
    }*/

    
  }


  
}