using System.Collections.Generic;
using UnityEngine;

namespace TnieYuPackage.Utils.Components
{
    public class HideObjectSceneManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> objects;
        [SerializeField] private List<Behaviour> behaviours;

        public void Start()
        {
            foreach (var obj in objects)
            {
                if (!obj.activeSelf) obj.SetActive(true);
            }
            foreach (var behaviour in behaviours)
            {
                if (!behaviour.enabled) behaviour.enabled = true;
            }

            objects.ForEach(o => o.SetActive(false));
            behaviours.ForEach(b => b.enabled = false);
        }
    }
}