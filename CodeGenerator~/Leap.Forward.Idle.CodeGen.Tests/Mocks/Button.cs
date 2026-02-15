using System;

namespace UnityEngine.UIElements
{
    public class Button : VisualElement
    {
        public event Action? clicked;

        internal void RaiseClickedEvent()
        {
            clicked?.Invoke();
        }
    }
}