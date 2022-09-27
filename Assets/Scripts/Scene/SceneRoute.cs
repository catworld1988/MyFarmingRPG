using System.Collections.Generic;

//场景路线
[System.Serializable]
public class SceneRoute
{
    public SceneName fromScenenName;
    public SceneName toScenenName;
    public List<ScenePath> scenePathList;

}