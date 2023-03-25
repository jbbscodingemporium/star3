namespace Star.Widget {

  public class HorizontalContainerWidget : ContainerWidget {

    public HorizontalContainerWidget(int minW, int minH, int maxW = MAX_W, int maxH = MAX_H) :
      base(minW, minH, maxW, maxH) {
    }

    protected override (int, int) CalculateSizeNeededForContents() {
      int width = 0;
      int height = 0;
      for (int i = 0; i < children.Count; ++i) {
        var child = children[i].child;
        var childSize = child.CalculateMySizeBeforeParentalMinimums();
        width += childSize.Item1;
        if (HasSpaceAfterElement(child)) { width += spaceBetweenElements; }
        height = Math.Max(childSize.Item2, height);
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
        x += child.W;
        if (HasSpaceAfterElement(child)) { x += spaceBetweenElements; }
      }
    }

    protected override (int, int) GetSpanningChildMinimumBounds() {
      return (0, H - borderTop - borderBot);
    }

    protected override void PrePositionMyContentsAndResizeSpaceFillingWidget() {
      if (spaceFillingWidget == null) return;

      int x = borderLeft;
      for (int i = 0; i < children.Count; ++i) {
        Widget child = children[i].child;
        if (HasSpaceAfterElement(child)) { x += spaceBetweenElements; }
        if (child == spaceFillingWidget) continue;
        x += child.W;
      }
      x += borderRight;

      int remainder = W - x;
      remainder = Math.Max(0,remainder);

      //Set the space-filling widget to be at least as large as the remainder.
      spaceFillingWidget.UpdateMyBoundsFromParent(remainder, null, null, null);
      //And resize it.
      spaceFillingWidget.Resize(false);
    }

    

  }

}