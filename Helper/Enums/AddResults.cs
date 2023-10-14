using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Helper.Enums
{
    public enum CommonAddResult
    {
        Success,
        NotExistsUser,
        Unknown,
    }
    public enum AddUserResult
    {
        Success,
        UserExists,
        Unknown,
    }
    internal enum AddTestResult
    {
        Success,
        TestExists,
        Unknown
    }
    internal enum AddQuestionResult
    {
        Success,
        QuestionExists,
        TestNotExists,
        Unknown
    }
    internal enum AddImageResult
    {
        Success,
        IdNotExists,
        ImageExists,
        Unknown
    }
    internal enum AddAnswerResult
    {
        Success,
        QuestionNotExists,
        AnswerExists,
        Unknown
    }
}