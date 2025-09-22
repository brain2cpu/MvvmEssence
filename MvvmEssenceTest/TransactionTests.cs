namespace MvvmEssenceTest;

public class TransactionTests
{
    [Fact]
    public void Commit_ShouldWork()
    {
        var vm = new TestViewModel();

        Assert.False(vm.Transaction.IsTransactionStarted);
        Assert.False(vm.Transaction.CanUndo);
        
        vm.Transaction.Begin();

        Assert.False(vm.Transaction.CanUndo);
        Assert.True(vm.Transaction.IsTransactionStarted);

        vm.LastName = "Test1";
        Assert.True(vm.Transaction.CanUndo);
        vm.Status = "in tran";

        vm.Transaction.Commit();
        
        Assert.False(vm.Transaction.CanUndo);
        Assert.False(vm.Transaction.IsTransactionStarted);
        Assert.Equal("Test1", vm.LastName);
        Assert.Equal("in tran", vm.Status);
    }

    [Fact]
    public void Undo_ShouldWork()
    {
        var vm = new TestViewModel();

        Assert.False(vm.Transaction.IsTransactionStarted);
        Assert.False(vm.Transaction.CanUndo);
        
        vm.Transaction.Begin();

        Assert.False(vm.Transaction.CanUndo);
        Assert.True(vm.Transaction.IsTransactionStarted);

        vm.LastName = "Test1";
        Assert.True(vm.Transaction.CanUndo);
        vm.Status = "in tran";

        vm.Transaction.Undo();
        
        Assert.False(vm.Transaction.CanUndo);
        Assert.False(vm.Transaction.IsTransactionStarted);
        Assert.Empty(vm.LastName);
        Assert.Equal("starting...", vm.Status);
    }
}
