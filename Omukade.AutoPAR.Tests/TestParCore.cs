using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Omukade.AutoPAR.Tests
{
    public class TestParCore
    {
        const string ASSEMBLY_FILENAME = "Omukade.AutoPAR.Tests.dll";

        [Fact]
        public void PublicifyTest()
        {
            ParCore core = new ParCore();
            using MemoryStream ms = core.ProcessAssembly(ASSEMBLY_FILENAME);

            // Use a load context so we don't try to override the existing tests DLL
            AssemblyLoadContext alc = new AssemblyLoadContext("PublicifyTest", true);
            try
            {
                Assembly processedAssembly = alc.LoadFromStream(ms);
                Type outerType = processedAssembly.GetType("Omukade.AutoPAR.Tests." + nameof(TestPublicifyType_InternalType))!;

                IEnumerable<FieldInfo> nonCompilerFields = outerType.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).Where(f => f.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) == null);
                Assert.Equal(5, nonCompilerFields.Count());
                Assert.All(nonCompilerFields, f => Assert.True(f.IsPublic, "Field " + f.Name + " was not public, when it was expected to be."));

                IEnumerable<MethodInfo> nonCompilerMethods = outerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(f => f.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) == null);
                Assert.Equal(7, nonCompilerMethods.Count());
                Assert.All(nonCompilerMethods, m => Assert.True(m.IsPublic, "Method " + m.Name + " was not public, when it was expected to be."));

                IEnumerable<ConstructorInfo> nonCompilerConstructors = outerType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(f => f.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) == null);
                Assert.Equal(2, nonCompilerConstructors.Count());
                Assert.All(nonCompilerConstructors, m => Assert.True(m.IsPublic, "A constructor was not public, when it was expected to be."));

                IEnumerable<PropertyInfo> nonCompilerProperties = outerType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(f => f.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) == null);
                Assert.Equal(8, nonCompilerProperties.Count());
                Assert.All(nonCompilerProperties, p => Assert.True(p.GetGetMethod()?.IsPublic ?? true, "Property " + p.Name + " getter was not public, when it was expected to be."));
                Assert.All(nonCompilerProperties, p => Assert.True(p.GetSetMethod()?.IsPublic ?? true, "Property " + p.Name + " settter was not public, when it was expected to be."));
            }
            finally
            {
                alc.Unload();
            }
        }
    }

    [ExcludeFromCodeCoverage]
    internal class TestPublicifyType_InternalType
    {
        static TestPublicifyType_InternalType() { }

        internal TestPublicifyType_InternalType() { }
        internal TestPublicifyType_InternalType(int foo) { }

        private int privateField;
        internal int internalField;
        protected int protectedField;
        protected internal int protectedInternalField;
        public int publicField;

        private int calculatedPrivateProperty => default;
        private int setOnlyPrivateProperty { set { } }

        private int privateProperty { get; set; }
        internal int internalProperty { get; set; }
        protected int protectedProperty { get; set; }
        protected internal int protectedInternalProperty { get; set; }
        public int publicProperty { get; set; }
        public int mixedProtectionProperty { get; private set; }

        private void privateMethod() { }
        internal void internalMethod() { }
        protected void protectedMethod() { }
        protected internal void protectedInternalMethod() { }
        public void publicMethod() { }

        internal class InternalType
        {
            private int privateField;
            internal int internalField;
            protected int protectedField;
            protected internal int protectedInternalField;
            public int publicField;

            private int privateProperty { get; set; }
            internal int internalProperty { get; set; }
            protected int protectedProperty { get; set; }
            protected internal int protectedInternalProperty { get; set; }
            public int publicProperty { get; set; }
            public int mixedProtectionProperty { get; private set; }

            private void privateMethod() { }
            internal void internalMethod() { }
            protected void protectedMethod() { }
            protected internal void protectedInternalMethod() { }
            public void publicMethod() { }
        }
    }
}
