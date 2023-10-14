using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Model
{
    public enum TestTables
    {
        Test,
        Question,
        ImageTest,
        ImageQuestion,
        Answer
    }
    internal static class TestNameTables
    {

        public static string[] Values = { "Test", "Question", "TestImage", "QuestionImage", "SimpleAnswer" };
        public static string GetValue(TestTables testTables) => Values[(int)testTables];
        public static string Test { get => Values[(int)TestTables.Test]; }
        public static string Question { get => Values[(int)TestTables.Question]; }
        public static string ImageTest { get => Values[(int)TestTables.ImageTest]; }
        public static string ImageQuestion { get => Values[(int)TestTables.ImageQuestion]; }
        public static string Answer { get => Values[(int)TestTables.Answer]; }
    }
}