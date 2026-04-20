using UnityEngine.UIElements;

namespace TnieYuPackage.Utils
{
    /// <summary>
    /// Base Element with style follow unity using Prefix of element and part name, which is "prefix__part".
    /// This is a common naming convention in Unity's UI Toolkit to allow for clear and organized styling of UI components.
    /// By using a consistent prefix, you can easily target specific elements in your USS (Unity Style Sheets) and maintain a clean structure for your UI styles.
    /// </summary>
    public abstract class BaseElement : VisualElement
    {
        /// <summary>
        /// Prefix element name
        /// </summary>
        protected abstract string Prefix { get; }

        /// <summary>
        /// Get actual style name for part.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public string GetStyleName(string part)
        {
            return $"{Prefix}__{part}";
        }
    }
}