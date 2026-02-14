using Leap.Forward.IdleHelpers;
using System.ComponentModel;

[IdleViewModel]
public partial class RootNamespaceViewModel : INotifyPropertyChanged
{
    [IdleProperty]
    private int _field1;

    [IdleProperty]
    private int _field2;

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