using Amazon;
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
        public static async Task<List<Message>> PeekSQSMessages(RegionEndpoint awsRegion, string awsAccId, string queueName, int nrOfMsgs = 10, int visibilityTimeout = 20, List<string> attrNames = null, List<string> msgAttrNames = null)
        {
            var request = new ReceiveMessageRequest { QueueUrl = $"https://sqs.eu-west-1.amazonaws.com/{awsAccId}/{queueName}", MaxNumberOfMessages = nrOfMsgs, VisibilityTimeout = visibilityTimeout};
            if (attrNames != null && true == attrNames?.Any())
                request.AttributeNames = attrNames;
            else
                request.AttributeNames = new List<string>{ "All" };

            if (msgAttrNames != null && true == msgAttrNames?.Any())
                request.MessageAttributeNames = msgAttrNames;

            AmazonSQSClient client = new AmazonSQSClient(new AmazonSQSConfig { RegionEndpoint = awsRegion });
            var msg = await client.ReceiveMessageAsync(request);
            return msg.Messages;
        }
    }
}
