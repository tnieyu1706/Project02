using EditorAttributes;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Game.Td
{
    public abstract class TdObjectRuntime<TPreset> : MonoBehaviour
        where TPreset : TdObjectPresetSo
    {
        [SerializeField, ReadOnly] public TPreset currentPreset;
        [SerializeField, Self] protected Animator animator;
        [SerializeField, Child] protected SpriteLibrary spriteLibrary;

        protected virtual void SetPreset(TPreset preset)
        {
            if (currentPreset is not null)
            {
                currentPreset.configurators.ForEach(conf => conf.UnConfig(gameObject));
                currentPreset.behaviourInstaller?.UnInstall(gameObject);
            }

            currentPreset = preset;

            SetPresetProperties(currentPreset);

            currentPreset.behaviourInstaller?.Install(gameObject);
            currentPreset.configurators.ForEach(conf => conf.Config(gameObject));
        }

        protected virtual void SetPresetProperties(TPreset preset)
        {
            spriteLibrary.spriteLibraryAsset = preset.libraryAsset;
        }
    }
}