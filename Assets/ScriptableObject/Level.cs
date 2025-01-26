using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Game Scene/Level")]
public class Level : ScriptableObject
{
    public int level;
    public string[] sublevels;
    public int tag;
    public bool collected;
}
