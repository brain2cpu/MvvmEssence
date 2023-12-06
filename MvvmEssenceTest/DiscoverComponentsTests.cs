namespace MvvmEssenceTest;

public class DiscoverComponentsTests
{
    [Fact]
    public void RegisterItems_AttributeOnlyConstructor()
    {
        var singletonHandlerCalled = false;
        var singletonDirectHandlerCalled = false;
        var transientHandlerCalled = false;
        var transientDirectHandlerCalled = false;

        // Arrange
        var discoverComponents = new DiscoverComponents(GetType().Assembly);

        void SingletonHandler(Type _, Type _1) => singletonHandlerCalled = true;
        void SingletonDirectHandler(Type _) => singletonDirectHandlerCalled = true;
        void TransientHandler(Type _, Type _1) => transientHandlerCalled = true;
        void TransientDirectHandler(Type _) => transientDirectHandlerCalled = true;

        // Act
        discoverComponents.RegisterItems(SingletonDirectHandler, TransientDirectHandler, SingletonHandler, TransientHandler);

        // Assert
        Assert.Equal(1, discoverComponents.Types.Count(x => x.isSingleton));
        Assert.Equal(1, discoverComponents.Types.Count(x => !x.isSingleton));
        Assert.True(singletonDirectHandlerCalled);
        Assert.False(singletonHandlerCalled);
        Assert.True(transientDirectHandlerCalled);
        Assert.False(transientHandlerCalled);
    }

    [Fact]
    public void RegisterItems_FilterBasedConstructor_UseInterfaces()
    {
        var singletonDirectHandlerCalled = 0; 
        var singletonHandlerCalled = 0; 
        var transientDirectHandlerCalled = 0;
        var transientHandlerCalled = 0;

        // Arrange
        var discoverComponents = new DiscoverComponents(GetType().Assembly, type =>
        {
            if (type.Namespace is not ("TestProject.ClassesUsedInReflection.Ns1" or "TestProject.ClassesUsedInReflection.Nsx"))
                return ClassRegistrationOption.Skip;

            if (!type.Name.EndsWith("1"))
                return ClassRegistrationOption.Skip;

            // default mode
            return ClassRegistrationOption.AsTransient;
        });

        void SingletonDirectHandler(Type _) => singletonDirectHandlerCalled++;
        void SingletonHandler(Type _, Type _1) => singletonHandlerCalled++;
        void TransientDirectHandler(Type _) => transientDirectHandlerCalled++;
        void TransientHandler(Type _, Type _1) => transientHandlerCalled++;

        // Act
        discoverComponents.RegisterItems(SingletonDirectHandler, TransientDirectHandler, SingletonHandler, TransientHandler);

        // Assert
        Assert.Equal(1, discoverComponents.Types.Count(x => x.isSingleton));
        Assert.Equal(2, discoverComponents.Types.Count(x => !x.isSingleton));
        Assert.Equal(1, singletonDirectHandlerCalled);
        Assert.Equal(0, singletonHandlerCalled);
        Assert.Equal(1, transientDirectHandlerCalled);
        Assert.Equal(1, transientHandlerCalled);
    }

    [Fact]
    public void RegisterItems_FilterBasedConstructor_IgnoreInterfaces()
    {
        var singletonDirectHandlerCalled = 0; 
        var transientDirectHandlerCalled = 0;

        // Arrange
        var discoverComponents = new DiscoverComponents(GetType().Assembly, type =>
        {
            if (type.Namespace is not ("TestProject.ClassesUsedInReflection.Ns1" or "TestProject.ClassesUsedInReflection.Nsx"))
                return ClassRegistrationOption.Skip;

            if (!type.Name.EndsWith("1"))
                return ClassRegistrationOption.Skip;

            // default mode
            return ClassRegistrationOption.AsTransient;
        });

        void SingletonDirectHandler(Type _) => singletonDirectHandlerCalled++;
        void TransientDirectHandler(Type _) => transientDirectHandlerCalled++;

        // Act
        discoverComponents.RegisterItems(SingletonDirectHandler, TransientDirectHandler);

        // Assert
        Assert.Equal(1, discoverComponents.Types.Count(x => x.isSingleton));
        Assert.Equal(2, discoverComponents.Types.Count(x => !x.isSingleton));
        Assert.Equal(1, singletonDirectHandlerCalled);
        Assert.Equal(2, transientDirectHandlerCalled);
    }
 }