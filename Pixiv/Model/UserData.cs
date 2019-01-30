using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace meguca.Pixiv.Model {
  class UserData {
    public long ID;
    public string Name;
    public string ProfileImg;
    public bool Premium;
    public int XRestrict;
    public bool Adult;
  }
}
