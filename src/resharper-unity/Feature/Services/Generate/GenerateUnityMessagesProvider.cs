using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.Unity.Feature.Services.Generate
{
    [GeneratorElementProvider(GeneratorUnityKinds.UnityMessages, typeof(CSharpLanguage))]
    public class GenerateUnityMessagesProvider : GeneratorProviderBase<CSharpGeneratorContext>
    {
        private readonly UnityApi myUnityApi;

        public GenerateUnityMessagesProvider(UnityApi unityApi)
        {
            myUnityApi = unityApi;
        }

        public override void Populate(CSharpGeneratorContext context)
        {
            if (!context.Project.IsUnityProject())
                return;

            var typeElement = context.ClassDeclaration.DeclaredElement as IClass;
            if (typeElement == null)
                return;

            var unityTypes = myUnityApi.GetBaseUnityTypes(typeElement).ToArray();
            var events = unityTypes.SelectMany(h => h.Messages)
                .Where(m => !typeElement.Methods.Any(m.Match)).ToArray();

            var classDeclaration = context.ClassDeclaration;
            var factory = CSharpElementFactory.GetInstance(classDeclaration);
            var methods = events
                .Select(e => e.CreateDeclaration(factory, classDeclaration))
                .Select(d => d.DeclaredElement)
                .Where(m => m != null);
            // Make sure we only add a method once (e.g. EditorWindow derives from ScriptableObject
            // and both declare the OnDestroy message)
            var elements = methods.Select(m => new GeneratorDeclaredElement<IMethod>(m))
                .Distinct(m => m.TestDescriptor);
            context.ProvidedElements.AddRange(elements);
        }

        public override double Priority => 100;
    }
}