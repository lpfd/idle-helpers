using System;

namespace Leap.Forward.IdleHelpers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClickHandlerAttribute : System.Attribute
    {
        private string _elementName;
        public ClickHandlerAttribute(string elementName)
        {
            _elementName = elementName;
        }

        public string ElementName => _elementName;
    }
}