using System;

namespace BBB;

internal class UserInputException : Exception
{
    public UserInputException(string s) : base(s)
    {
    }
}