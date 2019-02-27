using System.Collections;
using System.Collections.Generic;

public class Weapon
{
    public string name;
    public string prefabPath;
    public string type;
    public string category;
    public string thumbnailPath;

    public Weapon(string name, string type, string category, string prefabPath, string thumbnailPath) {
        this.name = name;
        this.type = type;
        this.category = category;
        this.prefabPath = prefabPath;
        this.thumbnailPath = thumbnailPath;
    }

}
