namespace Star.Widget {

  public class TextWidget : Widget {

    //Has information on a cell within the widget.
    public class TextCell {
      public char character;
      public char? overrideCharacter;

      public SFML.Graphics.Color? overrideFG = null;

    }

    public class TextCellData {
      private List<List<TextCell>> data = new List<List<TextCell>>();

      public int W { get; private set; } = 0;
      public int H { get; private set; } = 0;

      public TextCellData(int w, int h) {
        Resize(w,h);
      }

      public void EraseTileData() {
        for (int y = 0; y < H; ++y) {
          for (int x = 0; x < W; ++x) {
            data[y][x].character = (char)0;
            data[y][x].overrideCharacter = null;
            data[y][x].overrideFG = null;
          }
        }
      }

      public bool GoodCoord(int x, int y) => x >= 0 && y >= 0 && x < W && y < H;

      public char GetBaseCharacter(int x, int y) {
        if (!GoodCoord(x,y)) { throw new StarExcept("Error: tried to GetCharacter at a bad point."); }
        return data[y][x].character;
      }

      public char GetCharacter(int x, int y) {
        if (!GoodCoord(x,y)) { throw new StarExcept("Error: tried to GetCharacter at a bad point."); }
        var tile = data[y][x];
        if (tile.overrideCharacter != null) return (char)tile.overrideCharacter;
        return tile.character;
        //bool exists = tile.overrideCharacter != null;
        //if (exists) {
          //Log.Write("Okay we got the character.");
        //}
        //char c = tile.overrideCharacter ?? tile.character;
        //return c;
      }

      public void SetCharacter(int x, int y, char character) {
        if (!GoodCoord(x,y)) { throw new StarExcept("Error: tried to SetCharacter at a bad point."); }
        data[y][x].character = character;
      }

      public void SetOverrideCharacter(int x, int y, char? overrideCharacter) {
        if (!GoodCoord(x,y)) { throw new StarExcept("Error: tried to SetOverrideCharacter at a bad point."); }
        data[y][x].overrideCharacter = overrideCharacter;
      }

      public void SetOverrideFG(int x, int y, SFML.Graphics.Color? overrideFGColor) {
        if (!GoodCoord(x,y)) { throw new StarExcept("Error: tried to SetOverrideFG at a bad point."); }
        data[y][x].overrideFG = overrideFGColor;
      }

      public SFML.Graphics.Color? GetOverrideFG(int x, int y) {
        if (!GoodCoord(x,y)) { throw new StarExcept("Error: Tried to GetOverrideFG at a bad point."); }
        return data[y][x].overrideFG;
      }

      public void ClearOverrideData() {
        for (int y = 0; y < H; ++y) {
          for (int x = 0; x < W; ++x) {
            data[y][x].overrideCharacter = null;
            data[y][x].overrideFG = null;
          }
        }
      }

      public void Resize(int newW, int newH) {

        if (newW == W && newH == H) return;   //No resize needed.

        var oldCopy = data;
        data = new List<List<TextCell>>();

        for (int y = 0; y < newH; ++y) {
          var newRow = new List<TextCell>();
          for (int x = 0; x < newW; ++x) {
            TextCell tc = x < W && y < H ? oldCopy[y][x] : new TextCell();
            newRow.Add(tc);
          }
          data.Add(newRow);
        }

        W = newW;
        H = newH;
      }






    }

    public string str {get; private set;}

    //When this is not null, use it instead of the parents' prescribed max for laying out the characters.
    public int? specialMaxWordWrapWidth = null; 

    //Information on the text that's been laid out. Pull from this to paint characters to the screen.
    TextCellData cellData = new TextCellData(0,0);

    List<Sylph>? sylphs = null;

    public void ResetSylphs() {
      sylphs = null;
    }

    public TextWidget(string textString, int minW, int minH, int maxW = MAX_W, int maxH = MAX_H) 
      : base(minW, minH, maxW, maxH) {
      this.str = "";   //This is meaningless. It's here to get rid of warnings.
      SetText(textString, true);
    }

    public void SetText(string str, bool alwaysResize = false) {
      bool unchanged = (this.str == str);
      if (unchanged && !alwaysResize) { return; }
      if (!unchanged) { this.str = str; }
      ResetSylphs();
      Resize();
    }

    //Returns 0,0 if does not exist.
    //First number is length of rows, second is number of rows.
    private (int,int) GetCellDataWH() {
      return (cellData.W, cellData.H);
    }

    protected void ClearCellData() {
      cellData.EraseTileData();
    }

    //Resizes the cell data if it's the wrong size.
    private void CheckAndResizeCellData() {
      if (W != cellData.W || H != cellData.H) {
        cellData.Resize(W,H);
      }
    }

    protected override void PositionMyContents() {
      CheckAndResizeCellData();
      DetermineOrChangeLayout(true);
    }

    protected override (int, int) CalculateSizeNeededForContents() {
      return DetermineOrChangeLayout();
    }


