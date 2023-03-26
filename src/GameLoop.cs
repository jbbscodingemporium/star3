using SFML.System;
using Star;
using Star.Widget;

class GameLoop {
  SFWindow window;

  double framesPerSecond = 60.0;

  public GameLoop(SFWindow window) {
    this.window = window;
    SFML.Window.Window win = window.GetSFMLWindow();
    win.KeyPressed  += Input.HandleKeyPressEvent;
    win.KeyReleased += Input.HandleKeyReleaseEvent;
    win.SetKeyRepeatEnabled(false);
  }

  private TextureGrid LoadTestRoom() {
    var lines = System.IO.File.ReadAllLines("testroom.txt");

    if (lines == null) {throw new StarExcept("Unable to open file.");}

    int h = lines.Length;
    int w = lines[0].Length;

    var grid = new TextureGrid(AllSpriteSheets.environment, w, h);
    for (int y = 0; y < h; ++y) {
      var line = lines[y];
      for (int x = 0; x < w; ++x) {
        char ch = line[(int)x];
        switch (ch) {
          case '#':
          grid.SetTile(x,y,0);
          grid.SetFGColor(x,y,new SFML.Graphics.Color(240,200,255));
          break;
          case '.':
          grid.SetTile(x,y,12);
          grid.SetFGColor(x,y,new SFML.Graphics.Color(150,100,200));
          break;
          default:
          throw new StarExcept($"Unexpected character: {ch}");
        }
      }
    }

    return grid;
  }

  private ASCIIGrid LoadTestRoomASCII() {
    var lines = System.IO.File.ReadAllLines("testroom.txt");

    if (lines == null) {throw new StarExcept("Unable to open file.");}

    int h = lines.Length;
    int w = lines[0].Length;

    ASCIIGrid grid = new ASCIIGrid(w,h);
    for (int y = 0; y < h; ++y) {
      var line = lines[y];
      for (int x = 0; x < w; ++x) {
        char ch = line[(int)x];
        switch (ch) {
          case '#':
          grid.SetTile(x,y,'#');
          grid.SetBGColor(x,y,SFML.Graphics.Color.Blue);
          Console.WriteLine("ok");
          break;
          case '.':
          grid.SetTile(x,y,'.');
          grid.SetFGColor(x,y,SFML.Graphics.Color.Yellow);
          break;
          default:
          throw new StarExcept($"Unexpected character: {ch}");
        }
      }
    }

    return grid;
  }

  /*private Widget GenerateTestWidget1() {
    Widget widget = new Widget(5,3);
    widget.testfg = SFML.Graphics.Color.Yellow;
    widget.testbg = SFML.Graphics.Color.Blue;
    widget.testChar = '&';
    widget.UpdateIntrinsicBounds(3,3,-1,-1);

    //ContainerWidget parent = new ContainerWidget(0,0);
    ContainerWidget parent = new VerticalContainerWidget(0,0);
    parent.testfg = SFML.Graphics.Color.White;
    parent.testbg = SFML.Graphics.Color.Red;
    parent.testChar = '-';

    Widget w2 = new Widget(3,3);
    w2.testfg = SFML.Graphics.Color.Cyan;
    w2.testbg = SFML.Graphics.Color.Black;
    w2.testChar = '@';

    //Widget t2 = new TextWidget("wo",4,2);
    TextWidget t2 = new TextWidget("<red>live <blu>free</blu> or don't</red>",0,0);
    //t2.specialMaxWordWrapWidth = 11;
    t2.testbg = null;
    t2.testfg = SFML.Graphics.Color.White;

    parent.AddChild(t2,true);

    parent.AddChild(widget, true);
    parent.AddChild(w2);

    //ContainerWidget p2 = new ContainerWidget(0,0);
    ContainerWidget p2 = new VerticalContainerWidget(0,0);
    p2.testbg = SFML.Graphics.Color.Magenta;
    p2.testfg = SFML.Graphics.Color.White;
    p2.testChar = '.';

    //Widget swidge = new TextWidget("gooo", 0,0);
    Widget swidge = new TextWidget("<red>U<blu>S</blu>A</red>", 0,0);
    swidge.testfg = SFML.Graphics.Color.White;
    swidge.testbg = null;

    parent.AddChild(swidge);

    p2.AddChild(parent);

    w2.UpdateIntrinsicBounds(5,-1,-1,-1);

    p2.UpdateIntrinsicBounds(0,0,6,9);

    p2.UpdateIntrinsicBounds(0,0,15,7);

    p2.UpdateLogic();

    return p2;
  }*/

