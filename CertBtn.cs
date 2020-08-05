using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Cert Button", fileName = "certBtn")]
public class CertBtn : ScriptableObject
{
    public new string name;
    public string certPath;
    public int btnId;
    public GameObject btn;
    public Texture2D tex;
}
