using SFML.Graphics;

namespace Star {

  enum TEXID {
    ASCII,
    ORYX_ENV
  }

  static class TextureLibrary {
    static string texPath = "assets/textures/";

    private static Dictionary<TEXID,string>? texFilenames;

    private static Dictionary<TEXID,Texture>? textures;

    private static bool loaded = false;

    private static void AssignFileNames() {
      texFilenames = new Dictionary<TEXID, string>();
      texFilenames[TEXID.ASCII]    = "dosfont8.png";
      //texFilenames[TEXID.ASCII]    = "dosfont8x16.png";
      texFilenames[TEXID.ORYX_ENV] = "oryx_env_8x8.png";
    }

    public static void LoadIfNotLoaded() {
      if (loaded) return;
      LoadAll();
    }

    public static Texture GetTexture(TEXID texid) {
      if (textures == null) throw new Exception("Error: trying to gather textures but textures dict is null.");
      if (textures.ContainsKey(texid)) {
        return textures[texid];
      }
      throw new Exception($"Error: GetTexture does not contain key {texid}");
    }

    //Only call this once!
    private static void LoadAll() {

      //if (loaded) { throw new Exception("Error: Tried to load all the textures when they've already been loaded!"); }
      if (loaded) { throw new StarExcept("Error: Tried to load all the textures when they've already been loaded!"); }

      AssignFileNames();

      if (texFilenames == null) { throw new StarExcept("Error: Texture file name dictionary is not defined!"); }
      if (textures != null) { throw new StarExcept("Error: Tried to load textures when the textures are already loaded!"); }

      textures = new Dictionary<TEXID, Texture>();

      var texes = (TEXID[])Enum.GetValues(typeof(TEXID));
      foreach (var texid in texes) {
        string fname = texFilenames[texid] ?? "" ;

        if (fname == "") { throw new StarExcept($"Error: No filename found for texture {texid}"); }

        fname = texPath + fname;

        try {
          Texture t = new Texture(fname);
          textures[texid] = t;
          Log.Write($"Loaded texture {texid} with filename {texFilenames[texid]}.");
        } catch (SFML.LoadingFailedException e) {
          throw new StarExcept($"Texture loading error: failed to load file {fname}", e);
        }
        
      }

      loaded = true;
    }

    //Erases all of the loaded textures, then sets textures to null.
    //Call this when existing the program.
    public static void UnloadAll() {
      if (textures == null) return;

      foreach (var val in textures.Values) {
        val?.Dispose();
      }
      textures = null;
    }


  }

}