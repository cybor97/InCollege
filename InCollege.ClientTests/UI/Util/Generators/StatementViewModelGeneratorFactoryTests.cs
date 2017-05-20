using InCollege.Core.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using static InCollege.Client.UI.Util.Generators.StatementViewModelGeneratorFactory;

namespace InCollege.Client.UI.Util.Generators.Tests
{
    [TestClass]
    public class StatementViewModelGeneratorFactoryTests
    {
        [TestMethod]
        public void GetGeneratorTest()
        {
            var statementResults = new[]
               {
                   new StatementResult
                   {
                        Subject = new Subject { ID = 1, SubjectIndex="МДК 07.01" },
                        Student = new Account { ID = 1, FullName="ТестОр"},
                        MarkValue = 2
                   },
                   new StatementResult
                   {
                        Subject = new Subject { ID = 1, SubjectIndex="МДК 07.01" },
                        Student = new Account { ID = 2, FullName="Вася"},
                        MarkValue = 3
                   },
                   new StatementResult
                   {
                        Subject = new Subject { ID = 2, SubjectIndex="УП 07" },
                        Student = new Account { ID = 1, FullName="ТестОр"},
                        MarkValue = 4
                   },
                   new StatementResult
                   {
                        Subject = new Subject { ID = 2, SubjectIndex="УП 07" },
                        Student = new Account { ID = 2, FullName="Вася"},
                        MarkValue = 5
                   }
            };
            var generator = GetGenerator(GeneratorType.ComplexStatement);
            var columns = generator.GetColumns(statementResults);
            var something = generator.GetResults(generator.GetColumns(statementResults).Select(c => c.name), statementResults);
            Assert.IsNotNull(something);
        }

        [TestMethod]
        public void GetGeneratorTestNoRequiredField()
        {
            var something = GetGenerator(GeneratorType.ComplexStatement).GetResults(new[] { "iouoique", "something", "something_else", "visualstudio" }, null);
            Assert.IsNotNull(something);
        }

        [TestMethod]
        public void GetGeneratorTestInvalidField()
        {
            var something = GetGenerator(GeneratorType.ComplexStatement).GetResults(new[] { "fullName", "123", "something_else", "visualstudio" }, null);
            Assert.IsNotNull(something);
        }
    }
}