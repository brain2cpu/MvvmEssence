using System.Collections.Specialized;
using System.ComponentModel;

namespace MvvmEssenceTest;

public class ObservableCollectionExTests
{
    public class TestItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void TriggerChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Fact]
    public void AddRange_NullInput_ShouldNotThrow()
    {
        var collection = new ObservableCollectionEx<TestItem>();
        collection.AddRange(null);
        Assert.Empty(collection);
    }

    [Fact]
    public void RemoveRange_NullInput_ShouldNotThrow()
    {
        var collection = new ObservableCollectionEx<TestItem>();
        collection.RemoveRange(null);
        Assert.Empty(collection);
    }

    [Fact]
    public void AddRange_EmptyList_ShouldNotRaiseEvent()
    {
        var collection = new ObservableCollectionEx<TestItem>();
        bool eventRaised = false;

        collection.CollectionChanged += (_, _) => eventRaised = true;
        collection.AddRange(new List<TestItem>());

        Assert.False(eventRaised);
        Assert.Empty(collection);
    }

    [Fact]
    public void RemoveRange_EmptyList_ShouldNotRaiseEvent()
    {
        var collection = new ObservableCollectionEx<TestItem>();
        bool eventRaised = false;

        collection.CollectionChanged += (_, _) => eventRaised = true;
        collection.RemoveRange(new List<TestItem>());

        Assert.False(eventRaised);
    }

    [Fact]
    public void AddRange_WithDuplicates_ShouldAddAll()
    {
        var item = new TestItem();
        var collection = new ObservableCollectionEx<TestItem>();
        collection.AddRange(new List<TestItem> { item, item });

        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void PropertyChanged_WhileSuppressed_ShouldNotRaiseEventImmediately()
    {
        var item = new TestItem();
        var collection = new ObservableCollectionEx<TestItem> { item };
        bool eventRaised = false;

        collection.CollectionChanged += (_, _) => eventRaised = true;
        collection.SuppressNotification = true;
        item.TriggerChange("Test");
        collection.Add(new TestItem());

        Assert.False(eventRaised);
    }

    [Fact]
    public void PropertyChanged_ShouldTriggerReplaceOrToReset()
    {
        var item = new TestItem();
        var collection = new ObservableCollectionEx<TestItem> { item };
        NotifyCollectionChangedEventArgs? args = null;

        collection.CollectionChanged += (_, e) => args = e;

        item.TriggerChange("Test");

        Assert.NotNull(args);
        Assert.True(args.Action == NotifyCollectionChangedAction.Replace || args.Action == NotifyCollectionChangedAction.Reset);
    }

    [Fact]
    public void SuppressNotification_ToggleMultipleTimes_ShouldBehaveCorrectly()
    {
        var item = new TestItem();
        var collection = new ObservableCollectionEx<TestItem>();
        NotifyCollectionChangedEventArgs? args = null;

        collection.CollectionChanged += (_, e) => args = e;

        collection.SuppressNotification = true;
        collection.Add(item);
        collection.SuppressNotification = false;
        collection.SuppressNotification = true;
        item.TriggerChange("Test");
        collection.SuppressNotification = false;

        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
    }

    [Fact]
    public void PropertyChanged_AddRange_ShouldRaiseEventOnce()
    {
        var collection = new ObservableCollectionEx<TestItem>();
        var eventRaisedCount = 0;

        collection.CollectionChanged += (_, _) => eventRaisedCount++;
        collection.AddRange([new TestItem(), new TestItem(), new TestItem(), new TestItem()]);

        Assert.Equal(1, eventRaisedCount);
        Assert.Equal(4, collection.Count);
    }

    [Fact]
    public void PropertyChanged_RemoveRange_ShouldRaiseEventOnce()
    {
        var i1 = new TestItem();
        var i2 = new TestItem();
        var i3 = new TestItem();
        var i4 = new TestItem();
        var collection = new ObservableCollectionEx<TestItem>([i1, i2, i3, i4]);
        var eventRaisedCount = 0;

        collection.CollectionChanged += (_, _) => eventRaisedCount++;
        collection.RemoveRange([i1, i2, i3]);

        Assert.Equal(1, eventRaisedCount);
        Assert.Single(collection);
    }

    [Fact]
    public void PropertyChanged_Add_ShouldRaiseEventMultipleTimes()
    {
        var i1 = new TestItem();
        var i2 = new TestItem();
        var i3 = new TestItem();
        var i4 = new TestItem();
        var collection = new ObservableCollectionEx<TestItem>();
        var eventRaisedCount = 0;

        collection.CollectionChanged += (_, _) => eventRaisedCount++;
        collection.Add(i1);
        collection.Add(i2);
        collection.Add(i3);
        collection.Add(i4);
        collection.Remove(i1);
        collection.Remove(i2);

        Assert.Equal(6, eventRaisedCount);
        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void PropertyChanged_AfterRemoved_ShouldNotRaiseEvent()
    {
        var i1 = new TestItem();
        var i2 = new TestItem();
        var collection = new ObservableCollectionEx<TestItem> { i1, i2 };
        collection.Remove(i1);
        
        bool eventRaised = false;
        collection.CollectionChanged += (_, _) => eventRaised = true;
        
        i1.TriggerChange("Test");

        Assert.False(eventRaised);
    }
}
