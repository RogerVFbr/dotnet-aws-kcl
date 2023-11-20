//
// Copyright 2019 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
//

using System.Runtime.Serialization;
using ClientLibrary.inputs;

namespace ClientLibrary.messages
{
    [DataContract]
    internal class LeaseLostAction : Action
    {
        public const string ACTION = "leaseLost";

        public LeaseLostAction()
        {
            Type = ACTION;
        }
        
        public override void Dispatch(IShardRecordProcessor processor, Checkpointer checkpointer)
        {
            processor.LeaseLost(new DefaultLeaseLostInput());
        }
    }
}