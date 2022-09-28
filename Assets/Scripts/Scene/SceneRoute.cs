using System.Collections.Generic;

//场景路线
[System.Serializable]
public class SceneRoute
{
    public SceneName fromSceneName;
    public SceneName toSceneName;
    public List<ScenePath> scenePathList;

}