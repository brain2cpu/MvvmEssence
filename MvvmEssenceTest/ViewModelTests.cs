namespace TestProject;

public class ViewModelTests
{
    [Fact]
    public void ChangeNameCommand_Invoked_with_null_without_exception_handler_Throws_exception()
    {
        // arrange
        var initialValue = "Initial value";

        var vm = new ViewModel
        {
            Name = initialValue
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

        var vm = new ViewModel
        {
            Name = initialValue,
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

        var vm = new ViewModel
        {
            Name = initialValue
        };

        // act
        vm.ChangeNameCommand.Execute(finalValue);

        // assert
        Assert.Equal(finalValue, vm.Name);
        Assert.False(vm.IsBusy);
    }
}