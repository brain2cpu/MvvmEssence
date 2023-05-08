namespace TestProject;

public class DiscoverComponentsTests
{
    [Fact]
    public void RegisterItems_AttributeOnlyConstructor()
    {
        bool singletonHandlerCalled = false;
        bool transientHandlerCalled = false;

        // Arrange
        var discoverComponents = new DiscoverComponents(GetType().Assembly);

        void singletonHandler(Type _) => singletonHandlerCalled = true;
        void transientHandler(Type _) => transientHandlerCalled = true;

        // Act
        discoverComponents.RegisterItems(singletonHandler, transientHandler);

        // Assert
        Assert.Equal(1, discoverComponents.Types.Count(x => x.isSingleton));
        Assert.Equal(1, discoverComponents.Types.Count(x => !x.isSingleton));
        Assert.True(singletonHandlerCalled);
        Assert.True(transientHandlerCalled);
    }

    [Fact]
    public void RegisterItems_StringFilterBasedConstructor_SingleNsFilter()
    {
        int singletonHandlerCalled = 0; 
        int transientHandlerCalled = 0;

        // Arrange
        var discoverComponents = new DiscoverComponents(GetType().Assembly, false, new []{ "TestProject.ClassesUsedInReflection.Ns1" });

        void SingletonHandler(Type _) => singletonHandlerCalled++;
        void TransientHandler(Type _) => transientHandlerCalled++;

        // Act
        discoverComponents.RegisterItems(SingletonHandler, TransientHandler);

        // Assert
        Assert.Equal(1, discoverComponents.Types.Count(x => x.isSingleton));
        Assert.Equal(2, discoverComponents.Types.Count(x => !x.isSingleton));
        Assert.Equal(1, singletonHandlerCalled);
        Assert.Equal(2, transientHandlerCalled);
    }

    [Fact]
    public void RegisterItems_StringFilterBasedConstructor_RecursiveNsFilter()
    {
        int singletonHandlerCalled = 0;
        int transientHandlerCalled = 0;

        // Arrange
        var discoverComponents = new DiscoverComponents(GetType().Assembly, false,
            new[] { "TestProject.ClassesUsedInReflection.Ns1.*" });

        void singletonHandler(Type _) => singletonHandlerCalled++;
        void transientHandler(Type _) => transientHandlerCalled++;

        // Act
        discoverComponents.RegisterItems(singletonHandler, transientHandler);

        // Assert
        Assert.Equal(1, discoverComponents.Types.Count(x => x.isSingleton));
        Assert.Equal(3, discoverComponents.Types.Count(x => !x.isSingleton));
        Assert.Equal(1, singletonHandlerCalled);
        Assert.Equal(3, transientHandlerCalled);
    }

    [Fact]
    public void RegisterItems_StringFilterBasedConstructor_MultiNsFilter()
    {
        int singletonHandlerCalled = 0;
        int transientHandlerCalled = 0;

        // Arrange
        var discoverComponents = new DiscoverComponents(GetType().Assembly, true,
            new[] { "TestProject.ClassesUsedInReflection.Ns1.*", "TestProject.ClassesUsedInReflection.Nsx" });

        void singletonHandler(Type _) => singletonHandlerCalled++;
        void transientHandler(Type _) => transientHandlerCalled++;

        // Act
        discoverComponents.RegisterItems(singletonHandler, transientHandler);

        // Assert
        Assert.Equal(4, discoverComponents.Types.Count(x => x.isSingleton));
        Assert.Equal(1, discoverComponents.Types.Count(x => !x.isSingleton));
        Assert.Equal(4, singletonHandlerCalled);
        Assert.Equal(1, transientHandlerCalled);
    }

    [Fact]
    public void RegisterItems_StringFilterBasedConstructor_RecursiveNsFilterAndNameSuffix()
    {
        int singletonHandlerCalled = 0;
        int transientHandlerCalled = 0;

        // Arrange
        var discoverComponents = new DiscoverComponents(GetType().Assembly, false,
            new[] { "TestProject.ClassesUsedInReflection.Ns1.*" }, new[] { "1" });

        void singletonHandler(Type _) => singletonHandlerCalled++;
        void transientHandler(Type _) => transientHandlerCalled++;

        // Act
        discoverComponents.RegisterItems(singletonHandler, transientHandler);

        // Assert
        Assert.Equal(1, discoverComponents.Types.Count(x => x.isSingleton));
        Assert.Equal(2, discoverComponents.Types.Count(x => !x.isSingleton));
        Assert.Equal(1, singletonHandlerCalled);
        Assert.Equal(2, transientHandlerCalled);
    }
}