namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using System;
    using System.Runtime.CompilerServices;

    internal static class DocumentWorkspaceExtensions
    {
        public static HistoryFunctionResult ExecuteFunction(this DocumentWorkspace dw, HistoryFunction function)
        {
            HistoryFunctionResult successNoOp;
            bool flag = false;
            if ((function.ActionFlags & ActionFlags.KeepToolActive) != ActionFlags.KeepToolActive)
            {
                dw.PushNullTool();
                dw.Update();
                flag = true;
            }
            try
            {
                using (new WaitCursorChanger(dw))
                {
                    string localizedErrorText;
                    HistoryMemento memento = null;
                    try
                    {
                        memento = function.Execute(dw);
                        if (memento == null)
                        {
                            successNoOp = HistoryFunctionResult.SuccessNoOp;
                        }
                        else
                        {
                            successNoOp = HistoryFunctionResult.Success;
                        }
                        localizedErrorText = null;
                    }
                    catch (HistoryFunctionNonFatalException exception)
                    {
                        if (exception.InnerException is OutOfMemoryException)
                        {
                            successNoOp = HistoryFunctionResult.OutOfMemory;
                        }
                        else
                        {
                            successNoOp = HistoryFunctionResult.NonFatalError;
                        }
                        if (exception.LocalizedErrorText != null)
                        {
                            localizedErrorText = exception.LocalizedErrorText;
                        }
                        else if (exception.InnerException is OutOfMemoryException)
                        {
                            localizedErrorText = PdnResources.GetString2("ExecuteFunction.GenericOutOfMemory");
                        }
                        else
                        {
                            localizedErrorText = PdnResources.GetString2("ExecuteFunction.GenericError");
                        }
                    }
                    if (localizedErrorText != null)
                    {
                        Utility.ErrorBox(dw, localizedErrorText);
                    }
                    if (memento != null)
                    {
                        dw.History.PushNewMemento(memento);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    dw.PopNullTool();
                }
            }
            return successNoOp;
        }
    }
}

