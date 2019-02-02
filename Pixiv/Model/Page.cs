using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace meguca.Pixiv.Model {
  class Page {
    #region Properties
    public string Token;
    public Services Services;
    public string OneSignalAppId;
    public string PublicPath;
    public string CommonResourcePath;
    public bool Development;
    public UserData UserData;
    public Premium Premium;
    public Preload Preload;
    public Object Mute;
    #endregion

    public long Id {
      get {
        return Preload.Illustration.Values.FirstOrDefault().IllustID;
      }
    }

    public Illustration Illustration {
      get {
        return Preload.Illustration.Values.FirstOrDefault();
      }
    }
 
  }
}
