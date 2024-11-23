using Services;
using UnityEngine;

public class MateriaRegister : MonoBehaviour
{
    public ScoreManager scoreManager;
    private void Awake()
    {
        scoreManager = ServiceLocator.Get<ScoreManager>();
        scoreManager.AddMaterial(GetComponent<MeshRenderer>().material);
    }
}