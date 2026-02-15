using System.Collections.Generic;

namespace UnityEngine.UIElements
{
    public class VisualElement
    {
        public string? name;

        public object? dataSource;

        public List<VisualElement> Children { get; } = new List<VisualElement>();
    }
}