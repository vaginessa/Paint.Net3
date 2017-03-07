﻿namespace PaintDotNet
{
    using System;

    internal enum SaveTransactionState
    {
        Initializing,
        FailedInitialization,
        Initialized,
        Committing,
        FailedCommit,
        Committed,
        RollingBack,
        FailedRollback,
        RolledBack,
        Disposed
    }
}

