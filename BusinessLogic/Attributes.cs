namespace BusinessLogic;

[AttributeUsage(AttributeTargets.Class)]
public class TransientServiceAttribute : Attribute;
    
[AttributeUsage(AttributeTargets.Class)]
public class ScopedServiceAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class SingletonServiceAttribute : Attribute;