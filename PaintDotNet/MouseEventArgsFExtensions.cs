namespace PaintDotNet
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal static class MouseEventArgsFExtensions
    {
        public static System.Windows.Point Point(this MouseEventArgsF e) => 
            new System.Windows.Point(e.Fx, e.Fy);
    }
}

