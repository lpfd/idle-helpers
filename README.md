# Leap.Forward.Idle.CodeGen Source Generator

This package provides a Roslyn-based **Incremental Source Generator** designed to eliminate boilerplate code in Unity UI Toolkit ViewModels. It automates property change notification, UI binding, and reactive dependency tracking.

---

## Features

* **Observable Properties**: Automatically wraps fields into properties with change notification.
* **Automatic UI Binding**: Generates a `BindTo(UIDocument)` method to link your VM to a UI Document and its `dataSource`.
* **Event Auto-Subscription**: Connects methods to UI Toolkit `Button` click events via attributes.
* **Reactive Updaters**: Analyzes method bodies to automatically re-run "updater" methods when their dependent properties change.

---

## Usage Instructions

### 1. ViewModel Definition

To enable generation, mark your class with `[IdleViewModel]`. The class **must** be partial, implement `INotifyPropertyChanged`, and contain an `OnPropertyChanged` method.

```csharp
[IdleViewModel]
public partial class PlayerViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

```

### 2. Properties (`[IdleProperty]`)

Apply this to `private` or `protected` fields. The generator will create a PascalCase property and call `OnPropertyChanged`.

* **Prefix Trimming**: Automatically removes `_` and `m_` prefixes.
* **Unity Compatibility**: Adds `[CreateProperty]` to the generated property for UI Toolkit binding compatibility.

```csharp
[IdleProperty] 
private int _gold; // Generates property 'Gold'

```

### 3. UI Binding (`BindTo`)

The generator creates a `public void BindTo(UIDocument document)` method.

* It sets `root.dataSource = this`.
* It searches for buttons defined by `[ClickHandler]`.

### 4. Click Handlers (`[ClickHandler]`)

Mark any method with this attribute to automatically subscribe it to a `Button` in the UI Document.

```csharp
[ClickHandler("BuyButton")]
private void HandlePurchase() 
{
    Debug.Log("Item bought!");
}

```

### 5. Reactive Updaters (`[PropertyUpdater]`)

This is the most powerful feature. If you mark a method with `[PropertyUpdater]`, the generator inspects the code inside that method to see which `IdleProperty` fields or properties you are reading. It then **injects** a call to this method inside the setters of those properties.

```csharp
[PropertyUpdater]
private void UpdateTaxedGold()
{
    // The generator detects 'Gold' usage here.
    // Whenever 'Gold' changes, 'UpdateTaxedGold' is automatically called.
    TaxedGold = Gold * 0.8f;
}

```

---

## Generated Code Example

If you define `_gold` and `UpdateTaxedGold`, the generator produces:

```csharp
public float Gold 
{
    get => _gold;
    set 
    {
        if (!EqualityComparer<float>.Default.Equals(_gold, value)) 
        {
            _gold = value;
            OnPropertyChanged(nameof(Gold));
            UpdateTaxedGold(); // Injected dependency!
        }
    }
}

```

---

## Requirements & Limitations

* **Partial Classes**: Every ViewModel class must use the `partial` keyword.
* **Field Access**: Fields marked with `[IdleProperty]` **must not** be public (the generator will throw a compilation error).
* **Manual Call**: You must call `BindTo(yourDocument)` manually (usually in `OnEnable` or `Start`).
* **Dependency Limits**: `[PropertyUpdater]` only tracks properties within the same class marked with `[IdleProperty]`.

**Would you like me to generate a C# script for the Attribute definitions to ensure they match the generator's expectations exactly?**