  /*private Widget GenerateTestWidget2(out GUILogic outputLogic) {
    VerticalContainerWidget v = new VerticalContainerWidget(4,18,20);
    v.testfg = new SFML.Graphics.Color(128,128,128);
    v.testbg = new SFML.Graphics.Color(0,32,64);
    v.UpdateBorders(1,1,0,0);

    TextWidget desc = new TextWidget("You encounter a <bar>penguin</bar> of <lol>doom</lol>...!@# abcdef<bar>ghijkm</bar>nopqrst ok its fine",0,0);
    desc.testfg = SFML.Graphics.Color.White;
    desc.testbg = SFML.Graphics.Color.Red;
    v.AddChild(desc,true);

    Widget spacer = new Widget(0,0);
    //spacer.testbg = SFML.Graphics.Color.Yellow;
    v.AddSpaceFillingChild(spacer);

    HorizontalContainerWidget hz = new HorizontalContainerWidget(0,0);
    //hz.SetSpaceFillingChildToHaveExtraSpaceAtEnd(true);
    //ContainerWidget hz = new ContainerWidget(0,0,100,100);
    //hz.Resize();
    hz.testfg = new SFML.Graphics.Color(128,128,128);
    //hz.testbg = SFML.Graphics.Color.Black;
    hz.UpdateBorders(0,0,0,0);

    TextWidget t1 = new TextWidget("yes\n\nlol", 0,0);
    t1.testfg = SFML.Graphics.Color.White;

    //ContainerWidget t1c = new ContainerWidget(0,0);
    ContainerWidget t1c = new VerticalContainerWidget(0,0);
    t1c.testfg = SFML.Graphics.Color.White;
    t1c.testbg = SFML.Graphics.Color.Black;
    t1c.AddChild(t1);
    hz.AddChild(t1c, true);

    Widget empty = new Widget(0,0);
    //empty.testbg = SFML.Graphics.Color.Magenta;
    //hz.AddChild(empty,true);
    hz.AddSpaceFillingChild(empty);

    TextWidget t2 = new TextWidget("no", 0,0);
    t2.testfg = SFML.Graphics.Color.White;

    TextWidget t2a = new TextWidget("maybe",0,0);
    t2a.testfg = SFML.Graphics.Color.White;

    ContainerWidget t2c = new VerticalContainerWidget(0,0);
    t2c.testfg = SFML.Graphics.Color.White;
    t2c.testbg = SFML.Graphics.Color.Black;
    t2c.AddChild(t2);
    t2c.AddChild(t2a);

    hz.AddChild(t2c, true);

    v.AddChild(hz, true);
    //hz.testbg = SFML.Graphics.Color.Cyan;

    

    hz.SetSpaceBetweenElements(1);

    v.SetSpaceBetweenElements(1);
    v.SetSpaceFillingChildToHaveExtraSpaceAtEnd(false);

    GUILogic logic = new GUILogic();
    logic.ASSIGNTESTFUNCTION();

    //GUILogic dumblogic = new GUILOGICTEXTCHANGER(desc);
    //desc.SetGUILogic(dumblogic);

    //GUILogic dumblogic2 = new GUILogic();
    //dumblogic2.AddChild(dumblogic);

    //GUILogic fglogic = new GUILOGICCHANGER2(v);
    //v.SetGUILogic(fglogic);

    //outputLogic = dumblogic2;
    return v;
  }*/

