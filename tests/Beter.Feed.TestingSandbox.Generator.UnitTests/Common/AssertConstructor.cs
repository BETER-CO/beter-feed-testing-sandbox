using Moq;
using System.Reflection;
using Xunit.Sdk;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Common
{
    public class AssertConstructor
    {
        private readonly Type _type;
        private readonly ParameterInfo[] _parameters;

        internal AssertConstructor(Type type)
        {
            _type = type;
            _parameters = _type.GetConstructors().Single().GetParameters();
        }

        public AssertConstructor HasNullGuard(string parameterName)
        {
            var parameters = CreateParametersWithNullValueFor(parameterName);

            try
            {
                var e = Assert.ThrowsAny<TargetInvocationException>(() =>
                {
                    var instance = Activator.CreateInstance(_type, parameters);
                });

                var error = Assert.IsType<ArgumentNullException>(e.InnerException);
                Assert.Equal(parameterName, error.ParamName);
            }
            catch (XunitException)
            {
                Assert.Fail($"Does not exists null check for {parameterName}.");
            }

            return this;
        }

        public AssertConstructor HasNullGuard()
        {
            foreach (var parameter in _parameters)
            {
                HasNullGuard(parameter.Name);
            }

            return this;
        }

        private static Mock CreateMockParameter(ParameterInfo p)
        {
            return (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(p.ParameterType));
        }

        private object[] CreateParametersWithNullValueFor(string parameterName)
        {
            return _parameters.Select(p => p.Name == parameterName ? null : CreateMockParameter(p))
                .Select(m => m?.Object)
                .ToArray();
        }
    }

}
