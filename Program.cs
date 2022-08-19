using System.Reflection;
using ReflectionSample;

Console.Title = "Learning Reflection";

var iocContainer = new IoCContainer();
iocContainer.Register<IoCExampleClasses.IWaterService, IoCExampleClasses.TapWaterService>();
// var waterService = iocContainer.Resolve<IoCExampleClasses.IWaterService>();
// iocContainer.Register<IoCExampleClasses.IBeanService<IoCExampleClasses.Catimor>, IoCExampleClasses.ArabicaBeanService<IoCExampleClasses.Catimor>>();
iocContainer.Register(typeof(IoCExampleClasses.IBeanService<>), typeof(IoCExampleClasses.ArabicaBeanService<>));
iocContainer.Register<IoCExampleClasses.ICoffeeService, IoCExampleClasses.CoffeeService>();

var coffeeService = iocContainer.Resolve<IoCExampleClasses.ICoffeeService>();

static void CodeFromFourthModule()
{
    //List`1: signifies that we only have one generic type parameter
    var myList = new List<Person>();
    Console.WriteLine(myList.GetType().Name);

    var myDictionary = new Dictionary<string, int>();
    Console.WriteLine(myDictionary.GetType());

    //Get type argument (key, value) of generic type
    //It's is a generic type combined with generic type arguments, which are types in their own right
    //This is called a closed generic type, which that type parameters are known (string and int)
    var dictionaryType = myDictionary.GetType();
    foreach (var genericTypeArgument in dictionaryType.GenericTypeArguments)
    {
        Console.WriteLine(genericTypeArgument);
    }

    foreach (var genericArgument in dictionaryType.GetGenericArguments())
    {
        Console.WriteLine(genericArgument);
    }

    //This is called an unbound generic
    var openDictionaryType = typeof(Dictionary<,>);
    //Get type arguments of unbound generic. In this example the value is TKey, TValue
    foreach (var genericTypeArgument in openDictionaryType.GenericTypeArguments)
    {
        Console.WriteLine(genericTypeArgument);
    }

    var createdInstance = Activator.CreateInstance(typeof(List<Person>));
    Console.WriteLine(createdInstance?.GetType());

    //This is an open generic type
    var openResultType = typeof(Result<>);
    
    //MakeGenericType allows converting open types with type parameters that are not specified to closed types by specifying the type argument
    //Convert to a closed generic type, and from MakeGenericType we pass through a parameter list of types
    //those are the types of the generic type arguments(type is Person) 
    var closedResultType = openResultType.MakeGenericType(typeof(Person));
    var createdResult = Activator.CreateInstance(closedResultType);
    Console.WriteLine(createdResult?.GetType());

    //get the open generic type by the name of the types as string
    // var openResultType = Type.GetType("ReflectionSample.Result`1");
    // var closedResultType = openResultType.MakeGenericType(Type.GetType("ReflectionSample.Person"));
    // var createdResult = Activator.CreateInstance(closedResultType);
    // Console.WriteLine(createdResult.GetType());

    //How to invoke generic methods

    //Find method by name 
    var methodInfo = closedResultType.GetMethod("AlterAndReturnValue");
    Console.WriteLine(methodInfo);
    //Make type argument as generic(Employee) before invoking generic method with the argument
    var genericMethodInfo = methodInfo.MakeGenericMethod(typeof(Employee));
    genericMethodInfo.Invoke(createdResult, new object[] { new Employee() });
}

static void NetorkMonitorExample()
{
    NetworkMonitor.BootstrapFromConfiguration();

    Console.WriteLine("Monitoring network... something went wrong.");

    NetworkMonitor.Warn();
}

