namespace UnityEngine.UIElements
{
    public static class UQueryExtensions
    {
        public static T? Q<T>(this VisualElement element, string name) where T : VisualElement, new()
        {
            foreach (var child in element.Children)
            {
                if (child.name == name && child is T tChild)
                {
                    return tChild;
                }
            }
            return null;
        }
    }
}