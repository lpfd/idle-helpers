using Leap.Forward.IdleHelpers;
using System;
using System.ComponentModel;

namespace Leap.Forward.Idle.CodeGen.Tests
{
    [IdleViewModel]
    public partial class SampleViewModel : INotifyPropertyChanged
    {
        [IdleProperty]
        private int _field1;

        [IdleProperty]
        private int _field2;

        [IdleProperty]
        private TestStruct _structField;

        [IdleProperty]
        private int _buttonClickCounter;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [PropertyUpdater]
        public void UpdateField2()
        {
            Field2 = Field1 * 2;
        }

        [ClickHandler("MyButton")]
        public void OnMyButtonClick()
        {
            ButtonClickCounter += 1;
        }
    }

    public struct TestStruct : IEquatable<TestStruct>
    {
        public int Field;

        public override bool Equals(object? obj)
        {
            return obj is TestStruct @struct && Equals(@struct);
        }

        public bool Equals(TestStruct other)
        {
            return Field == other.Field;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Field);
        }

        public static bool operator ==(TestStruct left, TestStruct right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TestStruct left, TestStruct right)
        {
            return !(left == right);
        }
    }
}
