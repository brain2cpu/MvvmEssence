namespace MvvmEssenceTest;

public class ViewModelTests
{
    [Fact]
    public void ChangeNameCommand_Invoked_with_null_without_exception_handler_Throws_exception()
    {
        // arrange
        var initialValue = "Initial value";

        var vm = new TestViewModel
        {
            LastName = initialValue
        };

        // act
        // assert
        Assert.Throws<NullReferenceException>(() => vm.ChangeNameCommand.Execute(null));

        Assert.False(vm.IsBusy);
    } 
    
    [Fact]
    public void ChangeNameCommand_Invoked_with_null_with_exception_handler_Calls_the_handler()
    {
        // arrange
        var initialValue = "Initial value";
        Exception? xcp = null;

        var vm = new TestViewModel
        {
            LastName = initialValue,
            ExceptionHandler = exception => xcp = exception
        };

        // act
        vm.ChangeNameCommand.Execute(null);

        // assert
        Assert.NotNull(xcp);
        Assert.False(vm.IsBusy);
    } 
        
    [Fact]
    public void ChangeNameCommand_Invoked_with_valid_string_Sets_new_name()
    {
        // arrange
        var initialValue = "Initial value";
        var finalValue = "New value";

        var vm = new TestViewModel
        {
            LastName = initialValue
        };

        // act
        vm.ChangeNameCommand.Execute(finalValue);

        // assert
        Assert.Equal(finalValue, vm.LastName);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public void Set_NullableToNull_ShouldWork()
    {
        // arrange
        var vm = new TestViewModel();
        var oSt = vm.Status;

        // act
        vm.Status = null;

        // assert
        Assert.NotEmpty(oSt);
        Assert.True(vm.IsChanged);
        Assert.Null(vm.Status);
    }

    [Fact]
    public void Set_NonNullableToNull_ShouldWork()
    {
        // arrange
        var vm = new TestViewModel();
        var oSt = vm.LastName;

        // act
        vm.LastName = null;

        // assert
        Assert.Empty(oSt);
        Assert.True(vm.IsChanged);
        Assert.Null(vm.LastName);
    }
}