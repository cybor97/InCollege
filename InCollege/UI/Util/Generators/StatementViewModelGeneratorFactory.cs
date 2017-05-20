using InCollege.Core.Data;
using System;
using System.Collections.Generic;

namespace InCollege.Client.UI.Util.Generators
{
    public static class StatementViewModelGeneratorFactory
    {
        public enum GeneratorType
        {
            ComplexStatement,
            TotalStatement
        }

        public static StatementResultViewModelGenerator GetGenerator(GeneratorType generatorType)
        {
            switch (generatorType)
            {
                case GeneratorType.ComplexStatement:
                    return new ComplexStatementResultViewModelGenerator();
                case GeneratorType.TotalStatement:
                    return new TotalStatementResultViewModelGenerator();
            }
            throw new ArgumentException("Ошибка при создании генератора модели отображения. Тип ведомости не распознан.");
        }
    }

    public abstract class StatementResultViewModelGenerator
    {
        public abstract IList<(string name, string uiName)> GetColumns(IEnumerable<StatementResult> data);
        public abstract IList<object> GetResults(IEnumerable<string> columns, IEnumerable<StatementResult> data);
    }
}