static void CodeFromThirdModule()
{
    //Get member with late binding with specific Type
    var personType = typeof(Person);
    var personConstructors =
        personType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    foreach (var personConstructor in personConstructors)
    {
        Console.WriteLine(personConstructor);
    }

    var privatePersonConstructor = personType.GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic,
        null,
        new Type[] { typeof(string), typeof(int) },
        null);

    var person1 = personConstructors[0].Invoke(null);
    var person2 = personConstructors[1].Invoke(new object[] { "Quan" });
    var person3 = personConstructors[2].Invoke(new object[] { "Quan", 27 });

    var person4 = Activator.CreateInstance("ReflectionSample", "ReflectionSample.Person").Unwrap();

    var person5 = Activator.CreateInstance(
        "ReflectionSample",
        "ReflectionSample.Person",
        true,
        BindingFlags.Instance | BindingFlags.Public,
        null,
        new object[] { "Quan" },
        null,
        null);

    var personTypeFromString = Type.GetType("ReflectionSample.Person");
    var person6 = Activator.CreateInstance(personTypeFromString, new object[] { "Quan" });

    var person7 = Activator.CreateInstance(
        "ReflectionSample",
        "ReflectionSample.Person",
        true,
        BindingFlags.Instance | BindingFlags.NonPublic,
        null,
        new object[] { "Quan", 27 },
        null,
        null);

    var assembly = Assembly.GetExecutingAssembly();
    var person8 = assembly.CreateInstance("ReflectionSample.Person");

//create a new instance of a configured type 
    var actualTypeFromConfiguration = Type.GetType(GetTypeFromConfiguraion());
    var iTalkInstance = Activator.CreateInstance(actualTypeFromConfiguration) as ITalk;
    iTalkInstance.Talk("Hello world");

//working with object via dynamic
    dynamic dynamicITalkInstance = Activator.CreateInstance(actualTypeFromConfiguration);
    dynamicITalkInstance.Talk("Hello world!");

    var personForManipulation = Activator.CreateInstance(
        "ReflectionSample",
        "ReflectionSample.Person",
        true,
        BindingFlags.Instance | BindingFlags.NonPublic,
        null,
        new object[] { "Quan", 27 },
        null,
        null)?.Unwrap();

    var nameProperty = personForManipulation?.GetType().GetProperty("Name");
    nameProperty?.SetValue(personForManipulation, "Sven");

    var ageField = personForManipulation?.GetType().GetField("age");
    ageField?.SetValue(personForManipulation, 36);

    var privateField = personForManipulation?.GetType()
        .GetField("_aPrivateField", BindingFlags.Instance | BindingFlags.NonPublic);
    privateField?.SetValue(personForManipulation, "update private field value");

    personForManipulation?.GetType().InvokeMember("Name",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
        null, personForManipulation, new[] { "Emma" });

    personForManipulation?.GetType().InvokeMember("_aPrivateField",
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField,
        null, personForManipulation, new[] { "second update for private field value" });

    var talkMethod = personForManipulation?.GetType().GetMethod("Talk");
    talkMethod.Invoke(personForManipulation, new[] { "some thing to say" });

    personForManipulation?.GetType().InvokeMember("Yell",
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
        null, personForManipulation, new[] { "something to yell" });


    static string GetTypeFromConfiguraion()
    {
        return "ReflectionSample.Alien";
    }
}

static void CodeFromSecondModule()
{
// string name = "Quan";
// var stringType = name.GetType();
// Console.WriteLine(stringType);

    var currentAssembly = Assembly.GetExecutingAssembly();
// var typesFromCurrentAssembly = currentAssembly.GetTypes();
// foreach (var type in typesFromCurrentAssembly)
// {
//     Console.WriteLine(type.Name);
// }
//Load specific type from current assembly
    var oneTypeFromCurrentAssembly = currentAssembly.GetType("ReflectionSample.Person");

// //Load external assembly
// var externalAssembly = Assembly.Load("System.Text.Json");
// var typeFromExternalAssembly = externalAssembly.GetTypes();
//
// //Load modules
// var modulesFromExternalAssembly = externalAssembly.GetModules();
// var oneModuleFromExternalAssembly = externalAssembly.GetModule("System.Text.Json.dll");
//
// var typesFromModuleFromExternalAssembly = oneModuleFromExternalAssembly.GetTypes();
// var oneTypesFromModuleFromExternalAssembly = oneModuleFromExternalAssembly.GetType("System.Text.Json.JsonProperty");

    foreach (var constructor in oneTypeFromCurrentAssembly.GetConstructors())
    {
        Console.WriteLine(constructor);
    }

    foreach (var method in oneTypeFromCurrentAssembly.GetMethods(BindingFlags.Instance | BindingFlags.Public |
                                                                 BindingFlags.NonPublic))
    {
        Console.WriteLine($"{method}, public: {method.IsPublic}");
    }

    foreach (var field in oneTypeFromCurrentAssembly.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
    {
        Console.WriteLine(field);
    }
}

Console.ReadLine();