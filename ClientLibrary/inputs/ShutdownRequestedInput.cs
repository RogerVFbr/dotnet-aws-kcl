//
// Copyright 2019 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
//
namespace ClientLibrary.inputs
{
    public interface ShutdownRequestedInput
    {
        Checkpointer Checkpointer { get; }
    }
}