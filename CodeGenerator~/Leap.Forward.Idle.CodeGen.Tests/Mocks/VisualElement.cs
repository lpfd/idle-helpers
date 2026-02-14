namespace UnityEngine.UIElements
{
    public class VisualElement
    {
        public string? name;

        public object? dataSource;

        public List<VisualElement> Children { get; } = new List<VisualElement>();

        public T? Q<T>(string name) where T : VisualElement, new()
        {
            foreach (var child in Children)
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