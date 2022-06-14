UniTaskAnalyzer for Unity
===
in the case of a synchronous call to an asynchronous method on an object by interface type, the standard code analyzer does not highlight the construct and does not suggest a fix, unlike a call by class type (warning CS4014). UniTaskAnalyzer handles this situation and suggest to insert `await` operator or `Forget()` invocation.

Works both in Rider or VS

Getting started
---
Install via asset package available on [releases](https://github.com/Yggdrasills/UniTaskAnalyzer/releases) page.

Example:

```csharp
    private void Awake()
    {
          IBar bar = new Bar();

          // <!-- UniTaskVoid
          bar.Foo(); // UniTaskAnalyzer will give a warning. Suggest to add await operator
          var fooUniTaskVoid = bar.Foo(); // won't give a warning
          bar.Foo().Forget(); // won't give a warning
          // -->
          
          // <!-- UniTask
          bar.Foo2(); // UniTaskAnalyzer will give a warning. Suggest to add await operator or invoke Forget()
          bar.Foo2().Forget(); // won't give a warning
          var fooUniTask = bar.Foo2(); // won't give a warning
          await bar.Foo2(); // won't give a warning

          bar.Foo2().AsAsyncUnitUniTask().ContinueWith(null);
          bar.Foo2().AsAsyncUnitUniTask().ContinueWith(null).Forget();
          var fooUniTaskFullExpression = bar.Foo2().AsAsyncUnitUniTask().ContinueWith(null);
          await bar.Foo2().AsAsyncUnitUniTask().ContinueWith(null);

          func();
          func().Forget();
          var funcUniTask = func();
          await func();

          func.Invoke();
          func.Invoke().Forget();
          var funcUniTaskInvoke = func.Invoke();
          await func.Invoke();
          // -->

          // <!-- UniTask<>
          bar.Foo3();
          bar.Foo3().Forget();
          var fooUniTaskGeneric = bar.Foo3();
          await bar.Foo3();

          bar.Foo3().AsUniTask().ContinueWith(null);
          bar.Foo3().AsUniTask().ContinueWith(null).Forget();
          var fooUniTaskGenericFullExpression = bar.Foo3().AsUniTask().ContinueWith(null);
          await bar.Foo3().AsUniTask().ContinueWith(null);
          // -->
      }
    
    public interface IBar
    {
        UniTaskVoid Foo();

        UniTask Foo2();

        UniTask<int> Foo3();
    }

    public class Bar : IBar
    {
        public UniTaskVoid Foo()
        {
            throw new NotImplementedException();
        }

        public UniTask Foo2()
        {
            throw new NotImplementedException();
        }

        public UniTask<int> Foo3()
        {
            throw new NotImplementedException();
        }
    }
```

Known issues
---
1. Works only for UniTask, UniTask<>, UniTaskVoid. For [System.Threading.Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-6.0) there is another plugin [unused-task-warning](https://github.com/ykoksen/unused-task-warning) // thx to [ykoksen](https://github.com/ykoksen) for open-source solution
2. The analyzer does not work correctly when passing through a parameter [issue](https://github.com/Yggdrasills/UniTaskAnalyzer/issues/5)