  public void RunLoop() {
    var clock = new Clock();
    clock.Restart();

    Time timeSinceLastUpdate = Time.Zero;
    Time secondsPerFrame = Time.FromSeconds((float)(1.0 / framesPerSecond));

    long totalFramesElapsed = 0;

    Log.Write("Here");
    //var grid = LoadTestRoom();

    var grid = new ASCIIGrid(40,30);


    //Widget p2 = GenerateTestWidget1();

    //GUILogic menutestLogic;
    //Widget v = GenerateTestWidget2(out menutestLogic);

    ContainerWidget v = new VerticalContainerWidget(5,5, grid.W, grid.H);
    //v.testfg = SFML.Graphics.Color.White;

    TextWidget desc = new TextWidget("You have encountered a <cyan><randomcaps>PeNguIn Of DoOm</randomcaps></cyan>. It is being <leet><red>random</red></leet> and is looking for <leet>friends to be <red>random</red> with</leet>.\n", 1, 1);
    //desc.testfg = SFML.Graphics.Color.White;

    v.AddChild(desc,true);

    ContainerWidget row = new HorizontalContainerWidget(0,0);
    row.UpdateBorders(0,0,0,0);
    //row.testbg = SFML.Graphics.Color.Red;

    ContainerWidget b1 = new HorizontalContainerWidget(3,3, 15);
    TextWidget t1 = new TextWidget("<color,hue:yellow>1.</color> <leet>w00t</leet>.\n*<randcaps>hOlDs Up SpAtUla</randcaps>*", 1,1);
    //t1.testfg = b1.testfg = SFML.Graphics.Color.White;
    b1.AddChild(t1,true);

    ContainerWidget b2 = new HorizontalContainerWidget(3,3, 15);
    TextWidget t2 = new TextWidget("<yellow>2.</yellow> No way!\nI act serious.", 1,1);
    //t2.testfg = b2.testfg = SFML.Graphics.Color.White;
    b2.AddChild(t2,true);

    WidgetEventHandler b1handler = new WidgetEventHandler();
    b1.SetEventHandler(b1handler);

    WidgetEventHandler b2handler = new WidgetEventHandler();
    b2handler.onMouseOver = TestGUI.MouseOverWrite;
    b2.SetEventHandler(b2handler);

    Widget spaceFiller = new Widget(0,0);
    
    row.AddChild(b1,true);
    row.AddSpaceFillingChild(spaceFiller);
    row.AddChild(b2,true);

    v.AddChild(row,true);

    var styler = new WidgetStyler() {fg=SFML.Graphics.Color.Cyan,bg=new SFML.Graphics.Color(64,64,128)};
    //b2.baseStyler = styler;

    var highlightStyler = new WidgetStyler() {fg = SFML.Graphics.Color.Yellow, bg = new SFML.Graphics.Color(64,96,164)};
    b1.logicHighlightStyler = highlightStyler;
    b2.logicHighlightStyler = highlightStyler;

    var bgstyler = new WidgetStyler() {bg=new SFML.Graphics.Color(32,32,64)};
    v.baseStyler = bgstyler;

    var testGUI = new TestGUI();
    testGUI.AddButton1(b1handler);
    testGUI.AddButton2(b2handler, t2);
    //testGUI.AddHandler(b1handler);
    //testGUI.AddHandler(b2handler);

    //LogicCursorSet lcs = new LogicCursorSet();

    //var gl = new MenuReceiverGUILogic(lcs);
    //row.SetGUILogic(gl);

    //gl.testWidget = v;

    //var bl1 = gl.WireUpNewButton(b1);
    //var bl2 = gl.WireUpNewButton(b2);

    //bl1.SetOnDownKeyHandler('1', gl.HandleButtonPress);
    //bl2.SetOnDownKeyHandler('2', gl.HandleButtonPress);

    //lcs.AddElement(bl1);
    //lcs.AddElement(bl2);





    double secondsPerChange = 1.1625/2;
    int framesPerChange = (int)(secondsPerChange * framesPerSecond);
    int framesRemaining = framesPerChange;
    //int sequence = 0;
    //int delta = 1;

    Time totalTime = new Time();
    
    while (window.IsOpen) {
      totalTime += clock.ElapsedTime;

      timeSinceLastUpdate += clock.Restart();
      
      //Game logic.
      while (timeSinceLastUpdate > secondsPerFrame) {
        timeSinceLastUpdate -= secondsPerFrame;
        ++totalFramesElapsed;
        //Get input once per frame.
        Input.GetInput(window.HasFocus() );
        foreach (var keyEvent in Input.GetKeyButtonEvents()) {
          Log.Write(keyEvent.ToString());
          //bool processed = gl.ProcessEvent(keyEvent);
          bool processed = testGUI.ProcessEvent(keyEvent);
        }

        //Handle mouse-over logic
        var mousePixelLocation = Input.GetMouseCursorXY(window.GetSFMLWindow());
        var mouseUnscaledLocation = window.ScreenPointToUnscaledPoint(mousePixelLocation);
        XY mousexy = grid.TileCoordsFromPixels(mouseUnscaledLocation.x, mouseUnscaledLocation.y);
        bool handled = v.HandleMouseOverLogic(mousexy.x, mousexy.y);

        var mouseEvents = Input.GetMouseClickEvents();
        foreach (var mouseClick in mouseEvents) {
          v.PropagateMouseEvent(mousexy.x, mousexy.y, mouseClick);
          //Log.Write(mouseClick.consumed.ToString());
        }

        

        /*--framesRemaining;
        if (framesRemaining <= 0.0) {
          framesRemaining += framesPerChange;
          sequence += delta;
          if (sequence == 15 || sequence == -2) { delta *= -1; }
          //if (sequence == 3) { hz.SetSpaceFillingChildToHaveExtraSpaceAtEnd(true); }
          //if (sequence == 4) { v.UpdateIntrinsicBounds(20,11,20,100); }
          //if (sequence == -2) { v.UpdateIntrinsicBounds(14, 14, 14, 100); }
          v.UpdateIntrinsicBounds(15+sequence, 20, 15+sequence, 20);
          p2.UpdateIntrinsicBounds(0,0,sequence,sequence);
          p2.UpdateLogic();          
          if (sequence == 3) {
            var pos = Input.GetMouseCursorXY(window.GetSFMLWindow());
            Console.WriteLine(pos.x);
            Console.WriteLine(pos.y);
          }
        }*/

        v.UpdateLogic();



      }

      window.DispatchEvents();
      

      for (int y = 0; y < grid.H; ++y) {
        for (int x = 0; x < grid.W; ++x) {
          grid.SetBGColor(x,y,SFML.Graphics.Color.Transparent);
          grid.SetFGColor(x,y,SFML.Graphics.Color.White);
          grid.SetTile(x,y,' ');
        }
      }

      //p2.PaintToGrid(grid,0,0);
      v.PaintToGrid(grid, 0, 0);

      grid.DrawToRenderTexture(window.buffer);

      window.DrawToWindow();

      window.buffer.Clear();

      //Sleep for 10ms
      Thread.Sleep(10);
    }

  }
}