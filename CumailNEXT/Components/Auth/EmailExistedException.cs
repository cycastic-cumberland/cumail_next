﻿namespace CumailNEXT.Components.Auth;

public class EmailExistedException : Exception
{
    public EmailExistedException(string message = "") : base(message)  { }
}