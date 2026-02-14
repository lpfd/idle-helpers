using System.ComponentModel;
using UnityEngine.UIElements;

namespace Leap.Forward.Idle.CodeGen.Tests
{
    public class Tests
    {
        [Test]
        public void BindSampleViewModel()
        {
            var vm = new SampleViewModel();
            var doc = new UIDocument();
            var button = new Button() { name = "MyButton" };
            doc.rootVisualElement.Children.Add(button);
            vm.BindTo(doc);

            button.RaiseClickedEvent();

            var collectedArgs = new List<PropertyChangedEventArgs>();
            vm.PropertyChanged += (sender, args) =>
            {
                collectedArgs.Add(args);
            };
            vm.Field1 = 10;
            vm.Field1 = 10;
            Assert.That(2 == collectedArgs.Count);
            Assert.That("Field1" == collectedArgs[0].PropertyName);
            Assert.That("Field2" == collectedArgs[1].PropertyName);
            Assert.That(20 == vm.Field2);
        }
    }
}
