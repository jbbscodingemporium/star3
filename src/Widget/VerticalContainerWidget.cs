namespace Star.Widget {

  public class VerticalContainerWidget : ContainerWidget {
    public VerticalContainerWidget(int minW, int minH, int maxW = MAX_W, int maxH = MAX_H) :
      base(minW, minH, maxW, maxH) {

    }

    protected override (int, int) CalculateSizeNeededForContents() {
      int width = 0;
      int height = 0;
      foreach (var childWidget in children) {
        var child = childWidget.child;
        var childSize = child.CalculateMySizeBeforeParentalMinimums();
        width = Math.Max(width, childSize.Item1);
        height += childSize.Item2;
        if (HasSpaceAfterElement(child)) { height += spaceBetweenElements; }
      }
      width += borderLeft + borderRight;
      height += borderTop + borderBot;

      return (width,height);
    }

    protected override void PositionMyContents() {
      int x = borderLeft;
      int y = borderTop;
      for (int i = 0; i < children.Count; ++i) {
        children[i].xlocal = x;
        children[i].ylocal = y;
        Widget child = children[i].child;
        y += child.H;
        if (HasSpaceAfterElement(child)) { y += spaceBetweenElements; }
      }
    }

      protected override void PrePositionMyContentsAndResizeSpaceFillingWidget() {
      if (spaceFillingWidget == null) return;

      int y = borderTop;
      for (int i = 0; i < children.Count; ++i) {
        Widget child = children[i].child;
        if (HasSpaceAfterElement(child)) { y += spaceBetweenElements; }
        if (child == spaceFillingWidget) continue;
        y += child.H;
      }
      y += borderBot;

      int remainder = H - y;
      remainder = Math.Max(0,remainder);

      //Set the space-filling widget to be at least as large as the remainder.
      spaceFillingWidget.UpdateMyBoundsFromParent(null, remainder, null, null);
      //And resize it.
      spaceFillingWidget.Resize(false);
    }


    protected override (int, int) GetSpanningChildMinimumBounds(){
      return (W - borderLeft - borderRight, 0);
    }

  }
}