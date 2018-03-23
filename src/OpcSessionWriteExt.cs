using System;

namespace OpcPublisher
{
    using Opc.Ua;
    using static Workarounds.TraceWorkaround;

    public partial class OpcSession
    {
        private NodeId GetNodeIdFromExpandedNodeId(ExpandedNodeId expandedNodeId)
        {
            return new NodeId(expandedNodeId.Identifier,
                (ushort)(_namespaceTable.GetIndex(expandedNodeId.NamespaceUri)));
        }

        public bool WriteValue(WriteValuePayload payload)
        {
            // credits: https://github.com/OPCFoundation/UA-.NETStandard/issues/316
            if (!payload.IsValid)
            {
                return false;
            }
            try
            {
                var nodeToWrite = new WriteValue();
                var expandedNodeId = ExpandedNodeId.Parse(payload.ExtendedNodeId);
                nodeToWrite.NodeId = GetNodeIdFromExpandedNodeId(expandedNodeId);

                nodeToWrite.AttributeId = Attributes.Value;
                nodeToWrite.Value.WrappedValue = "actual data";

                var nodesToWrite = new WriteValueCollection();
                nodesToWrite.Add(nodeToWrite);

                // read the attribute
                StatusCodeCollection results = null;
                DiagnosticInfoCollection diagInfos = null;

                var responseHeader = OpcUaClientSession.Write(
                    null,
                    nodesToWrite,
                    out results,
                    out diagInfos
                );

                ClientBase.ValidateResponse(results, nodesToWrite);
                ClientBase.ValidateDiagnosticInfos(diagInfos, nodesToWrite);

                // check for error
                if (StatusCode.IsBad(results[0]))
                {
                    throw ServiceResultException.Create(results[0], 0,
                        diagInfos, responseHeader.StringTable);
                }
                Trace($"Value written to {payload.ExtendedNodeId}");
            }
            catch (Exception ex)
            {
                Trace($"An error occured when writing a value: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}