    //If actuallyChanging == true then we update the contents of cellData.
    public (int, int) DetermineOrChangeLayout(bool actuallyChanging = false) {

      //Erase all of our data if we're actually changing this.
      if (actuallyChanging) {
        ClearCellData();
      }

      //Only generate new sylphs if sylphs == null.
      //Setting sylphs = null makes it so that they get regenerated next time DetermineOrChangeLayout(true)
      //gets called.
      bool generatingSylphs = actuallyChanging && sylphs == null;
      if (generatingSylphs) {
        sylphs = new List<Sylph>();
      }

      //As we open and close sylphs we'll update these.
      int currentSylphIndex = -1;
      List<Sylph> openSylphs = new List<Sylph>();

      var tokens = WidgetLexer.GetTokens(str);
      int x = 0;
      int y = 0;

      //To avoid weird jittery resizing when resizing a container that holds this text box,
      //we can specify a special max word wrap width to override wMaxParent when doing the layout.
      int xmax = (specialMaxWordWrapWidth ?? wMaxParent) - 1;
      xmax = Math.Min(xmax, wMaxIntrinsic - 1);
      //int ymax = Math.Min(hMaxParent - 1, hMaxIntrinsic - 1);
      
      xmax = Math.Max(xmax,0);              //Avoid infinite loops!

      int canvasLineLength = xmax + 1;      //If xmax is 4 then the length of [0,1,2,3,4] == 5

      //Todo: Should this be -1 or 0?
      int largestX = -1;

      Log.Write($"Starting trace of ${str}");

      TextToken? processingToken = null;

      //When this is true, care about spacing words such that they don't run off the end of a line.
      //It becomes false when the text is too long to fit on a fresh line.
      bool tryToFitOnOneLine = true;

      while (tokens.Count > 0 || processingToken != null ) {
        if (processingToken == null) {
          processingToken = tokens[0];
          tokens.RemoveAt(0);
        }

        string text_to_paint = "";

        //Position the token to be painted on the grid.
        //Gather the text that will be painted, and remove it from the token.
        switch (processingToken.tokenType) {
        case TextToken.TOKENTYPE.NEWLINE:
          //Jump to a new line.
          x = 0;
          ++y;
          processingToken.doneWithThisToken = true;
          tryToFitOnOneLine = true;
          break;

        //Whitespace tokens are plotted to cellData unless we're on a brand new line or beyond our xmax,
        //in which case they're ignored
        case TextToken.TOKENTYPE.WHITESPACE:
          if (x != 0 && x <= xmax) {
            text_to_paint = " ";
          }
          processingToken.doneWithThisToken = true;
          tryToFitOnOneLine = true;
          break;

        case TextToken.TOKENTYPE.PLAINTEXT:
          if (x > xmax) {
            //Jump to a new line
            x = 0;
            ++y;
          }
          int spaceRemainingOnLine = xmax - x + 1;
          int remainingTokenPlainText = WidgetLexer.GetLengthToNextWhiteSpace(tokens);
          int tokenLength = processingToken.plainText.Length;
          int wordLength = tokenLength + remainingTokenPlainText;
          bool wordCanFitOnCurrentLine = wordLength <= spaceRemainingOnLine;
          //If we can't fit the word on the current line, then jump to a fresh line.
          if (!wordCanFitOnCurrentLine && x != 0 && tryToFitOnOneLine) {
            //Jump to a new line 
            x = 0;
            ++y;
            //By jumping onto a brand new line, we've done all we can to try to fit this word into the text
            //And should just write out the text wrapping it around until we reach some whitespace.
            //Hence, tryToFitOnOneLine = false
            tryToFitOnOneLine = false;
          }
          //By this point, either the word can fit on the line or we're on a fresh line,
          //or tryToFitOnOneLine is false and we don't care about words running off the end.
          bool tokenCanFitOnCurrentLine = tokenLength <= spaceRemainingOnLine;
          if (tokenCanFitOnCurrentLine) {
            text_to_paint = processingToken.plainText;
            processingToken.plainText = "";
            processingToken.doneWithThisToken = true;
          } else {
            text_to_paint = processingToken.plainText.Substring(0,spaceRemainingOnLine);
            processingToken.plainText = processingToken.plainText.Substring(spaceRemainingOnLine);
          }
          break;

        //If we're at an open sylph tag, then either grab existing sylph or create a new one
        case TextToken.TOKENTYPE.SYLPHOPEN:
          processingToken.doneWithThisToken = true;
          if (!actuallyChanging) { break; }
          if (sylphs == null) { throw new StarExcept("Error: sylphs list is null."); }

          ++currentSylphIndex;
          int depth = openSylphs.Count;

          //If we're currently generating the sylphs, then we need to make one of the type.
          //Otherwise, we just grab the one we've got.
          if (generatingSylphs) {
            var generatedSylph = SylphFactory.Generate(processingToken.sylphName, depth, processingToken.tags);
            sylphs.Add(generatedSylph);
          }
          Sylph sylph = sylphs[currentSylphIndex];
          sylph.ClearXYs();

          openSylphs.Add(sylph);

          if (sylph.name != processingToken.sylphName) {
            throw new StarExcept($"Parsing Error: sylph names do not match: {sylph.name} vs {processingToken.sylphName}");
          }
          
          Log.Write("Opened sylph: " + sylph.name);
          Log.Write($"Length of opensylphs is now: {openSylphs.Count}");
          break;

        case TextToken.TOKENTYPE.SYLPHCLOSE:
          processingToken.doneWithThisToken = true;
          if (!actuallyChanging) { break; }

          Sylph sylphRemoved = openSylphs.Last();
          openSylphs.RemoveAt(openSylphs.Count - 1);

          if (sylphRemoved.name != processingToken.sylphName) {
            throw new StarExcept("Parsing error: the sylph we just closed does not have the correct name:" + sylphRemoved.name + " vs " + processingToken.sylphName);
          }
          Log.Write("Closed sylph: " + sylphRemoved.name);
          Log.Write($"Length of opensylphs is now: {openSylphs.Count}");

          break;
        }

        //If we have characters to position on screen, then position them (and plot if actuallyChanging)
        if (actuallyChanging && y < H) {
          for (int i = 0; i < text_to_paint.Length; ++i) {
            if (x+i >= W) continue;
            cellData.SetCharacter(x+i,y, text_to_paint[i]);
            AssignPointToSylphs(x+i,y,openSylphs);
          }
        }
        x += text_to_paint.Length;
        Log.Write($"{x},{y}:+{text_to_paint}");

        //If we drew non-whitespace characters, then this could increase the largest X seen for determining the size needed for these contents.
        //Whitespace characters do not increase the needed size if they're at the end of a line.
        //Likewise sylph processing
        if (processingToken.OccupiesNonEmptySpace()) {
          largestX = Math.Max(x-1, largestX);       //Need the -1 here because the cursor jumps to 1 point beyond the end of the last thing written.
        }

        //Mark ourselves as being done with the token if we've finished with it.
        if (processingToken.doneWithThisToken) {
          processingToken = null;
        }
      }

      //Just to be safe, if we're actually changing our sylph xys then go through them and make sure that there aren't any invalid points in the sylphs.
      if (actuallyChanging && sylphs != null) {
        StarIntRect bounds = new StarIntRect(0,0,cellData.W, cellData.H);
        foreach (Sylph sy in sylphs) {
          sy.RemoveAnyPointsOutside(bounds);
        }
      }

      Log.Write("Reached the end of our layout.");

      //Return x+1,y+1 because we are dealing with widths and heights here.
      return (largestX+1,y+1);
    }

