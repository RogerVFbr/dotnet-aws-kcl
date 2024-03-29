//
// Copyright 2019 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using ClientLibrary.inputs;

namespace ClientLibrary.messages
{
    [DataContract]
    internal class Action
    {
        protected static readonly Dictionary<string, Type> Types = new Dictionary<string, Type>()
        {
            { InitializeAction.ACTION, typeof(InitializeAction) },
            { ProcessRecordsAction.ACTION, typeof(ProcessRecordsAction) },
            { LeaseLostAction.ACTION, typeof(LeaseLostAction) },
            { ShardEndedAction.ACTION, typeof(ShardEndedAction) },
            { ShutdownRequestedAction.ACTION, typeof(ShutdownRequestedAction) },
            { CheckpointAction.ACTION, typeof(CheckpointAction) },
            { StatusAction.ACTION, typeof(StatusAction) }
        };

        public static Action Parse(string json)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                try
                {
                    // Deserialize just the action field to get the type
                    var jsonSerializer = new DataContractJsonSerializer(typeof(Action));
                    var a = jsonSerializer.ReadObject(ms) as Action;
                    // Deserialize again into the appropriate type
                    ms.Position = 0;
                    jsonSerializer = new DataContractJsonSerializer(Types[a.Type]);
                    a = jsonSerializer.ReadObject(ms) as Action;
                    return a;
                }
                catch (Exception e)
                {
                    ms.Position = 0;
                    throw new MalformedActionException("Received an action which couldn't be understood: " + json, e);
                }
            }
        }

        [DataMember(Name = "action")]
        public string Type { get; set; }

        public virtual void Dispatch(IShardRecordProcessor processor, Checkpointer checkpointer)
        {
            throw new NotImplementedException("Actions need to implement Dispatch, this likely indicates a bug.");
        }

        public string ToJson()
        {
            var jsonSerializer = new DataContractJsonSerializer(GetType());
            var ms = new MemoryStream();
            jsonSerializer.WriteObject(ms, this);
            ms.Position = 0;
            using (var sr = new StreamReader(ms))
            {
                return sr.ReadLine();
            }
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}