using Amazon;
using Amazon.Runtime.Internal;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.SQSUtils
{
    public static class SQSUtility
    {
        public static async Task<List<Message>> PeekSQSMessages(PeekSQSMessagesRequestArgs args)
        {
            using (AmazonSQSClient client = new AmazonSQSClient(new AmazonSQSConfig { RegionEndpoint = args.awsRegion }))
            {
                var queueUrl = await client.GetQueueUrlAsync(new GetQueueUrlRequest() { QueueName = args.queueName, QueueOwnerAWSAccountId = args.awsAccId });
                if (!args.entireQueue)
                {
                    var request = new ReceiveMessageRequest { QueueUrl = queueUrl.QueueUrl, MaxNumberOfMessages = args.nrOfMsgs, VisibilityTimeout = args.visibilityTimeout };

                    if (args.attrNames != null && true == args.attrNames?.Any())
                        request.AttributeNames = args.attrNames;
                    else
                        request.AttributeNames = new List<string> { "All" };

                    if (args.msgAttrNames != null && true == args.msgAttrNames?.Any())
                        request.MessageAttributeNames = args.msgAttrNames;


                    var msg = await client.ReceiveMessageAsync(request);
                    return msg.Messages;
                }
                else
                {
                    var totalCnt = await GetQueueLength(args, client);
                    int msgsRead = 0;
                    List<Message> rslt = new List<Message>();
                    int nrOfCycles = totalCnt % 10 + 1;
                    int realVisibility = args.visibilityTimeout * (nrOfCycles);
                    while (msgsRead < totalCnt)
                    {
                        var request = new ReceiveMessageRequest { QueueUrl = queueUrl.QueueUrl, MaxNumberOfMessages = 10, VisibilityTimeout = realVisibility};

                        if (args.attrNames != null && true == args.attrNames?.Any())
                            request.AttributeNames = args.attrNames;
                        else
                            request.AttributeNames = new List<string> { "All" };

                        if (args.msgAttrNames != null && true == args.msgAttrNames?.Any())
                            request.MessageAttributeNames = args.msgAttrNames;


                        var msg = await client.ReceiveMessageAsync(request);
                        msgsRead += (true == msg?.Messages?.Any() ? msg.Messages.Count : 0);
                        rslt.AddRange(msg.Messages);
                    }
                    return rslt;
                }
            }

        }


        public static async Task<int> GetQueueLength(PeekSQSMessagesRequestArgs args, AmazonSQSClient client = null)
        {
            int rslt = -1;
            bool disposeClient = client == null;
            try
            {
                if (disposeClient)
                    client = new AmazonSQSClient(new AmazonSQSConfig { RegionEndpoint = args.awsRegion });
                var queueUrl = await client.GetQueueUrlAsync(new GetQueueUrlRequest() { QueueName = args.queueName, QueueOwnerAWSAccountId = args.awsAccId });

                var attrs = await client.GetQueueAttributesAsync(new GetQueueAttributesRequest() { QueueUrl = queueUrl.QueueUrl, AttributeNames = new() { "All" } });

                rslt = attrs.ApproximateNumberOfMessages;
            }
            finally
            {
                if (disposeClient) { client.Dispose(); client = null; }
            }
            return rslt;
        }
    }
}