    //private void AssignPointToSylphs(int x, int y, List<Sylph> sylphList, bool isWhiteSpace) {
    private void AssignPointToSylphs(int x, int y, List<Sylph> sylphList) {
      foreach(Sylph s in sylphList) {
        s.AddXY(x,y);
      }
    }

    //Todo: Actually check to make sure that this code works!
    private void UpdateAllSylphs() {
      if (sylphs == null) return;
      if (sylphs.Count == 0) return;

      long currentPriority = ((long)int.MinValue)-1;
      long nextPriority = ((long)int.MaxValue)+1;

      do {
        nextPriority = ((long)int.MaxValue)+1;
        foreach (var sy in sylphs) {
          long prio = sy.Priority;
          if (prio == currentPriority) {
            sy.Update(cellData);
          }
          if (prio <= currentPriority) continue;
          nextPriority = Math.Min(nextPriority, prio);
        }
        currentPriority = nextPriority;
      } while(nextPriority < ((long)int.MaxValue)+1);

    }

    public override void UpdateLogic() {
      cellData.ClearOverrideData();
      UpdateAllSylphs();
    }

    public override void PaintToWithStyler(ASCIIGrid asciiGrid, int xorigin, int yorigin, StarIntRect canvas, WidgetStyler? styler) {
      if (!canvas.Exists()) return;
      StarIntRect intersection = GetContentsCanvasIntersection(xorigin, yorigin, canvas);
      if (!intersection.Exists()) return;

      var cellDataWH = GetCellDataWH();
      int cellDataW = cellDataWH.Item1;
      int cellDataH = cellDataWH.Item2;

      var typicalFG = GetFG(styler);
      var typicalBG = GetBG(styler);

      for (int yScreen = intersection.y0; yScreen <= intersection.y1; ++yScreen) {
        for (int xScreen = intersection.x0; xScreen <= intersection.x1; ++xScreen) {
          int xlocal = xScreen - xorigin;
          int ylocal = yScreen - yorigin;
          if (xlocal < 0 || ylocal < 0 || xlocal >= cellDataW || ylocal >= cellDataH) continue;

          char tileCharacter = cellData.GetCharacter(xlocal,ylocal);
          //SFML.Graphics.Color fg = cellData.GetOverrideFG(xlocal,ylocal) ?? testfg;
          SFML.Graphics.Color fg = cellData.GetOverrideFG(xlocal,ylocal) ?? typicalFG;
          asciiGrid.SetTile(xScreen, yScreen, tileCharacter);
          asciiGrid.SetFGColor(xScreen, yScreen, fg);
          //asciiGrid.SetBGColor(xScreen, yScreen, testbg);
          asciiGrid.SetBGColor(xScreen, yScreen, typicalBG);
        }
      }


    }

  }

}