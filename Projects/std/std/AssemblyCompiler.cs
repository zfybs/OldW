using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CSharp;

namespace stdOldW
{
    /// <summary> 动态编译程序集。在程序运行的过程中动态地编译出一个程序集 .dll 或者 .exe。 </summary>
    public class AssemblyCompiler
    {
        #region Fields

        /// <summary>
        /// 源代码的架构
        /// </summary>
        private readonly CodeCompileUnit CodeUnit;

        /// <summary>
        /// 要引用的Dll的路径名称，比如"System.dll"
        /// </summary>
        private readonly string[] ReferenceDlls;

        /// <summary>
        /// 编译生成的Assembly的绝对路径
        /// </summary>
        private string AssemblyFullName;


        #endregion
        
        #region Properties

        /// <summary> 在执行了CompileAssembly方法后所得到的编译结果 </summary>
        private CompilerResults _compilerResults;

        /// <summary> 在执行了CompileAssembly方法后所得到的编译结果 </summary>
        public CompilerResults CompilerResults
        { get { return _compilerResults; } }

        #endregion

        /// <summary>
        /// 编译出对应的 CompilerResults 对象，可以通过CompilerResults.CompiledAssembly属性来返回编译成功的Assembly对象。
        /// </summary>
        /// <param name="codeUnit">源代码的架构。 codeUnit 对象的创建可以参考<see cref="CompileUnit"/>示例方法。</param>
        /// <param name="referenceDlls">要引用的Dll的路径名称，比如"System.dll"</param>
        /// <returns></returns>
        public AssemblyCompiler(CodeCompileUnit codeUnit, string[] referenceDlls)
        {
            CodeUnit = codeUnit;
            ReferenceDlls = referenceDlls;
        }

        /// <summary>
        /// 编译出对应的 CompilerResults 对象，可以通过CompilerResults.CompiledAssembly属性来返回编译成功的Assembly对象。
        /// </summary>
        /// <param name="dllFileName">最后要编译成的程序集的路径，比如"MyAssembly.dll"或者"MyAssembly.exe"</param>
        /// <returns></returns>
        public CompilerResults CompileAssembly(string dllFileName)
        {
            string sourceCode = GenerateCode(CodeUnit);
            _compilerResults = CompileCode(dllFileName, ReferenceDlls, sourceCode);
            return _compilerResults;
        }

        //根据CodeDOM产生程序代码，代码文件就是GenCodeHello.cs
        private string GenerateCode(CodeCompileUnit code)
        {
            CSharpCodeProvider cprovider = new CSharpCodeProvider();
            //当然换个Microsoft.VisualBasic.VBCodeProvider就产生VB.NET的代码
            ICodeGenerator gen = cprovider.CreateGenerator();

            // 也可以直接用字符串来存储源代码
            StringBuilder sb = new StringBuilder();
            StringWriter strW = new StringWriter(sb);

            // 将源代码存储起来

            gen.GenerateCodeFromCompileUnit(code, strW, new CodeGeneratorOptions());

            return sb.ToString();
        }

        //编译源代码
        private CompilerResults CompileCode(string dllFileName, string[] referenceDlls, string sourceCode)
        {
            CSharpCodeProvider cprovider = new CSharpCodeProvider();
            ICodeCompiler compiler = cprovider.CreateCompiler();

            //编译参数
            CompilerParameters cp = new CompilerParameters(referenceDlls, dllFileName, false);
            if (Path.GetExtension(dllFileName).IndexOf(dllFileName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // 说明文件名是以.exe 结尾
                cp.GenerateExecutable = true;
            }
            else
            {
                //生成DLL,不是EXE
                cp.GenerateExecutable = false;
            }


            CompilerResults cr = compiler.CompileAssemblyFromSource(cp, sourceCode);

            return cr;
        }

        /// <summary>
        /// 示例代码：生成CodeDOM图，这一步是最复杂的部分，后面生成代码与编译都是以这里的东西为蓝本
        /// </summary>
        /// <returns> 创建好的 CodeCompileUnit 中，包含了要生成的程序集中的全部代码，
        /// 后面只需要将通过 ICodeGenerator.GenerateCodeFromCompileUnit()方法即可以将这些代码编译为程序集。 </returns>
        /// <remarks>此函数仅为示例，并不在此类中执行。外部代码可以参数本函数来创建出对应的 CodeCompileUnit 源代码结构 </remarks>
        private static CodeCompileUnit CompileUnit()
        {
            //生成一个可编译的单元，这是最根部的东西
            CodeCompileUnit compunit = new CodeCompileUnit();
            CodeNamespace sampleNamespace = new CodeNamespace("Sample"); //定义一个名为Sample的命名空间

            CodeTypeDeclaration MyClass = new CodeTypeDeclaration("DemoClass"); //定义一个名为DemoClass的类

            // 添加字段
            CodeMemberField myField = new CodeMemberField("System.Int32", "myField1");
            myField.Attributes = MemberAttributes.Private;
            MyClass.Members.Add(myField);

            // 添加属性
            CodeMemberProperty MyProperty = new CodeMemberProperty();
            MyProperty.Name = "MyProperty1";
            MyProperty.Type = new CodeTypeReference("System.Int32");
            MyProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            MyProperty.GetStatements.Add(
                new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                    "myField1")));
            MyProperty.SetStatements.Add(
                new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "myField1"),
                    new CodePropertySetValueReferenceExpression()));
            MyClass.Members.Add(MyProperty);

            // 添加字段
            myField = new CodeMemberField("System.Int32", "myField2");
            myField.Attributes = MemberAttributes.Private;
            MyClass.Members.Add(myField);

            // 添加属性
            MyProperty = new CodeMemberProperty();
            MyProperty.Name = "MyProperty2";
            MyProperty.Type = new CodeTypeReference("System.Int32");
            MyProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            MyProperty.GetStatements.Add(
                new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                    "myField2")));
            MyProperty.SetStatements.Add(
                new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "myField2"),
                    new CodePropertySetValueReferenceExpression()));
            MyClass.Members.Add(MyProperty);


            //下面一系列语句把上述定义好的元素联接起来
            compunit.Namespaces.Add(sampleNamespace);
            sampleNamespace.Imports.Add(new CodeNamespaceImport("System")); //导入System命名空间
            sampleNamespace.Types.Add(MyClass);

            return compunit;
        }

        /// <summary> 删除编译生成的程序集文件 </summary>
        public bool DeleteAssembly()
        {
            bool succeed = false;
            if (File.Exists(AssemblyFullName))
            {
                File.Delete(AssemblyFullName);
                succeed = true;
            }
            return succeed;
        }
    }
}