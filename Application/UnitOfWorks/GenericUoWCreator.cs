namespace Application.UnitOfWorks;

public class GenericUoWCreator(IServiceProvider serviceProvider)
{
    public UnitOfWorks.GenericUoW Create(bool enableTrace)
    {
        return new GenericUoW(serviceProvider, enableTrace);
    }
}