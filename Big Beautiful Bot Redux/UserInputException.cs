using System;

namespace Big_Beautiful_Bot_Redux
{
    internal class UserInputException : Exception
    {
        public UserInputException(string s) : base(s)
        {
        }
    }
}